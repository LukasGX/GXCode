using Xunit;
using GXCodeInterpreter;
using System.Text.RegularExpressions;
using System.Runtime.CompilerServices;

namespace GXCode.Tests;

public class InlineDecArithmeticTests
{
    [Fact]
    public void Simple_Addition()
    {
        string exp = "0.5 + 0.5";
        decimal? result = GXCodeInterpreter.GXCodeInterpreter.CalculateDecArithmetic(exp);

        Assert.NotNull(result);
        Assert.Equal(1.0m, result);
    }

    [Fact]
    public void Simple_Subtraction()
    {
        string exp = "0.5 - 0.3";
        decimal? result = GXCodeInterpreter.GXCodeInterpreter.CalculateDecArithmetic(exp);

        Assert.NotNull(result);
        Assert.Equal(0.2m, result);
    }

    [Fact]
    public void Simple_Multiplication()
    {
        string exp = "1.5 * 2";
        decimal? result = GXCodeInterpreter.GXCodeInterpreter.CalculateDecArithmetic(exp);

        Assert.NotNull(result);
        Assert.Equal(3.0m, result);
    }

    [Fact]
    public void Simple_Division()
    {
        string exp = "5 / 2";
        decimal? result = GXCodeInterpreter.GXCodeInterpreter.CalculateDecArithmetic(exp);

        Assert.NotNull(result);
        Assert.Equal(2.5m, result);
    }

    [Fact]
    public void Simple_Exponentiation()
    {
        string exp = "2.5 ^ 2";
        decimal? result = GXCodeInterpreter.GXCodeInterpreter.CalculateDecArithmetic(exp);

        Assert.NotNull(result);
        Assert.Equal(6.25m, result);
    }

    [Fact]
    public void Negative_Exponentiation()
    {
        string exp = "5 ^ -2";
        decimal? result = GXCodeInterpreter.GXCodeInterpreter.CalculateDecArithmetic(exp);

        Assert.NotNull(result);
        Assert.Equal(0.04m, result);
    }

    [Fact]
    public void Multiple_DecimalPlaces()
    {
        string exp = "3.1415";
        decimal? result = GXCodeInterpreter.GXCodeInterpreter.CalculateDecArithmetic(exp);

        Assert.NotNull(result);
        Assert.Equal(3.1415m, result);
    }

    [Fact]
    public void Negative_Multiple_DecimalPlaces()
    {
        string exp = "-0.75";
        decimal? result = GXCodeInterpreter.GXCodeInterpreter.CalculateDecArithmetic(exp);

        Assert.NotNull(result);
        Assert.Equal(-0.75m, result);
    }

    [Fact]
    public void Mixed_Addition()
    {
        string exp = "2.5 + -1.25";
        decimal? result = GXCodeInterpreter.GXCodeInterpreter.CalculateDecArithmetic(exp);

        Assert.NotNull(result);
        Assert.Equal(1.25m, result);
    }
}