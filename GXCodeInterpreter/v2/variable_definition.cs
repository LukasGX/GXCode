using System.Text.RegularExpressions;

namespace GXCodeInterpreter;

partial class GXCodeInterpreter
{
    static List<string> SplitTopLevelItems(string s)
    {
        List<string> parts = new();
        if (string.IsNullOrWhiteSpace(s)) return parts;
        var sb = new System.Text.StringBuilder();
        bool inQuote = false;
        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];
            if (c == '"') { inQuote = !inQuote; sb.Append(c); continue; }
            if (c == ',' && !inQuote)
            {
                parts.Add(sb.ToString().Trim());
                sb.Clear();
                continue;
            }
            sb.Append(c);
        }
        parts.Add(sb.ToString().Trim());
        return parts;
    }

    public static void DeclareStr(string line, int lineNr, string block)
    {
        string pattern = @"^\s*str\s*([a-zA-Z0-9]+)\s*=\s*(.*);$";
        Match m = Regex.Match(line, pattern);
        if (!m.Success) throw new GXCodeInterpreterError("Could not detect str declaration");
        string name = m.Groups[1].Value;
        string value = m.Groups[2].Value.Trim();
        object typedValue = value.Trim('"');
        string storedType = "str";
        GXCodeProgram.scopeStack.Peek().Set(name, typedValue, storedType, isConst: false);
    }

    public static void DeclareInt(string line, int lineNr, string block)
    {
        string pattern = @"^\s*int\s*([a-zA-Z0-9]+)\s*=\s*(.*);$";
        Match m = Regex.Match(line, pattern);
        if (!m.Success) throw new GXCodeInterpreterError("Could not detect int declaration");
        string name = m.Groups[1].Value;
        string value = m.Groups[2].Value.Trim();

        bool parseTest = int.TryParse(value, out int intValue);
        if (parseTest)
            GXCodeProgram.scopeStack.Peek().Set(name, intValue, "int", isConst: false);
        else
        {
            int? val = CalculateIntArithmetic(value) ?? throw new GXCWrongTypeError(lineNr, value, "int", block);
            GXCodeProgram.scopeStack.Peek().Set(name, val, "int", isConst: false);
        }
    }

    public static void DeclareDec(string line, int lineNr, string block)
    {
        string pattern = @"^\s*dec\s*([a-zA-Z0-9]+)\s*=\s*(.*);$";
        Match m = Regex.Match(line, pattern);
        if (!m.Success) throw new GXCodeInterpreterError("Could not detect dec declaration");
        string name = m.Groups[1].Value;
        string value = m.Groups[2].Value.Trim();

        bool parseTest = decimal.TryParse(value, out decimal decValue);
        if (parseTest)
            GXCodeProgram.scopeStack.Peek().Set(name, decValue, "dec", isConst: false);
        else
        {
            decimal? val = CalculateDecArithmetic(value) ?? throw new GXCWrongTypeError(lineNr, value, "dec", block);
            GXCodeProgram.scopeStack.Peek().Set(name, val, "dec", isConst: false);
        }
    }

    public static void DeclareBool(string line, int lineNr, string block)
    {
        string pattern = @"^\s*bool\s*([a-zA-Z0-9]+)\s*=\s*(.*);$";
        Match m = Regex.Match(line, pattern);
        if (!m.Success) throw new GXCodeInterpreterError("Could not detect bool declaration");
        string name = m.Groups[1].Value;
        string value = m.Groups[2].Value.Trim();
        if (!bool.TryParse(value, out bool boolValue)) throw new GXCWrongTypeError(lineNr, value, "bool", block);
        GXCodeProgram.scopeStack.Peek().Set(name, boolValue, "bool", isConst: false);
    }

    public static void DeclareRex(string line, int lineNr, string block)
    {
        string pattern = @"^\s*rex\s*([a-zA-Z0-9]+)\s*=\s*(.*);$";
        Match m = Regex.Match(line, pattern);
        if (!m.Success) throw new GXCodeInterpreterError("Could not detect rex declaration");
        string name = m.Groups[1].Value;
        string value = m.Groups[2].Value.Trim();
        try { var rx = new Regex(value); GXCodeProgram.scopeStack.Peek().Set(name, rx, "rex", isConst: false); }
        catch (Exception) { throw new GXCWrongTypeError(lineNr, value, "rex", block); }
    }
}