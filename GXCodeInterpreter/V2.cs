using System;
using System.Numerics;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using System.Linq;

namespace GXCodeInterpreter
{
    class GXCodeProgram
    {
        public static void Start()
        {
            try {
                string content = File.ReadAllText("/home/lukas/Documents/Coding/C#/GXCode/GXCodeInterpreter/program.gxc");
                List<string> lines = GXCodeHelper.SplitCode(content);
                GXCodeEnvironment env = new(content, lines);

                Callstack cs = new();
                List<int> cs_ids = [];
                int lastCSID = -1;

                // main loop
                foreach (string line in env.Lines)
                {
                    LineType type = GXCodeInterpreter.GetLineType(line);

                    switch (type)
                    {
                        case LineType.UNKNOWN:
                            throw new GXCIndeterminableLineError(line);
                        case LineType.COMMENT:
                            continue;
                        case LineType.NEGLIGIBLE:
                            continue;
                        case LineType.NAMESPACE_DEFINITION:
                            env.Namespace = GXCodeInterpreter.GetNS(line);
                            break;
                        case LineType.ENTRYPOINT_DEFINITION_START:
                            bool hasEntrypoint = env.blocks.innerDict.Keys.Any(key => key.Item2 is GXC_CS_ENTRYPOINT);
                            if (hasEntrypoint)
                            {
                                throw new GXCMultipleEntrypointError();
                            }

                            GXC_CS_ENTRYPOINT n = new(lastCSID + 1);
                            env.blocks.Add(lastCSID + 1, n, []);
                            lastCSID += 1;

                            cs.CS.Add(n);
                            cs_ids.Add(n.ID);
                            break;
                        case LineType.CLOSING:
                            GXC_CS_ELEMENT last;
                            try
                            {
                                last = cs.CS.Last();
                            }
                            catch (InvalidOperationException)
                            {
                                throw new GXCNothingToCloseError();
                            }

                            cs.CS.Remove(last);
                            cs_ids.Remove(last.ID);
                            break;
                    }
                }
            }
            catch (GXCodeError e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error {e.Id}: {e.Message}");
                Console.ResetColor();
            }
            catch (GXCodeInterpreterError e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Interpreter Error: ${e.Message}");
                Console.ResetColor();
            }
        }
    }

    class GXCodeEnvironment(string code, List<string> lines)
    {
        public string Code { get; set; } = code;
        public List<string> Lines { get; set; } = lines;
        public string Namespace { get; set; } = "";
        public TripleDictionary<int, GXC_CS_ELEMENT, List<string>> blocks = new();
    }

    class GXCodeHelper
    {
        public static List<string> SplitCode(string code)
        {
            string[] ll = code.Split("\n");
            List<string> lines = [.. ll];
            return lines;
        }
    }

    public enum LineType
    {
        COMMENT,
        NAMESPACE_DEFINITION,
        METHOD_DEFINITION_START,
        RETURN_METHOD_DEFINITION_START,
        CLASS_DEFINITION_START,
        ENTRYPOINT_DEFINITION_START,
        IF_START,
        ELSE_IF_START,
        ELSE_START,
        SWITCH_START,
        CASE_START,
        REPEAT_START,
        ITERATE_START,
        WHILE_START,
        CLOSING,
        BUILTIN_OPERATION,
        VARIABLE_DECLARATION,
        VARIABLE_ASSIGNMENT,
        UNKNOWN,
        NEGLIGIBLE
    }

    class GXCodeInterpreter
    {
        public static LineType GetLineType(string line)
        {
            // negligible
            if (line.Trim() == "") return LineType.NEGLIGIBLE;

            // comment
            string commentPattern = @"^\s*\/\/.*$";
            if (Regex.IsMatch(line, commentPattern)) return LineType.COMMENT;

            // ns definition
            string nsPattern = @"^#ns\s+[a-zA-Z0-9]+$";
            if (Regex.IsMatch(line, nsPattern)) return LineType.NAMESPACE_DEFINITION;

            // definition start
            string methodPattern = @"^(?:\w+\s+)?method(?:\[\]|\{[a-z;]+\})\s+[a-zA-Z0-9]+\s*\{$";
            string returnPattern = @"^(?:\w+\s+)?(?:str|int|dec|bool|rex)(?:\[\]|\{[a-z;]+\})\s+[a-zA-Z0-9]+\s*\{$";
            string classPattern = @"^(?:\w+\s+)?class(?:\[\]|\{[a-z;]+\})\s+[a-zA-Z0-9]+\s*\{$";
            if (Regex.IsMatch(line, methodPattern)) return LineType.METHOD_DEFINITION_START;
            if (Regex.IsMatch(line, returnPattern)) return LineType.RETURN_METHOD_DEFINITION_START;
            if (Regex.IsMatch(line, classPattern)) return LineType.CLASS_DEFINITION_START;

            // entrypoint definition start
            string entrypointPattern = @"^\s*entrypoint\(\)\s*\{$";
            if (Regex.IsMatch(line, entrypointPattern)) return LineType.ENTRYPOINT_DEFINITION_START;

            // block start
            string ifPattern = @"^\s*if\s*\([^""']*\)\s*\{$";
            string elseIfPattern = @"^\s*else\s+if\s*\([^""']*\)\s*\{$";
            string elsePattern = @"^\s*else\s*\{$";

            string switchPattern = @"^\s*switch\s*\([a-zA-Z0-9]+\)\s*\{$";
            string casePattern = @"^\s*case\s+.*?\s*\{$";

            string repeatPattern = @"^\s*repeat\s*\([a-zA-Z0-9]+\)\s*\{$";
            string iteratePattern = @"^\s*iterate\s*\([a-zA-Z0-9]+\)\s*\{$";
            string whilePattern = @"^\s*while\s*\([^""']*\)\s*\{$";

            if (Regex.IsMatch(line, ifPattern)) return LineType.IF_START;
            if (Regex.IsMatch(line, elseIfPattern)) return LineType.ELSE_IF_START;
            if (Regex.IsMatch(line, elsePattern)) return LineType.ELSE_START;
            if (Regex.IsMatch(line, switchPattern)) return LineType.SWITCH_START;
            if (Regex.IsMatch(line, casePattern)) return LineType.CASE_START;
            if (Regex.IsMatch(line, repeatPattern)) return LineType.REPEAT_START;
            if (Regex.IsMatch(line, iteratePattern)) return LineType.ITERATE_START;
            if (Regex.IsMatch(line, whilePattern)) return LineType.WHILE_START;

            // closing
            string closingPattern = @"^\s*\}\s*$";
            if (Regex.IsMatch(line, closingPattern)) return LineType.CLOSING;

            // built-in operation
            string builtinPattern = @"^\s*out\s+.*;$";
            if (Regex.IsMatch(line, builtinPattern)) return LineType.BUILTIN_OPERATION;

            // variable declaration
            string assignmentPattern = @"^\s*(str|int|dec|bool|rex)(?!\s*\(\))(?:\[\]|\{[a-z;]+\})?\s*[a-zA-Z0-9]+\s*=\s*.*;$";
            if (Regex.IsMatch(line, assignmentPattern)) return LineType.VARIABLE_DECLARATION;

            // unknown
            return LineType.UNKNOWN;
        }

        public static string GetNS(string line)
        {
            string pattern = @"^#ns\s+([a-zA-Z0-9]+)$";
            Match match = Regex.Match(line, pattern);

            if (match.Success)
            {
                string ns = match.Groups[1].Value;
                return ns;
            }
            else
            {
                throw new GXCodeInterpreterError("Could not detect ns");
            }
        }
    }

    public class Callstack ()
    {
        public List<GXC_CS_ELEMENT> CS { get; set; } = [];
    }

    public class GXC_CS_ELEMENT(int id)
    {
        public int ID = id;
    }
    public class GXC_CS_ENTRYPOINT(int id) : GXC_CS_ELEMENT(id) {}
}