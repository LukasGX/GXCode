using Xunit;
using GXCodeInterpreter;
using System.Text.RegularExpressions;

namespace GXCode.Tests;

public partial class VariableDeclarationTests
{
    [Fact]
    public void Str()
    {
        string line = "str variable = \"Hi\";";

        GXCodeInterpreter.GXCodeInterpreter.DeclareStr(line, 5, "GXC_CS_ENTRYPOINT#0");

        Scope scope = GXCodeProgram.scopeStack.Peek();
        Assert.Single(scope.Variables);

        Variable variable = scope.Variables["variable"];

        var value = Assert.IsType<string>(variable.Value);
        Assert.Equal("Hi", value);

        Assert.Equal("variable", variable.Name);
        Assert.Equal("str", variable.Type);
        Assert.False(variable.IsConstant);
    }

    [Fact]
    public void Const_Str()
    {
        string line = "const str variable = \"Hi\";";

        GXCodeInterpreter.GXCodeInterpreter.DeclareConstStr(line, 5, "GXC_CS_ENTRYPOINT#0");

        Scope scope = GXCodeProgram.scopeStack.Peek();
        Assert.Single(scope.Variables);

        Variable variable = scope.Variables["variable"];

        var value = Assert.IsType<string>(variable.Value);
        Assert.Equal("Hi", value);

        Assert.Equal("variable", variable.Name);
        Assert.Equal("str", variable.Type);
        Assert.True(variable.IsConstant);
    }

    [Fact]
    public void Int()
    {
        string line = "int variable = 1;";

        GXCodeInterpreter.GXCodeInterpreter.DeclareInt(line, 5, "GXC_CS_ENTRYPOINT#0");

        Scope scope = GXCodeProgram.scopeStack.Peek();
        Assert.Single(scope.Variables);

        Variable variable = scope.Variables["variable"];

        var value = Assert.IsType<int>(variable.Value);
        Assert.Equal(1, value);

        Assert.Equal("variable", variable.Name);
        Assert.Equal("int", variable.Type);
        Assert.False(variable.IsConstant);
    }

    [Fact]
    public void Const_Int()
    {
        string line = "const int variable = 1;";

        GXCodeInterpreter.GXCodeInterpreter.DeclareConstInt(line, 5, "GXC_CS_ENTRYPOINT#0");

        Scope scope = GXCodeProgram.scopeStack.Peek();
        Assert.Single(scope.Variables);

        Variable variable = scope.Variables["variable"];

        var value = Assert.IsType<int>(variable.Value);
        Assert.Equal(1, value);

        Assert.Equal("variable", variable.Name);
        Assert.Equal("int", variable.Type);
        Assert.True(variable.IsConstant);
    }

    [Fact]
    public void Dec()
    {
        string line = "dec variable = 0.5;";

        GXCodeInterpreter.GXCodeInterpreter.DeclareDec(line, 5, "GXC_CS_ENTRYPOINT#0");

        Scope scope = GXCodeProgram.scopeStack.Peek();
        Assert.Single(scope.Variables);

        Variable variable = scope.Variables["variable"];

        var value = Assert.IsType<decimal>(variable.Value);
        Assert.Equal(new decimal(0.5), value);

        Assert.Equal("variable", variable.Name);
        Assert.Equal("dec", variable.Type);
        Assert.False(variable.IsConstant);
    }

    [Fact]
    public void Const_Dec()
    {
        string line = "const dec variable = 0.5;";

        GXCodeInterpreter.GXCodeInterpreter.DeclareConstDec(line, 5, "GXC_CS_ENTRYPOINT#0");

        Scope scope = GXCodeProgram.scopeStack.Peek();
        Assert.Single(scope.Variables);

        Variable variable = scope.Variables["variable"];

        var value = Assert.IsType<decimal>(variable.Value);
        Assert.Equal(new decimal(0.5), value);

        Assert.Equal("variable", variable.Name);
        Assert.Equal("dec", variable.Type);
        Assert.True(variable.IsConstant);
    }

    [Fact]
    public void Bool()
    {
        string line = "bool variable = true;";

        GXCodeInterpreter.GXCodeInterpreter.DeclareBool(line, 5, "GXC_CS_ENTRYPOINT#0");

        Scope scope = GXCodeProgram.scopeStack.Peek();
        Assert.Single(scope.Variables);

        Variable variable = scope.Variables["variable"];

        var value = Assert.IsType<bool>(variable.Value);
        Assert.True(value);

        Assert.Equal("variable", variable.Name);
        Assert.Equal("bool", variable.Type);
        Assert.False(variable.IsConstant);
    }

    [Fact]
    public void Const_Bool()
    {
        string line = "const bool variable = true;";

        GXCodeInterpreter.GXCodeInterpreter.DeclareConstBool(line, 5, "GXC_CS_ENTRYPOINT#0");

        Scope scope = GXCodeProgram.scopeStack.Peek();
        Assert.Single(scope.Variables);

        Variable variable = scope.Variables["variable"];

        var value = Assert.IsType<bool>(variable.Value);
        Assert.True(value);

        Assert.Equal("variable", variable.Name);
        Assert.Equal("bool", variable.Type);
        Assert.True(variable.IsConstant);
    }

    [Fact]
    public void Rex()
    {
        string line = "rex variable = /.*/;";

        GXCodeInterpreter.GXCodeInterpreter.DeclareRex(line, 5, "GXC_CS_ENTRYPOINT#0");

        Scope scope = GXCodeProgram.scopeStack.Peek();
        Assert.Single(scope.Variables);

        Variable variable = scope.Variables["variable"];

        var value = Assert.IsType<Regex>(variable.Value);
        Assert.Equal("/.*/", value.ToString());

        Assert.Equal("variable", variable.Name);
        Assert.Equal("rex", variable.Type);
        Assert.False(variable.IsConstant);
    }

    [Fact]
    public void Const_Rex()
    {
        string line = "const rex variable = /.*/;";

        GXCodeInterpreter.GXCodeInterpreter.DeclareConstRex(line, 5, "GXC_CS_ENTRYPOINT#0");

        Scope scope = GXCodeProgram.scopeStack.Peek();
        Assert.Single(scope.Variables);

        Variable variable = scope.Variables["variable"];

        var value = Assert.IsType<Regex>(variable.Value);
        Assert.Equal("/.*/", value.ToString());

        Assert.Equal("variable", variable.Name);
        Assert.Equal("rex", variable.Type);
        Assert.True(variable.IsConstant);
    }
}