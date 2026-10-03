using System.Text.RegularExpressions;

namespace GXCodeInterpreter;

partial class GXCodeInterpreter
{
    public static void AssignVariable(string line, int lineNr, string block)
    {
        string pattern = @"^\s*([a-zA-Z0-9]+)\s*=\s*(.*);$";
        Match match = Regex.Match(line, pattern);

        if (!match.Success)
        {
            throw new GXCodeInterpreterError("Could not detect variable assignment");
        }

        string name = match.Groups[1].Value;
        string value = match.Groups[2].Value.Trim();

        Variable? variable = GXCodeEnvironment.GetVariable(name)
            ?? throw new GXCUndeclaredVariableError(lineNr, name, block);

        var type = variable.Type;

        if (GXCodeProgram.scopeStack.Peek().Variables[name].IsConstant)
        {
            throw new GXCConstantAssignmentError(lineNr, name, block);
        }

        if (type is null)
        {
            throw new GXCUndeclaredVariableError(lineNr, name, block);
        }

        object typedValue;

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

        // handle arrays
        if (type.EndsWith("[]", StringComparison.Ordinal))
        {
            string baseType = type.Substring(0, type.Length - 2);

            Variable? arr = GXCodeEnvironment.GetVariable(value);

            // assign from another variable of same type
            if (arr is not null && arr.Type == type)
            {
                if (arr.Value is not List<string> &&
                    arr.Value is not List<int> &&
                    arr.Value is not List<decimal> &&
                    arr.Value is not List<bool> &&
                    arr.Value is not List<Regex>
                )
                {
                    throw new GXCWrongTypeError(lineNr, value, type, block);
                }
                typedValue = arr.Value;
            }
            else
            {
                // expect literal array
                if (!value.StartsWith("[") || !value.EndsWith("]"))
                    throw new GXCWrongTypeError(lineNr, value, type, block);

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
                        break;
                    case "int":
                        var iList = new List<int>();
                        foreach (var it in items)
                        {
                            if (int.TryParse(it.Trim(), out var iv)) iList.Add(iv);
                            else throw new GXCWrongTypeError(lineNr, value, type, block);
                        }
                        typedValue = iList;
                        break;
                    case "dec":
                        var dList = new List<decimal>();
                        foreach (var it in items)
                        {
                            if (decimal.TryParse(it.Trim(), out var dv)) dList.Add(dv);
                            else throw new GXCWrongTypeError(lineNr, value, type, block);
                        }
                        typedValue = dList;
                        break;
                    case "bool":
                        var bList = new List<bool>();
                        foreach (var it in items)
                        {
                            if (bool.TryParse(it.Trim(), out var bv)) bList.Add(bv);
                            else throw new GXCWrongTypeError(lineNr, value, type, block);
                        }
                        typedValue = bList;
                        break;
                    case "rex":
                        var rList = new List<Regex>();
                        foreach (var it in items)
                        {
                            try { rList.Add(new Regex(it.Trim())); }
                            catch { throw new GXCWrongTypeError(lineNr, value, type, block); }
                        }
                        typedValue = rList;
                        break;
                    default:
                        throw new GXCUnsupportedTypeError(lineNr, type, block);
                }
            }
        }
        // handle dictionaries like str{str}
        else if (type.Contains('{') && type.Contains('}'))
        {
            // exact stored format: keyType{valType}
            int braceOpen = type.IndexOf('{');
            string keyType = type.Substring(0, braceOpen);
            string valType = type.Substring(braceOpen + 1, type.Length - braceOpen - 2);

            Variable? dict = GXCodeEnvironment.GetVariable(value);

            if (dict is not null && dict.Type == type)
            {
                if (dict.Value is not Dictionary<string, string>)
                {
                    throw new GXCWrongTypeError(lineNr, value, type, block);
                }
                typedValue = dict.Value;
            }
            else
            {
                if (!value.StartsWith("{") || !value.EndsWith("}"))
                    throw new GXCWrongTypeError(lineNr, value, type, block);

                string inner = value.Substring(1, value.Length - 2);
                var pairs = SplitTopLevelItems(inner);

                if (keyType == "str" && valType == "str")
                {
                    var sd = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    foreach (var pair in pairs)
                    {
                        int idx = -1; bool inQ = false;
                        for (int i = 0; i < pair.Length; i++) { if (pair[i] == '"') inQ = !inQ; if (pair[i] == ':' && !inQ) { idx = i; break; } }
                        if (idx < 0) throw new GXCWrongTypeError(lineNr, value, type, block);
                        var k = pair.Substring(0, idx).Trim(); var v = pair.Substring(idx + 1).Trim();
                        if (k.StartsWith("\"") && k.EndsWith("\"")) k = k.Substring(1, k.Length - 2);
                        if (v.StartsWith("\"") && v.EndsWith("\"")) v = v.Substring(1, v.Length - 2);
                        sd[k] = v;
                    }
                    typedValue = sd;
                }
                else
                {
                    throw new GXCUnsupportedTypeError(lineNr, type, block);
                }
            }
        }
        else
        {
            // scalar types
            switch (type)
            {
                case "str":
                    Variable? str = GXCodeEnvironment.GetVariable(value);

                    if (value.StartsWith('"') && value.EndsWith('"'))
                    {
                        typedValue = value.Trim('"');
                    }
                    else if (str is not null && str.Type == "str")
                    {
                        if (str.Value is not string)
                        {
                            throw new GXCWrongTypeError(lineNr, value, "str", block);
                        }
                        typedValue = str.Value;
                    }
                    else
                    {
                        throw new GXCWrongTypeError(lineNr, value, "str", block);
                    }
                    break;
                case "int":
                    Variable? integer = GXCodeEnvironment.GetVariable(value);

                    if (int.TryParse(value, out int intValue))
                    {
                        typedValue = intValue;
                    }
                    else if (CalculateIntArithmetic(value) != null)
                    {
                        int val = CalculateIntArithmetic(value) ?? throw new GXCodeInterpreterError("CalculateIntArithmetic can not be null but is null");
                        typedValue = val;
                    }
                    else if (integer is not null && integer.Type == "int")
                    {
                        if (integer.Value is not int)
                        {
                            throw new GXCWrongTypeError(lineNr, value, "int", block);
                        }
                        typedValue = integer.Value;
                    }
                    else
                    {
                        throw new GXCWrongTypeError(lineNr, value, "int", block);
                    }
                    break;
                case "dec":
                    Variable? dec = GXCodeEnvironment.GetVariable(value);

                    if (decimal.TryParse(value, out decimal decValue))
                    {
                        typedValue = decValue;
                    }
                    else if (CalculateDecArithmetic(value) != null)
                    {
                        decimal val = CalculateDecArithmetic(value) ?? throw new GXCodeInterpreterError("CalculateIntArithmetic can not be null but is null");
                        typedValue = val;
                    }
                    else if (dec is not null && dec.Type == "dec")
                    {
                        if (dec.Value is not decimal)
                        {
                            throw new GXCWrongTypeError(lineNr, value, "dec", block);
                        }
                        typedValue = dec.Value;
                    }
                    else
                    {
                        throw new GXCWrongTypeError(lineNr, value, "dec", block);
                    }
                    break;
                case "bool":
                    Variable? boolean = GXCodeEnvironment.GetVariable(value);

                    if (bool.TryParse(value, out bool boolValue))
                    {
                        typedValue = boolValue;
                    }
                    else if (boolean is not null && boolean.Type == "bool")
                    {
                        if (boolean.Value is not bool)
                        {
                            throw new GXCWrongTypeError(lineNr, value, "bool", block);
                        }
                        typedValue = boolean.Value;
                    }
                    else
                    {
                        throw new GXCWrongTypeError(lineNr, value, "bool", block);
                    }
                    break;
                case "rex":
                    Variable? rex = GXCodeEnvironment.GetVariable(value);
                    
                    try
                    {
                        typedValue = new Regex(value);
                    }
                    catch
                    {
                        if (rex is not null && rex.Type == "rex")
                        {
                            if (rex.Value is not Regex)
                            {
                                throw new GXCWrongTypeError(lineNr, value, "rex", block);
                            }
                            typedValue = rex.Value;
                        }
                        else
                        {
                            throw new GXCWrongTypeError(lineNr, value, "rex", block);
                        }
                    }
                    break;
                default:
                    throw new GXCUnsupportedTypeError(lineNr, type, block);
            }
        }

        // set the variable (preserve the declared type)
        GXCodeProgram.scopeStack.Peek().Set(name, typedValue, type);
    }
}