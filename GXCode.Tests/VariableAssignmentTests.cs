using Xunit;
using GXCodeInterpreter;
using System.Text.RegularExpressions;

namespace GXCode.Tests;

public class VariableAssignmentTests
{
    [Fact]
    public void Assign_Str()
    {
        GXCodeProgram.ResetScopeStack();
        GXCodeInterpreter.GXCodeInterpreter.DeclareStr("str variable = \"\";", 5, "GXC_CS_ENTRYPOINT#0");

        string line = "variable = \"Hi\";";
        GXCodeInterpreter.GXCodeInterpreter.AssignVariable(line, 6, "GXC_CS_ENTRYPOINT#0");

        Scope scope = GXCodeProgram.scopeStack.Peek();
        Assert.Single(scope.Variables);

        Variable variable = scope.Variables["variable"];

        var value = Assert.IsType<string>(variable.Value);
        Assert.Equal("Hi", value);
    }

    [Fact]
    public void Assign_Int()
    {
        GXCodeProgram.ResetScopeStack();
        GXCodeInterpreter.GXCodeInterpreter.DeclareInt("int variable = 10;", 5, "GXC_CS_ENTRYPOINT#0");

        string line = "variable = 15;";
        GXCodeInterpreter.GXCodeInterpreter.AssignVariable(line, 6, "GXC_CS_ENTRYPOINT#0");

        Scope scope = GXCodeProgram.scopeStack.Peek();
        Assert.Single(scope.Variables);

        Variable variable = scope.Variables["variable"];

        var value = Assert.IsType<int>(variable.Value);
        Assert.Equal(15, value);
    }

    [Fact]
    public void Assign_Dec()
    {
        GXCodeProgram.ResetScopeStack();
        GXCodeInterpreter.GXCodeInterpreter.DeclareDec("dec variable = 0.5;", 5, "GXC_CS_ENTRYPOINT#0");

        string line = "variable = 1.5;";
        GXCodeInterpreter.GXCodeInterpreter.AssignVariable(line, 6, "GXC_CS_ENTRYPOINT#0");

        Scope scope = GXCodeProgram.scopeStack.Peek();
        Assert.Single(scope.Variables);

        Variable variable = scope.Variables["variable"];

        var value = Assert.IsType<decimal>(variable.Value);
        Assert.Equal(new decimal(1.5), value);
    }

    [Fact]
    public void Assign_Bool()
    {
        GXCodeProgram.ResetScopeStack();
        GXCodeInterpreter.GXCodeInterpreter.DeclareBool("bool variable = true;", 5, "GXC_CS_ENTRYPOINT#0");

        string line = "variable = false;";
        GXCodeInterpreter.GXCodeInterpreter.AssignVariable(line, 6, "GXC_CS_ENTRYPOINT#0");

        Scope scope = GXCodeProgram.scopeStack.Peek();
        Assert.Single(scope.Variables);

        Variable variable = scope.Variables["variable"];

        var value = Assert.IsType<bool>(variable.Value);
        Assert.False(value);
    }

    [Fact]
    public void Assign_Rex()
    {
        GXCodeProgram.ResetScopeStack();
        GXCodeInterpreter.GXCodeInterpreter.DeclareRex("rex variable = /\\d+/;", 5, "GXC_CS_ENTRYPOINT#0");

        string line = "variable = /.*/;";
        GXCodeInterpreter.GXCodeInterpreter.AssignVariable(line, 6, "GXC_CS_ENTRYPOINT#0");

        Scope scope = GXCodeProgram.scopeStack.Peek();
        Assert.Single(scope.Variables);

        Variable variable = scope.Variables["variable"];

        var value = Assert.IsType<Regex>(variable.Value);
        Assert.Equal("/.*/", value.ToString());
    }

    [Fact]
    public void Assign_Str_Array()
    {
        GXCodeProgram.ResetScopeStack();
        GXCodeInterpreter.GXCodeInterpreter.DeclareArray("str[] variable = [];", 5, "GXC_CS_ENTRYPOINT#0");

        string line = "variable = [\"a\"];";
        GXCodeInterpreter.GXCodeInterpreter.AssignVariable(line, 6, "GXC_CS_ENTRYPOINT#0");

        Scope scope = GXCodeProgram.scopeStack.Peek();
        Assert.Single(scope.Variables);

        Variable variable = scope.Variables["variable"];

        var value = Assert.IsType<List<string>>(variable.Value);
        Assert.Equal(["a"], value);
    }

    [Fact]
    public void Assign_Int_Array()
    {
        GXCodeProgram.ResetScopeStack();
        GXCodeInterpreter.GXCodeInterpreter.DeclareArray("int[] variable = [];", 5, "GXC_CS_ENTRYPOINT#0");

        string line = "variable = [15];";
        GXCodeInterpreter.GXCodeInterpreter.AssignVariable(line, 6, "GXC_CS_ENTRYPOINT#0");

        Scope scope = GXCodeProgram.scopeStack.Peek();
        Assert.Single(scope.Variables);

        Variable variable = scope.Variables["variable"];

        var value = Assert.IsType<List<int>>(variable.Value);
        Assert.Equal([15], value);
    }

    [Fact]
    public void Assign_Dec_Array()
    {
        GXCodeProgram.ResetScopeStack();
        GXCodeInterpreter.GXCodeInterpreter.DeclareArray("dec[] variable = [];", 5, "GXC_CS_ENTRYPOINT#0");

        string line = "variable = [3.1415];";
        GXCodeInterpreter.GXCodeInterpreter.AssignVariable(line, 6, "GXC_CS_ENTRYPOINT#0");

        Scope scope = GXCodeProgram.scopeStack.Peek();
        Assert.Single(scope.Variables);

        Variable variable = scope.Variables["variable"];

        var value = Assert.IsType<List<decimal>>(variable.Value);
        Assert.Equal([new(3.1415)], value);
    }

    [Fact]
    public void Assign_Bool_Array()
    {
        GXCodeProgram.ResetScopeStack();
        GXCodeInterpreter.GXCodeInterpreter.DeclareArray("bool[] variable = [];", 5, "GXC_CS_ENTRYPOINT#0");

        string line = "variable = [true];";
        GXCodeInterpreter.GXCodeInterpreter.AssignVariable(line, 6, "GXC_CS_ENTRYPOINT#0");

        Scope scope = GXCodeProgram.scopeStack.Peek();
        Assert.Single(scope.Variables);

        Variable variable = scope.Variables["variable"];

        var value = Assert.IsType<List<bool>>(variable.Value);
        Assert.Equal([true], value);
    }

    [Fact]
    public void Assign_Rex_Array()
    {
        GXCodeProgram.ResetScopeStack();
        GXCodeInterpreter.GXCodeInterpreter.DeclareArray("rex[] variable = [];", 5, "GXC_CS_ENTRYPOINT#0");

        string line = "variable = [/.*/];";
        GXCodeInterpreter.GXCodeInterpreter.AssignVariable(line, 6, "GXC_CS_ENTRYPOINT#0");

        Scope scope = GXCodeProgram.scopeStack.Peek();
        Assert.Single(scope.Variables);

        Variable variable = scope.Variables["variable"];

        var value = Assert.IsType<List<Regex>>(variable.Value);
        Assert.Equal("/.*/", value[0].ToString());
    }

    [Fact]
    public void Assign_Str_Dict()
    {
        GXCodeProgram.ResetScopeStack();
        GXCodeInterpreter.GXCodeInterpreter.DeclareDict("str{str} variable = {};", 5, "GXC_CS_ENTRYPOINT#0");

        string line = "variable = {\"a\": \"Hi\"};";
        GXCodeInterpreter.GXCodeInterpreter.AssignVariable(line, 6, "GXC_CS_ENTRYPOINT#0");

        Scope scope = GXCodeProgram.scopeStack.Peek();
        Assert.Single(scope.Variables);

        Variable variable = scope.Variables["variable"];

        var value = Assert.IsType<Dictionary<string, string>>(variable.Value);

        var expected = new Dictionary<string, string>
        {
            ["a"] = "Hi"
        };

        Assert.Equal(expected, value);
    }

    [Fact]
    public void Assign_Str_Var()
    {
        GXCodeProgram.ResetScopeStack();
        GXCodeInterpreter.GXCodeInterpreter.DeclareStr("str variable = \"\";", 5, "GXC_CS_ENTRYPOINT#0");
        GXCodeInterpreter.GXCodeInterpreter.DeclareStr("str variable2 = \"Hello\";", 5, "GXC_CS_ENTRYPOINT#0");

        string line = "variable = variable2;";
        GXCodeInterpreter.GXCodeInterpreter.AssignVariable(line, 6, "GXC_CS_ENTRYPOINT#0");

        Scope scope = GXCodeProgram.scopeStack.Peek();
        Assert.Equal(2, scope.Variables.Count);

        Variable variable = scope.Variables["variable"];

        var value = Assert.IsType<string>(variable.Value);
        Assert.Equal("Hello", value);
    }

    [Fact]
    public void Assign_Array()
    {
        GXCodeProgram.ResetScopeStack();
        GXCodeInterpreter.GXCodeInterpreter.DeclareArray("str[] variable = [];", 5, "GXC_CS_ENTRYPOINT#0");
        GXCodeInterpreter.GXCodeInterpreter.DeclareArray("str[] variable2 = [\"Hello\"];", 5, "GXC_CS_ENTRYPOINT#0");

        string line = "variable = variable2;";
        GXCodeInterpreter.GXCodeInterpreter.AssignVariable(line, 6, "GXC_CS_ENTRYPOINT#0");

        Scope scope = GXCodeProgram.scopeStack.Peek();
        Assert.Equal(2, scope.Variables.Count);

        Variable variable = scope.Variables["variable"];

        var value = Assert.IsType<List<string>>(variable.Value);
        Assert.Equal(["Hello"], value);
    }

    [Fact]
    public void Assign_Dict()
    {
        GXCodeProgram.ResetScopeStack();
        GXCodeInterpreter.GXCodeInterpreter.DeclareDict("str{str} variable = {};", 5, "GXC_CS_ENTRYPOINT#0");
        GXCodeInterpreter.GXCodeInterpreter.DeclareDict("str{str} variable2 = {\"a\": \"World\"};", 5, "GXC_CS_ENTRYPOINT#0");

        string line = "variable = variable2;";
        GXCodeInterpreter.GXCodeInterpreter.AssignVariable(line, 6, "GXC_CS_ENTRYPOINT#0");

        Scope scope = GXCodeProgram.scopeStack.Peek();
        Assert.Equal(2, scope.Variables.Count);

        Variable variable = scope.Variables["variable"];

        var value = Assert.IsType<Dictionary<string, string>>(variable.Value);

        var expected = new Dictionary<string, string>
        {
            ["a"] = "World"
        };

        Assert.Equal(expected, value);
    }
}