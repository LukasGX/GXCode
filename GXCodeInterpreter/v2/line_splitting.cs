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

    public static string GetClassName(string line)
    {
        string pattern = @"^(?:\s*([a-z]+)\s+)?class\s+([a-zA-Z0-9_]+)\s+\{$";
        Match match = Regex.Match(line, pattern);

        if (match.Success)
        {
            string name = match.Groups[2].Value;
            return name;
        }
        else
        {
            throw new GXCodeInterpreterError("Could not detect class name");
        }
    }

    public static string GetClassModifier(string line)
    {
        string pattern = @"^(?:\s*([a-z]+)\s+)?class\s+([a-zA-Z0-9_]+)\s+\{$";
        Match match = Regex.Match(line, pattern);

        if (match.Success)
        {
            string modifier = match.Groups[1].Value;
            return modifier;
        }
        else
        {
            throw new GXCodeInterpreterError("Could not detect class modifier");
        }
    }

    public static string GetMethodName(string line)
    {
        string pattern = @"^\s*(?:([a-z]+)\s+)?method\s+([a-zA-Z0-9_]+)\s*\((.*?)\)\s*\{$";
        Match match = Regex.Match(line, pattern);

        if (match.Success)
        {
            string name = match.Groups[2].Value;
            return name;
        }
        else
        {
            throw new GXCodeInterpreterError("Could not detect method name");
        }
    }

    public static string GetMethodModifier(string line)
    {
        string pattern = @"^\s*(?:([a-z]+)\s+)?method\s+([a-zA-Z0-9_]+)\s*\((.*?)\)\s*\{$";
        Match match = Regex.Match(line, pattern);

        if (match.Success)
        {
            string name = match.Groups[1].Value;
            return name;
        }
        else
        {
            throw new GXCodeInterpreterError("Could not detect method modifier");
        }
    }

    public static string GetMethodParameters(string line)
    {
        string pattern = @"^\s*(?:([a-z]+)\s+)?method\s+([a-zA-Z0-9_]+)\s*\((.*?)\)\s*\{$";
        Match match = Regex.Match(line, pattern);

        if (match.Success)
        {
            string name = match.Groups[3].Value;
            return name;
        }
        else
        {
            throw new GXCodeInterpreterError("Could not detect method parameters");
        }
    }

    public static string GetReturnMethodName(string line)
    {
        string pattern = @"^\s*(?:([a-z]+)\s+)?(str|int|dec|bool|rex)\s+([a-zA-Z0-9_]+)\s*\((.*?)\)\s*\{$";
        Match match = Regex.Match(line, pattern);

        if (match.Success)
        {
            string name = match.Groups[3].Value;
            return name;
        }
        else
        {
            throw new GXCodeInterpreterError("Could not detect return method name");
        }
    }

    public static string GetReturnMethodModifier(string line)
    {
        string pattern = @"^\s*(?:([a-z]+)\s+)?(str|int|dec|bool|rex)\s+([a-zA-Z0-9_]+)\s*\((.*?)\)\s*\{$";
        Match match = Regex.Match(line, pattern);

        if (match.Success)
        {
            string name = match.Groups[1].Value;
            return name;
        }
        else
        {
            throw new GXCodeInterpreterError("Could not detect return method modifier");
        }
    }

    public static string GetReturnMethodParameters(string line)
    {
        string pattern = @"^\s*(?:([a-z]+)\s+)?(str|int|dec|bool|rex)\s+([a-zA-Z0-9_]+)\s*\((.*?)\)\s*\{$";
        Match match = Regex.Match(line, pattern);

        if (match.Success)
        {
            string name = match.Groups[4].Value;
            return name;
        }
        else
        {
            throw new GXCodeInterpreterError("Could not detect return method parameters");
        }
    }

    public static string GetReturnMethodReturnType(string line)
    {
        string pattern = @"^\s*(?:([a-z]+)\s+)?(str|int|dec|bool|rex)\s+([a-zA-Z0-9_]+)\s*\((.*?)\)\s*\{$";
        Match match = Regex.Match(line, pattern);

        if (match.Success)
        {
            string name = match.Groups[2].Value;
            return name;
        }
        else
        {
            throw new GXCodeInterpreterError("Could not detect return method return type");
        }
    }
}