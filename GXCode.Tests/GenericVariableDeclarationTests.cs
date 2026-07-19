using Xunit;
using GXCodeInterpreter;

namespace GXCode.Tests;

public partial class VariableDeclarationTests
{
    public VariableDeclarationTests()
    {
        GXCodeProgram.ResetScopeStack();
    }

    [Fact]
    public void Array()
    {
        string line = "str[] array = [];";

        GXCodeInterpreter.GXCodeInterpreter.DeclareArray(line, 5, "GXC_CS_ENTRYPOINT#0");

        Scope scope = GXCodeProgram.scopeStack.Peek();
        Assert.Single(scope.Variables);

        Variable array = scope.Variables["array"];

        var value = Assert.IsType<List<string>>(array.Value);
        Assert.Empty(value);

        Assert.Equal("array", array.Name);
        Assert.Equal("str[]", array.Type);
        Assert.False(array.IsConstant);
    }

    [Fact]
    public void Const_Array()
    {
        string line = "const str[] array = [];";

        GXCodeInterpreter.GXCodeInterpreter.DeclareConstArray(line, 5, "GXC_CS_ENTRYPOINT#0");

        Scope scope = GXCodeProgram.scopeStack.Peek();
        Assert.Single(scope.Variables);

        Variable array = scope.Variables["array"];
        
        var value = Assert.IsType<List<string>>(array.Value);
        Assert.Empty(value);

        Assert.Equal("array", array.Name);
        Assert.Equal("str[]", array.Type);
        Assert.True(array.IsConstant);
    }

    [Fact]
    public void Dict()
    {
        string line = "str{int} dict = {};";

        GXCodeInterpreter.GXCodeInterpreter.DeclareDict(line, 5, "GXC_CS_ENTRYPOINT#0");

        Scope scope = GXCodeProgram.scopeStack.Peek();
        Assert.Single(scope.Variables);

        Variable dict = scope.Variables["dict"];
        
        var value = Assert.IsType<Dictionary<string, int>>(dict.Value);
        Assert.Empty(value);

        Assert.Equal("dict", dict.Name);
        Assert.Equal("str{int}", dict.Type);
        Assert.False(dict.IsConstant);
    }

    [Fact]
    public void Const_Dict()
    {
        string line = "const str{int} dict = {};";

        GXCodeInterpreter.GXCodeInterpreter.DeclareConstDict(line, 5, "GXC_CS_ENTRYPOINT#0");

        Scope scope = GXCodeProgram.scopeStack.Peek();
        Assert.Single(scope.Variables);

        Variable dict = scope.Variables["dict"];
        
        var value = Assert.IsType<Dictionary<string, int>>(dict.Value);
        Assert.Empty(value);

        Assert.Equal("dict", dict.Name);
        Assert.Equal("str{int}", dict.Type);
        Assert.True(dict.IsConstant);
    }
}