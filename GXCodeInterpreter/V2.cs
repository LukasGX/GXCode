using System;
using System.Numerics;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using System.Linq;

namespace GXCodeInterpreter
{
    class GXCodeProgram
    {
        public static Stack<Scope> scopeStack = new();

        public static void Start(string[] args)
        {
            try
            {
                string? filePath = null;
                GXCodeHelper.DebuggingEnabled = false;

                for (int i = 0; i < args.Length; i++)
                {
                    if (args[i] == "--debug")
                    {
                        GXCodeHelper.DebuggingEnabled = true;
                        Console.ForegroundColor = ConsoleColor.Magenta;
                        Console.WriteLine("DEBUG MODE ENABLED");
                        Console.ResetColor();
                    }
                    else if (args[i] == "--gxc" && i + 1 < args.Length)
                    {
                        filePath = args[i + 1];
                        i++;
                    }
                }

                if (string.IsNullOrEmpty(filePath))
                {
                    throw new GXCodeInterpreterError("No file specified. Use --gxc <path> to specify the GXCode file to run.");
                }

                string content = File.ReadAllText(filePath);
                List<string> lines = GXCodeHelper.SplitCode(content);
                GXCodeEnvironment env = new(content, lines);

                scopeStack.Push(new Scope());

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

                    GXCodeHelper.Debug($"Line {ri}: {line} (type: {type})");

                    GXC_CS_ELEMENT? last = cs.CS.LastOrDefault();

                    switch (type)
                    {
                        case LineType.UNKNOWN:
                            throw new GXCIndeterminableLineError(line, ri, null);
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
                                throw new GXCNestedEntrypointError(ri, last.GetType().Name, null);
                            }

                            bool hasEntrypoint = env.blocks.Values.Any(val => val is GXC_CS_ENTRYPOINT);
                            if (hasEntrypoint)
                            {
                                throw new GXCMultipleEntrypointError(ri, null);
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
                                throw new GXCStrayBlockError(ri, typeof(GXC_CS_IF).Name, false, null);
                            }
                            else if (last is GXC_CS_CLASS)
                            {
                                throw new GXCStrayBlockError(ri, typeof(GXC_CS_IF).Name, true, null);
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
                                throw new GXCStrayBlockError(ri, typeof(GXC_CS_ELSE_IF).Name, false, null);
                            }
                            else if (last is GXC_CS_CLASS)
                            {
                                throw new GXCStrayBlockError(ri, typeof(GXC_CS_ELSE_IF).Name, true, null);
                            }
                            if (inMem is not GXC_CS_IF && inMem is not GXC_CS_ELSE_IF)
                            {
                                throw new GXCStrayElseIfError(ri, null);
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
                                throw new GXCStrayBlockError(ri, typeof(GXC_CS_ELSE).Name, false, null);
                            }
                            else if (last is GXC_CS_CLASS)
                            {
                                throw new GXCStrayBlockError(ri, typeof(GXC_CS_ELSE).Name, true, null);
                            }
                            if (inMem is not GXC_CS_IF && inMem is not GXC_CS_ELSE_IF)
                            {
                                throw new GXCStrayElseError(ri, null);
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
                                throw new GXCStrayBlockError(ri, typeof(GXC_CS_SWITCH).Name, false, null);
                            }
                            else if (last is GXC_CS_CLASS)
                            {
                                throw new GXCStrayBlockError(ri, typeof(GXC_CS_SWITCH).Name, true, null);
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
                                throw new GXCStrayBlockError(ri, typeof(GXC_CS_CASE).Name, false, null);
                            }
                            else if (last is GXC_CS_CLASS)
                            {
                                throw new GXCStrayBlockError(ri, typeof(GXC_CS_CASE).Name, true, null);
                            }
                            if (last is not GXC_CS_SWITCH)
                            {
                                throw new GXCStrayCaseError(ri, null);
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
                                throw new GXCStrayBlockError(ri, typeof(GXC_CS_REPEAT).Name, false, null);
                            }
                            else if (last is GXC_CS_CLASS)
                            {
                                throw new GXCStrayBlockError(ri, typeof(GXC_CS_REPEAT).Name, true, null);
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
                                throw new GXCStrayBlockError(ri, typeof(GXC_CS_ITERATE).Name, false, null);
                            }
                            else if (last is GXC_CS_CLASS)
                            {
                                throw new GXCStrayBlockError(ri, typeof(GXC_CS_ITERATE).Name, true, null);
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
                                throw new GXCStrayBlockError(ri, typeof(GXC_CS_WHILE).Name, false, null);
                            }
                            else if (last is GXC_CS_CLASS)
                            {
                                throw new GXCStrayBlockError(ri, typeof(GXC_CS_WHILE).Name, true, null);
                            }

                            string whileCondition = GXCodeInterpreter.GetWhileCondition(line);
                            GXC_CS_WHILE n_while = new(lastCSID + 1, whileCondition);
                            env.blocks.Add(lastCSID + 1, n_while);
                            lastCSID += 1;
                            
                            cs.CS.Add(n_while);
                            cs_ids.Add(n_while.ID);

                            env.blocks[last.ID].Lines.Add($"[BLOCK {n_while.ID}]");
                            break;
                        case LineType.CLOSING:
                            if (last is null)
                            {
                                throw new GXCNothingToCloseError(ri, null);
                            }

                            inMem = last;
                            cs.CS.Remove(last);
                            cs_ids.Remove(last.ID);
                            break;
                        case LineType.BUILTIN_OPERATION:
                            if (last is null)
                            {
                                throw new GXCStrayBuiltinOperationError(ri, false, null);
                            }
                            if (last is GXC_CS_CLASS)
                            {
                                throw new GXCStrayBuiltinOperationError(ri, true, null);
                            }

                            env.blocks[last.ID].Lines.Add(line);
                            break;
                        case LineType.VARIABLE_DECLARATION:
                            if (last is null)
                            {
                                throw new GXCStrayVariableDeclarationError(ri, null);
                            }

                            env.blocks[last.ID].Lines.Add(line);
                            break;
                        case LineType.VARIABLE_ASSIGNMENT:
                            if (last is null)
                            {
                                throw new GXCStrayVariableAssignmentError(ri, null);
                            }

                            env.blocks[last.ID].Lines.Add(line);
                            break;
                        case LineType.VARIABLE_ARITHMETIC:
                            if (last is null)
                            {
                                throw new GXCStrayVariableArithmeticError(ri, null);
                            }

                            env.blocks[last.ID].Lines.Add(line);
                            break;
                    }
                }

                // begin with the entrypoint block
                GXC_CS_ENTRYPOINT? entrypoint = env.blocks.Values.FirstOrDefault(val => val is GXC_CS_ENTRYPOINT) as GXC_CS_ENTRYPOINT ?? throw new GXCMissingEntrypointError(null);
                GXCodeInterpreter.ExecuteBlock(env, entrypoint);

                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Program ran without any errors");
                Console.ResetColor();
            }
            catch (GXCodeError e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error {e.Id}: {e.Message} {(e.LineNr == 0 ? "" : $"at line {e.LineNr}")} {(e.Block == null ? "" : $"of block {e.Block}")}");
                Console.ResetColor();
            }
            catch (GXCodeInterpreterError e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Interpreter Error: {e.Message}");
                Console.ResetColor();
            }
            catch (GXCodeBreak)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Program was ended intentionally");
                Console.ResetColor();
            }
        }
    }

    public class Variable
    {
        public string Name { get; init; }
        public object Value { get; set; }
        public string Type { get; init; }  // "str", "int", "bool", etc.
        
        public Variable(string name, object value, string type)
        {
            Name = name;
            Value = value;
            Type = type;
        }
    }

    public class Scope
    {
        public Dictionary<string, Variable> Variables = new(StringComparer.OrdinalIgnoreCase);
        public Scope? Parent = null;
        
        public Scope(Scope? parent = null)
        {
            Parent = parent;
        }
        
        public void Set(string name, object value, string type)
        {
            for (var scope = this; scope != null; scope = scope.Parent)
            {
                if (scope.Variables.TryGetValue(name, out var existing))
                {
                    existing.Value = value;
                    return;
                }
            }

            // not found in any parent: create in current scope
            Variables[name] = new Variable(name, value, type);
        }
        
        public bool TryGet(string name, out object? value, out string? type)
        {
            for (var scope = this; scope != null; scope = scope.Parent)
            {
                if (scope.Variables.TryGetValue(name, out var variable))
                {
                    value = variable.Value;
                    type = variable.Type;
                    return true;
                }
            }
            
            value = null;
            type = null;
            return false;
        }
        
        public bool HasType(string name, string expectedType)
        {
            return TryGet(name, out _, out var type) && type == expectedType;
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
        public static bool DebuggingEnabled = true;

        public static List<string> SplitCode(string code)
        {
            string[] ll = code.Split("\n");
            List<string> lines = [.. ll];
            return lines;
        }
        public static void Debug(string message) {
            if (!GXCodeHelper.DebuggingEnabled) return;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[DEBUG] {message}");
            Console.ResetColor();
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
        VARIABLE_ARITHMETIC,
        UNKNOWN,
        NEGLIGIBLE
    }

    public enum ShortLineType
    {
        BUILTIN_OPERATION,
        VARIABLE_DECLARATION,
        VARIABLE_ASSIGNMENT,
        VARIABLE_ARITHMETIC,
        BLOCK_INDICATOR,
        UNKNOWN
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
            string outBuiltinPattern = @"^\s*out\s+.*;$";
            if (Regex.IsMatch(line, outBuiltinPattern)) return LineType.BUILTIN_OPERATION;

            string exitBuiltinPattern = @"^\s*exit;\s*$";
            if (Regex.IsMatch(line, exitBuiltinPattern)) return LineType.BUILTIN_OPERATION;

            // variable declaration
            string declarationPattern = @"^\s*(str|int|dec|bool|rex)(?!\s*\(\))(?:\[\]|\{[a-z;]+\})?\s*[a-zA-Z0-9]+\s*=\s*.*;$";
            if (Regex.IsMatch(line, declarationPattern)) return LineType.VARIABLE_DECLARATION;

            // variable assignment
            string assignmentPattern = @"^\s*[a-zA-Z0-9]+\s*=\s*.*;$";
            if (Regex.IsMatch(line, assignmentPattern)) return LineType.VARIABLE_ASSIGNMENT;

            // variable arithmetic
            string arithmeticPattern = @"^\s*[a-zA-Z0-9]+\s*(?:[*+]-=|\*=|\+=|-=|\*=)\s*.*;$";
            if (Regex.IsMatch(line, arithmeticPattern)) return LineType.VARIABLE_ARITHMETIC;

            // unknown
            return LineType.UNKNOWN;
        }

        public static ShortLineType GetShortLineType(string line)
        {
            // BUILT-IN OPERATION
            string outBuiltinPattern = @"^\s*out\s+.*;$";
            if (Regex.IsMatch(line, outBuiltinPattern)) return ShortLineType.BUILTIN_OPERATION;

            string exitBuiltinPattern = @"^\s*exit;\s*$";
            if (Regex.IsMatch(line, exitBuiltinPattern)) return ShortLineType.BUILTIN_OPERATION;

            // VARIABLE DECLARATION
            string declarationPattern = @"^\s*(str|int|dec|bool|rex)(?!\s*\(\))(?:\[\]|\{[a-z;]+\})?\s*[a-zA-Z0-9]+\s*=\s*.*;$";
            if (Regex.IsMatch(line, declarationPattern)) return ShortLineType.VARIABLE_DECLARATION;

            // VARIABLE ASSIGNMENT
            string assignmentPattern = @"^\s*[a-zA-Z0-9]+\s*=\s*.*;$";
            if (Regex.IsMatch(line, assignmentPattern)) return ShortLineType.VARIABLE_ASSIGNMENT;

            // VARIABLE ARITHMETIC
            string arithmeticPattern = @"^\s*[a-zA-Z0-9]+\s*(?:[*+]-=|\*=|\+=|-=|\*=)\s*.*;$";
            if (Regex.IsMatch(line, arithmeticPattern)) return ShortLineType.VARIABLE_ARITHMETIC;

            // BLOCK INDICATOR
            string blockIndicatorPattern = @"^\s*\[BLOCK\s+[0-99999999999]+\]\s*$";
            if (Regex.IsMatch(line, blockIndicatorPattern)) return ShortLineType.BLOCK_INDICATOR;

            return ShortLineType.UNKNOWN;
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

        public static bool EvaluateCondition(GXCodeEnvironment env, string condition)
        {
            condition = condition.Trim();

            // match comparisons: left <op> right
            var cmp = Regex.Match(condition, "^(.*?)(==|!=|<=|>=|<|>)(.*)$");
            if (!cmp.Success)
            {
                // single token: treat as bool variable or literal
                string token = condition;
                if (bool.TryParse(token, out var bv)) return bv;
                if (GXCodeProgram.scopeStack.Peek().TryGet(token, out var val, out var type) && type == "bool")
                {
                    return val is bool b && b;
                }
                throw new GXCodeInterpreterError($"Could not evaluate condition: {condition}");
            }

            string left = cmp.Groups[1].Value.Trim();
            string op = cmp.Groups[2].Value;
            string right = cmp.Groups[3].Value.Trim();

            object Resolve(string token)
            {
                if (token.StartsWith("\"") && token.EndsWith("\"")) return token.Substring(1, token.Length - 2);
                if (bool.TryParse(token, out var b)) return b;
                if (int.TryParse(token, out var i)) return i;
                if (decimal.TryParse(token, out var d)) return d;
                // variable lookup
                if (GXCodeProgram.scopeStack.Peek().TryGet(token, out var v, out var t))
                {
                    if (v is null)
                    {
                        throw new GXCodeInterpreterError($"Variable {token} is null");
                    }
                    return v;
                };
                throw new GXCodeInterpreterError($"Unknown identifier in condition: {token}");
            }

            var lval = Resolve(left);
            var rval = Resolve(right);

            // numeric comparison if both numbers
            bool bothNumeric = (lval is int || lval is decimal) && (rval is int || rval is decimal);
            if (bothNumeric)
            {
                decimal ln = Convert.ToDecimal(lval);
                decimal rn = Convert.ToDecimal(rval);
                return op switch
                {
                    "==" => ln == rn,
                    "!=" => ln != rn,
                    "<" => ln < rn,
                    ">" => ln > rn,
                    "<=" => ln <= rn,
                    ">=" => ln >= rn,
                    _ => throw new GXCodeInterpreterError($"Unsupported operator {op}")
                };
            }

            // boolean comparison
            if (lval is bool lb && rval is bool rb)
            {
                return op switch
                {
                    "==" => lb == rb,
                    "!=" => lb != rb,
                    _ => throw new GXCodeInterpreterError($"Unsupported boolean operator {op}")
                };
            }

            // fallback to string comparison
            string ls = lval?.ToString() ?? "";
            string rs = rval?.ToString() ?? "";
            return op switch
            {
                "==" => string.Equals(ls, rs, StringComparison.Ordinal),
                "!=" => !string.Equals(ls, rs, StringComparison.Ordinal),
                _ => throw new GXCodeInterpreterError($"Unsupported operator {op} for string operands")
            };
        }

        public static void ExecuteBlock(GXCodeEnvironment env, GXC_CS_ELEMENT block)
        {
            GXCodeProgram.scopeStack.Push(new Scope(GXCodeProgram.scopeStack.Peek()));

            if (block is GXC_CS_IF ifBlock)
            {
                GXCodeHelper.Debug($"Evaluating IF condition: {ifBlock.Condition}");
                bool isTrue = EvaluateCondition(env, ifBlock.Condition);
                if (!isTrue)
                {
                    GXCodeHelper.Debug("Condition is false, skipping IF block");
                    GXCodeProgram.scopeStack.Pop();
                    return;
                }
            }
            else if (block is GXC_CS_ELSE_IF elseIfBlock)
            {
                GXCodeHelper.Debug($"Evaluating ELSE IF condition: {elseIfBlock.Condition}");
                bool isTrue = EvaluateCondition(env, elseIfBlock.Condition);
                if (!isTrue)
                {
                    GXCodeHelper.Debug("Condition is false, skipping ELSE IF block");
                    GXCodeProgram.scopeStack.Pop();
                    return;
                }
            }
            else if (block is GXC_CS_SWITCH switchBlock)
            {
                if (!GXCodeProgram.scopeStack.Peek().TryGet(switchBlock.Variable, out var switchVal, out var switchType))
                {
                    throw new GXCodeInterpreterError($"Unknown variable {switchBlock.Variable} in switch statement");
                }

                bool caseMatched = false;
                foreach (var line in block.Lines)
                {
                    if (line.StartsWith("[BLOCK "))
                    {
                        int caseId = int.Parse(Regex.Match(line, @"^\s*\[BLOCK\s+([0-99999999999]+)\]\s*$").Groups[1].Value);
                        if (env.blocks[caseId] is GXC_CS_CASE caseBlock)
                        {
                            string caseValue = caseBlock.Value.Trim();
                            if ((caseValue.StartsWith("\"") && caseValue.EndsWith("\"") && caseValue.Substring(1, caseValue.Length - 2) == switchVal?.ToString()) ||
                                caseValue == switchVal?.ToString())
                            {
                                GXCodeHelper.Debug($"Switch case matched: {caseValue}");
                                ExecuteBlock(env, caseBlock);
                                caseMatched = true;
                                return;
                            }
                        }
                    }
                }

                if (!caseMatched)
                {
                    GXCodeHelper.Debug("No matching switch case found, skipping switch block");
                    GXCodeProgram.scopeStack.Pop();
                    return;
                }
            }
            else if (block is GXC_CS_REPEAT repeatBlock)
            {
                // repeatBlock.Variable can be an integer literal or a variable name
                string token = repeatBlock.Variable?.Trim() ?? "";

                if (!int.TryParse(token, out int iterations))
                {
                    if (!GXCodeProgram.scopeStack.Peek().TryGet(token, out var repeatVal, out var repeatType))
                    {
                        throw new GXCodeInterpreterError($"Unknown variable {token} in repeat statement");
                    }
                    if (repeatType != "int")
                    {
                        throw new GXCodeInterpreterError($"Repeat variable {token} must be of type int");
                    }
                    if (repeatVal is null)
                    {
                        throw new GXCodeInterpreterError($"Variable {token} is null");
                    }
                    iterations = (int)repeatVal;
                }

                for (int i = 0; i < iterations; i++)
                {
                    GXCodeHelper.Debug($"Repeat iteration {i + 1} of {iterations}");
                    // create an iteration-local scope
                    GXCodeProgram.scopeStack.Push(new Scope(GXCodeProgram.scopeStack.Peek()));
                    ExecuteBlockBody(env, repeatBlock);
                    GXCodeProgram.scopeStack.Pop();
                }
                GXCodeProgram.scopeStack.Pop();
                return;
            }
            else if (block is GXC_CS_ITERATE iterateBlock)
            {
                if (!GXCodeProgram.scopeStack.Peek().TryGet(iterateBlock.Variable, out var iterateVal, out var iterateType))
                {
                    throw new GXCodeInterpreterError($"Unknown variable {iterateBlock.Variable} in iterate statement");
                }
                if (iterateType != "str[]" && iterateType != "int[]" && iterateType != "dec[]" && iterateType != "bool[]")
                {
                    throw new GXCodeInterpreterError($"Iterate variable {iterateBlock.Variable} must be an array");
                }

                IEnumerable<object> collection = iterateVal switch
                {
                    List<string> sList => sList.Cast<object>(),
                    List<int> iList => iList.Cast<object>(),
                    List<decimal> dList => dList.Cast<object>(),
                    List<bool> bList => bList.Cast<object>(),
                    _ => throw new GXCodeInterpreterError($"Unsupported iterate variable type {iterateType}")
                };

                foreach (var item in collection)
                {
                    GXCodeHelper.Debug($"Iterating item: {item}");
                    GXCodeProgram.scopeStack.Push(new Scope(GXCodeProgram.scopeStack.Peek()));
                    GXCodeProgram.scopeStack.Peek().Set("element", item, iterateType.Substring(0, iterateType.Length - 2));
                    ExecuteBlockBody(env, iterateBlock);
                    GXCodeProgram.scopeStack.Pop();
                }
                return;
            }
            else if (block is GXC_CS_WHILE whileBlock)
            {
                while (EvaluateCondition(env, whileBlock.Condition))
                {
                    GXCodeHelper.Debug("While condition is true, executing block");
                    GXCodeProgram.scopeStack.Push(new Scope(GXCodeProgram.scopeStack.Peek()));
                    ExecuteBlockBody(env, whileBlock);
                    GXCodeProgram.scopeStack.Pop();
                }
                GXCodeHelper.Debug("While condition is false, exiting block");
                GXCodeProgram.scopeStack.Pop();
                return;
            }

            ExecuteBlockBody(env, block);
            GXCodeProgram.scopeStack.Pop();
        }

        // Execute the lines inside a block (helper extracted to avoid accidental recursion)
        public static void ExecuteBlockBody(GXCodeEnvironment env, GXC_CS_ELEMENT block)
        {
            for (int i = 0; i < block.Lines.Count; i++)
            {
                string line = block.Lines[i];
                ShortLineType type = GetShortLineType(line);
                GXCodeHelper.Debug($"Line {i + 1} of {block.GetType().Name}#{block.ID}: {line} (type: {type})");

                switch (type)
                {
                    case ShortLineType.UNKNOWN:
                        throw new GXCodeInterpreterError($"Undetected indeterminable line structure of {line}");
                    case ShortLineType.BUILTIN_OPERATION:
                        ExecuteBuiltinOperation(line);
                        break;
                    case ShortLineType.VARIABLE_DECLARATION:
                        DeclareVariable(line, i+1, $"{block.GetType().Name}#{block.ID}");
                        break;
                    case ShortLineType.VARIABLE_ASSIGNMENT:
                        AssignVariable(line, i+1, $"{block.GetType().Name}#{block.ID}");
                        break;
                    case ShortLineType.VARIABLE_ARITHMETIC:
                        PerformVariableArithmetic(line, i+1, $"{block.GetType().Name}#{block.ID}");
                        break;
                    case ShortLineType.BLOCK_INDICATOR:
                        int nestedId = int.Parse(Regex.Match(line, @"^\s*\[BLOCK\s+([0-99999999999]+)\]\s*$").Groups[1].Value);
                        ExecuteBlock(env, env.blocks[nestedId]);
                        break;
                }
            }
        }

        public static void ExecuteBuiltinOperation(string line)
        {
            // out
            string outPattern = @"^\s*out\s+(.*);$";
            Match outMatch = Regex.Match(line, outPattern);

            if (outMatch.Success)
            {
                string output = outMatch.Groups[1].Value;

                if (GXCodeProgram.scopeStack.Peek().TryGet(output, out object? variableValue, out var type))
                {
                    Console.WriteLine(variableValue);
                }
                else
                {
                    Console.WriteLine(output.Trim('"'));
                }
                return;
            }

            // exit
            string exitPattern = @"^\s*exit;\s*$";
            if (Regex.IsMatch(line, exitPattern)) throw new GXCodeBreak();

            throw new GXCodeInterpreterError("Could not detect built-in operation");
        }

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

            if (!GXCodeProgram.scopeStack.Peek().TryGet(name, out _, out var type))
            {
                throw new GXCUndeclaredVariableError(lineNr, name, block);
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

                // assign from another variable of same type
                if (GXCodeProgram.scopeStack.Peek().TryGet(value, out var varVal, out var varType) && varType == type)
                {
                    if (varVal is not List<string> && varVal is not List<int> && varVal is not List<decimal> && varVal is not List<bool> && varVal is not List<Regex>)
                    {
                        throw new GXCWrongTypeError(lineNr, value, type, block);
                    }
                    typedValue = varVal;
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

                if (GXCodeProgram.scopeStack.Peek().TryGet(value, out var varVal, out var varType) && varType == type)
                {
                    if (varVal is not Dictionary<string, string>)
                    {
                        throw new GXCWrongTypeError(lineNr, value, type, block);
                    }
                    typedValue = varVal;
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
                        if (value.StartsWith('"') && value.EndsWith('"'))
                        {
                            typedValue = value.Trim('"');
                        }
                        else if (GXCodeProgram.scopeStack.Peek().TryGet(value, out object? varValue, out var varType) && varType == "str")
                        {
                            if (varValue is not string)
                            {
                                throw new GXCWrongTypeError(lineNr, value, "str", block);
                            }
                            typedValue = varValue;
                        }
                        else
                        {
                            throw new GXCWrongTypeError(lineNr, value, "str", block);
                        }
                        break;
                    case "int":
                        if (int.TryParse(value, out int intValue))
                        {
                            typedValue = intValue;
                        }
                        else if (GXCodeProgram.scopeStack.Peek().TryGet(value, out object? varValue, out string? varType) && varType == "int")
                        {
                            if (varValue is not int)
                            {
                                throw new GXCWrongTypeError(lineNr, value, "int", block);
                            }
                            typedValue = varValue;
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
                        }
                        else if (GXCodeProgram.scopeStack.Peek().TryGet(value, out object? varValue, out string? varType) && varType == "dec")
                        {
                            if (varValue is not decimal)
                            {
                                throw new GXCWrongTypeError(lineNr, value, "dec", block);
                            }
                            typedValue = varValue;
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
                        }
                        else if (GXCodeProgram.scopeStack.Peek().TryGet(value, out object? varValue, out string? varType) && varType == "bool")
                        {
                            if (varValue is not bool)
                            {
                                throw new GXCWrongTypeError(lineNr, value, "bool", block);
                            }
                            typedValue = varValue;
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
                        }
                        catch
                        {
                            if (GXCodeProgram.scopeStack.Peek().TryGet(value, out object? varValue, out string? varType) && varType == "rex")
                            {
                                if (varValue is not Regex)
                                {
                                    throw new GXCWrongTypeError(lineNr, value, "rex", block);
                                }
                                typedValue = varValue;
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

        public static void PerformVariableArithmetic(string line, int lineNr, string block)
        {
            string pattern = @"^\s*([a-zA-Z0-9]+)\s*([*+\-]=|-=|\*=)\s*(.*);$";
            Match match = Regex.Match(line, pattern);

            if (!match.Success)
            {
                throw new GXCodeInterpreterError("Could not detect variable arithmetic operation");
            }

            string name = match.Groups[1].Value;
            string op = match.Groups[2].Value;
            string value = match.Groups[3].Value.Trim();

            if (!GXCodeProgram.scopeStack.Peek().TryGet(name, out var currentVal, out var type))
            {
                throw new GXCUndeclaredVariableError(lineNr, name, block);
            }

            if (type is null)
            {
                throw new GXCUndeclaredVariableError(lineNr, name, block);
            }
            // Only int and dec supported
            if (type == "int")
            {
                if (currentVal is not int currInt)
                    throw new GXCWrongTypeError(lineNr, name, "int", block);

                int operand;
                if (int.TryParse(value, out var litInt))
                {
                    operand = litInt;
                }
                else if (GXCodeProgram.scopeStack.Peek().TryGet(value, out var varVal, out var varType) && varType == "int")
                {
                    if (varVal is not int) throw new GXCWrongTypeError(lineNr, value, "int", block);
                    operand = (int)varVal;
                }
                else
                {
                    throw new GXCWrongTypeError(lineNr, value, "int", block);
                }

                int result = op switch
                {
                    "+=" => currInt + operand,
                    "-=" => currInt - operand,
                    "*=" => currInt * operand,
                    _ => throw new GXCodeInterpreterError("Unknown arithmetic operator")
                };

                GXCodeProgram.scopeStack.Peek().Set(name, result, "int");
                return;
            }

            if (type == "dec")
            {
                decimal currDec;
                if (currentVal is decimal d) currDec = d;
                else if (currentVal is int i) currDec = Convert.ToDecimal(i);
                else throw new GXCWrongTypeError(lineNr, name, "dec", block);

                decimal operand;
                if (decimal.TryParse(value, out var litDec))
                {
                    operand = litDec;
                }
                else if (GXCodeProgram.scopeStack.Peek().TryGet(value, out var varVal, out var varType))
                {
                    if (varType == "dec")
                    {
                        if (varVal is not decimal) throw new GXCWrongTypeError(lineNr, value, "dec", block);
                        operand = (decimal)varVal;
                    }
                    else if (varType == "int")
                    {
                        if (varVal is not int) throw new GXCWrongTypeError(lineNr, value, "int", block);
                        operand = Convert.ToDecimal((int)varVal);
                    }
                    else
                    {
                        throw new GXCWrongTypeError(lineNr, value, "dec", block);
                    }
                }
                else
                {
                    throw new GXCWrongTypeError(lineNr, value, "dec", block);
                }

                decimal result = op switch
                {
                    "+=" => currDec + operand,
                    "-=" => currDec - operand,
                    "*=" => currDec * operand,
                    _ => throw new GXCodeInterpreterError("Unknown arithmetic operator")
                };

                GXCodeProgram.scopeStack.Peek().Set(name, result, "dec");
                return;
            }

            // unsupported type for arithmetic
            throw new GXCWrongArithmeticError(lineNr, block);
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