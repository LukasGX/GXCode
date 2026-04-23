using System;
using System.Runtime.Serialization;

namespace GXCodeInterpreter
{
    [Serializable]
    public class GXCodeError(string id, string message) : Exception(message)
    {
        public string Id { get; } = id;
    }

    [Serializable]
    public class GXCIndeterminableLineError(string line) : GXCodeError("GX0001", $"Indeterminable line structure: {line}") {}

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