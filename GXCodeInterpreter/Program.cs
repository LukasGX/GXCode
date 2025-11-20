using System;
using System.Numerics;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using System.Linq;

namespace GXCodeInterpreter
{
    class Program
    {
        public static void Main(string[] args)
        {
            if (args.Contains("--no-debug"))
            {
                Helper.Debugging = false;
            }

            Environment env = new();
            string content = File.ReadAllText("D:\\Data\\Coding\\GXCodeInterpreter\\GXCodeInterpreter\\program.gxc");
            Interpreter interpreter = new(content, env);

            try
            {
                // Comments
                interpreter.StripComments();
                // Namespace
                interpreter.DetectNamespace();
                // Entrypoint
                Entrypoint entrypoint = interpreter.DetectEntryPoint();
                foreach (string line in entrypoint.Lines)
                {
                    interpreter.Execute(line.Trim());
                }

                // no errors
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Program ran with no errors");
                Console.ResetColor();
            }
            catch (GXCodeError e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error {e.Id}: {e.Message}");
                Console.ResetColor();
            }
            catch (GXCodeInterpreterError e)
            {
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine($"Interpreter Error: {e.Message}");
                Console.ResetColor();
            }
            catch (GXCodeBreak)
            {
                // no errors
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Program was ended intentionally");
                Console.ResetColor();
            }
        }
    }

    public class Environment
    {
        //                      type,  name,  value
        public TripleDictionary<Type, string, object> variables { get; set; } = new();
        public TripleDictionary<List<Type>, List<string>, List<object>> dictionaries;
        public string? Namespace { get; set; }
        public List<CallstackElement> callstack { get; set; } = [];
    }

    public static class Helper
    {
        public static bool Debugging = true;

        public static List<List<string>> RegEx(string text, string pattern, bool singleLine = false)
        {
            Regex rg;
            if (singleLine) { rg = new(pattern, RegexOptions.Singleline); }
            else { rg = new(pattern, RegexOptions.Multiline); }
            MatchCollection matched = rg.Matches(text);
            var result = new List<List<string>>();
            foreach (Match m in matched)
            {
                var groups = new List<string>();
                for (int i = 1; i < m.Groups.Count; i++)
                    groups.Add(m.Groups[i].Value);
                result.Add(groups);
            }
            return result;
        }

        public static void Debug(string msg)
        {
            if (!Debugging) return;

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("[Debug] ");
            Console.ResetColor();
            Console.WriteLine(msg);
        }
    }

    public class Lists
    {
        public static Dictionary<string, Type> PrimitiveTypes = new()
        {
            {"str", typeof(string)},
            {"int", typeof(short)},
            {"dec", typeof(double)},
            {"bool", typeof(bool)},
            {"rex", typeof(string)},
            {"<T1>[]", typeof(List<>)},
            {"<T1>{<T2>}", typeof(Dictionary<,>)}
        };

        public static List<string> Keywords = new()
        {
            "out", "return", "exit"
        };
    }

    public class Interpreter
    {
        public string Code;
        public Environment Env;

        public Interpreter(string code, Environment env)
        {
            Code = code;
            Env = env;
        }

        public void StripComments()
        {
            string pattern = @"// .*?$";
            string newCode = Regex.Replace(Code, pattern, "", RegexOptions.Multiline);

            Code = newCode;
        }

        public void DetectNamespace()
        {
            string pattern = @"#ns ([a-zA-Z0-9_]+)";
            List<List<string>> matches = Helper.RegEx(Code, pattern);
            if (matches.Count == 1)
            {
                string ns = matches[0][0];
                Env.Namespace = ns;
                Helper.Debug($"Namespace: {ns}");
            }
            else if (matches.Count < 1)
            {
                throw new GXCodeError("GX0006", "No namespace definition found!");
            }
            else
            {
                throw new GXCodeError("GX0007", "Too many namespace definitions!");
            }
        }

        public Entrypoint DetectEntryPoint()
        {
            string pattern = @"(?s)entrypoint\((.*?)\)\s*\{(.*)\}";
            MatchCollection matches = Regex.Matches(Code, pattern);

            if (matches.Count > 1)
            {
                throw new GXCodeError("GX0001", "Too many entrypoints!");
            }

            if (matches.Count == 1)
            {
                // string arguments = matches[0].Groups[1].Value;
                string instructions = matches[0].Groups[2].Value;

                List<string> lines = instructions.Split("\r\n").ToList();
                lines.RemoveAll(s => string.IsNullOrWhiteSpace(s));

                Entrypoint entrypoint = new(lines);
                return entrypoint;
            }
            else
            {
                throw new GXCodeError("GX0002", "No entrypoint found!");
            }
        }

        public void Execute(string line, bool skipCallstackCheck = false)
        {
            if (line != "")
            {
                char lastChar = line[line.Length - 1];
                if (lastChar != ';' && lastChar != '{' && lastChar != '}')
                    throw new GXCodeError("GX0004", $"Unexpected {lastChar}, Expected ; or {{ or }}");
            }
            else return;

            // check for line pattern
            if (!skipCallstackCheck)
            {
                // closing: only treat callstack elements that are currently open (not Closed)
                CallstackElement? lcs = Env.callstack.LastOrDefault();
                if (lcs is CS_If csIf && !csIf.Closed)
                {
                    csIf.codelines.Add(line);

                    List<List<string>> foundClosing = Helper.RegEx(line, @"^}");
                    if (foundClosing.Count == 1)
                    {
                        if (csIf.condition == true)
                        {
                            foreach (string codeline in csIf.codelines)
                            {
                                Execute(codeline, true);
                            }

                            Env.callstack.Remove(lcs);
                        }
                        else
                        {
                            csIf.Closed = true;
                        }
                    }

                    return;
                }
                else if (lcs is CS_Else_If csEIf && !csEIf.Closed)
                {
                    csEIf.codelines.Add(line);

                    List<List<string>> foundClosing = Helper.RegEx(line, @"^}");
                    if (foundClosing.Count == 1)
                    {
                        if (csEIf.condition == true)
                        {
                            foreach (string codeline in csEIf.codelines)
                            {
                                Execute(codeline, true);
                            }
                        }

                        Env.callstack.Remove(lcs);
                    }

                    return;
                }
                else if (lcs is CS_Else csE && !csE.Closed)
                {
                    csE.codelines.Add(line);

                    List<List<string>> foundClosing = Helper.RegEx(line, @"^}");
                    if (foundClosing.Count == 1)
                    {
                        foreach (string codeline in csE.codelines)
                        {
                            Execute(codeline, true);
                        }

                        Env.callstack.Remove(lcs);
                    }

                    return;
                }
                else if (lcs is CS_Repeat csR && !csR.Closed)
                {
                    csR.codelines.Add(line);

                    List<List<string>> foundClosing = Helper.RegEx(line, @"^}");
                    if (foundClosing.Count == 1)
                    {
                        for (int i = 1; i <= csR.times; i++)
                        {
                            foreach (string codeline in csR.codelines)
                            {
                                Execute(codeline, true);
                            }
                        }

                        Env.callstack.Remove(lcs);
                    }

                    return;
                }
                else if (lcs is CS_Iterate csI && !csI.Closed)
                {
                    csI.codelines.Add(line);

                    List<List<string>> foundClosing = Helper.RegEx(line, @"^}");
                    if (foundClosing.Count == 1)
                    {
                        List<object> outerList = Env.variables.Get3By2(csI.array).ToList();
                        // IEnumerable<object> innerList = (IEnumerable<object>)outerList[0];
                        // List<object> list = innerList.ToList();

                        object listObj = outerList[0];
                        List<object> objects;

                        if (listObj is System.Collections.IEnumerable enumerable && !(listObj is string))
                        {
                            objects = new List<object>();
                            foreach (object item in enumerable)
                            {
                                objects.Add(item);
                            }
                        }
                        else
                        {
                            throw new Exception("Kein gültiges IEnumerable");
                        }

                        object lastElement = objects[0];

                        foreach (object element in objects)
                        {
                            Env.variables.Remove(typeof(object), "element", lastElement);
                            Env.variables.Add(typeof(object), "element", element);
                            foreach (string codeline in csI.codelines)
                            {
                                Execute(codeline, true);
                            }
                            lastElement = element;
                        }

                        Env.callstack.Remove(lcs);
                    }

                    return;
                }
            }

            // variable definition
            List<List<string>> found = Helper.RegEx(line, @"^([a-zA-Z0-9_\[\]\{\};]+) ([a-zA-Z0-9_]+) = (.*);$");
            if (found.Count == 1)
            {
                // type checking
                string type = found[0][0];

                string pat = @"^([a-zA-Z0-9_]+)\[\]$";
                Match matches = Regex.Match(type, pat);
                bool isArray = matches.Success;
                string? arrayType = isArray ? matches.Groups[1].Value : null;

                string pat2 = @"^([a-zA-Z0-9_]+)\{([a-zA-Z0-9_;]+)$";
                Match matchX = Regex.Match(type, pat2);
                bool isDict = matchX.Success;
                string? dictType = isDict ? matchX.Groups[1].Value + "; " + matchX.Groups[2].Value : null;

                if (!Lists.PrimitiveTypes.ContainsKey(type) && !isArray && !isDict) throw new GXCodeError("GX0003", $"Unknown type {type}");

                string name = found[0][1];
                string value = found[0][2];

                if (isArray)
                {
                    string pattern = @"^\[(.*)\]$";
                    Match match = Regex.Match(value, pattern);
                    if (!match.Success) throw new GXCodeError("GX0006", $"{value} is not a valid {arrayType} array");
                    string values = match.Groups[1].Value;

                    if (arrayType != null && !Lists.PrimitiveTypes.ContainsKey(arrayType)) throw new GXCodeError("GX0003", $"Unknown type {type}");

                    if (arrayType != null && Lists.PrimitiveTypes.TryGetValue(arrayType, out Type? type1))
                    {
                        if (type1 == typeof(string))
                        {
                            string elementPattern = @"""([^""]*)""";
                            List<List<string>> elementMatches = Helper.RegEx(values, elementPattern);

                            List<string> elements = new();

                            if (elementMatches.Count > 0)
                            {
                                foreach (List<string> matcher in elementMatches)
                                {
                                    elements.Add(matcher[0]);
                                }
                            }

                            Env.variables.Add(typeof(List<string>), name, elements);
                        }
                        else if (type1 == typeof(short))
                        {
                            List<string> elementsRaw = values.Split(',').ToList();
                            List<int> elements = new();

                            foreach (string element in elementsRaw)
                            {
                                try
                                {
                                    int result = int.Parse(element);
                                    elements.Add(result);
                                }
                                catch (FormatException)
                                {
                                    throw new GXCodeError("GX0006", $"{element} is not a valid integer");
                                }
                            }

                            Env.variables.Add(typeof(List<int>), name, elements);
                        }
                        else if (type1 == typeof(double))
                        {
                            List<string> elementsRaw = values.Split(',').ToList();
                            List<double> elements = new();

                            foreach (string element in elementsRaw)
                            {
                                try
                                {
                                    double result = double.Parse(element, System.Globalization.CultureInfo.InvariantCulture);
                                    elements.Add(result);
                                }
                                catch (FormatException)
                                {
                                    throw new GXCodeError("GX0006", $"{element} is not a valid decimal");
                                }
                            }

                            Env.variables.Add(typeof(List<double>), name, elements);
                        }
                        else if (type1 == typeof(bool))
                        {
                            List<string> elementsRaw = values.Split(',').ToList();
                            List<bool> elements = new();

                            foreach (string element in elementsRaw)
                            {
                                try
                                {
                                    bool result = bool.Parse(element);
                                    elements.Add(result);
                                }
                                catch (FormatException)
                                {
                                    throw new GXCodeError("GX0006", $"{element} is not a valid boolean");
                                }
                            }

                            Env.variables.Add(typeof(List<bool>), name, elements);
                        }
                    }
                    else throw new GXCodeInterpreterError("Error with Lists class");
                }
                else if (isDict)
                {

                }
                else
                {
                    string pattern = @"^""([^""]*)""$";
                    bool match = Regex.IsMatch(value, pattern);
                    char lastCharacter = value[value.Length - 1];
                    if (type == "str" && !match) throw new GXCodeError("GX0004", $"Unexpected {lastCharacter}, Expected \"");

                    if (Lists.PrimitiveTypes.TryGetValue(type, out Type? type1))
                    {
                        if (type1 == typeof(string)) Env.variables.Add(type1, name, value.Trim('\"'));
                        else if (type1 == typeof(short))
                        {
                            try
                            {
                                int result = int.Parse(value);
                                Env.variables.Add(type1, name, result);
                            }
                            catch (FormatException)
                            {
                                throw new GXCodeError("GX0006", $"{value} is not a valid integer");
                            }
                        }
                        else if (type1 == typeof(double))
                        {
                            try
                            {
                                double result = double.Parse(value, System.Globalization.CultureInfo.InvariantCulture);
                                Env.variables.Add(type1, name, result);
                            }
                            catch (FormatException)
                            {
                                throw new GXCodeError("GX0006", $"{value} is not a valid decimal");
                            }
                        }
                        else if (type1 == typeof(bool))
                        {
                            try
                            {
                                bool result = bool.Parse(value);
                                Env.variables.Add(type1, name, result);
                            }
                            catch (FormatException)
                            {
                                throw new GXCodeError("GX0006", $"{value} is not a valid boolean");
                            }
                        }
                        else throw new NotImplementedException($"{type1} is not implemented yet");

                        Helper.Debug($"Variable {name} of type {type} set to {value}");
                        return;
                    }
                    else throw new GXCodeInterpreterError("Error with Lists class");
                }
            }

            // keyword str
            List<List<string>> found2 = Helper.RegEx(line, @"^([a-zA-Z0-9_]+) (.*);$");
            if (found2.Count == 1)
            {
                string keyword = found2[0][0].Trim();
                if (!Lists.Keywords.Contains(keyword)) throw new GXCodeError("GX0005", $"Unknown keyword {keyword}");

                string attr = found2[0][1].Trim();
                string pattern = @"^""([^""]*)""$";
                bool isString = Regex.IsMatch(attr, pattern);

                // hardcoded keywords
                if (keyword == "out" && isString)
                {
                    Console.WriteLine(attr);
                }
                else if (keyword == "out" && !isString && Env.variables.Contains2(attr))
                {
                    List<object> output = Env.variables.Get3By2(attr).ToList();
                    List<Type> types = Env.variables.Get1By2(attr).ToList();
                    string? print = types[0] == typeof(string)
                        ? output[0].ToString()
                        : output[0]?.ToString() ?? "";
                    Console.WriteLine(print);
                }
            }

            // just keyword
            List<List<string>> found6 = Helper.RegEx(line, @"^([a-zA-Z0-9_]+);$");
            if (found6.Count == 1)
            {
                string keyword = found6[0][0].Trim();
                if (!Lists.Keywords.Contains(keyword)) throw new GXCodeError("GX0005", $"Unknown keyword {keyword}");

                // hardcoded keywords
                if (keyword == "exit")
                {
                    throw new GXCodeBreak();
                }
            }

            // if statement
            List<List<string>> found3 = Helper.RegEx(line, @"if \((.*)\) {", singleLine: true);
            if (found3.Count == 1)
            {
                CS_If cs_if = new();
                string condition = found3[0][0];

                cs_if.condition = evaluateIf(condition);
                Env.callstack.Add(cs_if);
            }

            CallstackElement? ics = Env.callstack.LastOrDefault();

            // else if statement
            List<List<string>> found4 = Helper.RegEx(line, @"else if \((.*)\) {", singleLine: true);
            if (found4.Count == 1 && ics is CS_If && ics.Closed == true)
            {
                CS_Else_If cs_eif = new();
                string condition = found4[0][0];

                cs_eif.condition = evaluateIf(condition);
                Env.callstack.Add(cs_eif);
            }

            // else statement
            List<List<string>> found5 = Helper.RegEx(line, @"else {", singleLine: true);
            if (found5.Count == 1 && ics is CS_If && ics.Closed == true)
            {
                CS_Else cs_e = new();
                Env.callstack.Add(cs_e);
            }

            CallstackElement? icl = Env.callstack.LastOrDefault();

            // rm if
            if (found4.Count == 0 && found5.Count == 0 && icl is CS_If && icl.Closed)
            {
                Env.callstack.Remove(icl);
            }

            // repeat statement
            List<List<string>> found7 = Helper.RegEx(line, @"repeat \((\d+)\) {", singleLine: true);
            if (found7.Count == 1)
            {
                CS_Repeat csR = new();
                string value = found7[0][0];

                try
                {
                    csR.times = int.Parse(value);
                }
                catch (FormatException)
                {
                    throw new GXCodeError("GX0006", $"{value} is not a valid integer");
                }

                Env.callstack.Add(csR);
            }

            // repeat statement with variable
            // reserve found8

            // iterate statement
            List<List<string>> found9 = Helper.RegEx(line, @"iterate \((.*)\) {", singleLine: true);
            if (found9.Count == 1)
            {
                CS_Iterate csI = new();

                string iterate = found9[0][0];
                csI.array = iterate;

                Env.callstack.Add(csI);
            }
        }
        
        private bool evaluateIf(string condition)
        {
            bool evaluationResult = false;

            List<List<string>> boolean = Helper.RegEx(condition, @"^([a-zA-Z0-9_]+)$");
            if (boolean.Count == 1)
            {
                evaluationResult = EvaluateBool(boolean[0][0]);
            }

            List<List<string>> negativeBoolean = Helper.RegEx(condition, @"^\!([a-zA-Z0-9_]+)$");
            if (negativeBoolean.Count == 1)
            {
                evaluationResult = !EvaluateBool(negativeBoolean[0][0]);
            }

            List<List<string>> comparision = Helper.RegEx(condition, @"^([a-zA-Z0-9_]+) == (.+?)$");
            if (comparision.Count == 1)
            {
                throw new NotImplementedException();
            }

            List<List<string>> negativeComparision = Helper.RegEx(condition, @"^([a-zA-Z0-9_]+) != (.+?)$");
            if (negativeComparision.Count == 1)
            {
                throw new NotImplementedException();
            }

            Helper.Debug($"Evaluated: {evaluationResult}");
            return evaluationResult;
        }

        private bool EvaluateBool(string found)
        {
            if (!Env.variables.Contains2(found))
            {
                throw new GXCodeError("GX0008", $"Unknown variable {found}");
            }

            if (Env.variables.Get1By2(found).ToList()[0] != typeof(bool))
            {
                throw new GXCodeError("GX0009", $"Unexpected non-boolean variable {found}");
            }

            string? value = Env.variables.Get3By2(found).ToList()[0].ToString();
            try
            {
                bool result = bool.Parse(value);
                if (result) return true;
                else return false;
            }
            catch (FormatException)
            {
                throw new GXCodeInterpreterError($"Error with saved variable {found}");
            }
            catch (ArgumentNullException)
            {
                throw new GXCodeInterpreterError($"Error with saved variable {found}");
            }
        }
    }

    public class CallstackElement
    {
        public bool Closed { get; set; } = false;
    }

    public class CS_If : CallstackElement
    {
        public List<string> codelines { get; set; } = [];
        public bool condition { get; set; } = false;
    }

    public class CS_Else_If : CallstackElement
    {
        public List<string> codelines { get; set; } = [];
        public bool condition { get; set; } = false;
    }

    public class CS_Else : CallstackElement
    {
        public List<string> codelines { get; set; } = [];
    }

    public class CS_Repeat : CallstackElement
    {
        public List<string> codelines { get; set; } = [];
        public int times { get; set; }
    }

    public class CS_Iterate : CallstackElement
    {
        public List<string> codelines { get; set; } = [];
        public string array { get; set; } = "";
    }
    
    public class Entrypoint
    {
        public List<string> Lines;

        public Entrypoint(List<string> lines)
        {
            Lines = lines;
        }
    }
}