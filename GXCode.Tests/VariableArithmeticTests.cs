using Xunit;
using GXCodeInterpreter;
using System.Text.RegularExpressions;

namespace GXCode.Tests;

public class VariableArithmeticTests
{
    [Fact]
    public void Addition()
    {
        GXCodeProgram.ResetScopeStack();
        GXCodeInterpreter.GXCodeInterpreter.DeclareInt("int variable = 10;", 5, "GXC_CS_ENTRYPOINT#0");

        string line = "variable += 5;";
        GXCodeInterpreter.GXCodeInterpreter.PerformVariableArithmetic(line, 6, "GXC_CS_ENTRYPOINT#0");

        Scope scope = GXCodeProgram.scopeStack.Peek();
        Assert.Single(scope.Variables);

        Variable variable = scope.Variables["variable"];

        var value = Assert.IsType<int>(variable.Value);
        Assert.Equal(15, value);
    }

    [Fact]
    public void Subtraction()
    {
        GXCodeProgram.ResetScopeStack();
        GXCodeInterpreter.GXCodeInterpreter.DeclareInt("int variable = 10;", 5, "GXC_CS_ENTRYPOINT#0");

        string line = "variable -= 5;";
        GXCodeInterpreter.GXCodeInterpreter.PerformVariableArithmetic(line, 6, "GXC_CS_ENTRYPOINT#0");

        Scope scope = GXCodeProgram.scopeStack.Peek();
        Assert.Single(scope.Variables);

        Variable variable = scope.Variables["variable"];

        var value = Assert.IsType<int>(variable.Value);
        Assert.Equal(5, value);
    }

    [Fact]
    public void Multiplication()
    {
        GXCodeProgram.ResetScopeStack();
        GXCodeInterpreter.GXCodeInterpreter.DeclareInt("int variable = 10;", 5, "GXC_CS_ENTRYPOINT#0");

        string line = "variable *= 3;";
        GXCodeInterpreter.GXCodeInterpreter.PerformVariableArithmetic(line, 6, "GXC_CS_ENTRYPOINT#0");

        Scope scope = GXCodeProgram.scopeStack.Peek();
        Assert.Single(scope.Variables);

        Variable variable = scope.Variables["variable"];

        var value = Assert.IsType<int>(variable.Value);
        Assert.Equal(30, value);
    }

    [Fact]
    public void Division()
    {
        GXCodeProgram.ResetScopeStack();
        GXCodeInterpreter.GXCodeInterpreter.DeclareInt("int variable = 10;", 5, "GXC_CS_ENTRYPOINT#0");

        string line = "variable /= 2;";
        GXCodeInterpreter.GXCodeInterpreter.PerformVariableArithmetic(line, 6, "GXC_CS_ENTRYPOINT#0");

        Scope scope = GXCodeProgram.scopeStack.Peek();
        Assert.Single(scope.Variables);

        Variable variable = scope.Variables["variable"];

        var value = Assert.IsType<int>(variable.Value);
        Assert.Equal(5, value);
    }

    [Fact]
    public void Exponentiation()
    {
        GXCodeProgram.ResetScopeStack();
        GXCodeInterpreter.GXCodeInterpreter.DeclareInt("int variable = 10;", 5, "GXC_CS_ENTRYPOINT#0");

        string line = "variable ^= 2;";
        GXCodeInterpreter.GXCodeInterpreter.PerformVariableArithmetic(line, 6, "GXC_CS_ENTRYPOINT#0");

        Scope scope = GXCodeProgram.scopeStack.Peek();
        Assert.Single(scope.Variables);

        Variable variable = scope.Variables["variable"];

        var value = Assert.IsType<int>(variable.Value);
        Assert.Equal(100, value);
    }

    [Fact]
    public void Addition_Variable()
    {
        GXCodeProgram.ResetScopeStack();
        GXCodeInterpreter.GXCodeInterpreter.DeclareInt("int variable = 10;", 5, "GXC_CS_ENTRYPOINT#0");
        GXCodeInterpreter.GXCodeInterpreter.DeclareInt("int variable2 = 5;", 6, "GXC_CS_ENTRYPOINT#0");

        GXCodeInterpreter.GXCodeInterpreter.PerformVariableArithmetic("variable += variable2;", 7, "GXC_CS_ENTRYPOINT#0");

        Scope scope = GXCodeProgram.scopeStack.Peek();
        Assert.Equal(2, scope.Variables.Count);

        Variable variable = scope.Variables["variable"];

        var value = Assert.IsType<int>(variable.Value);
        Assert.Equal(15, value);
    }

    [Fact]
    public void Subtraction_Variable()
    {
        GXCodeProgram.ResetScopeStack();
        GXCodeInterpreter.GXCodeInterpreter.DeclareInt("int variable = 10;", 5, "GXC_CS_ENTRYPOINT#0");
        GXCodeInterpreter.GXCodeInterpreter.DeclareInt("int variable2 = 5;", 6, "GXC_CS_ENTRYPOINT#0");

        GXCodeInterpreter.GXCodeInterpreter.PerformVariableArithmetic("variable -= variable2;", 7, "GXC_CS_ENTRYPOINT#0");

        Scope scope = GXCodeProgram.scopeStack.Peek();
        Assert.Equal(2, scope.Variables.Count);

        Variable variable = scope.Variables["variable"];

        var value = Assert.IsType<int>(variable.Value);
        Assert.Equal(5, value);
    }

    [Fact]
    public void Multiplication_Variable()
    {
        GXCodeProgram.ResetScopeStack();
        GXCodeInterpreter.GXCodeInterpreter.DeclareInt("int variable = 10;", 5, "GXC_CS_ENTRYPOINT#0");
        GXCodeInterpreter.GXCodeInterpreter.DeclareInt("int variable2 = 3;", 6, "GXC_CS_ENTRYPOINT#0");

        GXCodeInterpreter.GXCodeInterpreter.PerformVariableArithmetic("variable *= variable2;", 7, "GXC_CS_ENTRYPOINT#0");

        Scope scope = GXCodeProgram.scopeStack.Peek();
        Assert.Equal(2, scope.Variables.Count);

        Variable variable = scope.Variables["variable"];

        var value = Assert.IsType<int>(variable.Value);
        Assert.Equal(30, value);
    }

    [Fact]
    public void Division_Variable()
    {
        GXCodeProgram.ResetScopeStack();
        GXCodeInterpreter.GXCodeInterpreter.DeclareInt("int variable = 10;", 5, "GXC_CS_ENTRYPOINT#0");
        GXCodeInterpreter.GXCodeInterpreter.DeclareInt("int variable2 = 2;", 6, "GXC_CS_ENTRYPOINT#0");

        GXCodeInterpreter.GXCodeInterpreter.PerformVariableArithmetic("variable /= variable2;", 7, "GXC_CS_ENTRYPOINT#0");

        Scope scope = GXCodeProgram.scopeStack.Peek();
        Assert.Equal(2, scope.Variables.Count);

        Variable variable = scope.Variables["variable"];

        var value = Assert.IsType<int>(variable.Value);
        Assert.Equal(5, value);
    }

    [Fact]
    public void Exponentiation_Variable()
    {
        GXCodeProgram.ResetScopeStack();
        GXCodeInterpreter.GXCodeInterpreter.DeclareInt("int variable = 10;", 5, "GXC_CS_ENTRYPOINT#0");
        GXCodeInterpreter.GXCodeInterpreter.DeclareInt("int variable2 = 2;", 6, "GXC_CS_ENTRYPOINT#0");

        GXCodeInterpreter.GXCodeInterpreter.PerformVariableArithmetic("variable ^= variable2;", 7, "GXC_CS_ENTRYPOINT#0");

        Scope scope = GXCodeProgram.scopeStack.Peek();
        Assert.Equal(2, scope.Variables.Count);

        Variable variable = scope.Variables["variable"];

        var value = Assert.IsType<int>(variable.Value);
        Assert.Equal(100, value);
    }
}