using Xunit;
using GXCodeInterpreter;

namespace GXCode.Tests;

public class FastVariableActionTests
{
    public FastVariableActionTests()
    {
        GXCodeProgram.ResetScopeStack();
    }

    [Fact]
    public void Increment()
    {
        GXCodeInterpreter.GXCodeInterpreter.DeclareInt("int number = 1;", 5, "GXC_CS_ENTRYPOINT#0");

        string line = "number++;";
        GXCodeInterpreter.GXCodeInterpreter.IncrementVariable(line, 6, "GXC_CS_ENTRYPOINT#0");

        Scope scope = GXCodeProgram.scopeStack.Peek();
        Assert.Single(scope.Variables);

        Variable number = scope.Variables["number"];

        var value = Assert.IsType<int>(number.Value);
        Assert.Equal(2, value);
    }

    [Fact]
    public void Increment_Twice()
    {
        GXCodeInterpreter.GXCodeInterpreter.DeclareInt("int number = 1;", 5, "GXC_CS_ENTRYPOINT#0");

        string line = "number++;";
        GXCodeInterpreter.GXCodeInterpreter.IncrementVariable(line, 6, "GXC_CS_ENTRYPOINT#0");
        GXCodeInterpreter.GXCodeInterpreter.IncrementVariable(line, 7, "GXC_CS_ENTRYPOINT#0");

        Scope scope = GXCodeProgram.scopeStack.Peek();
        Assert.Single(scope.Variables);

        Variable number = scope.Variables["number"];

        var value = Assert.IsType<int>(number.Value);
        Assert.Equal(3, value);
    }

    [Fact]
    public void Decrement()
    {
        GXCodeInterpreter.GXCodeInterpreter.DeclareInt("int number = 2;", 5, "GXC_CS_ENTRYPOINT#0");

        string line = "number--;";
        GXCodeInterpreter.GXCodeInterpreter.DecrementVariable(line, 6, "GXC_CS_ENTRYPOINT#0");

        Scope scope = GXCodeProgram.scopeStack.Peek();
        Assert.Single(scope.Variables);

        Variable number = scope.Variables["number"];

        var value = Assert.IsType<int>(number.Value);
        Assert.Equal(1, value);
    }

    [Fact]
    public void Decrement_Below_Zero()
    {
        GXCodeInterpreter.GXCodeInterpreter.DeclareInt("int number = 0;", 5, "GXC_CS_ENTRYPOINT#0");

        string line = "number--;";
        GXCodeInterpreter.GXCodeInterpreter.DecrementVariable(line, 6, "GXC_CS_ENTRYPOINT#0");

        Scope scope = GXCodeProgram.scopeStack.Peek();
        Assert.Single(scope.Variables);

        Variable number = scope.Variables["number"];

        var value = Assert.IsType<int>(number.Value);
        Assert.Equal(-1, value);
    }

    [Fact]
    public void Increment_Bool_ShouldThrow()
    {
        Assert.Throws<GXCFastVarActionWrongVarTypeError>(() =>
        {
            GXCodeInterpreter.GXCodeInterpreter.DeclareBool("bool boolVar = true;", 5, "GXC_CS_ENTRYPOINT#0");

            string line = "boolVar++;";
            GXCodeInterpreter.GXCodeInterpreter.IncrementVariable(line, 6, "GXC_CS_ENTRYPOINT#0");
        });
    }

    [Fact]
    public void Increment_UnknownVariable_ShouldThrow()
    {
        Assert.Throws<GXCUndeclaredVariableError>(() =>
        {
            string line = "number++;";
            GXCodeInterpreter.GXCodeInterpreter.IncrementVariable(line, 6, "GXC_CS_ENTRYPOINT#0");
        });
    }

    [Fact]
    public void Increment_Const_ShouldThrow()
    {
        Assert.Throws<GXCConstantAssignmentError>(() =>
        {
            GXCodeInterpreter.GXCodeInterpreter.DeclareConstInt("const int number = 1;", 5, "GXC_CS_ENTRYPOINT#0");

            string line = "number++;";
            GXCodeInterpreter.GXCodeInterpreter.IncrementVariable(line, 6, "GXC_CS_ENTRYPOINT#0");
        });
    }

    [Fact]
    public void Decrement_Twice()
    {
        GXCodeInterpreter.GXCodeInterpreter.DeclareInt("int number = 2;", 5, "GXC_CS_ENTRYPOINT#0");

        string line = "number--;";
        GXCodeInterpreter.GXCodeInterpreter.DecrementVariable(line, 6, "GXC_CS_ENTRYPOINT#0");
        GXCodeInterpreter.GXCodeInterpreter.DecrementVariable(line, 7, "GXC_CS_ENTRYPOINT#0");

        Scope scope = GXCodeProgram.scopeStack.Peek();
        Assert.Single(scope.Variables);

        Variable number = scope.Variables["number"];

        var value = Assert.IsType<int>(number.Value);
        Assert.Equal(0, value);
    }

    [Fact]
    public void Decrement_Bool_ShouldThrow()
    {
        Assert.Throws<GXCFastVarActionWrongVarTypeError>(() =>
        {
            GXCodeInterpreter.GXCodeInterpreter.DeclareBool("bool boolVar = true;", 5, "GXC_CS_ENTRYPOINT#0");

            string line = "boolVar--;";
            GXCodeInterpreter.GXCodeInterpreter.DecrementVariable(line, 6, "GXC_CS_ENTRYPOINT#0");
        });
    }

    [Fact]
    public void Decrement_UnknownVariable_ShouldThrow()
    {
        Assert.Throws<GXCUndeclaredVariableError>(() =>
        {
            string line = "number--;";
            GXCodeInterpreter.GXCodeInterpreter.DecrementVariable(line, 6, "GXC_CS_ENTRYPOINT#0");
        });
    }

    [Fact]
    public void Decrement_Const_ShouldThrow()
    {
        Assert.Throws<GXCConstantAssignmentError>(() =>
        {
            GXCodeInterpreter.GXCodeInterpreter.DeclareConstInt("const int number = 1;", 5, "GXC_CS_ENTRYPOINT#0");

            string line = "number--;";
            GXCodeInterpreter.GXCodeInterpreter.DecrementVariable(line, 6, "GXC_CS_ENTRYPOINT#0");
        });
    }
}