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

    // Scalar-specific declare methods (no shared DoDeclare)
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

    public static void DeclareInt(string line, int lineNr, string block)
    {
        string pattern = @"^\s*int\s*([a-zA-Z0-9]+)\s*=\s*(.*);$";
        Match m = Regex.Match(line, pattern);
        if (!m.Success) throw new GXCodeInterpreterError("Could not detect int declaration");
        string name = m.Groups[1].Value;
        string value = m.Groups[2].Value.Trim();
        if (!int.TryParse(value, out int intValue)) throw new GXCWrongTypeError(lineNr, value, "int", block);
        GXCodeProgram.scopeStack.Peek().Set(name, intValue, "int", isConst: false);
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

    public static void DeclareDec(string line, int lineNr, string block)
    {
        string pattern = @"^\s*dec\s*([a-zA-Z0-9]+)\s*=\s*(.*);$";
        Match m = Regex.Match(line, pattern);
        if (!m.Success) throw new GXCodeInterpreterError("Could not detect dec declaration");
        string name = m.Groups[1].Value;
        string value = m.Groups[2].Value.Trim();
        if (!decimal.TryParse(value, out decimal decValue)) throw new GXCWrongTypeError(lineNr, value, "dec", block);
        GXCodeProgram.scopeStack.Peek().Set(name, decValue, "dec", isConst: false);
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

    public static void DeclareArray(string line, int lineNr, string block)
    {
        string pattern = @"^\s*(?:const\s+)?(str|int|dec|bool|rex)\[\]\s*([a-zA-Z0-9]+)\s*=\s*(.*);$";
        Match m = Regex.Match(line, pattern);
        if (!m.Success) throw new GXCodeInterpreterError("Could not detect array declaration");
        string baseType = m.Groups[1].Value;
        string name = m.Groups[2].Value;
        string value = m.Groups[3].Value.Trim();

        if (!value.StartsWith("[") || !value.EndsWith("]"))
            throw new GXCWrongTypeError(lineNr, value, baseType + "[]", block);

        string inner = value.Substring(1, value.Length - 2);
        var items = SplitTopLevelItems(inner);

        object typedValue;
        string storedType;

        switch (baseType)
        {
            case "str":
                var sList = new List<string>();
                foreach (var it in items)
                {
                    string v = it.Trim();
                    if (v.StartsWith("\"") && v.EndsWith("\"")) v = v.Substring(1, v.Length - 2);
                    sList.Add(v);
                }
                typedValue = sList; storedType = "str[]"; break;
            case "int":
                var iList = new List<int>();
                foreach (var it in items)
                {
                    if (int.TryParse(it.Trim(), out var iv)) iList.Add(iv);
                    else throw new GXCWrongTypeError(lineNr, value, "int[]", block);
                }
                typedValue = iList; storedType = "int[]"; break;
            case "dec":
                var dList = new List<decimal>();
                foreach (var it in items)
                {
                    if (decimal.TryParse(it.Trim(), out var dv)) dList.Add(dv);
                    else throw new GXCWrongTypeError(lineNr, value, "dec[]", block);
                }
                typedValue = dList; storedType = "dec[]"; break;
            case "bool":
                var bList = new List<bool>();
                foreach (var it in items)
                {
                    if (bool.TryParse(it.Trim(), out var bv)) bList.Add(bv);
                    else throw new GXCWrongTypeError(lineNr, value, "bool[]", block);
                }
                typedValue = bList; storedType = "bool[]"; break;
            case "rex":
                var rList = new List<Regex>();
                foreach (var it in items)
                {
                    try { rList.Add(new Regex(it.Trim())); }
                    catch { throw new GXCWrongTypeError(lineNr, value, "rex[]", block); }
                }
                typedValue = rList; storedType = "rex[]"; break;
            default:
                throw new GXCUnsupportedTypeError(lineNr, baseType + "[]", block);
        }

        GXCodeProgram.scopeStack.Peek().Set(name, typedValue, storedType, isConst: false);
    }

    public static void DeclareConstArray(string line, int lineNr, string block)
    {
        // same as DeclareArray but marks as const
        string pattern = @"^\s*const\s+(str|int|dec|bool|rex)\[\]\s*([a-zA-Z0-9]+)\s*=\s*(.*);$";
        Match m = Regex.Match(line, pattern);
        if (!m.Success) throw new GXCodeInterpreterError("Could not detect const array declaration");
        string baseType = m.Groups[1].Value;
        string name = m.Groups[2].Value;
        string value = m.Groups[3].Value.Trim();

        if (!value.StartsWith("[") || !value.EndsWith("]"))
            throw new GXCWrongTypeError(lineNr, value, baseType + "[]", block);

        string inner = value.Substring(1, value.Length - 2);
        var items = SplitTopLevelItems(inner);

        object typedValue;
        string storedType;

        switch (baseType)
        {
            case "str":
                var sList = new List<string>();
                foreach (var it in items)
                {
                    string v = it.Trim();
                    if (v.StartsWith("\"") && v.EndsWith("\"")) v = v.Substring(1, v.Length - 2);
                    sList.Add(v);
                }
                typedValue = sList; storedType = "str[]"; break;
            case "int":
                var iList = new List<int>();
                foreach (var it in items)
                {
                    if (int.TryParse(it.Trim(), out var iv)) iList.Add(iv);
                    else throw new GXCWrongTypeError(lineNr, value, "int[]", block);
                }
                typedValue = iList; storedType = "int[]"; break;
            case "dec":
                var dList = new List<decimal>();
                foreach (var it in items)
                {
                    if (decimal.TryParse(it.Trim(), out var dv)) dList.Add(dv);
                    else throw new GXCWrongTypeError(lineNr, value, "dec[]", block);
                }
                typedValue = dList; storedType = "dec[]"; break;
            case "bool":
                var bList = new List<bool>();
                foreach (var it in items)
                {
                    if (bool.TryParse(it.Trim(), out var bv)) bList.Add(bv);
                    else throw new GXCWrongTypeError(lineNr, value, "bool[]", block);
                }
                typedValue = bList; storedType = "bool[]"; break;
            case "rex":
                var rList = new List<Regex>();
                foreach (var it in items)
                {
                    try { rList.Add(new Regex(it.Trim())); }
                    catch { throw new GXCWrongTypeError(lineNr, value, "rex[]", block); }
                }
                typedValue = rList; storedType = "rex[]"; break;
            default:
                throw new GXCUnsupportedTypeError(lineNr, baseType + "[]", block);
        }

        GXCodeProgram.scopeStack.Peek().Set(name, typedValue, storedType, isConst: true);
    }

    public static void DeclareDict(string line, int lineNr, string block)
    {
        string pattern = @"^\s*(?:const\s+)?(str|int|dec|bool|rex)\{([a-z;]+)\}\s*([a-zA-Z0-9]+)\s*=\s*(.*);$";
        Match m = Regex.Match(line, pattern);
        if (!m.Success) throw new GXCodeInterpreterError("Could not detect dict declaration");
        string baseType = m.Groups[1].Value;
        string dictValueType = m.Groups[2].Value;
        string name = m.Groups[3].Value;
        string value = m.Groups[4].Value.Trim();

        if (!value.StartsWith("{") || !value.EndsWith("}"))
            throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);

        string inner = value.Substring(1, value.Length - 2);
        var pairs = SplitTopLevelItems(inner);

        object typedValue;
        string storedType;

        switch (baseType)
        {
            case "str" when dictValueType == "str":
                var sd = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var pair in pairs)
                {
                    int idx = -1; bool inQ = false;
                    for (int i = 0; i < pair.Length; i++) { if (pair[i] == '"') inQ = !inQ; if (pair[i] == ':' && !inQ) { idx = i; break; } }
                    if (idx < 0) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                    var k = pair.Substring(0, idx).Trim(); var v = pair.Substring(idx + 1).Trim();
                    if (k.StartsWith("\"") && k.EndsWith("\"")) k = k.Substring(1, k.Length - 2);
                    if (v.StartsWith("\"") && v.EndsWith("\"")) v = v.Substring(1, v.Length - 2);
                    sd[k] = v;
                }
                typedValue = sd; storedType = "str{str}"; break;
            default:
                throw new GXCUnsupportedTypeError(lineNr, baseType + "{" + dictValueType + "}", block);
        }

        GXCodeProgram.scopeStack.Peek().Set(name, typedValue, storedType, isConst: false);
    }

    public static void DeclareConstDict(string line, int lineNr, string block)
    {
        string pattern = @"^\s*const\s+(str)\{([a-z;]+)\}\s*([a-zA-Z0-9]+)\s*=\s*(.*);$";
        Match m = Regex.Match(line, pattern);
        if (!m.Success) throw new GXCodeInterpreterError("Could not detect const dict declaration");
        string baseType = m.Groups[1].Value;
        string dictValueType = m.Groups[2].Value;
        string name = m.Groups[3].Value;
        string value = m.Groups[4].Value.Trim();

        if (!value.StartsWith("{") || !value.EndsWith("}"))
            throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);

        string inner = value.Substring(1, value.Length - 2);
        var pairs = SplitTopLevelItems(inner);

        object typedValue;
        string storedType;

        switch (baseType)
        {
            case "str" when dictValueType == "str":
                var sd = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var pair in pairs)
                {
                    int idx = -1; bool inQ = false;
                    for (int i = 0; i < pair.Length; i++) { if (pair[i] == '"') inQ = !inQ; if (pair[i] == ':' && !inQ) { idx = i; break; } }
                    if (idx < 0) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                    var k = pair.Substring(0, idx).Trim(); var v = pair.Substring(idx + 1).Trim();
                    if (k.StartsWith("\"") && k.EndsWith("\"")) k = k.Substring(1, k.Length - 2);
                    if (v.StartsWith("\"") && v.EndsWith("\"")) v = v.Substring(1, v.Length - 2);
                    sd[k] = v;
                }
                typedValue = sd; storedType = "str{str}"; break;
            default:
                throw new GXCUnsupportedTypeError(lineNr, baseType + "{" + dictValueType + "}", block);
        }

        GXCodeProgram.scopeStack.Peek().Set(name, typedValue, storedType, isConst: true);
    }
}