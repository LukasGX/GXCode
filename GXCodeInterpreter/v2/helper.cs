namespace GXCodeInterpreter;
class GXCodeHelper
{
    public static bool DebuggingEnabled = true;

    public static List<string> SplitCode(string code)
    {
        string[] ll = code.Split("\n");
        List<string> lines = [.. ll];
        return lines;
    }
    public static void Debug(string message) {
        if (!GXCodeHelper.DebuggingEnabled) return;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"[DEBUG] {message}");
        Console.ResetColor();
    }
}