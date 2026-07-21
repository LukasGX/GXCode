using Xunit;
using GXCodeInterpreter;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;

namespace GXCode.Tests;

public class InlineIntArithmeticTests
{
    [Fact]
    public void Simple_Addition()
    {
        string exp = "5 + 5";
        int? result = GXCodeInterpreter.GXCodeInterpreter.CalculateIntArithmetic(exp);

        Assert.NotNull(result);
        Assert.Equal(10, result);
    }

    [Fact]
    public void Simple_Subtraction()
    {
        string exp = "10 - 5";
        int? result = GXCodeInterpreter.GXCodeInterpreter.CalculateIntArithmetic(exp);

        Assert.NotNull(result);
        Assert.Equal(5, result);
    }

    [Fact]
    public void Simple_Multiplication()
    {
        string exp = "5 * 5";
        int? result = GXCodeInterpreter.GXCodeInterpreter.CalculateIntArithmetic(exp);

        Assert.NotNull(result);
        Assert.Equal(25, result);
    }

    [Fact]
    public void Simple_Division()
    {
        string exp = "50 / 5";
        int? result = GXCodeInterpreter.GXCodeInterpreter.CalculateIntArithmetic(exp);

        Assert.NotNull(result);
        Assert.Equal(10, result);
    }

    [Fact]
    public void Simple_Exponentiation()
    {
        string exp = "5 ^ 3";
        int? result = GXCodeInterpreter.GXCodeInterpreter.CalculateIntArithmetic(exp);

        Assert.NotNull(result);
        Assert.Equal(125, result);
    }

    [Fact]
    public void OperatorPrecedence_AdditionMultiplication()
    {
        string exp = "2 + 3 * 4";
        int? result = GXCodeInterpreter.GXCodeInterpreter.CalculateIntArithmetic(exp);

        Assert.NotNull(result);
        Assert.Equal(14, result);
    }

    [Fact]
    public void OperatorPrecedence_MultiplicationAddition()
    {
        string exp = "2 * 3 + 4";
        int? result = GXCodeInterpreter.GXCodeInterpreter.CalculateIntArithmetic(exp);

        Assert.NotNull(result);
        Assert.Equal(10, result);
    }

    [Fact]
    public void OperatorPrecedence_ExponentiationMultiplication()
    {
        string exp = "2 * 3 ^ 2";
        int? result = GXCodeInterpreter.GXCodeInterpreter.CalculateIntArithmetic(exp);

        Assert.NotNull(result);
        Assert.Equal(18, result);
    }

    [Fact]
    public void OperatorPrecedence_MultiplicationExponentiation()
    {
        string exp = "2 ^ 3 * 2";
        int? result = GXCodeInterpreter.GXCodeInterpreter.CalculateIntArithmetic(exp);

        Assert.NotNull(result);
        Assert.Equal(16, result);
    }

    [Fact]
    public void Parentheses_Simple()
    {
        string exp = "(2 + 3) * 4";
        int? result = GXCodeInterpreter.GXCodeInterpreter.CalculateIntArithmetic(exp);

        Assert.NotNull(result);
        Assert.Equal(20, result);
    }

    [Fact]
    public void Parentheses_Nested()
    {
        string exp = "((2 + 3) * 4)";
        int? result = GXCodeInterpreter.GXCodeInterpreter.CalculateIntArithmetic(exp);

        Assert.NotNull(result);
        Assert.Equal(20, result);
    }

    [Fact]
    public void Parentheses_Multiple()
    {
        string exp = "(2 + 3) * (4 + 5)";
        int? result = GXCodeInterpreter.GXCodeInterpreter.CalculateIntArithmetic(exp);

        Assert.NotNull(result);
        Assert.Equal(45, result);
    }

    [Fact]
    public void Exponentiation_RightAssociative()
    {
        string exp = "2 ^ 3 ^ 2";
        int? result = GXCodeInterpreter.GXCodeInterpreter.CalculateIntArithmetic(exp);

        Assert.NotNull(result);
        Assert.Equal(512, result);
    }

    [Fact]
    public void Subtration_LeftAssociative()
    {
        string exp = "10 - 3 - 2";
        int? result = GXCodeInterpreter.GXCodeInterpreter.CalculateIntArithmetic(exp);

        Assert.NotNull(result);
        Assert.Equal(5, result);
    }

    [Fact]
    public void Division_LeftAssociative()
    {
        string exp = "20 / 2 / 5";
        int? result = GXCodeInterpreter.GXCodeInterpreter.CalculateIntArithmetic(exp);

        Assert.NotNull(result);
        Assert.Equal(2, result);
    }

    [Fact]
    public void Exponentiation_Parentheses_1()
    {
        string exp = "2 ^ (3 ^ 2)";
        int? result = GXCodeInterpreter.GXCodeInterpreter.CalculateIntArithmetic(exp);

        Assert.NotNull(result);
        Assert.Equal(512, result);
    }

    [Fact]
    public void Exponentiation_Parentheses_2()
    {
        string exp = "(2 ^ 3) ^ 2";
        int? result = GXCodeInterpreter.GXCodeInterpreter.CalculateIntArithmetic(exp);

        Assert.NotNull(result);
        Assert.Equal(64, result);
    }

    [Fact]
    public void ComplexExpression_1()
    {
        string exp = "2 + 3 * 4 - 5";
        int? result = GXCodeInterpreter.GXCodeInterpreter.CalculateIntArithmetic(exp);

        Assert.NotNull(result);
        Assert.Equal(9, result);
    }

    [Fact]
    public void ComplexExpression_2()
    {
        string exp = "(2 + 3) ^ 2 + 10 / 2";
        int? result = GXCodeInterpreter.GXCodeInterpreter.CalculateIntArithmetic(exp);

        Assert.NotNull(result);
        Assert.Equal(30, result);
    }

    [Fact]
    public void ComplexExpression_3()
    {
        string exp = "2 ^ 3 + 4 * (8 - 5)";
        int? result = GXCodeInterpreter.GXCodeInterpreter.CalculateIntArithmetic(exp);

        Assert.NotNull(result);
        Assert.Equal(20, result);
    }

    [Fact]
    public void Whitespace_None()
    {
        string exp = "2+3";
        int? result = GXCodeInterpreter.GXCodeInterpreter.CalculateIntArithmetic(exp);

        Assert.NotNull(result);
        Assert.Equal(5, result);
    }

    [Fact]
    public void Whitespace_Normal()
    {
        string exp = "2 + 3";
        int? result = GXCodeInterpreter.GXCodeInterpreter.CalculateIntArithmetic(exp);

        Assert.NotNull(result);
        Assert.Equal(5, result);
    }

    [Fact]
    public void Whitespace_Excessive()
    {
        string exp = "   2   +   3";
        int? result = GXCodeInterpreter.GXCodeInterpreter.CalculateIntArithmetic(exp);

        Assert.NotNull(result);
        Assert.Equal(5, result);
    }

    [Fact]
    public void Invalid_EmptyString()
    {
        string exp = "";
        int? result = GXCodeInterpreter.GXCodeInterpreter.CalculateIntArithmetic(exp);

        Assert.Null(result);
    }

    [Fact]
    public void Invalid_Text()
    {
        string exp = "Hello World";
        int? result = GXCodeInterpreter.GXCodeInterpreter.CalculateIntArithmetic(exp);

        Assert.Null(result);
    }

    [Fact]
    public void Invalid_UnknownOperator()
    {
        string exp = "2++3";
        int? result = GXCodeInterpreter.GXCodeInterpreter.CalculateIntArithmetic(exp);

        Assert.Null(result);
    }

    [Fact]
    public void Invalid_UnclosedParenthesis()
    {
        string exp = "(2+3";
        int? result = GXCodeInterpreter.GXCodeInterpreter.CalculateIntArithmetic(exp);

        Assert.Null(result);
    }

    [Fact]
    public void Invalid_ClosingParenthesis()
    {
        string exp = "2+3)";
        int? result = GXCodeInterpreter.GXCodeInterpreter.CalculateIntArithmetic(exp);

        Assert.Null(result);
    }

    [Fact]
    public void Invalid_OnlyOperator()
    {
        string exp = "+";
        int? result = GXCodeInterpreter.GXCodeInterpreter.CalculateIntArithmetic(exp);

        Assert.Null(result);
    }

    [Fact]
    public void Invalid_DivisionThroughZero()
    {
        string exp = "2 / 0";
        int? result = GXCodeInterpreter.GXCodeInterpreter.CalculateIntArithmetic(exp);

        Assert.Null(result);
    }

    [Fact]
    public void Invalid_ParenthesesOnly()
    {
        string exp = "()";
        int? result = GXCodeInterpreter.GXCodeInterpreter.CalculateIntArithmetic(exp);

        Assert.Null(result);
    }

    [Fact]
    public void Invalid_SingleNumberWithOperator_1()
    {
        string exp = "2+";
        int? result = GXCodeInterpreter.GXCodeInterpreter.CalculateIntArithmetic(exp);

        Assert.Null(result);
    }

    [Fact]
    public void Invalid_SingleNumberWithOperator_2()
    {
        string exp = "*2";
        int? result = GXCodeInterpreter.GXCodeInterpreter.CalculateIntArithmetic(exp);

        Assert.Null(result);
    }

    [Fact]
    public void Invalid_ImplicitMultiplication()
    {
        string exp = "2 (3 + 4)";
        int? result = GXCodeInterpreter.GXCodeInterpreter.CalculateIntArithmetic(exp);

        Assert.Null(result);
    }

    [Fact]
    public void Invalid_Double_Dot()
    {
        string exp = "2..3";
        int? result = GXCodeInterpreter.GXCodeInterpreter.CalculateIntArithmetic(exp);

        Assert.Null(result);
    }

    [Fact]
    public void Invalid_Letter()
    {
        string exp = "2a+3";
        int? result = GXCodeInterpreter.GXCodeInterpreter.CalculateIntArithmetic(exp);

        Assert.Null(result);
    }

    [Fact]
    public void SingleNumber()
    {
        string exp = "42";
        int? result = GXCodeInterpreter.GXCodeInterpreter.CalculateIntArithmetic(exp);

        Assert.NotNull(result);
        Assert.Equal(42, result);
    }

    [Fact]
    public void Division_Integer()
    {
        string exp = "5 / 2";
        int? result = GXCodeInterpreter.GXCodeInterpreter.CalculateIntArithmetic(exp);

        Assert.NotNull(result);
        Assert.Equal(2, result);
    }

    [Fact]
    public void ZeroExponent()
    {
        string exp = "2 ^ 0";
        int? result = GXCodeInterpreter.GXCodeInterpreter.CalculateIntArithmetic(exp);

        Assert.NotNull(result);
        Assert.Equal(1, result);
    }

    [Fact]
    public void ExponentOne()
    {
        string exp = "2 ^ 1";
        int? result = GXCodeInterpreter.GXCodeInterpreter.CalculateIntArithmetic(exp);

        Assert.NotNull(result);
        Assert.Equal(2, result);
    }

    [Fact]
    public void NegativeResult()
    {
        string exp = "2 - 5";
        int? result = GXCodeInterpreter.GXCodeInterpreter.CalculateIntArithmetic(exp);

        Assert.NotNull(result);
        Assert.Equal(-3, result);
    }

    [Fact]
    public void Simple_Negative_Addition()
    {
        string exp = "-5 + 2";
        int? result = GXCodeInterpreter.GXCodeInterpreter.CalculateIntArithmetic(exp);

        Assert.NotNull(result);
        Assert.Equal(-3, result);
    }

    [Fact]
    public void Simple_Negative_Subtraction()
    {
        string exp = "-5 - 2";
        int? result = GXCodeInterpreter.GXCodeInterpreter.CalculateIntArithmetic(exp);

        Assert.NotNull(result);
        Assert.Equal(-7, result);
    }

    [Fact]
    public void Simple_Negative_Multiplication_1()
    {
        string exp = "2 * -3";
        int? result = GXCodeInterpreter.GXCodeInterpreter.CalculateIntArithmetic(exp);

        Assert.NotNull(result);
        Assert.Equal(-6, result);
    }

    [Fact]
    public void Simple_Negative_Multiplication_2()
    {
        string exp = "-2 * -3";
        int? result = GXCodeInterpreter.GXCodeInterpreter.CalculateIntArithmetic(exp);

        Assert.NotNull(result);
        Assert.Equal(6, result);
    }

    [Fact]
    public void Simple_Negative_Division()
    {
        string exp = "-4 / 2";
        int? result = GXCodeInterpreter.GXCodeInterpreter.CalculateIntArithmetic(exp);

        Assert.NotNull(result);
        Assert.Equal(-2, result);
    }

    [Fact]
    public void Simple_Negative_Exponentiation_1()
    {
        string exp = "-2 ^ 3";
        int? result = GXCodeInterpreter.GXCodeInterpreter.CalculateIntArithmetic(exp);

        Assert.NotNull(result);
        Assert.Equal(-8, result);
    }

    [Fact]
    public void Simple_Negative_Exponentiation_2()
    {
        string exp = "5 ^ -2";
        int? result = GXCodeInterpreter.GXCodeInterpreter.CalculateIntArithmetic(exp);

        Assert.NotNull(result);
        Assert.Equal(0, result);
    }

    [Fact]
    public void Excessive_Parentheses()
    {
        string exp = "(((((((5)))))))";
        int? result = GXCodeInterpreter.GXCodeInterpreter.CalculateIntArithmetic(exp);

        Assert.NotNull(result);
        Assert.Equal(5, result);
    }

    [Fact]
    public void Addition_NegativeOperand()
    {
        string exp = "5 + -3";
        int? result = GXCodeInterpreter.GXCodeInterpreter.CalculateIntArithmetic(exp);

        Assert.NotNull(result);
        Assert.Equal(2, result);
    }

    [Fact]
    public void Subtraction_NegativeOperand()
    {
        string exp = "5 - -3";
        int? result = GXCodeInterpreter.GXCodeInterpreter.CalculateIntArithmetic(exp);

        Assert.NotNull(result);
        Assert.Equal(8, result);
    }

    [Fact]
    public void Mixed_Priority()
    {
        string exp = "2 ^ 3 * 4 + 5 - 6 / 2";
        int? result = GXCodeInterpreter.GXCodeInterpreter.CalculateIntArithmetic(exp);

        Assert.NotNull(result);
        Assert.Equal(34, result);
    }

    [Fact]
    public void IntegerOverflow()
    {
        string exp = "100000 * 100000";
        int? result = GXCodeInterpreter.GXCodeInterpreter.CalculateIntArithmetic(exp);

        Assert.NotNull(result);
        Assert.Equal(1410065408, result);
    }
}