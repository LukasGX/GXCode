using System.Text.RegularExpressions;
namespace GXCodeInterpreter;

partial class GXCodeInterpreter
{
    public static void PerformVariableArithmetic(string line, int lineNr, string block)
    {
        string pattern = @"^\s*([a-zA-Z0-9]+)\s*(\+=|-=|\*=|/=|\^=)\s*(.*);$";
        Match match = Regex.Match(line, pattern);

        if (!match.Success)
        {
            throw new GXCodeInterpreterError("Could not detect variable arithmetic operation");
        }

        string name = match.Groups[1].Value;
        string op = match.Groups[2].Value;
        string value = match.Groups[3].Value.Trim();

        Variable? variable = GXCodeEnvironment.GetVariable(name)
            ?? throw new GXCUndeclaredVariableError(lineNr, name, block);

        var currentVal = variable.Value;
        var type = variable.Type
            ?? throw new GXCUndeclaredVariableError(lineNr, name, block);

        // Only int and dec supported
        if (type == "int")
        {
            if (currentVal is not int currInt)
                throw new GXCWrongTypeError(lineNr, name, "int", block);

            Variable? integer = GXCodeEnvironment.GetVariable(value);

            int operand;
            if (int.TryParse(value, out var litInt))
            {
                operand = litInt;
            }
            else if (integer is not null && integer.Type == "int")
            {
                if (integer.Value is not int) throw new GXCWrongTypeError(lineNr, value, "int", block);
                operand = (int) integer.Value;
            }
            else
            {
                throw new GXCWrongTypeError(lineNr, value, "int", block);
            }

            int result = op switch
            {
                "+=" => currInt + operand,
                "-=" => currInt - operand,
                "*=" => currInt * operand,
                "/=" => currInt / operand,
                "^=" => (int)Math.Pow(currInt, operand),
                _ => throw new GXCodeInterpreterError("Unknown arithmetic operator")
            };

            GXCodeProgram.scopeStack.Peek().Set(name, result, "int");
            return;
        }

        if (type == "dec")
        {
            decimal currDec;
            if (currentVal is decimal d) currDec = d;
            else if (currentVal is int i) currDec = Convert.ToDecimal(i);
            else throw new GXCWrongTypeError(lineNr, name, "dec", block);

            Variable? dec = GXCodeEnvironment.GetVariable(value);

            decimal operand;
            if (decimal.TryParse(value, out var litDec))
            {
                operand = litDec;
            }
            else if (dec is not null)
            {
                if (dec.Type == "dec")
                {
                    if (dec.Value is not decimal) throw new GXCWrongTypeError(lineNr, value, "dec", block);
                    operand = (decimal) dec.Value;
                }
                else if (dec.Type == "int")
                {
                    if (dec.Value is not int) throw new GXCWrongTypeError(lineNr, value, "int", block);
                    operand = Convert.ToDecimal((int) dec.Value);
                }
                else
                {
                    throw new GXCWrongTypeError(lineNr, value, "dec", block);
                }
            }
            else
            {
                throw new GXCWrongTypeError(lineNr, value, "dec", block);
            }

            decimal result = op switch
            {
                "+=" => currDec + operand,
                "-=" => currDec - operand,
                "*=" => currDec * operand,
                "/=" => currDec / operand,
                "^=" => (decimal)Math.Pow((double)currDec, (double)operand),
                _ => throw new GXCodeInterpreterError("Unknown arithmetic operator")
            };

            GXCodeProgram.scopeStack.Peek().Set(name, result, "dec");
            return;
        }

        // unsupported type for arithmetic
        throw new GXCWrongArithmeticError(lineNr, block);
    }
}