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

                // main loop
                foreach (string line in env.Lines)
                {
                    LineType type = GXCodeInterpreter.GetLineType(line);
                    if (type == LineType.UNKNOWN) throw new GXCIndeterminableLineError(line);
                    if (type == LineType.COMMENT || type == LineType.NEGLIGIBLE) continue;
                }
            }
            catch (GXCodeError e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Error {e.Id}: {e.Message}");
                Console.ResetColor();
            }
        }
    }

    class GXCodeEnvironment(string code, List<string> lines)
    {
        public string Code { get; set; } = code;
        public List<string> Lines { get; set; } = lines;
        public string Namespace { get; set; } = "";
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
    }
}