using System.Text.RegularExpressions;

namespace GXCodeInterpreter;

partial class GXCodeInterpreter
{
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
}