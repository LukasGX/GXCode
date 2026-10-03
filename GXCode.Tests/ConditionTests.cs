using Xunit;
using GXCodeInterpreter;
using System.Runtime.CompilerServices;

namespace GXCode.Tests;

public class ConditionTests
{
    public ConditionTests()
    {
        GXCodeProgram.ResetScopeStack();
    }

    [Fact]
    public void Boolean_Variable()
    {
        GXCodeInterpreter.GXCodeInterpreter.DeclareBool("bool cond = true;", 5, "GXC_CS_ENTRYPOINT#0");
        bool result = GXCodeInterpreter.GXCodeInterpreter.EvaluateCondition("cond");

        Assert.True(result);
    }

    [Fact]
    public void Boolean_Variable_Negative()
    {
        GXCodeInterpreter.GXCodeInterpreter.DeclareBool("bool cond = true;", 5, "GXC_CS_ENTRYPOINT#0");
        bool result = GXCodeInterpreter.GXCodeInterpreter.EvaluateCondition("!cond");

        Assert.False(result);
    }

    [Fact]
    public void Strings_Matching()
    {
        bool result = GXCodeInterpreter.GXCodeInterpreter.EvaluateCondition("\"abc\" == \"abc\"");

        Assert.True(result);
    }

    [Fact]
    public void Strings_NonMatching()
    {
        bool result = GXCodeInterpreter.GXCodeInterpreter.EvaluateCondition("\"abc\" == \"def\"");

        Assert.False(result);
    }

    [Fact]
    public void Strings_Negative_Matching()
    {
        bool result = GXCodeInterpreter.GXCodeInterpreter.EvaluateCondition("\"abc\" != \"abc\"");

        Assert.False(result);
    }

    [Fact]
    public void Strings_Negative_NonMatching()
    {
        bool result = GXCodeInterpreter.GXCodeInterpreter.EvaluateCondition("\"abc\" != \"def\"");

        Assert.True(result);
    }

    [Fact]
    public void Single_StrVar_Matching()
    {
        GXCodeInterpreter.GXCodeInterpreter.DeclareStr("str ex = \"Hi\";", 5, "GXC_CS_ENTRYPOINT#0");
        bool result = GXCodeInterpreter.GXCodeInterpreter.EvaluateCondition("ex == \"Hi\"");

        Assert.True(result);
    }

    [Fact]
    public void Single_StrVar_NonMatching()
    {
        GXCodeInterpreter.GXCodeInterpreter.DeclareStr("str ex = \"Hi\";", 5, "GXC_CS_ENTRYPOINT#0");
        bool result = GXCodeInterpreter.GXCodeInterpreter.EvaluateCondition("ex == \"abc\"");

        Assert.False(result);
    }

    [Fact]
    public void StrVars_Matching()
    {
        GXCodeInterpreter.GXCodeInterpreter.DeclareStr("str ex = \"Hi\";", 5, "GXC_CS_ENTRYPOINT#0");
        GXCodeInterpreter.GXCodeInterpreter.DeclareStr("str ey = \"Hi\";", 5, "GXC_CS_ENTRYPOINT#0");
        bool result = GXCodeInterpreter.GXCodeInterpreter.EvaluateCondition("ex == ey");

        Assert.True(result);
    }

    [Fact]
    public void StrVars_NonMatching()
    {
        GXCodeInterpreter.GXCodeInterpreter.DeclareStr("str ex = \"Hi\";", 5, "GXC_CS_ENTRYPOINT#0");
        GXCodeInterpreter.GXCodeInterpreter.DeclareStr("str ey = \"abc\";", 5, "GXC_CS_ENTRYPOINT#0");
        bool result = GXCodeInterpreter.GXCodeInterpreter.EvaluateCondition("ex == ey");

        Assert.False(result);
    }

    [Fact]
    public void Integers_Equal_Matching()
    {
        bool result = GXCodeInterpreter.GXCodeInterpreter.EvaluateCondition("1 == 1");

        Assert.True(result);
    }

    [Fact]
    public void Integers_Equal_NonMatching()
    {
        bool result = GXCodeInterpreter.GXCodeInterpreter.EvaluateCondition("1 == 2");

        Assert.False(result);
    }

    [Fact]
    public void Integers_NegativeEqual_Matching()
    {
        bool result = GXCodeInterpreter.GXCodeInterpreter.EvaluateCondition("1 != 1");

        Assert.False(result);
    }

    [Fact]
    public void Integers_NegativeEqual_NonMatching()
    {
        bool result = GXCodeInterpreter.GXCodeInterpreter.EvaluateCondition("1 != 2");

        Assert.True(result);
    }

    [Fact]
    public void Integers_Greater_Matching()
    {
        bool result = GXCodeInterpreter.GXCodeInterpreter.EvaluateCondition("2 > 1");

        Assert.True(result);
    }

    [Fact]
    public void Integers_Greater_NonMatching()
    {
        bool result = GXCodeInterpreter.GXCodeInterpreter.EvaluateCondition("1 > 1");

        Assert.False(result);
    }

    [Fact]
    public void Integers_Less_Matching()
    {
        bool result = GXCodeInterpreter.GXCodeInterpreter.EvaluateCondition("1 < 2");

        Assert.True(result);
    }

    [Fact]
    public void Integers_Less_NonMatching()
    {
        bool result = GXCodeInterpreter.GXCodeInterpreter.EvaluateCondition("1 < 1");

        Assert.False(result);
    }

    [Fact]
    public void Integers_GreaterEqual_Matching_1()
    {
        bool result = GXCodeInterpreter.GXCodeInterpreter.EvaluateCondition("2 >= 2");

        Assert.True(result);
    }

    [Fact]
    public void Integers_GreaterEqual_Matching_2()
    {
        bool result = GXCodeInterpreter.GXCodeInterpreter.EvaluateCondition("2 >= 1");

        Assert.True(result);
    }

    [Fact]
    public void Integers_GreaterEqual_NonMatching()
    {
        bool result = GXCodeInterpreter.GXCodeInterpreter.EvaluateCondition("2 >= 3");

        Assert.False(result);
    }

    [Fact]
    public void Integers_LessEqual_Matching_1()
    {
        bool result = GXCodeInterpreter.GXCodeInterpreter.EvaluateCondition("2 <= 2");

        Assert.True(result);
    }

    [Fact]
    public void Integers_LessEqual_Matching_2()
    {
        bool result = GXCodeInterpreter.GXCodeInterpreter.EvaluateCondition("1 <= 2");

        Assert.True(result);
    }

    [Fact]
    public void Integers_LessEqual_NonMatching()
    {
        bool result = GXCodeInterpreter.GXCodeInterpreter.EvaluateCondition("2 <= 1");

        Assert.False(result);
    }

    [Fact]
    public void Single_AND()
    {
        bool result = GXCodeInterpreter.GXCodeInterpreter.EvaluateConditions("1 == 1 && 2 == 2", "");

        Assert.True(result);
    }

    [Fact]
    public void Multiple_AND()
    {
        bool result = GXCodeInterpreter.GXCodeInterpreter.EvaluateConditions("1 == 1 && 2 == 2 && 3 == 3", "");

        Assert.True(result);
    }

    [Fact]
    public void Single_OR()
    {
        bool result = GXCodeInterpreter.GXCodeInterpreter.EvaluateConditions("1 == 1 || 2 == 3", "");

        Assert.True(result);
    }

    [Fact]
    public void Multiple_OR()
    {
        bool result = GXCodeInterpreter.GXCodeInterpreter.EvaluateConditions("1 == 1 || 2 == 3 || 3 == 4", "");

        Assert.True(result);
    }

    [Fact]
    public void Mixed_AND_OR()
    {
        bool result = GXCodeInterpreter.GXCodeInterpreter.EvaluateConditions("1 == 1 && 2 == 2 || 2 == 3", "");

        Assert.True(result);
    }

    [Fact]
    public void Mixed_AND_OR_Parentheses()
    {
        bool result = GXCodeInterpreter.GXCodeInterpreter.EvaluateConditions("(1 == 1 && 2 == 2) || 2 == 3", "");

        Assert.True(result);
    }

    [Fact]
    public void Mixed_AND_OR_Nested_Parentheses()
    {
        bool result = GXCodeInterpreter.GXCodeInterpreter.EvaluateConditions("((1 == 4 || 60 == 60) && 2 == 2) || 2 == 3", "");

        Assert.True(result);
    }

    [Fact]
    public void Nested_Parentheses_Multiple()
    {
        bool result = GXCodeInterpreter.GXCodeInterpreter.EvaluateConditions(
            "(1 == 1 && (2 == 2 || 3 == 4))", ""
        );

        Assert.True(result);
    }

    [Fact]
    public void Deeply_Nested()
    {
        bool result = GXCodeInterpreter.GXCodeInterpreter.EvaluateConditions(
            "(((1 == 1)))", ""
        );

        Assert.True(result);
    }

    [Fact]
    public void Precedence_Test()
    {
        bool result = GXCodeInterpreter.GXCodeInterpreter.EvaluateConditions(
            "false || true && false", ""
        );

        Assert.False(result);
    }

    [Fact]
    public void Unknown_Variable()
    {
        Assert.Throws<GXCodeInterpreterError>(() =>
            GXCodeInterpreter.GXCodeInterpreter.EvaluateCondition("abc == 1"));
    }

    [Fact]
    public void Missing_Right_Operand()
    {
        Assert.Throws<GXCWrongConditionError>(() =>
            GXCodeInterpreter.GXCodeInterpreter.EvaluateConditions("1 == 1 &&", ""));
    }

    [Fact]
    public void Missing_Left_Operand()
    {
        Assert.Throws<GXCWrongConditionError>(() =>
            GXCodeInterpreter.GXCodeInterpreter.EvaluateConditions("&& 1 == 1", ""));
    }

    [Fact]
    public void Unmatched_Left_Parenthesis()
    {
        Assert.Throws<GXCWrongConditionError>(() =>
            GXCodeInterpreter.GXCodeInterpreter.EvaluateConditions("(1 == 1", ""));
    }

    [Fact]
    public void Unmatched_Right_Parenthesis()
    {
        Assert.Throws<GXCWrongConditionError>(() =>
            GXCodeInterpreter.GXCodeInterpreter.EvaluateConditions("1 == 1)", ""));
    }

    [Fact]
    public void Empty_Condition()
    {
        Assert.Throws<GXCWrongConditionError>(() =>
            GXCodeInterpreter.GXCodeInterpreter.EvaluateConditions("", ""));
    }
}