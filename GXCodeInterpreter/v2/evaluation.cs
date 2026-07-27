using System.Text;
using System.Text.RegularExpressions;

namespace GXCodeInterpreter;

partial class GXCodeInterpreter
{
    public static bool EvaluateCondition(string condition)
    {
        condition = condition.Trim();

        // match comparisons: left <op> right
        var cmp = Regex.Match(condition, "^(.*?)(==|!=|<=|>=|<|>)(.*)$");
        if (!cmp.Success)
        {
            // single token: treat as bool variable or literal
            string token = condition;
            if (bool.TryParse(token, out var bv)) return bv;

            string checkVar;
            bool rev;

            if (token.StartsWith('!'))
            {
                checkVar = token[1..];
                rev = true;
            }
            else
            {
                checkVar = token;
                rev = false;
            }

            Variable? variable = GXCodeEnvironment.GetVariable(checkVar);

            if (variable is not null && variable.Type == "bool")
            {
                if (variable.Value is not bool b) return false;
                return rev ? !b : b;
            }
            throw new GXCodeInterpreterError($"Could not evaluate condition: {condition}");
        }

        string left = cmp.Groups[1].Value.Trim();
        string op = cmp.Groups[2].Value;
        string right = cmp.Groups[3].Value.Trim();

        object Resolve(string token)
        {
            if (token.StartsWith("\"") && token.EndsWith("\"")) return token.Substring(1, token.Length - 2);
            if (bool.TryParse(token, out var b)) return b;
            if (int.TryParse(token, out var i)) return i;
            if (decimal.TryParse(token, out var d)) return d;
            // variable lookup
            Variable? variable = GXCodeEnvironment.GetVariable(token);
            if (variable is not null)
            {
                if (variable.Value is null)
                {
                    throw new GXCodeInterpreterError($"Variable {token} is null");
                }
                return variable.Value;
            };
            throw new GXCodeInterpreterError($"Unknown identifier in condition: {token}");
        }

        var lval = Resolve(left);
        var rval = Resolve(right);

        // numeric comparison if both numbers
        bool bothNumeric = (lval is int || lval is decimal) && (rval is int || rval is decimal);
        if (bothNumeric)
        {
            decimal ln = Convert.ToDecimal(lval);
            decimal rn = Convert.ToDecimal(rval);
            return op switch
            {
                "==" => ln == rn,
                "!=" => ln != rn,
                "<" => ln < rn,
                ">" => ln > rn,
                "<=" => ln <= rn,
                ">=" => ln >= rn,
                _ => throw new GXCodeInterpreterError($"Unsupported operator {op}")
            };
        }

        // boolean comparison
        if (lval is bool lb && rval is bool rb)
        {
            return op switch
            {
                "==" => lb == rb,
                "!=" => lb != rb,
                _ => throw new GXCodeInterpreterError($"Unsupported boolean operator {op}")
            };
        }

        // fallback to string comparison
        string ls = lval?.ToString() ?? "";
        string rs = rval?.ToString() ?? "";
        return op switch
        {
            "==" => string.Equals(ls, rs, StringComparison.Ordinal),
            "!=" => !string.Equals(ls, rs, StringComparison.Ordinal),
            _ => throw new GXCodeInterpreterError($"Unsupported operator {op} for string operands")
        };
    }

    public static bool EvaluateConditions(string condition, string block)
    {
        try
        { 
            Dictionary<string, (int precedence, Func<bool, bool, bool> operation)> operators = new()
            {
                { "||", (1, (a, b) => a || b) },
                { "&&", (2, (a, b) => a && b) }
            };

            List<string> tokens = TokenizeConditions(condition, block);

            Stack<bool> values = new();
            Stack<string> ops = new();

            foreach (string token in tokens)
            {
                if (token == "(")
                {
                    ops.Push(token);
                }
                else if (token == ")")
                {
                    if (ops.Count == 0)
                        throw new GXCWrongConditionError(condition, "Unexpected ')'", block);

                    while (ops.Peek() != "(")
                        ApplyBoolOperation(values, ops.Pop(), operators, block, condition);

                    if (ops.Count == 0)
                        throw new GXCWrongConditionError(condition, "Missing '('", block);

                    ops.Pop();
                }
                else if (operators.ContainsKey(token))
                {
                    while (
                        ops.Count > 0 &&
                        ops.Peek() != "(" &&
                        operators.ContainsKey(ops.Peek()) &&
                        operators[ops.Peek()].precedence >= operators[token].precedence
                    )
                    {
                        ApplyBoolOperation(values, ops.Pop(), operators, block, condition);
                    }

                    ops.Push(token);
                }
                else
                {
                    values.Push(EvaluateCondition(token));
                }
            }



            while (ops.Count > 0)
            {
                string op = ops.Pop();

                if (op == "(")
                    throw new GXCWrongConditionError(condition, "Missing ')'", block);

                ApplyBoolOperation(values, op, operators, condition, block);
            }

            if (values.Count != 1)
                throw new GXCWrongConditionError(condition, "Invalid boolean expression", block);

            return values.Pop();
        }
        catch (GXCWrongConditionError)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new GXCWrongConditionError(condition, ex.Message, block);
        }
    }

    private static void ApplyBoolOperation(
    Stack<bool> values,
    string op,
    Dictionary<string, (int precedence, Func<bool, bool, bool> operation)> operators,
    string block,
    string fullCondition)
    {
        if (values.Count < 2)
            throw new GXCWrongConditionError(fullCondition, "Missing operand", block);

        bool right = values.Pop();
        bool left = values.Pop();

        values.Push(operators[op].operation(left, right));
    }

    private static List<string> TokenizeConditions(string input, string block)
    {
        List<string> tokens = new();

        StringBuilder current = new();

        int depth = 0;
        bool inString = false;

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            if (c == '"')
            {
                inString = !inString;
                current.Append(c);
                continue;
            }

            if (inString)
            {
                current.Append(c);
                continue;
            }

            if (c == '(')
            {
                if (current.ToString().Trim().Length > 0)
                {
                    tokens.Add(current.ToString().Trim());
                    current.Clear();
                }

                tokens.Add("(");
                depth++;
                continue;
            }

            if (c == ')')
            {
                if (current.ToString().Trim().Length > 0)
                {
                    tokens.Add(current.ToString().Trim());
                    current.Clear();
                }

                tokens.Add(")");
                depth--;
                if (depth < 0)
                    throw new GXCWrongConditionError(input, "Unexpected ')'", block);
                continue;
            }

            if (depth > 0 &&
                i + 1 < input.Length &&
                c == '&' &&
                input[i + 1] == '&')
            {
                if (current.ToString().Trim().Length > 0)
                {
                    tokens.Add(current.ToString().Trim());
                    current.Clear();
                }

                tokens.Add("&&");
                i++;
                continue;
            }

            if (depth > 0 &&
                i + 1 < input.Length &&
                c == '|' &&
                input[i + 1] == '|')
            {
                if (current.ToString().Trim().Length > 0)
                {
                    tokens.Add(current.ToString().Trim());
                    current.Clear();
                }

                tokens.Add("||");
                i++;
                continue;
            }

            if (depth == 0 &&
                i + 1 < input.Length &&
                c == '&' &&
                input[i + 1] == '&')
            {
                if (current.ToString().Trim().Length > 0)
                {
                    tokens.Add(current.ToString().Trim());
                    current.Clear();
                }

                tokens.Add("&&");
                i++;
                continue;
            }

            if (depth == 0 &&
                i + 1 < input.Length &&
                c == '|' &&
                input[i + 1] == '|')
            {
                if (current.ToString().Trim().Length > 0)
                {
                    tokens.Add(current.ToString().Trim());
                    current.Clear();
                }

                tokens.Add("||");
                i++;
                continue;
            }

            current.Append(c);
        }

        if (current.ToString().Trim().Length > 0)
            tokens.Add(current.ToString().Trim());

        if (depth != 0)
            throw new GXCWrongConditionError(input, "Missing ')'.", block);

        return tokens;
    }
}