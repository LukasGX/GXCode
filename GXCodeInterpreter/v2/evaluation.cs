using System.Text.RegularExpressions;

namespace GXCodeInterpreter;

partial class GXCodeInterpreter
{
    public static bool EvaluateCondition(GXCodeEnvironment env, string condition)
        {
            condition = condition.Trim();

            // match comparisons: left <op> right
            var cmp = Regex.Match(condition, "^(.*?)(==|!=|<=|>=|<|>)(.*)$");
            if (!cmp.Success)
            {
                // single token: treat as bool variable or literal
                string token = condition;
                if (bool.TryParse(token, out var bv)) return bv;
                if (GXCodeProgram.scopeStack.Peek().TryGet(token, out var val, out var type) && type == "bool")
                {
                    return val is bool b && b;
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
                if (GXCodeProgram.scopeStack.Peek().TryGet(token, out var v, out var t))
                {
                    if (v is null)
                    {
                        throw new GXCodeInterpreterError($"Variable {token} is null");
                    }
                    return v;
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
}