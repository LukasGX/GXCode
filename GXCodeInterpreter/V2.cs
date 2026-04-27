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

                GXC_CS_ELEMENT? inMem = null;

                bool inMultiLineComment = false;

                // split the code and save into env.blocks
                for (int i = 0; i < env.Lines.Count; i++)
                {
                    string line = env.Lines[i];
                    int ri = i + 1;
                    LineType type = GXCodeInterpreter.GetLineType(line, inMultiLineComment);

                    // Console.WriteLine($"Line {ri}: {line} (type: {type})");

                    GXC_CS_ELEMENT? last = cs.CS.LastOrDefault();

                    switch (type)
                    {
                        case LineType.UNKNOWN:
                            throw new GXCIndeterminableLineError(line, ri);
                        case LineType.COMMENT:
                            continue;
                        case LineType.MULTILINE_COMMENT_INDICATOR:
                            inMultiLineComment = !inMultiLineComment;
                            continue;
                        case LineType.NEGLIGIBLE:
                            continue;
                        case LineType.NAMESPACE_DEFINITION:
                            env.Namespace = GXCodeInterpreter.GetNS(line);
                            break;
                        case LineType.ENTRYPOINT_DEFINITION_START:
                            if (last is not null)
                            {
                                throw new GXCNestedEntrypointError(ri, last.GetType().Name);
                            }

                            bool hasEntrypoint = env.blocks.Values.Any(val => val is GXC_CS_ENTRYPOINT);
                            if (hasEntrypoint)
                            {
                                throw new GXCMultipleEntrypointError(ri);
                            }

                            GXC_CS_ENTRYPOINT n_entrypoint = new(lastCSID + 1);
                            env.blocks.Add(lastCSID + 1, n_entrypoint);
                            lastCSID += 1;

                            cs.CS.Add(n_entrypoint);
                            cs_ids.Add(n_entrypoint.ID);
                            break;
                        case LineType.IF_START:
                            if (last is null)
                            {
                                throw new GXCStrayBlockError(ri, typeof(GXC_CS_IF).Name, false);
                            }
                            else if (last is GXC_CS_CLASS)
                            {
                                throw new GXCStrayBlockError(ri, typeof(GXC_CS_IF).Name, true);
                            }

                            string IfCondition = GXCodeInterpreter.GetIfCondition(line);
                            GXC_CS_IF n_if = new(lastCSID + 1, IfCondition);
                            env.blocks.Add(lastCSID + 1, n_if);
                            lastCSID += 1;

                            cs.CS.Add(n_if);
                            cs_ids.Add(n_if.ID);

                            env.blocks[last.ID].Lines.Add($"[BLOCK {n_if.ID}]");
                            break;
                        case LineType.ELSE_IF_START:
                            if (last is null)
                            {
                                throw new GXCStrayBlockError(ri, typeof(GXC_CS_ELSE_IF).Name, false);
                            }
                            else if (last is GXC_CS_CLASS)
                            {
                                throw new GXCStrayBlockError(ri, typeof(GXC_CS_ELSE_IF).Name, true);
                            }
                            if (inMem is not GXC_CS_IF && inMem is not GXC_CS_ELSE_IF)
                            {
                                throw new GXCStrayElseIfError(ri);
                            }

                            string ElseIfCondition = GXCodeInterpreter.GetElseIfCondition(line);
                            GXC_CS_ELSE_IF n_else_if = new(lastCSID + 1, ElseIfCondition);
                            env.blocks.Add(lastCSID + 1, n_else_if);
                            lastCSID += 1;

                            cs.CS.Add(n_else_if);
                            cs_ids.Add(n_else_if.ID);

                            env.blocks[last.ID].Lines.Add($"[BLOCK {n_else_if.ID}]");
                            break;
                        case LineType.ELSE_START:
                            if (last is null)
                            {
                                throw new GXCStrayBlockError(ri, typeof(GXC_CS_ELSE).Name, false);
                            }
                            else if (last is GXC_CS_CLASS)
                            {
                                throw new GXCStrayBlockError(ri, typeof(GXC_CS_ELSE).Name, true);
                            }
                            if (inMem is not GXC_CS_IF && inMem is not GXC_CS_ELSE_IF)
                            {
                                throw new GXCStrayElseError(ri);
                            }

                            GXC_CS_ELSE n_else = new(lastCSID + 1);
                            env.blocks.Add(lastCSID + 1, n_else);
                            lastCSID += 1;
                            
                            cs.CS.Add(n_else);
                            cs_ids.Add(n_else.ID);

                            env.blocks[last.ID].Lines.Add($"[BLOCK {n_else.ID}]");
                            break;
                        case LineType.SWITCH_START:
                            if (last is null)
                            {
                                throw new GXCStrayBlockError(ri, typeof(GXC_CS_SWITCH).Name, false);
                            }
                            else if (last is GXC_CS_CLASS)
                            {
                                throw new GXCStrayBlockError(ri, typeof(GXC_CS_SWITCH).Name, true);
                            }

                            string switchVar = GXCodeInterpreter.GetSwitchVariable(line);
                            GXC_CS_SWITCH n_switch = new(lastCSID + 1, switchVar);
                            env.blocks.Add(lastCSID + 1, n_switch);
                            lastCSID += 1;

                            cs.CS.Add(n_switch);
                            cs_ids.Add(n_switch.ID);

                            env.blocks[last.ID].Lines.Add($"[BLOCK {n_switch.ID}]");
                            break;
                        case LineType.CASE_START:
                            if (last is null)
                            {
                                throw new GXCStrayBlockError(ri, typeof(GXC_CS_CASE).Name, false);
                            }
                            else if (last is GXC_CS_CLASS)
                            {
                                throw new GXCStrayBlockError(ri, typeof(GXC_CS_CASE).Name, true);
                            }
                            if (last is not GXC_CS_SWITCH)
                            {
                                throw new GXCStrayCaseError(ri);
                            }

                            string caseValue = GXCodeInterpreter.GetCaseValue(line);
                            GXC_CS_CASE n_case = new(lastCSID + 1, caseValue);
                            env.blocks.Add(lastCSID + 1, n_case);
                            lastCSID += 1;

                            cs.CS.Add(n_case);
                            cs_ids.Add(n_case.ID);

                            env.blocks[last.ID].Lines.Add($"[BLOCK {n_case.ID}]");
                            break;
                        case LineType.REPEAT_START:
                            if (last is null)
                            {
                                throw new GXCStrayBlockError(ri, typeof(GXC_CS_REPEAT).Name, false);
                            }
                            else if (last is GXC_CS_CLASS)
                            {
                                throw new GXCStrayBlockError(ri, typeof(GXC_CS_REPEAT).Name, true);
                            }

                            string repeatVar = GXCodeInterpreter.GetRepeatVariable(line);
                            GXC_CS_REPEAT n_repeat = new(lastCSID + 1, repeatVar);
                            env.blocks.Add(lastCSID + 1, n_repeat);
                            lastCSID += 1;

                            cs.CS.Add(n_repeat);
                            cs_ids.Add(n_repeat.ID);

                            env.blocks[last.ID].Lines.Add($"[BLOCK {n_repeat.ID}]");
                            break;
                        case LineType.ITERATE_START:
                            if (last is null)
                            {
                                throw new GXCStrayBlockError(ri, typeof(GXC_CS_ITERATE).Name, false);
                            }
                            else if (last is GXC_CS_CLASS)
                            {
                                throw new GXCStrayBlockError(ri, typeof(GXC_CS_ITERATE).Name, true);
                            }

                            string iterateVar = GXCodeInterpreter.GetIterateVariable(line);
                            GXC_CS_ITERATE n_iterate = new(lastCSID + 1, iterateVar);
                            env.blocks.Add(lastCSID + 1, n_iterate);
                            lastCSID += 1;

                            cs.CS.Add(n_iterate);
                            cs_ids.Add(n_iterate.ID);

                            env.blocks[last.ID].Lines.Add($"[BLOCK {n_iterate.ID}]");
                            break;
                        case LineType.WHILE_START:
                            if (last is null)
                            {
                                throw new GXCStrayBlockError(ri, typeof(GXC_CS_WHILE).Name, false);
                            }
                            else if (last is GXC_CS_CLASS)
                            {
                                throw new GXCStrayBlockError(ri, typeof(GXC_CS_WHILE).Name, true);
                            }

                            string whileCondition = GXCodeInterpreter.GetWhileCondition(line);
                            GXC_CS_ITERATE n_while = new(lastCSID + 1, whileCondition);
                            env.blocks.Add(lastCSID + 1, n_while);
                            lastCSID += 1;
                            
                            cs.CS.Add(n_while);
                            cs_ids.Add(n_while.ID);

                            env.blocks[last.ID].Lines.Add($"[BLOCK {n_while.ID}]");
                            break;
                        case LineType.CLOSING:
                            if (last is null)
                            {
                                throw new GXCNothingToCloseError(ri);
                            }

                            inMem = last;
                            cs.CS.Remove(last);
                            cs_ids.Remove(last.ID);
                            break;
                        case LineType.BUILTIN_OPERATION:
                            if (last is null)
                            {
                                throw new GXCStrayBuiltinOperationError(ri, false);
                            }
                            if (last is GXC_CS_CLASS)
                            {
                                throw new GXCStrayBuiltinOperationError(ri, true);
                            }

                            env.blocks[last.ID].Lines.Add(line);
                            break;
                        case LineType.VARIABLE_DECLARATION:
                            if (last is null)
                            {
                                throw new GXCStrayVariableDeclarationError(ri);
                            }

                            env.blocks[last.ID].Lines.Add(line);
                            break;
                        case LineType.VARIABLE_ASSIGNMENT:
                            if (last is null)
                            {
                                throw new GXCStrayVariableAssignmentError(ri);
                            }

                            env.blocks[last.ID].Lines.Add(line);
                            break;
                    }
                }

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Program ran without any errors");
                Console.ResetColor();
            }
            catch (GXCodeError e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error {e.Id}: {e.Message} at line {e.LineNr}");
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
        public Dictionary<int, GXC_CS_ELEMENT> blocks = new();
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
        MULTILINE_COMMENT_INDICATOR,
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
        public static LineType GetLineType(string line, bool inMultiLineComment)
        {
            // multiline comment indicator
            string multiCommentPattern = @"^\s*\/\/\/.*$";
            if (Regex.IsMatch(line, multiCommentPattern)) return LineType.MULTILINE_COMMENT_INDICATOR;

            // skip if in multiline comment
            if (inMultiLineComment) return LineType.COMMENT;

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
            string declarationPattern = @"^\s*(str|int|dec|bool|rex)(?!\s*\(\))(?:\[\]|\{[a-z;]+\})?\s*[a-zA-Z0-9]+\s*=\s*.*;$";
            if (Regex.IsMatch(line, declarationPattern)) return LineType.VARIABLE_DECLARATION;

            // variable assignment
            string assignmentPattern = @"^\s*[a-zA-Z0-9]+\s*=\s*.*;$";
            if (Regex.IsMatch(line, assignmentPattern)) return LineType.VARIABLE_ASSIGNMENT;

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

        public static string GetIfCondition(string line)
        {
            string pattern = @"^\s*if\s*\(([^""']*)\)\s*\{$";
            Match match = Regex.Match(line, pattern);

            if (match.Success)
            {
                string condition = match.Groups[1].Value;
                return condition;
            }
            else
            {
                throw new GXCodeInterpreterError("Could not detect if condition");
            }
        }

        public static string GetElseIfCondition(string line)
        {
            string pattern = @"^\s*else\s+if\s*\(([^""']*)\)\s*\{$";
            Match match = Regex.Match(line, pattern);

            if (match.Success)
            {
                string condition = match.Groups[1].Value;
                return condition;
            }
            else
            {
                throw new GXCodeInterpreterError("Could not detect else if condition");
            }
        }

        public static string GetSwitchVariable(string line)
        {
            string pattern = @"^\s*switch\s*\(([a-zA-Z0-9]+)\)\s*\{$";
            Match match = Regex.Match(line, pattern);

            if (match.Success)
            {
                string variable = match.Groups[1].Value;
                return variable;
            }
            else
            {
                throw new GXCodeInterpreterError("Could not detect switch variable");
            }
        }

        public static string GetCaseValue(string line)
        {
            string pattern = @"^\s*case\s+(.*?)\s*\{$";
            Match match = Regex.Match(line, pattern);

            if (match.Success)
            {
                string value = match.Groups[1].Value;
                return value;
            }
            else
            {
                throw new GXCodeInterpreterError("Could not detect case value");
            }
        }

        public static string GetRepeatVariable(string line)
        {
            string pattern = @"^\s*repeat\s*\(([a-zA-Z0-9]+)\)\s*\{$";
            Match match = Regex.Match(line, pattern);

            if (match.Success)
            {
                string variable = match.Groups[1].Value;
                return variable;
            }
            else
            {
                throw new GXCodeInterpreterError("Could not detect repeat variable");
            }
        }

        public static string GetIterateVariable(string line)
        {
            string pattern = @"^\s*iterate\s*\(([a-zA-Z0-9]+)\)\s*\{$";
            Match match = Regex.Match(line, pattern);

            if (match.Success)
            {
                string variable = match.Groups[1].Value;
                return variable;
            }
            else
            {
                throw new GXCodeInterpreterError("Could not detect iterate variable");
            }
        }

        public static string GetWhileCondition(string line)
        {
            string pattern = @"^\s*while\s*\(([^""']*)\)\s*\{$";
            Match match = Regex.Match(line, pattern);

            if (match.Success)
            {
                string condition = match.Groups[1].Value;
                return condition;
            }
            else
            {
                throw new GXCodeInterpreterError("Could not detect while condition");
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
        public List<string> Lines { get; set; } = [];
    }
    public class GXC_CS_ENTRYPOINT(int id) : GXC_CS_ELEMENT(id) {}
    public class GXC_CS_IF(int id, string condition) : GXC_CS_ELEMENT(id)
    {
        public string Condition { get; set; } = condition;
    }
    public class GXC_CS_ELSE_IF(int id, string condition) : GXC_CS_ELEMENT(id)
    {
        public string Condition { get; set; } = condition;
    }
    public class GXC_CS_ELSE(int id) : GXC_CS_ELEMENT(id) {}
    public class GXC_CS_SWITCH(int id, string var) : GXC_CS_ELEMENT(id)
    {
        public string Variable { get; set; } = var;
    }
    public class GXC_CS_CASE(int id, string val) : GXC_CS_ELEMENT(id)
    {
        public string Value { get; set; } = val;
    }
    public class GXC_CS_REPEAT(int id, string var) : GXC_CS_ELEMENT(id)
    {
        public string Variable { get; set; } = var;
    }
    public class GXC_CS_ITERATE(int id, string var) : GXC_CS_ELEMENT(id)
    {
        public string Variable { get; set; } = var;
    }
    public class GXC_CS_WHILE(int id, string condition) : GXC_CS_ELEMENT(id)
    {
        public string Condition { get; set; } = condition;
    }
    public class GXC_CS_CLASS(int id) : GXC_CS_ELEMENT(id) {}
    public class GXC_CS_METHOD(int id) : GXC_CS_ELEMENT(id) {}
}