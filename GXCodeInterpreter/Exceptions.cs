using System;
using System.Runtime.Serialization;

namespace GXCodeInterpreter
{
    [Serializable]
    public class GXCodeError(string id, string message, int lineNr = 0) : Exception(message)
    {
        public string Id { get; } = id;
        public int LineNr { get; } = lineNr;
    }

    [Serializable]
    public class GXCIndeterminableLineError(string line, int lineNr) : GXCodeError("GX0001", $"Indeterminable line structure: {line}", lineNr) {}

    [Serializable]
    public class GXCNothingToCloseError(int lineNr) : GXCodeError("GX0002", $"Cannot close undefined block", lineNr) {}

    [Serializable]
    public class GXCMultipleEntrypointError(int lineNr) : GXCodeError("GX0003", $"Multiple entrypoint definitions", lineNr) {}

    [Serializable]
    public class GXCStrayElseIfError(int lineNr) : GXCodeError("GX0004", $"Else if without matching if block", lineNr) {}

    [Serializable]
    public class GXCStrayElseError(int lineNr) : GXCodeError("GX0005", $"Else without matching if block", lineNr) {}

    [Serializable]
    public class GXCStrayCaseError(int lineNr) : GXCodeError("GX0006", $"Case without matching switch block", lineNr) {}

    [Serializable]
    public class GXCNestedEntrypointError(int lineNr, string nest) : GXCodeError("GX0007", $"Entrypoint definition must be at the top level, but is nested in {nest}", lineNr) {}

    [Serializable]
    public class GXCStrayBlockError(int lineNr, string blockType, bool classLevel) : GXCodeError("GX0008", $"{blockType} block not allowed at {(classLevel ? "class" : "top")} level", lineNr) {}

    [Serializable]
    public class GXCStrayBuiltinOperationError(int lineNr, bool classLevel) : GXCodeError("GX0009", $"Built-in operations are not allowed at {(classLevel ? "class" : "top")} level", lineNr) {}

    [Serializable]
    public class GXCStrayVariableDeclarationError(int lineNr) : GXCodeError("GX0010", $"Variable declarations are not allowed at top level", lineNr) {}

    [Serializable]
    public class GXCStrayVariableAssignmentError(int lineNr) : GXCodeError("GX0011", $"Variable assignments are not allowed at top level", lineNr) {}

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