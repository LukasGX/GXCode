using System;
using System.Runtime.Serialization;

namespace GXCodeInterpreter
{
    [Serializable]
    public class GXCodeError : Exception
    {
        public string Id { get; }

        public GXCodeError(string id, string message)
            : base(message)
        {
            Id = id;
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