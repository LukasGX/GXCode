using System.Text.RegularExpressions;

namespace GXCodeInterpreter;

partial class GXCodeInterpreter
{
    public static void DeclareVariable(string line, int lineNr, string block)
    {
        string pattern = @"^\s*(str|int|dec|bool|rex)(\[\]|\{(str|int|dec|bool|rex)\})?\s*([a-zA-Z0-9]+)\s*=\s*(.*);$";
        Match match = Regex.Match(line, pattern);

        if (!match.Success)
        {
            throw new GXCodeInterpreterError("Could not detect variable declaration");
        }

        string baseType = match.Groups[1].Value;
        string arrayOrDictToken = match.Groups[2].Value;
        string dictValueType = match.Groups[3].Value;
        string name = match.Groups[4].Value;
        string value = match.Groups[5].Value.Trim();

        object typedValue;
        string storedType;

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

        if (arrayOrDictToken == "[]")
        {
            // parse array syntax: [a, b, c]
            if (!value.StartsWith("[") || !value.EndsWith("]"))
                throw new GXCWrongTypeError(lineNr, value, baseType + "[]", block);

            string inner = value.Substring(1, value.Length - 2);
            var items = SplitTopLevelItems(inner);

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
                    typedValue = sList;
                    storedType = "str[]";
                    break;
                case "int":
                    var iList = new List<int>();
                    foreach (var it in items)
                    {
                        if (int.TryParse(it.Trim(), out var iv)) iList.Add(iv);
                        else throw new GXCWrongTypeError(lineNr, value, "int[]", block);
                    }
                    typedValue = iList;
                    storedType = "int[]";
                    break;
                case "dec":
                    var dList = new List<decimal>();
                    foreach (var it in items)
                    {
                        if (decimal.TryParse(it.Trim(), out var dv)) dList.Add(dv);
                        else throw new GXCWrongTypeError(lineNr, value, "dec[]", block);
                    }
                    typedValue = dList;
                    storedType = "dec[]";
                    break;
                case "bool":
                    var bList = new List<bool>();
                    foreach (var it in items)
                    {
                        if (bool.TryParse(it.Trim(), out var bv)) bList.Add(bv);
                        else throw new GXCWrongTypeError(lineNr, value, "bool[]", block);
                    }
                    typedValue = bList;
                    storedType = "bool[]";
                    break;
                case "rex":
                    var rList = new List<Regex>();
                    foreach (var it in items)
                    {
                        try { rList.Add(new Regex(it.Trim())); }
                        catch { throw new GXCWrongTypeError(lineNr, value, "rex[]", block); }
                    }
                    typedValue = rList;
                    storedType = "rex[]";
                    break;
                default:
                    throw new GXCUnsupportedTypeError(lineNr, baseType + "[]", block);
            }
        }
        else if (!string.IsNullOrEmpty(dictValueType))
        {
            // dict: baseType{dictValueType} ; value expected like {k:v, k2:v2}
            if (!value.StartsWith("{") || !value.EndsWith("}"))
                throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);

            string inner = value.Substring(1, value.Length - 2);
            var pairs = SplitTopLevelItems(inner);

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
                    typedValue = sd;
                    storedType = "str{str}";
                    break;
                default:
                    throw new GXCUnsupportedTypeError(lineNr, baseType + "{" + dictValueType + "}", block);
            }
        }
        else
        {
            // scalar types
            switch (baseType)
            {
                case "str":
                    typedValue = value.Trim('"');
                    storedType = "str";
                    break;
                case "int":
                    if (int.TryParse(value, out int intValue))
                    {
                        typedValue = intValue;
                        storedType = "int";
                    }
                    else
                    {
                        throw new GXCWrongTypeError(lineNr, value, "int", block);
                    }
                    break;
                case "dec":
                    if (decimal.TryParse(value, out decimal decValue))
                    {
                        typedValue = decValue;
                        storedType = "dec";
                    }
                    else
                    {
                        throw new GXCWrongTypeError(lineNr, value, "dec", block);
                    }
                    break;
                case "bool":
                    if (bool.TryParse(value, out bool boolValue))
                    {
                        typedValue = boolValue;
                        storedType = "bool";
                    }
                    else
                    {
                        throw new GXCWrongTypeError(lineNr, value, "bool", block);
                    }
                    break;
                case "rex":
                    try
                    {
                        typedValue = new Regex(value);
                        storedType = "rex";
                    }
                    catch (Exception)
                    {
                        throw new GXCWrongTypeError(lineNr, value, "rex", block);
                    }
                    break;
                default:
                    throw new GXCUnsupportedTypeError(lineNr, baseType, block);
            }
        }

        GXCodeProgram.scopeStack.Peek().Set(name, typedValue, storedType);
    }
}