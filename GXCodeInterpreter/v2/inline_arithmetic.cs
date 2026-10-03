using System.Text.RegularExpressions;
using System.Globalization;

namespace GXCodeInterpreter;

partial class GXCodeInterpreter
{
    public static int? CalculateIntArithmetic(string input)
    {
        return CalculateArithmetic(
            input,
            s =>
            {
                bool success = int.TryParse(
                    s,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out int value);

                return (success, value);
            },
            (a, b) => a + b,
            (a, b) => a - b,
            (a, b) => a * b,
            (a, b) => a / b,
            (a, b) => (int)Math.Pow(a, b)
        );
    }

    public static decimal? CalculateDecArithmetic(string input)
    {
        return CalculateArithmetic(
            input,
            s =>
            {
                bool success = decimal.TryParse(
                    s,
                    NumberStyles.Number,
                    CultureInfo.InvariantCulture,
                    out decimal value);

                return (success, value);
            },
            (a, b) => a + b,
            (a, b) => a - b,
            (a, b) => a * b,
            (a, b) => a / b,
            (a, b) => (decimal)Math.Pow((double)a, (double)b)
        );
    }

    private static T? CalculateArithmetic<T>(
    string input,
    Func<string, (bool success, T value)> parse,
    Func<T, T, T> add,
    Func<T, T, T> subtract,
    Func<T, T, T> multiply,
    Func<T, T, T> divide,
    Func<T, T, T> power)
    where T : struct
    {
        try
        {
            Dictionary<string, (int precedence, bool rightAssociative, Func<T, T, T> operation)> operators = new()
            {
                ["+"] = (1, false, add),
                ["-"] = (1, false, subtract),
                ["*"] = (2, false, multiply),
                ["/"] = (2, false, divide),
                ["^"] = (3, true, power)
            };

            Stack<T> values = new();
            Stack<string> ops = new();

            List<string>? tokens = Tokenize(input);

            if (tokens is null)
                return null;

            foreach (string token in tokens)
            {
                var parsed = parse(token);

                if (parsed.success)
                {
                    values.Push(parsed.value);
                }
                else if (token == "(")
                {
                    ops.Push(token);
                }
                else if (token == ")")
                {
                    while (ops.Count > 0 && ops.Peek() != "(")
                    {
                        ApplyOperation(values, ops.Pop(), operators);
                    }

                    if (ops.Count == 0)
                        return null;

                    ops.Pop();
                }
                else
                {
                    if (!operators.ContainsKey(token))
                        return null;

                    while (ops.Count > 0 && ops.Peek() != "(")
                    {
                        var current = operators[token];
                        var top = operators[ops.Peek()];

                        bool shouldApply =
                            (!current.rightAssociative && top.precedence >= current.precedence) ||
                            (current.rightAssociative && top.precedence > current.precedence);

                        if (!shouldApply)
                            break;

                        ApplyOperation(values, ops.Pop(), operators);
                    }

                    ops.Push(token);
                }
            }

            while (ops.Count > 0)
            {
                if (ops.Peek() == "(")
                    return null;

                ApplyOperation(values, ops.Pop(), operators);
            }

            if (values.Count != 1)
                return null;

            return values.Pop();
        }
        catch
        {
            return null;
        }
    }

    private static void ApplyOperation<T>(
    Stack<T> values,
    string op,
    Dictionary<string, (int precedence, bool rightAssociative, Func<T, T, T> operation)> operators)
    {
        T b = values.Pop();
        T a = values.Pop();

        values.Push(operators[op].operation(a, b));
    }

    private static List<string>? Tokenize(string input)
    {
        List<string> tokens = new();

        int i = 0;

        while (i < input.Length)
        {
            if (char.IsWhiteSpace(input[i]))
            {
                i++;
                continue;
            }

            // Zahl (positiv oder negativ, ganzzahlig oder mit Nachkommastellen)
            if (char.IsDigit(input[i]) ||
                (input[i] == '-' &&
                (tokens.Count == 0 || "+-*/^(".Contains(tokens[^1])) &&
                i + 1 < input.Length &&
                (char.IsDigit(input[i + 1]) || input[i + 1] == '.')))
            {
                int start = i;

                if (input[i] == '-')
                    i++;

                bool foundDot = false;

                while (i < input.Length)
                {
                    if (char.IsDigit(input[i]))
                    {
                        i++;
                        continue;
                    }

                    if (input[i] == '.' && !foundDot)
                    {
                        foundDot = true;
                        i++;
                        continue;
                    }

                    break;
                }

                string number = input[start..i];

                // "-", ".", "-." usw. verhindern
                if (number == "-" || number == "." || number == "-.")
                    return null;

                tokens.Add(number);
                continue;
            }

            if ("+-*/^()".Contains(input[i]))
            {
                tokens.Add(input[i].ToString());
                i++;
                continue;
            }

            return null;
        }

        return tokens;
    }
}