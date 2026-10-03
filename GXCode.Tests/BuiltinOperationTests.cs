using Xunit;
using GXCodeInterpreter;

namespace GXCode.Tests;

public class BuiltinOperationTests
{
    [Fact]
    public void Exit()
    {
        Assert.Throws<GXCodeBreak>(() =>
        {
            string content = """
            #ns abc

            entrypoint() {
                exit;
                str a = "HI";
            }
            """;

            List<string> lines = GXCodeHelper.SplitCode(content);
            GXCodeRoot.Start(content, lines);
        });
    }
}