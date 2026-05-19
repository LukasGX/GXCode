using System.Text.RegularExpressions;

namespace GXCodeInterpreter;

partial class GXCodeInterpreter
{
    public static void DeclareInstance(string line, int lineNr, string block)
    {
        string pattern = @"^\s*inst<(.*)>\s+([a-zA-Z0-9_]+)\s*=\s*(.*);?$";
        Match m = Regex.Match(line, pattern);
        if (!m.Success) throw new GXCodeInterpreterError("Could not detect instance declaration");

        string basis = m.Groups[1].Value;
        string name = m.Groups[2].Value;
        string initiator = m.Groups[3].Value;

        GXCodeClassInstance typedValue = new();
        string storedType = $"inst<{basis}>";
        GXCodeProgram.scopeStack.Peek().Set(name, typedValue, storedType, isConst: false);
    }
}