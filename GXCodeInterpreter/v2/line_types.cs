using System.Text.RegularExpressions;
namespace GXCodeInterpreter;
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
    STR_DECLARATION,
    CONST_STR_DECLARATION,
    INT_DECLARATION,
    CONST_INT_DECLARATION,
    DEC_DECLARATION,
    CONST_DEC_DECLARATION,
    BOOL_DECLARATION,
    CONST_BOOL_DECLARATION,
    REX_DECLARATION,
    CONST_REX_DECLARATION,
    ARRAY_DECLARATION,
    CONST_ARRAY_DECLARATION,
    DICT_DECLARATION,
    CONST_DICT_DECLARATION,
    VARIABLE_ASSIGNMENT,
    VARIABLE_ARITHMETIC,
    UNKNOWN,
    NEGLIGIBLE
}

public enum ShortLineType
{
    BUILTIN_OPERATION,
    STR_DECLARATION,
    CONST_STR_DECLARATION,
    INT_DECLARATION,
    CONST_INT_DECLARATION,
    DEC_DECLARATION,
    CONST_DEC_DECLARATION,
    BOOL_DECLARATION,
    CONST_BOOL_DECLARATION,
    REX_DECLARATION,
    CONST_REX_DECLARATION,
    ARRAY_DECLARATION,
    CONST_ARRAY_DECLARATION,
    DICT_DECLARATION,
    CONST_DICT_DECLARATION,
    VARIABLE_ASSIGNMENT,
    VARIABLE_ARITHMETIC,
    BLOCK_INDICATOR,
    UNKNOWN
}

partial class GXCodeInterpreter
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
        string methodPattern = @"^\s*(?:([a-z]+)\s+)?method\s+([a-zA-Z0-9_]+)\s*\((.*?)\)\s*\{$";
        string returnPattern = @"^\s*(?:([a-z]+)\s+)?(str|int|dec|bool|rex)(?:\[\]|\{[a-z;]+\})?\s+([a-zA-Z0-9_]+)\s*(?:\((.*?)\))?\s*\{$";
        string classPattern = @"^(?:\s*([a-z]+)\s+)?class\s+([a-zA-Z0-9_]+)\s+\{$";
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

        string shoutBuiltinPattern = @"^\s*shout\s+.*;$";
        if (Regex.IsMatch(line, shoutBuiltinPattern)) return LineType.BUILTIN_OPERATION;

        string exitBuiltinPattern = @"^\s*exit;$";
        if (Regex.IsMatch(line, exitBuiltinPattern)) return LineType.BUILTIN_OPERATION;

        string returnBuiltinPattern = @"^\s*return\s+.*;$";
        if (Regex.IsMatch(line, returnBuiltinPattern)) return LineType.BUILTIN_OPERATION;

        // variable declaration
        // const str
        string constStrDeclarationPattern = @"^\s*const\s+str(?!\s*\(\))\s+[a-zA-Z0-9]+\s*=\s*.*;$";
        if (Regex.IsMatch(line, constStrDeclarationPattern)) return LineType.CONST_STR_DECLARATION;

        // const int
        string constIntDeclarationPattern = @"^\s*const\s+int(?!\s*\(\))\s+[a-zA-Z0-9]+\s*=\s*.*;$";
        if (Regex.IsMatch(line, constIntDeclarationPattern)) return LineType.CONST_INT_DECLARATION;

        // const dec
        string constDecDeclarationPattern = @"^\s*const\s+dec(?!\s*\(\))\s+[a-zA-Z0-9]+\s*=\s*.*;$";
        if (Regex.IsMatch(line, constDecDeclarationPattern)) return LineType.CONST_DEC_DECLARATION;

        // const bool
        string constBoolDeclarationPattern = @"^\s*const\s+bool(?!\s*\(\))\s+[a-zA-Z0-9]+\s*=\s*.*;$";
        if (Regex.IsMatch(line, constBoolDeclarationPattern)) return LineType.CONST_BOOL_DECLARATION;

        // const rex
        string constRexDeclarationPattern = @"^\s*const\s+rex(?!\s*\(\))\s+[a-zA-Z0-9]+\s*=\s*.*;$";
        if (Regex.IsMatch(line, constRexDeclarationPattern)) return LineType.CONST_REX_DECLARATION;

        // str
        string strDeclarationPattern = @"^\s*str(?!\s*\(\))\s+[a-zA-Z0-9]+\s*=\s*.*;$";
        if (Regex.IsMatch(line, strDeclarationPattern)) return LineType.STR_DECLARATION;

        // int
        string intDeclarationPattern = @"^\s*int(?!\s*\(\))\s+[a-zA-Z0-9]+\s*=\s*.*;$";
        if (Regex.IsMatch(line, intDeclarationPattern)) return LineType.INT_DECLARATION;

        // dec
        string decDeclarationPattern = @"^\s*dec(?!\s*\(\))\s+[a-zA-Z0-9]+\s*=\s*.*;$";
        if (Regex.IsMatch(line, decDeclarationPattern)) return LineType.DEC_DECLARATION;

        // bool
        string boolDeclarationPattern = @"^\s*bool(?!\s*\(\))\s+[a-zA-Z0-9]+\s*=\s*.*;$";
        if (Regex.IsMatch(line, boolDeclarationPattern)) return LineType.BOOL_DECLARATION;

        // rex
        string rexDeclarationPattern = @"^\s*rex(?!\s*\(\))\s+[a-zA-Z0-9]+\s*=\s*.*;$";
        if (Regex.IsMatch(line, rexDeclarationPattern)) return LineType.REX_DECLARATION;

        // array
        string arrayDeclarationPattern = @"^\s*(str|int|dec|bool|rex)\s*\[\]\s+[a-zA-Z0-9]+\s*=\s*.*;$";
        if (Regex.IsMatch(line, arrayDeclarationPattern)) return LineType.ARRAY_DECLARATION;

        // const array
        string constArrayDeclarationPattern = @"^\s*const\s+(str|int|dec|bool|rex)\s*\[\]\s+[a-zA-Z0-9]+\s*=\s*.*;$";
        if (Regex.IsMatch(line, constArrayDeclarationPattern)) return LineType.CONST_ARRAY_DECLARATION;

        // dict
        string dictDeclarationPattern = @"^\s*(str|int|dec|bool|rex)\s*\{[a-z;]+\}\s+[a-zA-Z0-9]+\s*=\s*.*;$";
        if (Regex.IsMatch(line, dictDeclarationPattern)) return LineType.DICT_DECLARATION;

        // const dict
        string constDictDeclarationPattern = @"^\s*const\s+(str|int|dec|bool|rex)\s*\{[a-z;]+\}\s+[a-zA-Z0-9]+\s*=\s*.*;$";
        if (Regex.IsMatch(line, constDictDeclarationPattern)) return LineType.CONST_DICT_DECLARATION;

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

        string shoutBuiltinPattern = @"^\s*shout\s+.*;$";
        if (Regex.IsMatch(line, shoutBuiltinPattern)) return ShortLineType.BUILTIN_OPERATION;

        string exitBuiltinPattern = @"^\s*exit;\s*$";
        if (Regex.IsMatch(line, exitBuiltinPattern)) return ShortLineType.BUILTIN_OPERATION;

        // VARIABLE DECLARATION
        // const str
        string constStrDeclarationPattern = @"^\s*const\s+str(?!\s*\(\))\s+[a-zA-Z0-9]+\s*=\s*.*;$";
        if (Regex.IsMatch(line, constStrDeclarationPattern)) return ShortLineType.CONST_STR_DECLARATION;

        // const int
        string constIntDeclarationPattern = @"^\s*const\s+int(?!\s*\(\))\s+[a-zA-Z0-9]+\s*=\s*.*;$";
        if (Regex.IsMatch(line, constIntDeclarationPattern)) return ShortLineType.CONST_INT_DECLARATION;

        // const dec
        string constDecDeclarationPattern = @"^\s*const\s+dec(?!\s*\(\))\s+[a-zA-Z0-9]+\s*=\s*.*;$";
        if (Regex.IsMatch(line, constDecDeclarationPattern)) return ShortLineType.CONST_DEC_DECLARATION;

        // const bool
        string constBoolDeclarationPattern = @"^\s*const\s+bool(?!\s*\(\))\s+[a-zA-Z0-9]+\s*=\s*.*;$";
        if (Regex.IsMatch(line, constBoolDeclarationPattern)) return ShortLineType.CONST_BOOL_DECLARATION;

        // const rex
        string constRexDeclarationPattern = @"^\s*const\s+rex(?!\s*\(\))\s+[a-zA-Z0-9]+\s*=\s*.*;$";
        if (Regex.IsMatch(line, constRexDeclarationPattern)) return ShortLineType.CONST_REX_DECLARATION;

        // str
        string strDeclarationPattern = @"^\s*str(?!\s*\(\))\s+[a-zA-Z0-9]+\s*=\s*.*;$";
        if (Regex.IsMatch(line, strDeclarationPattern)) return ShortLineType.STR_DECLARATION;

        // int
        string intDeclarationPattern = @"^\s*int(?!\s*\(\))\s+[a-zA-Z0-9]+\s*=\s*.*;$";
        if (Regex.IsMatch(line, intDeclarationPattern)) return ShortLineType.INT_DECLARATION;

        // dec
        string decDeclarationPattern = @"^\s*dec(?!\s*\(\))\s+[a-zA-Z0-9]+\s*=\s*.*;$";
        if (Regex.IsMatch(line, decDeclarationPattern)) return ShortLineType.DEC_DECLARATION;

        // bool
        string boolDeclarationPattern = @"^\s*bool(?!\s*\(\))\s+[a-zA-Z0-9]+\s*=\s*.*;$";
        if (Regex.IsMatch(line, boolDeclarationPattern)) return ShortLineType.BOOL_DECLARATION;

        // rex
        string rexDeclarationPattern = @"^\s*rex(?!\s*\(\))\s+[a-zA-Z0-9]+\s*=\s*.*;$";
        if (Regex.IsMatch(line, rexDeclarationPattern)) return ShortLineType.REX_DECLARATION;

        // array
        string arrayDeclarationPattern = @"^\s*(str|int|dec|bool|rex)\s*\[\]\s+[a-zA-Z0-9]+\s*=\s*.*;$";
        if (Regex.IsMatch(line, arrayDeclarationPattern)) return ShortLineType.ARRAY_DECLARATION;

        // const array
        string constArrayDeclarationPattern = @"^\s*const\s+(str|int|dec|bool|rex)\s*\[\]\s+[a-zA-Z0-9]+\s*=\s*.*;$";
        if (Regex.IsMatch(line, constArrayDeclarationPattern)) return ShortLineType.CONST_ARRAY_DECLARATION;

        // dict
        string dictDeclarationPattern = @"^\s*(str|int|dec|bool|rex)\s*\{[a-z;]+\}\s+[a-zA-Z0-9]+\s*=\s*.*;$";
        if (Regex.IsMatch(line, dictDeclarationPattern)) return ShortLineType.DICT_DECLARATION;

        // const dict
        string constDictDeclarationPattern = @"^\s*const\s+(str|int|dec|bool|rex)\s*\{[a-z;]+\}\s+[a-zA-Z0-9]+\s*=\s*.*;$";
        if (Regex.IsMatch(line, constDictDeclarationPattern)) return ShortLineType.CONST_DICT_DECLARATION;

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
}