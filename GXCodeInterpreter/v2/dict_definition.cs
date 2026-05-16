using System.Text.RegularExpressions;

namespace GXCodeInterpreter;

partial class GXCodeInterpreter
{
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
            case "str":
                switch (dictValueType)
                {
                    case "str":
                        var sdss = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        foreach (var pair in pairs)
                        {
                            int idx = -1; bool inQ = false;
                            for (int i = 0; i < pair.Length; i++) { if (pair[i] == '"') inQ = !inQ; if (pair[i] == ':' && !inQ) { idx = i; break; } }
                            if (idx < 0) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            var k = pair.Substring(0, idx).Trim(); var v = pair.Substring(idx + 1).Trim();
                            if (k.StartsWith("\"") && k.EndsWith("\"")) k = k.Substring(1, k.Length - 2);
                            if (v.StartsWith("\"") && v.EndsWith("\"")) v = v.Substring(1, v.Length - 2);
                            sdss[k] = v;
                        }
                        typedValue = sdss; storedType = "str{str}"; break;
                    case "int":
                        var sdi = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                        foreach (var pair in pairs)
                        {
                            int idx = -1; bool inQ = false;
                            for (int i = 0; i < pair.Length; i++) { if (pair[i] == '"') inQ = !inQ; if (pair[i] == ':' && !inQ) { idx = i; break; } }
                            if (idx < 0) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            var k = pair.Substring(0, idx).Trim(); var v = pair.Substring(idx + 1).Trim();
                            if (k.StartsWith("\"") && k.EndsWith("\"")) k = k.Substring(1, k.Length - 2);
                            if (int.TryParse(v.Trim('"'), out var iv)) sdi[k] = iv; else throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                        }
                        typedValue = sdi; storedType = "str{int}"; break;
                    case "dec":
                        var sdd = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
                        foreach (var pair in pairs)
                        {
                            int idx = -1; bool inQ = false;
                            for (int i = 0; i < pair.Length; i++) { if (pair[i] == '"') inQ = !inQ; if (pair[i] == ':' && !inQ) { idx = i; break; } }
                            if (idx < 0) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            var k = pair.Substring(0, idx).Trim(); var v = pair.Substring(idx + 1).Trim();
                            if (k.StartsWith("\"") && k.EndsWith("\"")) k = k.Substring(1, k.Length - 2);
                            if (decimal.TryParse(v.Trim('"'), out var dv)) sdd[k] = dv; else throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                        }
                        typedValue = sdd; storedType = "str{dec}"; break;
                    case "bool":
                        var sdb = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
                        foreach (var pair in pairs)
                        {
                            int idx = -1; bool inQ = false;
                            for (int i = 0; i < pair.Length; i++) { if (pair[i] == '"') inQ = !inQ; if (pair[i] == ':' && !inQ) { idx = i; break; } }
                            if (idx < 0) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            var k = pair.Substring(0, idx).Trim(); var v = pair.Substring(idx + 1).Trim();
                            if (k.StartsWith("\"") && k.EndsWith("\"")) k = k.Substring(1, k.Length - 2);
                            if (bool.TryParse(v.Trim('"'), out var bv)) sdb[k] = bv; else throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                        }
                        typedValue = sdb; storedType = "str{bool}"; break;
                    case "rex":
                        var sdr = new Dictionary<string, Regex>(StringComparer.OrdinalIgnoreCase);
                        foreach (var pair in pairs)
                        {
                            int idx = -1; bool inQ = false;
                            for (int i = 0; i < pair.Length; i++) { if (pair[i] == '"') inQ = !inQ; if (pair[i] == ':' && !inQ) { idx = i; break; } }
                            if (idx < 0) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            var k = pair.Substring(0, idx).Trim(); var v = pair.Substring(idx + 1).Trim();
                            if (k.StartsWith("\"") && k.EndsWith("\"")) k = k.Substring(1, k.Length - 2);
                            try { sdr[k] = new Regex(v.Trim('"')); } catch { throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block); }
                        }
                        typedValue = sdr; storedType = "str{rex}"; break;
                    default: throw new GXCUnsupportedTypeError(lineNr, baseType + "{" + dictValueType + "}", block);
                }
                break;
            case "int":
                switch (dictValueType)
                {
                    case "str":
                        var ids = new Dictionary<int, string>();
                        foreach (var pair in pairs)
                        {
                            int idx = -1; bool inQ = false;
                            for (int i = 0; i < pair.Length; i++) { if (pair[i] == '"') inQ = !inQ; if (pair[i] == ':' && !inQ) { idx = i; break; } }
                            if (idx < 0) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            var kTok = pair.Substring(0, idx).Trim(); var vTok = pair.Substring(idx + 1).Trim();
                            var kStr = kTok.StartsWith("\"") && kTok.EndsWith("\"") ? kTok.Substring(1, kTok.Length - 2) : kTok;
                            if (!int.TryParse(kStr, out var k)) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            if (vTok.StartsWith("\"") && vTok.EndsWith("\"")) vTok = vTok.Substring(1, vTok.Length - 2);
                            ids[k] = vTok;
                        }
                        typedValue = ids; storedType = "int{str}"; break;
                    case "int":
                        var idi = new Dictionary<int, int>();
                        foreach (var pair in pairs)
                        {
                            int idx = -1; bool inQ = false;
                            for (int i = 0; i < pair.Length; i++) { if (pair[i] == '"') inQ = !inQ; if (pair[i] == ':' && !inQ) { idx = i; break; } }
                            if (idx < 0) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            var kTok = pair.Substring(0, idx).Trim(); var vTok = pair.Substring(idx + 1).Trim();
                            var kStr = kTok.StartsWith("\"") && kTok.EndsWith("\"") ? kTok.Substring(1, kTok.Length - 2) : kTok;
                            if (!int.TryParse(kStr, out var k)) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            if (!int.TryParse(vTok.Trim('"'), out var v)) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            idi[k] = v;
                        }
                        typedValue = idi; storedType = "int{int}"; break;
                    case "dec":
                        var idd = new Dictionary<int, decimal>();
                        foreach (var pair in pairs)
                        {
                            int idx = -1; bool inQ = false;
                            for (int i = 0; i < pair.Length; i++) { if (pair[i] == '"') inQ = !inQ; if (pair[i] == ':' && !inQ) { idx = i; break; } }
                            if (idx < 0) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            var kTok = pair.Substring(0, idx).Trim(); var vTok = pair.Substring(idx + 1).Trim();
                            var kStr = kTok.StartsWith("\"") && kTok.EndsWith("\"") ? kTok.Substring(1, kTok.Length - 2) : kTok;
                            if (!int.TryParse(kStr, out var k)) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            if (!decimal.TryParse(vTok.Trim('"'), out var v)) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            idd[k] = v;
                        }
                        typedValue = idd; storedType = "int{dec}"; break;
                    case "bool":
                        var idb = new Dictionary<int, bool>();
                        foreach (var pair in pairs)
                        {
                            int idx = -1; bool inQ = false;
                            for (int i = 0; i < pair.Length; i++) { if (pair[i] == '"') inQ = !inQ; if (pair[i] == ':' && !inQ) { idx = i; break; } }
                            if (idx < 0) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            var kTok = pair.Substring(0, idx).Trim(); var vTok = pair.Substring(idx + 1).Trim();
                            var kStr = kTok.StartsWith("\"") && kTok.EndsWith("\"") ? kTok.Substring(1, kTok.Length - 2) : kTok;
                            if (!int.TryParse(kStr, out var k)) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            if (!bool.TryParse(vTok.Trim('"'), out var v)) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            idb[k] = v;
                        }
                        typedValue = idb; storedType = "int{bool}"; break;
                    case "rex":
                        var idr = new Dictionary<int, Regex>();
                        foreach (var pair in pairs)
                        {
                            int idx = -1; bool inQ = false;
                            for (int i = 0; i < pair.Length; i++) { if (pair[i] == '"') inQ = !inQ; if (pair[i] == ':' && !inQ) { idx = i; break; } }
                            if (idx < 0) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            var kTok = pair.Substring(0, idx).Trim(); var vTok = pair.Substring(idx + 1).Trim();
                            var kStr = kTok.StartsWith("\"") && kTok.EndsWith("\"") ? kTok.Substring(1, kTok.Length - 2) : kTok;
                            if (!int.TryParse(kStr, out var k)) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            try { idr[k] = new Regex(vTok.Trim('"')); } catch { throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block); }
                        }
                        typedValue = idr; storedType = "int{rex}"; break;
                    default: throw new GXCUnsupportedTypeError(lineNr, baseType + "{" + dictValueType + "}", block);
                }
                break;
            case "dec":
                switch (dictValueType)
                {
                    case "str":
                        var dds = new Dictionary<decimal, string>();
                        foreach (var pair in pairs)
                        {
                            int idx = -1; bool inQ = false;
                            for (int i = 0; i < pair.Length; i++) { if (pair[i] == '"') inQ = !inQ; if (pair[i] == ':' && !inQ) { idx = i; break; } }
                            if (idx < 0) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            var kTok = pair.Substring(0, idx).Trim(); var vTok = pair.Substring(idx + 1).Trim();
                            var kStr = kTok.StartsWith("\"") && kTok.EndsWith("\"") ? kTok.Substring(1, kTok.Length - 2) : kTok;
                            if (!decimal.TryParse(kStr, out var k)) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            if (vTok.StartsWith("\"") && vTok.EndsWith("\"")) vTok = vTok.Substring(1, vTok.Length - 2);
                            dds[k] = vTok;
                        }
                        typedValue = dds; storedType = "dec{str}"; break;
                    case "int":
                        var ddi = new Dictionary<decimal, int>();
                        foreach (var pair in pairs)
                        {
                            int idx = -1; bool inQ = false;
                            for (int i = 0; i < pair.Length; i++) { if (pair[i] == '"') inQ = !inQ; if (pair[i] == ':' && !inQ) { idx = i; break; } }
                            if (idx < 0) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            var kTok = pair.Substring(0, idx).Trim(); var vTok = pair.Substring(idx + 1).Trim();
                            var kStr = kTok.StartsWith("\"") && kTok.EndsWith("\"") ? kTok.Substring(1, kTok.Length - 2) : kTok;
                            if (!decimal.TryParse(kStr, out var k)) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            if (!int.TryParse(vTok.Trim('"'), out var v)) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            ddi[k] = v;
                        }
                        typedValue = ddi; storedType = "dec{int}"; break;
                    case "dec":
                        var ddd = new Dictionary<decimal, decimal>();
                        foreach (var pair in pairs)
                        {
                            int idx = -1; bool inQ = false;
                            for (int i = 0; i < pair.Length; i++) { if (pair[i] == '"') inQ = !inQ; if (pair[i] == ':' && !inQ) { idx = i; break; } }
                            if (idx < 0) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            var kTok = pair.Substring(0, idx).Trim(); var vTok = pair.Substring(idx + 1).Trim();
                            var kStr = kTok.StartsWith("\"") && kTok.EndsWith("\"") ? kTok.Substring(1, kTok.Length - 2) : kTok;
                            if (!decimal.TryParse(kStr, out var k)) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            if (!decimal.TryParse(vTok.Trim('"'), out var v)) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            ddd[k] = v;
                        }
                        typedValue = ddd; storedType = "dec{dec}"; break;
                    case "bool":
                        var ddb = new Dictionary<decimal, bool>();
                        foreach (var pair in pairs)
                        {
                            int idx = -1; bool inQ = false;
                            for (int i = 0; i < pair.Length; i++) { if (pair[i] == '"') inQ = !inQ; if (pair[i] == ':' && !inQ) { idx = i; break; } }
                            if (idx < 0) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            var kTok = pair.Substring(0, idx).Trim(); var vTok = pair.Substring(idx + 1).Trim();
                            var kStr = kTok.StartsWith("\"") && kTok.EndsWith("\"") ? kTok.Substring(1, kTok.Length - 2) : kTok;
                            if (!decimal.TryParse(kStr, out var k)) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            if (!bool.TryParse(vTok.Trim('"'), out var v)) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            ddb[k] = v;
                        }
                        typedValue = ddb; storedType = "dec{bool}"; break;
                    case "rex":
                        var ddr = new Dictionary<decimal, Regex>();
                        foreach (var pair in pairs)
                        {
                            int idx = -1; bool inQ = false;
                            for (int i = 0; i < pair.Length; i++) { if (pair[i] == '"') inQ = !inQ; if (pair[i] == ':' && !inQ) { idx = i; break; } }
                            if (idx < 0) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            var kTok = pair.Substring(0, idx).Trim(); var vTok = pair.Substring(idx + 1).Trim();
                            var kStr = kTok.StartsWith("\"") && kTok.EndsWith("\"") ? kTok.Substring(1, kTok.Length - 2) : kTok;
                            if (!decimal.TryParse(kStr, out var k)) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            try { ddr[k] = new Regex(vTok.Trim('"')); } catch { throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block); }
                        }
                        typedValue = ddr; storedType = "dec{rex}"; break;
                    default: throw new GXCUnsupportedTypeError(lineNr, baseType + "{" + dictValueType + "}", block);
                }
                break;
            case "bool":
                switch (dictValueType)
                {
                    case "str":
                        var bds = new Dictionary<bool, string>();
                        foreach (var pair in pairs)
                        {
                            int idx = -1; bool inQ = false;
                            for (int i = 0; i < pair.Length; i++) { if (pair[i] == '"') inQ = !inQ; if (pair[i] == ':' && !inQ) { idx = i; break; } }
                            if (idx < 0) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            var kTok = pair.Substring(0, idx).Trim(); var vTok = pair.Substring(idx + 1).Trim();
                            var kStr = kTok.StartsWith("\"") && kTok.EndsWith("\"") ? kTok.Substring(1, kTok.Length - 2) : kTok;
                            if (!bool.TryParse(kStr, out var k)) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            if (vTok.StartsWith("\"") && vTok.EndsWith("\"")) vTok = vTok.Substring(1, vTok.Length - 2);
                            bds[k] = vTok;
                        }
                        typedValue = bds; storedType = "bool{str}"; break;
                    case "int":
                        var bdi = new Dictionary<bool, int>();
                        foreach (var pair in pairs)
                        {
                            int idx = -1; bool inQ = false;
                            for (int i = 0; i < pair.Length; i++) { if (pair[i] == '"') inQ = !inQ; if (pair[i] == ':' && !inQ) { idx = i; break; } }
                            if (idx < 0) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            var kTok = pair.Substring(0, idx).Trim(); var vTok = pair.Substring(idx + 1).Trim();
                            var kStr = kTok.StartsWith("\"") && kTok.EndsWith("\"") ? kTok.Substring(1, kTok.Length - 2) : kTok;
                            if (!bool.TryParse(kStr, out var k)) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            if (!int.TryParse(vTok.Trim('"'), out var v)) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            bdi[k] = v;
                        }
                        typedValue = bdi; storedType = "bool{int}"; break;
                    case "dec":
                        var bdd = new Dictionary<bool, decimal>();
                        foreach (var pair in pairs)
                        {
                            int idx = -1; bool inQ = false;
                            for (int i = 0; i < pair.Length; i++) { if (pair[i] == '"') inQ = !inQ; if (pair[i] == ':' && !inQ) { idx = i; break; } }
                            if (idx < 0) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            var kTok = pair.Substring(0, idx).Trim(); var vTok = pair.Substring(idx + 1).Trim();
                            var kStr = kTok.StartsWith("\"") && kTok.EndsWith("\"") ? kTok.Substring(1, kTok.Length - 2) : kTok;
                            if (!bool.TryParse(kStr, out var k)) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            if (!decimal.TryParse(vTok.Trim('"'), out var v)) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            bdd[k] = v;
                        }
                        typedValue = bdd; storedType = "bool{dec}"; break;
                    case "bool":
                        var bdb = new Dictionary<bool, bool>();
                        foreach (var pair in pairs)
                        {
                            int idx = -1; bool inQ = false;
                            for (int i = 0; i < pair.Length; i++) { if (pair[i] == '"') inQ = !inQ; if (pair[i] == ':' && !inQ) { idx = i; break; } }
                            if (idx < 0) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            var kTok = pair.Substring(0, idx).Trim(); var vTok = pair.Substring(idx + 1).Trim();
                            var kStr = kTok.StartsWith("\"") && kTok.EndsWith("\"") ? kTok.Substring(1, kTok.Length - 2) : kTok;
                            if (!bool.TryParse(kStr, out var k)) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            if (!bool.TryParse(vTok.Trim('"'), out var v)) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            bdb[k] = v;
                        }
                        typedValue = bdb; storedType = "bool{bool}"; break;
                    case "rex":
                        var bdr = new Dictionary<bool, Regex>();
                        foreach (var pair in pairs)
                        {
                            int idx = -1; bool inQ = false;
                            for (int i = 0; i < pair.Length; i++) { if (pair[i] == '"') inQ = !inQ; if (pair[i] == ':' && !inQ) { idx = i; break; } }
                            if (idx < 0) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            var kTok = pair.Substring(0, idx).Trim(); var vTok = pair.Substring(idx + 1).Trim();
                            var kStr = kTok.StartsWith("\"") && kTok.EndsWith("\"") ? kTok.Substring(1, kTok.Length - 2) : kTok;
                            if (!bool.TryParse(kStr, out var k)) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            try { bdr[k] = new Regex(vTok.Trim('"')); } catch { throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block); }
                        }
                        typedValue = bdr; storedType = "bool{rex}"; break;
                    default: throw new GXCUnsupportedTypeError(lineNr, baseType + "{" + dictValueType + "}", block);
                }
                break;
            case "rex":
                switch (dictValueType)
                {
                    case "str":
                        var rds = new Dictionary<Regex, string>(new RegexComparer());
                        foreach (var pair in pairs)
                        {
                            int idx = -1; bool inQ = false;
                            for (int i = 0; i < pair.Length; i++) { if (pair[i] == '"') inQ = !inQ; if (pair[i] == ':' && !inQ) { idx = i; break; } }
                            if (idx < 0) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            var kTok = pair.Substring(0, idx).Trim(); var vTok = pair.Substring(idx + 1).Trim();
                            try { var kRx = new Regex(kTok.Trim('"')); rds[kRx] = vTok.StartsWith("\"") && vTok.EndsWith("\"") ? vTok.Substring(1, vTok.Length - 2) : vTok; }
                            catch { throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block); }
                        }
                        typedValue = rds; storedType = "rex{str}"; break;
                    case "int":
                        var rdi = new Dictionary<Regex, int>(new RegexComparer());
                        foreach (var pair in pairs)
                        {
                            int idx = -1; bool inQ = false;
                            for (int i = 0; i < pair.Length; i++) { if (pair[i] == '"') inQ = !inQ; if (pair[i] == ':' && !inQ) { idx = i; break; } }
                            if (idx < 0) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            var kTok = pair.Substring(0, idx).Trim(); var vTok = pair.Substring(idx + 1).Trim();
                            try { var kRx = new Regex(kTok.Trim('"')); if (!int.TryParse(vTok.Trim('"'), out var v)) throw new Exception(); rdi[kRx] = v; }
                            catch { throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block); }
                        }
                        typedValue = rdi; storedType = "rex{int}"; break;
                    case "dec":
                        var rdd = new Dictionary<Regex, decimal>(new RegexComparer());
                        foreach (var pair in pairs)
                        {
                            int idx = -1; bool inQ = false;
                            for (int i = 0; i < pair.Length; i++) { if (pair[i] == '"') inQ = !inQ; if (pair[i] == ':' && !inQ) { idx = i; break; } }
                            if (idx < 0) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            var kTok = pair.Substring(0, idx).Trim(); var vTok = pair.Substring(idx + 1).Trim();
                            try { var kRx = new Regex(kTok.Trim('"')); if (!decimal.TryParse(vTok.Trim('"'), out var v)) throw new Exception(); rdd[kRx] = v; }
                            catch { throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block); }
                        }
                        typedValue = rdd; storedType = "rex{dec}"; break;
                    case "bool":
                        var rdb = new Dictionary<Regex, bool>(new RegexComparer());
                        foreach (var pair in pairs)
                        {
                            int idx = -1; bool inQ = false;
                            for (int i = 0; i < pair.Length; i++) { if (pair[i] == '"') inQ = !inQ; if (pair[i] == ':' && !inQ) { idx = i; break; } }
                            if (idx < 0) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            var kTok = pair.Substring(0, idx).Trim(); var vTok = pair.Substring(idx + 1).Trim();
                            try { var kRx = new Regex(kTok.Trim('"')); if (!bool.TryParse(vTok.Trim('"'), out var v)) throw new Exception(); rdb[kRx] = v; }
                            catch { throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block); }
                        }
                        typedValue = rdb; storedType = "rex{bool}"; break;
                    case "rex":
                        var rdr = new Dictionary<Regex, Regex>(new RegexComparer());
                        foreach (var pair in pairs)
                        {
                            int idx = -1; bool inQ = false;
                            for (int i = 0; i < pair.Length; i++) { if (pair[i] == '"') inQ = !inQ; if (pair[i] == ':' && !inQ) { idx = i; break; } }
                            if (idx < 0) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            var kTok = pair.Substring(0, idx).Trim(); var vTok = pair.Substring(idx + 1).Trim();
                            try { var kRx = new Regex(kTok.Trim('"')); var vRx = new Regex(vTok.Trim('"')); rdr[kRx] = vRx; }
                            catch { throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block); }
                        }
                        typedValue = rdr; storedType = "rex{rex}"; break;
                    default: throw new GXCUnsupportedTypeError(lineNr, baseType + "{" + dictValueType + "}", block);
                }
                break;
            default:
                throw new GXCUnsupportedTypeError(lineNr, baseType + "{" + dictValueType + "}", block);
        }

        GXCodeProgram.scopeStack.Peek().Set(name, typedValue, storedType, isConst: false);
    }

    public static void DeclareConstDict(string line, int lineNr, string block)
    {
        string pattern = @"^\s*const\s+(str|int|dec|bool|rex)\{([a-z;]+)\}\s*([a-zA-Z0-9]+)\s*=\s*(.*);$";
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

        // Mirror the non-const variant but mark as const when storing
        switch (baseType)
        {
            case "str":
                switch (dictValueType)
                {
                    case "str":
                        var sdss = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        foreach (var pair in pairs)
                        {
                            int idx = -1; bool inQ = false;
                            for (int i = 0; i < pair.Length; i++) { if (pair[i] == '"') inQ = !inQ; if (pair[i] == ':' && !inQ) { idx = i; break; } }
                            if (idx < 0) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            var k = pair.Substring(0, idx).Trim(); var v = pair.Substring(idx + 1).Trim();
                            if (k.StartsWith("\"") && k.EndsWith("\"")) k = k.Substring(1, k.Length - 2);
                            if (v.StartsWith("\"") && v.EndsWith("\"")) v = v.Substring(1, v.Length - 2);
                            sdss[k] = v;
                        }
                        typedValue = sdss; storedType = "str{str}"; break;
                    case "int":
                        var sdi = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
                        foreach (var pair in pairs)
                        {
                            int idx = -1; bool inQ = false;
                            for (int i = 0; i < pair.Length; i++) { if (pair[i] == '"') inQ = !inQ; if (pair[i] == ':' && !inQ) { idx = i; break; } }
                            if (idx < 0) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            var k = pair.Substring(0, idx).Trim(); var v = pair.Substring(idx + 1).Trim();
                            if (k.StartsWith("\"") && k.EndsWith("\"")) k = k.Substring(1, k.Length - 2);
                            if (int.TryParse(v.Trim('"'), out var iv)) sdi[k] = iv; else throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                        }
                        typedValue = sdi; storedType = "str{int}"; break;
                    case "dec":
                        var sdd = new Dictionary<string, decimal>(StringComparer.OrdinalIgnoreCase);
                        foreach (var pair in pairs)
                        {
                            int idx = -1; bool inQ = false;
                            for (int i = 0; i < pair.Length; i++) { if (pair[i] == '"') inQ = !inQ; if (pair[i] == ':' && !inQ) { idx = i; break; } }
                            if (idx < 0) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            var k = pair.Substring(0, idx).Trim(); var v = pair.Substring(idx + 1).Trim();
                            if (k.StartsWith("\"") && k.EndsWith("\"")) k = k.Substring(1, k.Length - 2);
                            if (decimal.TryParse(v.Trim('"'), out var dv)) sdd[k] = dv; else throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                        }
                        typedValue = sdd; storedType = "str{dec}"; break;
                    case "bool":
                        var sdb = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
                        foreach (var pair in pairs)
                        {
                            int idx = -1; bool inQ = false;
                            for (int i = 0; i < pair.Length; i++) { if (pair[i] == '"') inQ = !inQ; if (pair[i] == ':' && !inQ) { idx = i; break; } }
                            if (idx < 0) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            var k = pair.Substring(0, idx).Trim(); var v = pair.Substring(idx + 1).Trim();
                            if (k.StartsWith("\"") && k.EndsWith("\"")) k = k.Substring(1, k.Length - 2);
                            if (bool.TryParse(v.Trim('"'), out var bv)) sdb[k] = bv; else throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                        }
                        typedValue = sdb; storedType = "str{bool}"; break;
                    case "rex":
                        var sdr = new Dictionary<string, Regex>(StringComparer.OrdinalIgnoreCase);
                        foreach (var pair in pairs)
                        {
                            int idx = -1; bool inQ = false;
                            for (int i = 0; i < pair.Length; i++) { if (pair[i] == '"') inQ = !inQ; if (pair[i] == ':' && !inQ) { idx = i; break; } }
                            if (idx < 0) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            var k = pair.Substring(0, idx).Trim(); var v = pair.Substring(idx + 1).Trim();
                            if (k.StartsWith("\"") && k.EndsWith("\"")) k = k.Substring(1, k.Length - 2);
                            try { sdr[k] = new Regex(v.Trim('"')); } catch { throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block); }
                        }
                        typedValue = sdr; storedType = "str{rex}"; break;
                    default: throw new GXCUnsupportedTypeError(lineNr, baseType + "{" + dictValueType + "}", block);
                }
                break;
            case "int":
                switch (dictValueType)
                {
                    case "str":
                        var ids = new Dictionary<int, string>();
                        foreach (var pair in pairs)
                        {
                            int idx = -1; bool inQ = false;
                            for (int i = 0; i < pair.Length; i++) { if (pair[i] == '"') inQ = !inQ; if (pair[i] == ':' && !inQ) { idx = i; break; } }
                            if (idx < 0) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            var kTok = pair.Substring(0, idx).Trim(); var vTok = pair.Substring(idx + 1).Trim();
                            var kStr = kTok.StartsWith("\"") && kTok.EndsWith("\"") ? kTok.Substring(1, kTok.Length - 2) : kTok;
                            if (!int.TryParse(kStr, out var k)) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            if (vTok.StartsWith("\"") && vTok.EndsWith("\"")) vTok = vTok.Substring(1, vTok.Length - 2);
                            ids[k] = vTok;
                        }
                        typedValue = ids; storedType = "int{str}"; break;
                    case "int":
                        var idi = new Dictionary<int, int>();
                        foreach (var pair in pairs)
                        {
                            int idx = -1; bool inQ = false;
                            for (int i = 0; i < pair.Length; i++) { if (pair[i] == '"') inQ = !inQ; if (pair[i] == ':' && !inQ) { idx = i; break; } }
                            if (idx < 0) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            var kTok = pair.Substring(0, idx).Trim(); var vTok = pair.Substring(idx + 1).Trim();
                            var kStr = kTok.StartsWith("\"") && kTok.EndsWith("\"") ? kTok.Substring(1, kTok.Length - 2) : kTok;
                            if (!int.TryParse(kStr, out var k)) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            if (!int.TryParse(vTok.Trim('"'), out var v)) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            idi[k] = v;
                        }
                        typedValue = idi; storedType = "int{int}"; break;
                    case "dec":
                        var idd = new Dictionary<int, decimal>();
                        foreach (var pair in pairs)
                        {
                            int idx = -1; bool inQ = false;
                            for (int i = 0; i < pair.Length; i++) { if (pair[i] == '"') inQ = !inQ; if (pair[i] == ':' && !inQ) { idx = i; break; } }
                            if (idx < 0) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            var kTok = pair.Substring(0, idx).Trim(); var vTok = pair.Substring(idx + 1).Trim();
                            var kStr = kTok.StartsWith("\"") && kTok.EndsWith("\"") ? kTok.Substring(1, kTok.Length - 2) : kTok;
                            if (!int.TryParse(kStr, out var k)) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            if (!decimal.TryParse(vTok.Trim('"'), out var v)) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            idd[k] = v;
                        }
                        typedValue = idd; storedType = "int{dec}"; break;
                    case "bool":
                        var idb = new Dictionary<int, bool>();
                        foreach (var pair in pairs)
                        {
                            int idx = -1; bool inQ = false;
                            for (int i = 0; i < pair.Length; i++) { if (pair[i] == '"') inQ = !inQ; if (pair[i] == ':' && !inQ) { idx = i; break; } }
                            if (idx < 0) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            var kTok = pair.Substring(0, idx).Trim(); var vTok = pair.Substring(idx + 1).Trim();
                            var kStr = kTok.StartsWith("\"") && kTok.EndsWith("\"") ? kTok.Substring(1, kTok.Length - 2) : kTok;
                            if (!int.TryParse(kStr, out var k)) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            if (!bool.TryParse(vTok.Trim('"'), out var v)) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            idb[k] = v;
                        }
                        typedValue = idb; storedType = "int{bool}"; break;
                    case "rex":
                        var idr = new Dictionary<int, Regex>();
                        foreach (var pair in pairs)
                        {
                            int idx = -1; bool inQ = false;
                            for (int i = 0; i < pair.Length; i++) { if (pair[i] == '"') inQ = !inQ; if (pair[i] == ':' && !inQ) { idx = i; break; } }
                            if (idx < 0) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            var kTok = pair.Substring(0, idx).Trim(); var vTok = pair.Substring(idx + 1).Trim();
                            var kStr = kTok.StartsWith("\"") && kTok.EndsWith("\"") ? kTok.Substring(1, kTok.Length - 2) : kTok;
                            if (!int.TryParse(kStr, out var k)) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            try { idr[k] = new Regex(vTok.Trim('"')); } catch { throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block); }
                        }
                        typedValue = idr; storedType = "int{rex}"; break;
                    default: throw new GXCUnsupportedTypeError(lineNr, baseType + "{" + dictValueType + "}", block);
                }
                break;
            case "dec":
                switch (dictValueType)
                {
                    case "str":
                        var dds = new Dictionary<decimal, string>();
                        foreach (var pair in pairs)
                        {
                            int idx = -1; bool inQ = false;
                            for (int i = 0; i < pair.Length; i++) { if (pair[i] == '"') inQ = !inQ; if (pair[i] == ':' && !inQ) { idx = i; break; } }
                            if (idx < 0) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            var kTok = pair.Substring(0, idx).Trim(); var vTok = pair.Substring(idx + 1).Trim();
                            var kStr = kTok.StartsWith("\"") && kTok.EndsWith("\"") ? kTok.Substring(1, kTok.Length - 2) : kTok;
                            if (!decimal.TryParse(kStr, out var k)) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            if (vTok.StartsWith("\"") && vTok.EndsWith("\"")) vTok = vTok.Substring(1, vTok.Length - 2);
                            dds[k] = vTok;
                        }
                        typedValue = dds; storedType = "dec{str}"; break;
                    case "int":
                        var ddi = new Dictionary<decimal, int>();
                        foreach (var pair in pairs)
                        {
                            int idx = -1; bool inQ = false;
                            for (int i = 0; i < pair.Length; i++) { if (pair[i] == '"') inQ = !inQ; if (pair[i] == ':' && !inQ) { idx = i; break; } }
                            if (idx < 0) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            var kTok = pair.Substring(0, idx).Trim(); var vTok = pair.Substring(idx + 1).Trim();
                            var kStr = kTok.StartsWith("\"") && kTok.EndsWith("\"") ? kTok.Substring(1, kTok.Length - 2) : kTok;
                            if (!decimal.TryParse(kStr, out var k)) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            if (!int.TryParse(vTok.Trim('"'), out var v)) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            ddi[k] = v;
                        }
                        typedValue = ddi; storedType = "dec{int}"; break;
                    case "dec":
                        var ddd = new Dictionary<decimal, decimal>();
                        foreach (var pair in pairs)
                        {
                            int idx = -1; bool inQ = false;
                            for (int i = 0; i < pair.Length; i++) { if (pair[i] == '"') inQ = !inQ; if (pair[i] == ':' && !inQ) { idx = i; break; } }
                            if (idx < 0) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            var kTok = pair.Substring(0, idx).Trim(); var vTok = pair.Substring(idx + 1).Trim();
                            var kStr = kTok.StartsWith("\"") && kTok.EndsWith("\"") ? kTok.Substring(1, kTok.Length - 2) : kTok;
                            if (!decimal.TryParse(kStr, out var k)) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            if (!decimal.TryParse(vTok.Trim('"'), out var v)) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            ddd[k] = v;
                        }
                        typedValue = ddd; storedType = "dec{dec}"; break;
                    case "bool":
                        var ddb = new Dictionary<decimal, bool>();
                        foreach (var pair in pairs)
                        {
                            int idx = -1; bool inQ = false;
                            for (int i = 0; i < pair.Length; i++) { if (pair[i] == '"') inQ = !inQ; if (pair[i] == ':' && !inQ) { idx = i; break; } }
                            if (idx < 0) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            var kTok = pair.Substring(0, idx).Trim(); var vTok = pair.Substring(idx + 1).Trim();
                            var kStr = kTok.StartsWith("\"") && kTok.EndsWith("\"") ? kTok.Substring(1, kTok.Length - 2) : kTok;
                            if (!decimal.TryParse(kStr, out var k)) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            if (!bool.TryParse(vTok.Trim('"'), out var v)) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            ddb[k] = v;
                        }
                        typedValue = ddb; storedType = "dec{bool}"; break;
                    case "rex":
                        var ddr = new Dictionary<decimal, Regex>();
                        foreach (var pair in pairs)
                        {
                            int idx = -1; bool inQ = false;
                            for (int i = 0; i < pair.Length; i++) { if (pair[i] == '"') inQ = !inQ; if (pair[i] == ':' && !inQ) { idx = i; break; } }
                            if (idx < 0) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            var kTok = pair.Substring(0, idx).Trim(); var vTok = pair.Substring(idx + 1).Trim();
                            var kStr = kTok.StartsWith("\"") && kTok.EndsWith("\"") ? kTok.Substring(1, kTok.Length - 2) : kTok;
                            if (!decimal.TryParse(kStr, out var k)) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            try { ddr[k] = new Regex(vTok.Trim('"')); } catch { throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block); }
                        }
                        typedValue = ddr; storedType = "dec{rex}"; break;
                    default: throw new GXCUnsupportedTypeError(lineNr, baseType + "{" + dictValueType + "}", block);
                }
                break;
            case "bool":
                switch (dictValueType)
                {
                    case "str":
                        var bds = new Dictionary<bool, string>();
                        foreach (var pair in pairs)
                        {
                            int idx = -1; bool inQ = false;
                            for (int i = 0; i < pair.Length; i++) { if (pair[i] == '"') inQ = !inQ; if (pair[i] == ':' && !inQ) { idx = i; break; } }
                            if (idx < 0) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            var kTok = pair.Substring(0, idx).Trim(); var vTok = pair.Substring(idx + 1).Trim();
                            var kStr = kTok.StartsWith("\"") && kTok.EndsWith("\"") ? kTok.Substring(1, kTok.Length - 2) : kTok;
                            if (!bool.TryParse(kStr, out var k)) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            if (vTok.StartsWith("\"") && vTok.EndsWith("\"")) vTok = vTok.Substring(1, vTok.Length - 2);
                            bds[k] = vTok;
                        }
                        typedValue = bds; storedType = "bool{str}"; break;
                    case "int":
                        var bdi = new Dictionary<bool, int>();
                        foreach (var pair in pairs)
                        {
                            int idx = -1; bool inQ = false;
                            for (int i = 0; i < pair.Length; i++) { if (pair[i] == '"') inQ = !inQ; if (pair[i] == ':' && !inQ) { idx = i; break; } }
                            if (idx < 0) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            var kTok = pair.Substring(0, idx).Trim(); var vTok = pair.Substring(idx + 1).Trim();
                            var kStr = kTok.StartsWith("\"") && kTok.EndsWith("\"") ? kTok.Substring(1, kTok.Length - 2) : kTok;
                            if (!bool.TryParse(kStr, out var k)) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            if (!int.TryParse(vTok.Trim('"'), out var v)) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            bdi[k] = v;
                        }
                        typedValue = bdi; storedType = "bool{int}"; break;
                    case "dec":
                        var bdd = new Dictionary<bool, decimal>();
                        foreach (var pair in pairs)
                        {
                            int idx = -1; bool inQ = false;
                            for (int i = 0; i < pair.Length; i++) { if (pair[i] == '"') inQ = !inQ; if (pair[i] == ':' && !inQ) { idx = i; break; } }
                            if (idx < 0) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            var kTok = pair.Substring(0, idx).Trim(); var vTok = pair.Substring(idx + 1).Trim();
                            var kStr = kTok.StartsWith("\"") && kTok.EndsWith("\"") ? kTok.Substring(1, kTok.Length - 2) : kTok;
                            if (!bool.TryParse(kStr, out var k)) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            if (!decimal.TryParse(vTok.Trim('"'), out var v)) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            bdd[k] = v;
                        }
                        typedValue = bdd; storedType = "bool{dec}"; break;
                    case "bool":
                        var bdb = new Dictionary<bool, bool>();
                        foreach (var pair in pairs)
                        {
                            int idx = -1; bool inQ = false;
                            for (int i = 0; i < pair.Length; i++) { if (pair[i] == '"') inQ = !inQ; if (pair[i] == ':' && !inQ) { idx = i; break; } }
                            if (idx < 0) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            var kTok = pair.Substring(0, idx).Trim(); var vTok = pair.Substring(idx + 1).Trim();
                            var kStr = kTok.StartsWith("\"") && kTok.EndsWith("\"") ? kTok.Substring(1, kTok.Length - 2) : kTok;
                            if (!bool.TryParse(kStr, out var k)) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            if (!bool.TryParse(vTok.Trim('"'), out var v)) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            bdb[k] = v;
                        }
                        typedValue = bdb; storedType = "bool{bool}"; break;
                    case "rex":
                        var bdr = new Dictionary<bool, Regex>();
                        foreach (var pair in pairs)
                        {
                            int idx = -1; bool inQ = false;
                            for (int i = 0; i < pair.Length; i++) { if (pair[i] == '"') inQ = !inQ; if (pair[i] == ':' && !inQ) { idx = i; break; } }
                            if (idx < 0) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            var kTok = pair.Substring(0, idx).Trim(); var vTok = pair.Substring(idx + 1).Trim();
                            var kStr = kTok.StartsWith("\"") && kTok.EndsWith("\"") ? kTok.Substring(1, kTok.Length - 2) : kTok;
                            if (!bool.TryParse(kStr, out var k)) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            try { bdr[k] = new Regex(vTok.Trim('"')); } catch { throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block); }
                        }
                        typedValue = bdr; storedType = "bool{rex}"; break;
                    default: throw new GXCUnsupportedTypeError(lineNr, baseType + "{" + dictValueType + "}", block);
                }
                break;
            case "rex":
                switch (dictValueType)
                {
                    case "str":
                        var rds = new Dictionary<Regex, string>(new RegexComparer());
                        foreach (var pair in pairs)
                        {
                            int idx = -1; bool inQ = false;
                            for (int i = 0; i < pair.Length; i++) { if (pair[i] == '"') inQ = !inQ; if (pair[i] == ':' && !inQ) { idx = i; break; } }
                            if (idx < 0) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            var kTok = pair.Substring(0, idx).Trim(); var vTok = pair.Substring(idx + 1).Trim();
                            try { var kRx = new Regex(kTok.Trim('"')); rds[kRx] = vTok.StartsWith("\"") && vTok.EndsWith("\"") ? vTok.Substring(1, vTok.Length - 2) : vTok; }
                            catch { throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block); }
                        }
                        typedValue = rds; storedType = "rex{str}"; break;
                    case "int":
                        var rdi = new Dictionary<Regex, int>(new RegexComparer());
                        foreach (var pair in pairs)
                        {
                            int idx = -1; bool inQ = false;
                            for (int i = 0; i < pair.Length; i++) { if (pair[i] == '"') inQ = !inQ; if (pair[i] == ':' && !inQ) { idx = i; break; } }
                            if (idx < 0) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            var kTok = pair.Substring(0, idx).Trim(); var vTok = pair.Substring(idx + 1).Trim();
                            try { var kRx = new Regex(kTok.Trim('"')); if (!int.TryParse(vTok.Trim('"'), out var v)) throw new Exception(); rdi[kRx] = v; }
                            catch { throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block); }
                        }
                        typedValue = rdi; storedType = "rex{int}"; break;
                    case "dec":
                        var rdd = new Dictionary<Regex, decimal>(new RegexComparer());
                        foreach (var pair in pairs)
                        {
                            int idx = -1; bool inQ = false;
                            for (int i = 0; i < pair.Length; i++) { if (pair[i] == '"') inQ = !inQ; if (pair[i] == ':' && !inQ) { idx = i; break; } }
                            if (idx < 0) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            var kTok = pair.Substring(0, idx).Trim(); var vTok = pair.Substring(idx + 1).Trim();
                            try { var kRx = new Regex(kTok.Trim('"')); if (!decimal.TryParse(vTok.Trim('"'), out var v)) throw new Exception(); rdd[kRx] = v; }
                            catch { throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block); }
                        }
                        typedValue = rdd; storedType = "rex{dec}"; break;
                    case "bool":
                        var rdb = new Dictionary<Regex, bool>(new RegexComparer());
                        foreach (var pair in pairs)
                        {
                            int idx = -1; bool inQ = false;
                            for (int i = 0; i < pair.Length; i++) { if (pair[i] == '"') inQ = !inQ; if (pair[i] == ':' && !inQ) { idx = i; break; } }
                            if (idx < 0) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            var kTok = pair.Substring(0, idx).Trim(); var vTok = pair.Substring(idx + 1).Trim();
                            try { var kRx = new Regex(kTok.Trim('"')); if (!bool.TryParse(vTok.Trim('"'), out var v)) throw new Exception(); rdb[kRx] = v; }
                            catch { throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block); }
                        }
                        typedValue = rdb; storedType = "rex{bool}"; break;
                    case "rex":
                        var rdr = new Dictionary<Regex, Regex>(new RegexComparer());
                        foreach (var pair in pairs)
                        {
                            int idx = -1; bool inQ = false;
                            for (int i = 0; i < pair.Length; i++) { if (pair[i] == '"') inQ = !inQ; if (pair[i] == ':' && !inQ) { idx = i; break; } }
                            if (idx < 0) throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block);
                            var kTok = pair.Substring(0, idx).Trim(); var vTok = pair.Substring(idx + 1).Trim();
                            try { var kRx = new Regex(kTok.Trim('"')); var vRx = new Regex(vTok.Trim('"')); rdr[kRx] = vRx; }
                            catch { throw new GXCWrongTypeError(lineNr, value, baseType + "{" + dictValueType + "}", block); }
                        }
                        typedValue = rdr; storedType = "rex{rex}"; break;
                    default: throw new GXCUnsupportedTypeError(lineNr, baseType + "{" + dictValueType + "}", block);
                }
                break;
            default:
                throw new GXCUnsupportedTypeError(lineNr, baseType + "{" + dictValueType + "}", block);
        }

        GXCodeProgram.scopeStack.Peek().Set(name, typedValue, storedType, isConst: true);
    }
}

// Simple comparer for Regex to use as dictionary keys (compares pattern + options)
internal class RegexComparer : IEqualityComparer<Regex>
{
    public bool Equals(Regex? x, Regex? y)
    {
        if (ReferenceEquals(x, y)) return true;
        if (x is null || y is null) return false;
        return x.ToString() == y.ToString() && x.Options == y.Options;
    }

    public int GetHashCode(Regex obj)
    {
        return HashCode.Combine(obj.ToString(), obj.Options);
    }
}