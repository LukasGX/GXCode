using System.Text.RegularExpressions;

namespace GXCodeInterpreter;

partial class GXCodeInterpreter
{
    public static void DeclareInstance(string line, int lineNr, string block, GXCodeEnvironment env)
    {
        string pattern = @"^\s*inst<(.*)>\s+([a-zA-Z0-9_]+)\s*=\s*(.*?);$";
        Match m = Regex.Match(line, pattern);
        if (!m.Success) throw new GXCodeInterpreterError("Could not detect instance declaration");

        string basis = m.Groups[1].Value;
        string name = m.Groups[2].Value;
        string initiator = m.Groups[3].Value;

        string initiatorPattern = @"^\s*new\s*\((.*)\)$";
        Match initiatorMatch = Regex.Match(initiator, initiatorPattern);
        if (!initiatorMatch.Success) throw new GXCWrongInstanceInitiatorError(lineNr, block);

        // execute init block
        object classDefRaw = env.blocks.FirstOrDefault(c => c.Value is GXC_CS_CLASS cls && cls.Name == basis).Value ?? throw new GXCodeInterpreterError($"Could not find class {basis}");
        GXC_CS_CLASS classDef = (GXC_CS_CLASS)classDefRaw;
        GXC_CS_INIT? initDef = null;
        if (classDef.InitBlock != -1)
        {
            object initDefObj = env.blocks[classDef.InitBlock];
            initDef = (GXC_CS_INIT)initDefObj;
        }
        if (initDef == null || classDef.InitBlock == -1) throw new GXCClassMissingInitError(lineNr, basis, block);
        ExecuteBlock(env, initDef);

        GXCodeClassInstance typedValue = new();
        string storedType = $"inst<{basis}>";
        GXCodeProgram.scopeStack.Peek().Set(name, typedValue, storedType, isConst: false);
    }
}