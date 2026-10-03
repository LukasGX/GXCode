using Xunit;
using GXCodeInterpreter;

namespace GXCode.Tests;

public class HelperTests
{
    [Fact]
    public void SplitCodeHelper()
    {
        string code = """
        #ns abc
        entrypoint() {
            out "Hello World";
        }
        """;

        List<string> lines = GXCodeHelper.SplitCode(code);

        Assert.Equal(4, lines.Count);
        Assert.Equal("#ns abc", lines[0]);
        Assert.Equal("entrypoint() {", lines[1]);
        Assert.Equal("    out \"Hello World\";", lines[2]);
        Assert.Equal("}", lines[3]);
    }

    [Fact]
    public void DebugPrintHelper()
    {
        var originalOut = Console.Out;
        var writer = new StringWriter();
        Console.SetOut(writer);

        GXCodeHelper.DebuggingEnabled = true;
        GXCodeHelper.Debug("Initialising...");

        Assert.Equal("[DEBUG] Initialising..." + System.Environment.NewLine, writer.ToString());

        Console.SetOut(originalOut);
    }

    [Fact]
    public void DebugPrintHelper_DebuggingDisabled()
    {
        var originalOut = Console.Out;
        var writer = new StringWriter();
        Console.SetOut(writer);

        GXCodeHelper.DebuggingEnabled = false;
        GXCodeHelper.Debug("Initialising...");

        Assert.Empty(writer.ToString());

        Console.SetOut(originalOut);
    }
}