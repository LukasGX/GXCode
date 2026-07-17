using Xunit;
using GXCodeInterpreter;

namespace GXCode.Tests;

public class LineRecognitionTests
{
    [Fact]
    public void MultilineCommentIndicator()
    {
        string line = "///";
        LineType result = GXCodeInterpreter.GXCodeInterpreter.GetLineType(line, false);

        Assert.Equal(LineType.MULTILINE_COMMENT_INDICATOR, result);
    }

    [Fact]
    public void InMultilineComment()
    {
        string line = "something";
        LineType result = GXCodeInterpreter.GXCodeInterpreter.GetLineType(line, true);

        Assert.Equal(LineType.COMMENT, result);
    }

    [Fact]
    public void Negligible()
    {
        string line = "      ";
        LineType result = GXCodeInterpreter.GXCodeInterpreter.GetLineType(line, false);

        Assert.Equal(LineType.NEGLIGIBLE, result);
    }

    [Fact]
    public void Comment()
    {
        string line = "// test";
        LineType result = GXCodeInterpreter.GXCodeInterpreter.GetLineType(line, false);

        Assert.Equal(LineType.COMMENT, result);
    }

    [Fact]
    public void NsDefinition()
    {
        string line = "#ns abc";
        LineType result = GXCodeInterpreter.GXCodeInterpreter.GetLineType(line, false);

        Assert.Equal(LineType.NAMESPACE_DEFINITION, result);
    }

    [Fact]
    public void MethodDefinitionStart()
    {
        string line = "method test() {";
        LineType result = GXCodeInterpreter.GXCodeInterpreter.GetLineType(line, false);

        Assert.Equal(LineType.METHOD_DEFINITION_START, result);
    }

    [Fact]
    public void ReturnMethodDefinitionStart()
    {
        string line = "str test() {";
        LineType result = GXCodeInterpreter.GXCodeInterpreter.GetLineType(line, false);

        Assert.Equal(LineType.RETURN_METHOD_DEFINITION_START, result);
    }

    [Fact]
    public void ClassDefinitionStart()
    {
        string line = "class abc {";
        LineType result = GXCodeInterpreter.GXCodeInterpreter.GetLineType(line, false);

        Assert.Equal(LineType.CLASS_DEFINITION_START, result);
    }

    [Fact]
    public void PrivateClassDefinitionStart()
    {
        string line = "private class abc {";
        LineType result = GXCodeInterpreter.GXCodeInterpreter.GetLineType(line, false);

        Assert.Equal(LineType.CLASS_DEFINITION_START, result);
    }

    [Fact]
    public void InitDefinitionStart()
    {
        string line = "init() {";
        LineType result = GXCodeInterpreter.GXCodeInterpreter.GetLineType(line, false);

        Assert.Equal(LineType.INIT_DEFINITION_START, result);
    }

    [Fact]
    public void EntrypointDefinitionStart()
    {
        string line = "entrypoint() {";
        LineType result = GXCodeInterpreter.GXCodeInterpreter.GetLineType(line, false);

        Assert.Equal(LineType.ENTRYPOINT_DEFINITION_START, result);
    }

    [Fact]
    public void IfStart()
    {
        string line = "if (checked) {";
        LineType result = GXCodeInterpreter.GXCodeInterpreter.GetLineType(line, false);

        Assert.Equal(LineType.IF_START, result);
    }

    [Fact]
    public void ElseIfStart()
    {
        string line = "else if (checked) {";
        LineType result = GXCodeInterpreter.GXCodeInterpreter.GetLineType(line, false);

        Assert.Equal(LineType.ELSE_IF_START, result);
    }

    [Fact]
    public void ElseStart()
    {
        string line = "else {";
        LineType result = GXCodeInterpreter.GXCodeInterpreter.GetLineType(line, false);

        Assert.Equal(LineType.ELSE_START, result);
    }

    [Fact]
    public void SwitchStart()
    {
        string line = "switch (answer) {";
        LineType result = GXCodeInterpreter.GXCodeInterpreter.GetLineType(line, false);

        Assert.Equal(LineType.SWITCH_START, result);
    }

    [Fact]
    public void CaseStart()
    {
        string line = "case 'Hi' {";
        LineType result = GXCodeInterpreter.GXCodeInterpreter.GetLineType(line, false);

        Assert.Equal(LineType.CASE_START, result);
    }

    [Fact]
    public void RepeatStart()
    {
        string line = "repeat (4) {";
        LineType result = GXCodeInterpreter.GXCodeInterpreter.GetLineType(line, false);

        Assert.Equal(LineType.REPEAT_START, result);
    }

    [Fact]
    public void IterateStart()
    {
        string line = "iterate (array) {";
        LineType result = GXCodeInterpreter.GXCodeInterpreter.GetLineType(line, false);

        Assert.Equal(LineType.ITERATE_START, result);
    }

    [Fact]
    public void WhileStart()
    {
        string line = "while (checked) {";
        LineType result = GXCodeInterpreter.GXCodeInterpreter.GetLineType(line, false);

        Assert.Equal(LineType.WHILE_START, result);
    }

    [Fact]
    public void Closing()
    {
        string line = "}";
        LineType result = GXCodeInterpreter.GXCodeInterpreter.GetLineType(line, false);

        Assert.Equal(LineType.CLOSING, result);
    }

    [Fact]
    public void BuiltInOperation()
    {
        // out
        string outLine = "out 'Hi';";
        LineType outResult = GXCodeInterpreter.GXCodeInterpreter.GetLineType(outLine, false);

        Assert.Equal(LineType.BUILTIN_OPERATION, outResult);

        // shout
        string shoutLine = "shout 'Hi';";
        LineType shoutResult = GXCodeInterpreter.GXCodeInterpreter.GetLineType(shoutLine, false);

        Assert.Equal(LineType.BUILTIN_OPERATION, shoutResult);

        // exit
        string exitLine = "shout 'Hi';";
        LineType exitResult = GXCodeInterpreter.GXCodeInterpreter.GetLineType(exitLine, false);

        Assert.Equal(LineType.BUILTIN_OPERATION, exitResult);

        // return
        string returnLine = "return 0;";
        LineType returnResult = GXCodeInterpreter.GXCodeInterpreter.GetLineType(returnLine, false);
    }

    [Fact]
    public void InstanceDeclaration()
    {
        string line = "inst<Car> car = new();";
        LineType result = GXCodeInterpreter.GXCodeInterpreter.GetLineType(line, false);

        Assert.Equal(LineType.INSTANCE_DECLARATION, result);
    }

    [Fact]
    public void ConstStrDeclaration()
    {
        string line = "const str message = \"Hello World\";";
        LineType result = GXCodeInterpreter.GXCodeInterpreter.GetLineType(line, false);

        Assert.Equal(LineType.CONST_STR_DECLARATION, result);
    }

    [Fact]
    public void ConstIntDeclaration()
    {
        string line = "const int number = 42;";
        LineType result = GXCodeInterpreter.GXCodeInterpreter.GetLineType(line, false);

        Assert.Equal(LineType.CONST_INT_DECLARATION, result);
    }

    [Fact]
    public void ConstDecDeclaration()
    {
        string line = "const dec number = 3.14;";
        LineType result = GXCodeInterpreter.GXCodeInterpreter.GetLineType(line, false);

        Assert.Equal(LineType.CONST_DEC_DECLARATION, result);
    }

    [Fact]
    public void ConstBoolDeclaration()
    {
        string line = "const bool flag = true;";
        LineType result = GXCodeInterpreter.GXCodeInterpreter.GetLineType(line, false);

        Assert.Equal(LineType.CONST_BOOL_DECLARATION, result);
    }

    [Fact]
    public void ConstRexDeclaration()
    {
        string line = "const rex pattern = /\\d+/;";
        LineType result = GXCodeInterpreter.GXCodeInterpreter.GetLineType(line, false);

        Assert.Equal(LineType.CONST_REX_DECLARATION, result);
    }

    [Fact]
    public void StrDeclaration()
    {
        string line = "str message = \"Hello World\";";
        LineType result = GXCodeInterpreter.GXCodeInterpreter.GetLineType(line, false);

        Assert.Equal(LineType.STR_DECLARATION, result);
    }

    [Fact]
    public void IntDeclaration()
    {
        string line = "int number = 42;";
        LineType result = GXCodeInterpreter.GXCodeInterpreter.GetLineType(line, false);

        Assert.Equal(LineType.INT_DECLARATION, result);
    }

    [Fact]
    public void DecDeclaration()
    {
        string line = "dec number = 3.14;";
        LineType result = GXCodeInterpreter.GXCodeInterpreter.GetLineType(line, false);

        Assert.Equal(LineType.DEC_DECLARATION, result);
    }

    [Fact]
    public void BoolDeclaration()
    {
        string line = "bool flag = true;";
        LineType result = GXCodeInterpreter.GXCodeInterpreter.GetLineType(line, false);

        Assert.Equal(LineType.BOOL_DECLARATION, result);
    }

    [Fact]
    public void RexDeclaration()
    {
        string line = "rex pattern = /\\d+/;";
        LineType result = GXCodeInterpreter.GXCodeInterpreter.GetLineType(line, false);

        Assert.Equal(LineType.REX_DECLARATION, result);
    }

    [Fact]
    public void ConstArrayDeclaration()
    {
        string line = "const str[] array = [];";
        LineType result = GXCodeInterpreter.GXCodeInterpreter.GetLineType(line, false);

        Assert.Equal(LineType.CONST_ARRAY_DECLARATION, result);
    }

    [Fact]
    public void ArrayDeclaration()
    {
        string line = "str[] array = [];";
        LineType result = GXCodeInterpreter.GXCodeInterpreter.GetLineType(line, false);

        Assert.Equal(LineType.ARRAY_DECLARATION, result);
    }

    [Fact]
    public void ConstDictDeclaration()
    {
        string line = "const str{int} dict = {};";
        LineType result = GXCodeInterpreter.GXCodeInterpreter.GetLineType(line, false);

        Assert.Equal(LineType.CONST_DICT_DECLARATION, result);
    }

    [Fact]
    public void DictDeclaration()
    {
        string line = "str{int} dict = {};";
        LineType result = GXCodeInterpreter.GXCodeInterpreter.GetLineType(line, false);

        Assert.Equal(LineType.DICT_DECLARATION, result);
    }

    [Fact]
    public void VariableAssignment()
    {
        string line = "text = \"Lorem ipsum...\";";
        LineType result = GXCodeInterpreter.GXCodeInterpreter.GetLineType(line, false);

        Assert.Equal(LineType.VARIABLE_ASSIGNMENT, result);
    }

    [Fact]
    public void VariableArithmetic()
    {
        string line = "number += 3;";
        LineType result = GXCodeInterpreter.GXCodeInterpreter.GetLineType(line, false);

        Assert.Equal(LineType.VARIABLE_ARITHMETIC, result);
    }

    [Fact]
    public void Unknown()
    {
        string line = "Hello World;";
        LineType result = GXCodeInterpreter.GXCodeInterpreter.GetLineType(line, false);

        Assert.Equal(LineType.UNKNOWN, result);
    }
}