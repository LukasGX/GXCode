using System;
using System.Runtime.Serialization;

namespace GXCodeInterpreter
{
    [Serializable]
    public class GXCodeError(string id, string message, string? block = null, int lineNr = 0) : Exception(message)
    {
        public string Id { get; } = id;
        public int LineNr { get; } = lineNr;
        public string? Block { get; } = block;
    }

    [Serializable]
    public class GXCIndeterminableLineError(string line, int lineNr, string? block) : GXCodeError("GX0001", $"Indeterminable line structure: {line}", block, lineNr) {}

    [Serializable]
    public class GXCNothingToCloseError(int lineNr, string? block) : GXCodeError("GX0002", $"Cannot close undefined block", block, lineNr) {}

    [Serializable]
    public class GXCMultipleEntrypointError(int lineNr, string? block) : GXCodeError("GX0003", $"Multiple entrypoint definitions", block, lineNr) {}

    [Serializable]
    public class GXCStrayElseIfError(int lineNr, string? block) : GXCodeError("GX0004", $"Else if without matching if block", block, lineNr) {}

    [Serializable]
    public class GXCStrayElseError(int lineNr, string? block) : GXCodeError("GX0005", $"Else without matching if block", block, lineNr) {}

    [Serializable]
    public class GXCStrayCaseError(int lineNr, string? block) : GXCodeError("GX0006", $"Case without matching switch block", block, lineNr) {}

    [Serializable]
    public class GXCNestedEntrypointError(int lineNr, string nest, string? block) : GXCodeError("GX0007", $"Entrypoint definition must be at the top level, but is nested in {nest}", block, lineNr) {}

    [Serializable]
    public class GXCStrayBlockError(int lineNr, string blockType, bool classLevel, string? block) : GXCodeError("GX0008", $"{blockType} block not allowed at {(classLevel ? "class" : "top")} level", block, lineNr) {}

    [Serializable]
    public class GXCStrayBuiltinOperationError(int lineNr, bool classLevel, string? block) : GXCodeError("GX0009", $"Built-in operations are not allowed at {(classLevel ? "class" : "top")} level", block, lineNr) {}

    [Serializable]
    public class GXCStrayVariableDeclarationError(int lineNr, string? block) : GXCodeError("GX0010", $"Variable declarations are not allowed at top level", block, lineNr) {}

    [Serializable]
    public class GXCStrayVariableAssignmentError(int lineNr, string? block) : GXCodeError("GX0011", $"Variable assignments are not allowed at top level", block, lineNr) {}

    [Serializable]
    public class GXCMissingEntrypointError(string? block) : GXCodeError("GX0012", $"Missing entry point", block, 0) {}

    [Serializable]
    public class GXCWrongTypeError(int lineNr, string tried, string should, string? block) : GXCodeError("GX0013", $"{tried} cannot be used as {should}", block, lineNr) {}

    [Serializable]
    public class GXCUnsupportedTypeError(int lineNr, string tried, string? block) : GXCodeError("GX0014", $"{tried} is not a valid variable type", block, lineNr) {}

    [Serializable]
    public class GXCUndeclaredVariableError(int lineNr, string tried, string? block) : GXCodeError("GX0015", $"Unknown variable: {tried}", block, lineNr) {}

    [Serializable]
    public class GXCodeBreak : Exception
    {
        public GXCodeBreak()
            : base("")
        {
        }
    }

    [Serializable]
    public class GXCodeInterpreterError : Exception
    {
        public GXCodeInterpreterError(string message)
            : base(message)
        {
        }
    }
}