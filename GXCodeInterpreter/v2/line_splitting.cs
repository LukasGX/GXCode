using System.Text.RegularExpressions;
namespace GXCodeInterpreter;

partial class GXCodeInterpreter
{
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