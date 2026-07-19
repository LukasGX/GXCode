using Xunit;
using GXCodeInterpreter;

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
}