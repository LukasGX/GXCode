using Xunit;
using GXCodeInterpreter;

namespace GXCode.Tests;

public class LineSplittingTests
{
    [Fact]
    public void NS_NS()
    {
        string line = "#ns abc";
        string result = GXCodeInterpreter.GXCodeInterpreter.GetNS(line);

        Assert.Equal("abc", result);
    }

    [Fact]
    public void If_Condition()
    {
        string line = "if (protected) {";
        string result = GXCodeInterpreter.GXCodeInterpreter.GetIfCondition(line);

        Assert.Equal("protected", result);
    }

    [Fact]
    public void ElseIf_Condition()
    {
        string line = "else if (protected) {";
        string result = GXCodeInterpreter.GXCodeInterpreter.GetElseIfCondition(line);

        Assert.Equal("protected", result);
    }

    [Fact]
    public void Switch_Variable()
    {
        string line = "switch (name) {";
        string result = GXCodeInterpreter.GXCodeInterpreter.GetSwitchVariable(line);

        Assert.Equal("name", result);
    }

    [Fact]
    public void Case_Value()
    {
        string line = "case \"John\" {";
        string result = GXCodeInterpreter.GXCodeInterpreter.GetCaseValue(line);

        Assert.Equal("\"John\"", result);
    }

    [Fact]
    public void Repeat_Variable()
    {
        string line = "repeat (10) {";
        string result = GXCodeInterpreter.GXCodeInterpreter.GetRepeatVariable(line);

        Assert.Equal("10", result);
    }

    [Fact]
    public void Iterate_Variable()
    {
        string line = "iterate (array) {";
        string result = GXCodeInterpreter.GXCodeInterpreter.GetIterateVariable(line);

        Assert.Equal("array", result);
    }

    [Fact]
    public void While_Condition()
    {
        string line = "while (true) {";
        string result = GXCodeInterpreter.GXCodeInterpreter.GetWhileCondition(line);

        Assert.Equal("true", result);
    }

    [Fact]
    public void Class_Name()
    {
        string line = "class Car {";
        string result = GXCodeInterpreter.GXCodeInterpreter.GetClassName(line);

        Assert.Equal("Car", result);
    }

    [Fact]
    public void Class_Modifier()
    {
        string line = "class Car {";
        string result = GXCodeInterpreter.GXCodeInterpreter.GetClassModifier(line);

        Assert.Empty(result);
    }

    [Fact]
    public void Private_Class_Modifier()
    {
        string line = "private class Car {";
        string result = GXCodeInterpreter.GXCodeInterpreter.GetClassModifier(line);

        Assert.Equal("private", result);
    }

    [Fact]
    public void Method_Name()
    {
        string line = "method Help() {";
        string result = GXCodeInterpreter.GXCodeInterpreter.GetMethodName(line);

        Assert.Equal("Help", result);
    }

    [Fact]
    public void Method_Modifier()
    {
        string line = "method Help() {";
        string result = GXCodeInterpreter.GXCodeInterpreter.GetMethodModifier(line);

        Assert.Empty(result);
    }

    [Fact]
    public void Private_Method_Modifier()
    {
        string line = "private method Help() {";
        string result = GXCodeInterpreter.GXCodeInterpreter.GetMethodModifier(line);

        Assert.Equal("private", result);
    }

    [Fact]
    public void Method_Parameters()
    {
        string line = "method Help() {";
        string result = GXCodeInterpreter.GXCodeInterpreter.GetMethodParameters(line);

        Assert.Empty(result);
    }

    [Fact]
    public void Method_Parameters_2()
    {
        string line = "method Help(str input) {";
        string result = GXCodeInterpreter.GXCodeInterpreter.GetMethodParameters(line);

        Assert.Equal("str input", result);
    }

    [Fact]
    public void Return_Method_Name()
    {
        string line = "str Help() {";
        string result = GXCodeInterpreter.GXCodeInterpreter.GetReturnMethodName(line);

        Assert.Equal("Help", result);
    }

    [Fact]
    public void Return_Method_Modifier()
    {
        string line = "str Help() {";
        string result = GXCodeInterpreter.GXCodeInterpreter.GetReturnMethodModifier(line);

        Assert.Empty(result);
    }

    [Fact]
    public void Private_Return_Method_Modifier()
    {
        string line = "private str Help() {";
        string result = GXCodeInterpreter.GXCodeInterpreter.GetReturnMethodModifier(line);

        Assert.Equal("private", result);
    }

    [Fact]
    public void Return_Method_Parameters()
    {
        string line = "str Help() {";
        string result = GXCodeInterpreter.GXCodeInterpreter.GetReturnMethodParameters(line);

        Assert.Empty(result);
    }

    [Fact]
    public void Return_Method_Parameters_2()
    {
        string line = "str Help(str input) {";
        string result = GXCodeInterpreter.GXCodeInterpreter.GetReturnMethodParameters(line);

        Assert.Equal("str input", result);
    }
}