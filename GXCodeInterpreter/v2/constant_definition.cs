using System.Text.RegularExpressions;

namespace GXCodeInterpreter;

partial class GXCodeInterpreter
{
    public static void DeclareConstStr(string line, int lineNr, string block)
    {
        string pattern = @"^\s*const\s+str\s*([a-zA-Z0-9]+)\s*=\s*(.*);$";
        Match m = Regex.Match(line, pattern);
        if (!m.Success) throw new GXCodeInterpreterError("Could not detect const str declaration");
        string name = m.Groups[1].Value;
        string value = m.Groups[2].Value.Trim();
        object typedValue = value.Trim('"');
        string storedType = "str";
        GXCodeProgram.scopeStack.Peek().Set(name, typedValue, storedType, isConst: true);
    }

    public static void DeclareConstInt(string line, int lineNr, string block)
    {
        string pattern = @"^\s*const\s+int\s*([a-zA-Z0-9]+)\s*=\s*(.*);$";
        Match m = Regex.Match(line, pattern);
        if (!m.Success) throw new GXCodeInterpreterError("Could not detect const int declaration");
        string name = m.Groups[1].Value;
        string value = m.Groups[2].Value.Trim();
        if (!int.TryParse(value, out int intValue)) throw new GXCWrongTypeError(lineNr, value, "int", block);
        GXCodeProgram.scopeStack.Peek().Set(name, intValue, "int", isConst: true);
    }

    public static void DeclareConstDec(string line, int lineNr, string block)
    {
        string pattern = @"^\s*const\s+dec\s*([a-zA-Z0-9]+)\s*=\s*(.*);$";
        Match m = Regex.Match(line, pattern);
        if (!m.Success) throw new GXCodeInterpreterError("Could not detect const dec declaration");
        string name = m.Groups[1].Value;
        string value = m.Groups[2].Value.Trim();
        if (!decimal.TryParse(value, out decimal decValue)) throw new GXCWrongTypeError(lineNr, value, "dec", block);
        GXCodeProgram.scopeStack.Peek().Set(name, decValue, "dec", isConst: true);
    }

    public static void DeclareConstBool(string line, int lineNr, string block)
    {
        string pattern = @"^\s*const\s+bool\s*([a-zA-Z0-9]+)\s*=\s*(.*);$";
        Match m = Regex.Match(line, pattern);
        if (!m.Success) throw new GXCodeInterpreterError("Could not detect const bool declaration");
        string name = m.Groups[1].Value;
        string value = m.Groups[2].Value.Trim();
        if (!bool.TryParse(value, out bool boolValue)) throw new GXCWrongTypeError(lineNr, value, "bool", block);
        GXCodeProgram.scopeStack.Peek().Set(name, boolValue, "bool", isConst: true);
    }

    public static void DeclareConstRex(string line, int lineNr, string block)
    {
        string pattern = @"^\s*const\s+rex\s*([a-zA-Z0-9]+)\s*=\s*(.*);$";
        Match m = Regex.Match(line, pattern);
        if (!m.Success) throw new GXCodeInterpreterError("Could not detect const rex declaration");
        string name = m.Groups[1].Value;
        string value = m.Groups[2].Value.Trim();
        try { var rx = new Regex(value); GXCodeProgram.scopeStack.Peek().Set(name, rx, "rex", isConst: true); }
        catch (Exception) { throw new GXCWrongTypeError(lineNr, value, "rex", block); }
    }
}