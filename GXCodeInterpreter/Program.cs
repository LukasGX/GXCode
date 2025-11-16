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
            Environment env = new();
            Interpreter interpreter = new("""
            entrypoint(){
                str abc = "def";
                int number = 0;
                out abc;
                out number;
            }
            """, env);

            try
            {
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
        }
    }

    public class Environment
    {
        //                      type,  name,  value
        public TripleDictionary<Type, string, object> variables = new();
    }

    public class Helper
    {
        public static List<List<string>> RegEx(string text, string pattern)
        {
            Regex rg = new(pattern);
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
            {"dec", typeof(float)},
            {"bool", typeof(bool)},
            {"rex", typeof(string)},
            {"<T1>[]", typeof(Array)},
            {"<T1>{<T2>}", typeof(Dictionary<,>)}
        };

        public static List<string> Keywords = new()
        {
            "out", "return"
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

        public Entrypoint DetectEntryPoint()
        {
            string pattern = @"(?s)entrypoint\((.*?)\)\s*\{(.*?)\}";
            MatchCollection matches = Regex.Matches(Code, pattern);

            if (matches.Count > 1)
            {
                throw new GXCodeError("GX0001", "Too many entrypoints!");
            }

            if (matches.Count == 1)
            {
                // string arguments = matches[0].Groups[1].Value;
                string instructions = matches[0].Groups[2].Value;

                List<string> lines = instructions.Split('\n').ToList();
                lines.RemoveAll(s => string.IsNullOrWhiteSpace(s));

                Entrypoint entrypoint = new(lines);
                return entrypoint;
            }
            else
            {
                throw new GXCodeError("GX0002", "No entrypoint found!");
            }
        }

        public void Execute(string line)
        {
            if (line != "")
            {
                char lastChar = line[line.Length - 1];
                if (lastChar != ';' && lastChar != '{' && lastChar != '}')
                    throw new GXCodeError("GX0004", $"Unexpected {lastChar}, Expected ; or {{ or }}");
            }
            else return;

            // check for line pattern
            // variable definition
            List<List<string>> found = Helper.RegEx(line, @"^([a-zA-Z0-9_\[\]\{\};]+) ([a-zA-Z0-9_]+) = (.*);$");
            if (found.Count == 1)
            {
                string type = found[0][0];
                if (!Lists.PrimitiveTypes.ContainsKey(type)) throw new GXCodeError("GX0003", $"Unknown type {type}");

                string name = found[0][1];
                string value = found[0][2];

                string pattern = @"^""([^""]*)""$";
                bool match = Regex.IsMatch(value, pattern);
                char lastCharacter = value[value.Length - 1];
                if (type == "str" && !match) throw new GXCodeError("GX0004", $"Unexpected {lastCharacter}, Expected \"");

                if (Lists.PrimitiveTypes.TryGetValue(type, out Type? type1))
                {
                    Env.variables.Add(type1, name, value);
                    Helper.Debug($"Variable {name} of type {type} set to {value}");
                    return;
                }
                else
                {
                    throw new GXCodeInterpreterError("Error with Lists class");
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
                    Console.WriteLine(attr.Trim('\"'));
                }
                else if (keyword == "out" && !isString && Env.variables.Contains2(attr))
                {
                    List<object> output = Env.variables.Get3By2(attr).ToList();
                    List<Type> types = Env.variables.Get1By2(attr).ToList();
                    string print = types[0] == typeof(string) 
                        ? output[0].ToString().Trim('\"') 
                        : output[0]?.ToString() ?? "";
                    Console.WriteLine(print);
                }
            }
        }
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