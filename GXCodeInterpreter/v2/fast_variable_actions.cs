using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
namespace GXCodeInterpreter;

public partial class GXCodeInterpreter
{
    public static void IncrementVariable(string line, int lineNr, string block)
    {
        string pattern = @"^\s*([a-zA-Z0-9]+)\s*\+\+;$";
        Match m = Regex.Match(line, pattern);
        if (!m.Success) throw new GXCodeInterpreterError("Could not detect variable increment");

        string name = m.Groups[1].Value;

        Variable? variable = GXCodeEnvironment.GetVariable(name)
            ?? throw new GXCUndeclaredVariableError(lineNr, name, block);

        var value = variable.Value;
        var type = variable.Type;

        if (variable.IsConstant)
            throw new GXCConstantAssignmentError(lineNr, name, block);

        if (type is null)
            throw new GXCUndeclaredVariableError(lineNr, name, block);

        if (value is not int)
            throw new GXCFastVarActionWrongVarTypeError(lineNr, type, "increment", block);

        if (type != "int")
            throw new GXCFastVarActionWrongVarTypeError(lineNr, type, "increment", block);

        int typedValue = (int) value;

        GXCodeProgram.scopeStack.Peek().Set(name, typedValue + 1, type);
    }

    public static void DecrementVariable(string line, int lineNr, string block)
    {
        string pattern = @"^\s*([a-zA-Z0-9]+)\s*--;$";
        Match m = Regex.Match(line, pattern);
        if (!m.Success) throw new GXCodeInterpreterError("Could not detect variable decrement");

        string name = m.Groups[1].Value;

        Variable? variable = GXCodeEnvironment.GetVariable(name)
            ?? throw new GXCUndeclaredVariableError(lineNr, name, block);

        var value = variable.Value;
        var type = variable.Type;

        if (variable.IsConstant)
            throw new GXCConstantAssignmentError(lineNr, name, block);

        if (type is null)
            throw new GXCUndeclaredVariableError(lineNr, name, block);

        if (value is not int)
            throw new GXCFastVarActionWrongVarTypeError(lineNr, type, "decrement", block);

        if (type != "int")
            throw new GXCFastVarActionWrongVarTypeError(lineNr, type, "decrement", block);

        int typedValue = (int) value;

        GXCodeProgram.scopeStack.Peek().Set(name, typedValue - 1, type);
    }
}