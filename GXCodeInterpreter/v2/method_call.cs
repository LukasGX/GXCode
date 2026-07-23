using System.Text;
using System.Text.RegularExpressions;

namespace GXCodeInterpreter;

public partial class GXCodeInterpreter
{
    public static void CallMethod(GXCodeEnvironment env, string line, int lineNr, string block)
    {
        string pattern = @"^\s*([a-zA-Z0-9_]+)\.([a-zA-Z0-9_]+)\((.*)\)";
        Match m = Regex.Match(line, pattern);
        if (!m.Success) throw new GXCodeInterpreterError("Could not detect method call");

        string instName = m.Groups[1].Value;
        string methodName = m.Groups[2].Value;
        string parameters = m.Groups[3].Value;

        // check instName
        if (!GXCodeProgram.scopeStack.Peek().TryGet(instName, out object? value, out var type) || type is null)
            throw new GXCUndeclaredVariableError(lineNr, instName, block);

        string instPattern = @"^inst<(.*)>";
        Match instM = Regex.Match(type, instPattern);

        if (value is not GXCodeClassInstance || !instM.Success)
            throw new GXCNotAnInstanceError(lineNr, instName, block);

        // get class name
        string className = instM.Groups[1].Value;

        // get method
        GXC_CS_CLASS refClass = env.blocks.Values
            .OfType<GXC_CS_CLASS>()
            .FirstOrDefault(c => c.Name == className)
            ?? throw new GXCodeInterpreterError($"Class {className} not found");

        GXC_CS_METHOD method = env.blocks.Values
            .OfType<GXC_CS_METHOD>()
            .FirstOrDefault(m =>
                m.Name == methodName &&
                m.ParentClass == refClass.ID)
            ?? throw new GXCodeInterpreterError($"Method {className}.{methodName} not found");

        // execute method
        ExecuteBlock(env, method);
    }
}