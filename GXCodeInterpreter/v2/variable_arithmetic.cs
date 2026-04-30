using System.Text.RegularExpressions;
namespace GXCodeInterpreter;

partial class GXCodeInterpreter
{
    public static void PerformVariableArithmetic(string line, int lineNr, string block)
    {
        string pattern = @"^\s*([a-zA-Z0-9]+)\s*([*+\-]=|-=|\*=)\s*(.*);$";
        Match match = Regex.Match(line, pattern);

        if (!match.Success)
        {
            throw new GXCodeInterpreterError("Could not detect variable arithmetic operation");
        }

        string name = match.Groups[1].Value;
        string op = match.Groups[2].Value;
        string value = match.Groups[3].Value.Trim();

        if (!GXCodeProgram.scopeStack.Peek().TryGet(name, out var currentVal, out var type))
        {
            throw new GXCUndeclaredVariableError(lineNr, name, block);
        }

        if (type is null)
        {
            throw new GXCUndeclaredVariableError(lineNr, name, block);
        }
        // Only int and dec supported
        if (type == "int")
        {
            if (currentVal is not int currInt)
                throw new GXCWrongTypeError(lineNr, name, "int", block);

            int operand;
            if (int.TryParse(value, out var litInt))
            {
                operand = litInt;
            }
            else if (GXCodeProgram.scopeStack.Peek().TryGet(value, out var varVal, out var varType) && varType == "int")
            {
                if (varVal is not int) throw new GXCWrongTypeError(lineNr, value, "int", block);
                operand = (int)varVal;
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

            decimal operand;
            if (decimal.TryParse(value, out var litDec))
            {
                operand = litDec;
            }
            else if (GXCodeProgram.scopeStack.Peek().TryGet(value, out var varVal, out var varType))
            {
                if (varType == "dec")
                {
                    if (varVal is not decimal) throw new GXCWrongTypeError(lineNr, value, "dec", block);
                    operand = (decimal)varVal;
                }
                else if (varType == "int")
                {
                    if (varVal is not int) throw new GXCWrongTypeError(lineNr, value, "int", block);
                    operand = Convert.ToDecimal((int)varVal);
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
                _ => throw new GXCodeInterpreterError("Unknown arithmetic operator")
            };

            GXCodeProgram.scopeStack.Peek().Set(name, result, "dec");
            return;
        }

        // unsupported type for arithmetic
        throw new GXCWrongArithmeticError(lineNr, block);
    }
}