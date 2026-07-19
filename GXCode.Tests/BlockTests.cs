using Xunit;
using GXCodeInterpreter;

namespace GXCode.Tests;

public class BlockTests
{
    [Fact]
    public void Single_Entrypoint()
    {
        string code = """
        entrypoint() {
            out "Hello World";
        }
        """;
        List<string> lines = GXCodeHelper.SplitCode(code);

        ExecutionResult result = GXCodeRoot.Start(code, lines);
        GXCodeEnvironment? env = result.Env;

        Assert.NotNull(env);
        Assert.Single(env.blocks);

        var entrypoint = Assert.IsType<GXC_CS_ENTRYPOINT>(env.blocks[0]);

        Assert.Single(entrypoint.Lines);
        Assert.Equal("    out \"Hello World\";", entrypoint.Lines[0]);
    }

    [Fact]
    public void Single_If()
    {
        string code = """
        entrypoint() {
            bool condition = true;

            if (condition) {
                out "OK";
            }
        }
        """;
        List<string> lines = GXCodeHelper.SplitCode(code);

        ExecutionResult result = GXCodeRoot.Start(code, lines);
        GXCodeEnvironment? env = result.Env;

        Assert.NotNull(env);
        Assert.Equal(2, env.blocks.Count);

        Assert.IsType<GXC_CS_ENTRYPOINT>(env.blocks[0]);
        var ifBlock = Assert.IsType<GXC_CS_IF>(env.blocks[1]);

        Assert.Single(ifBlock.Lines);
        Assert.Equal("        out \"OK\";", ifBlock.Lines[0]);
    }

    [Fact]
    public void Single_ElseIf()
    {
        string code = """
        entrypoint() {
            bool condition = true;

            if (condition) {
                out "OK";
            }
            else if (condition) {
                out "Not OK";
            }
        }
        """;
        List<string> lines = GXCodeHelper.SplitCode(code);

        ExecutionResult result = GXCodeRoot.Start(code, lines);
        GXCodeEnvironment? env = result.Env;

        Assert.NotNull(env);
        Assert.Equal(3, env.blocks.Count);

        Assert.IsType<GXC_CS_ENTRYPOINT>(env.blocks[0]);
        var elseIfBlock = Assert.IsType<GXC_CS_ELSE_IF>(env.blocks[2]);

        Assert.Single(elseIfBlock.Lines);
        Assert.Equal("        out \"Not OK\";", elseIfBlock.Lines[0]);
    }

    [Fact]
    public void Single_Else()
    {
        string code = """
        entrypoint() {
            bool condition = true;

            if (condition) {
                out "OK";
            }
            else {
                out "Not OK";
            }
        }
        """;
        List<string> lines = GXCodeHelper.SplitCode(code);

        ExecutionResult result = GXCodeRoot.Start(code, lines);
        GXCodeEnvironment? env = result.Env;

        Assert.NotNull(env);
        Assert.Equal(3, env.blocks.Count);

        Assert.IsType<GXC_CS_ENTRYPOINT>(env.blocks[0]);
        var elseBlock = Assert.IsType<GXC_CS_ELSE>(env.blocks[2]);

        Assert.Single(elseBlock.Lines);
        Assert.Equal("        out \"Not OK\";", elseBlock.Lines[0]);
    }

    [Fact]
    public void Single_Switch()
    {
        string code = """
        entrypoint() {
            str char = "B";

            switch (char) {
                case "A" {
                    out 1;
                }
                case "B" {
                    out 2;
                }
            }
        }
        """;
        List<string> lines = GXCodeHelper.SplitCode(code);

        ExecutionResult result = GXCodeRoot.Start(code, lines);
        GXCodeEnvironment? env = result.Env;

        Assert.NotNull(env);
        Assert.Equal(4, env.blocks.Count);

        Assert.IsType<GXC_CS_ENTRYPOINT>(env.blocks[0]);

        var switchBlock = Assert.IsType<GXC_CS_SWITCH>(env.blocks[1]);
        Assert.Equal(2, switchBlock.Lines.Count);

        var caseBlock1 = Assert.IsType<GXC_CS_CASE>(env.blocks[2]);
        Assert.Single(caseBlock1.Lines);
        Assert.Equal("            out 1;", caseBlock1.Lines[0]);

        var caseBlock2 = Assert.IsType<GXC_CS_CASE>(env.blocks[3]);
        Assert.Single(caseBlock2.Lines);
        Assert.Equal("            out 2;", caseBlock2.Lines[0]);
    }

    [Fact]
    public void Single_Repeat()
    {
        string code = """
        entrypoint() {
            repeat (10) {
                out "Hello World!";
            }
        }
        """;
        List<string> lines = GXCodeHelper.SplitCode(code);

        ExecutionResult result = GXCodeRoot.Start(code, lines);
        GXCodeEnvironment? env = result.Env;

        Assert.NotNull(env);
        Assert.Equal(2, env.blocks.Count);

        Assert.IsType<GXC_CS_ENTRYPOINT>(env.blocks[0]);
        var repeatBlock = Assert.IsType<GXC_CS_REPEAT>(env.blocks[1]);

        Assert.Single(repeatBlock.Lines);
        Assert.Equal("        out \"Hello World!\";", repeatBlock.Lines[0]);
    }

    [Fact]
    public void Single_Iterate()
    {
        string code = """
        entrypoint() {
            str[] words = ["Hello", "World"];

            iterate (words) {
                out element;
            }
        }
        """;
        List<string> lines = GXCodeHelper.SplitCode(code);

        ExecutionResult result = GXCodeRoot.Start(code, lines);
        GXCodeEnvironment? env = result.Env;

        Assert.NotNull(env);
        Assert.Equal(2, env.blocks.Count);

        Assert.IsType<GXC_CS_ENTRYPOINT>(env.blocks[0]);
        var iterateBlock = Assert.IsType<GXC_CS_ITERATE>(env.blocks[1]);

        Assert.Single(iterateBlock.Lines);
        Assert.Equal("        out element;", iterateBlock.Lines[0]);
    }

    [Fact]
    public void Single_While()
    {
        string code = """
        entrypoint() {
            while (false) {
                out "y";
            }
        }
        """;
        List<string> lines = GXCodeHelper.SplitCode(code);

        ExecutionResult result = GXCodeRoot.Start(code, lines);
        GXCodeEnvironment? env = result.Env;

        Assert.NotNull(env);
        Assert.Equal(2, env.blocks.Count);

        Assert.IsType<GXC_CS_ENTRYPOINT>(env.blocks[0]);
        var whileBlock = Assert.IsType<GXC_CS_WHILE>(env.blocks[1]);

        Assert.Single(whileBlock.Lines);
        Assert.Equal("        out \"y\";", whileBlock.Lines[0]);
    }

    [Fact]
    public void Single_Class()
    {
        string code = """
        class Car {
            str color = "";
        }

        entrypoint() {
        }
        """;
        List<string> lines = GXCodeHelper.SplitCode(code);

        ExecutionResult result = GXCodeRoot.Start(code, lines);
        GXCodeEnvironment? env = result.Env;

        Assert.NotNull(env);
        Assert.Equal(2, env.blocks.Count);

        Assert.IsType<GXC_CS_ENTRYPOINT>(env.blocks[1]);
        var classBlock = Assert.IsType<GXC_CS_CLASS>(env.blocks[0]);

        Assert.Single(classBlock.Lines);
        Assert.Equal("    str color = \"\";", classBlock.Lines[0]);
    }

    [Fact]
    public void Single_Init()
    {
        string code = """
        class Car {
            str color = "";

            init() {
            }
        }

        entrypoint() {
        }
        """;
        List<string> lines = GXCodeHelper.SplitCode(code);

        ExecutionResult result = GXCodeRoot.Start(code, lines);
        GXCodeEnvironment? env = result.Env;

        Assert.NotNull(env);
        Assert.Equal(3, env.blocks.Count);

        Assert.IsType<GXC_CS_ENTRYPOINT>(env.blocks[2]);

        var classBlock = Assert.IsType<GXC_CS_CLASS>(env.blocks[0]);
        Assert.Single(classBlock.Lines);

        var initBlock = Assert.IsType<GXC_CS_INIT>(env.blocks[1]);
        Assert.Empty(initBlock.Lines);
    }

    [Fact]
    public void Single_Method()
    {
        string code = """
        class Car {
            str color = "";

            method ChangeColor(str newColor) {
                color = newColor;
            }
        }

        entrypoint() {
        }
        """;
        List<string> lines = GXCodeHelper.SplitCode(code);

        ExecutionResult result = GXCodeRoot.Start(code, lines);
        GXCodeEnvironment? env = result.Env;

        Assert.NotNull(env);
        Assert.Equal(3, env.blocks.Count);

        Assert.IsType<GXC_CS_ENTRYPOINT>(env.blocks[2]);

        var classBlock = Assert.IsType<GXC_CS_CLASS>(env.blocks[0]);
        Assert.Single(classBlock.Lines);

        var methodBlock = Assert.IsType<GXC_CS_METHOD>(env.blocks[1]);
        Assert.Single(methodBlock.Lines);
        Assert.Equal("        color = newColor;", methodBlock.Lines[0]);
    }

    [Fact]
    public void Single_ReturnMethod()
    {
        string code = """
        class Car {
            str color = "";

            str GetColor() {
                return color;
            }
        }

        entrypoint() {
        }
        """;
        List<string> lines = GXCodeHelper.SplitCode(code);

        ExecutionResult result = GXCodeRoot.Start(code, lines);
        GXCodeEnvironment? env = result.Env;

        Assert.NotNull(env);
        Assert.Equal(3, env.blocks.Count);

        Assert.IsType<GXC_CS_ENTRYPOINT>(env.blocks[2]);

        var classBlock = Assert.IsType<GXC_CS_CLASS>(env.blocks[0]);
        Assert.Single(classBlock.Lines);

        var returnMethodBlock = Assert.IsType<GXC_CS_RETURN_METHOD>(env.blocks[1]);
        Assert.Single(returnMethodBlock.Lines);
        Assert.Equal("        return color;", returnMethodBlock.Lines[0]);
    }
}