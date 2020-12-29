using System;

namespace CSLox
{
    [Serializable]
    public class LoxParseErrorException : Exception
    {
        public LoxParseErrorException() : base() { }
        public LoxParseErrorException(string message) : base(message) { }
        public LoxParseErrorException(string message, Exception innerException) : base(message, innerException) { }

        protected LoxParseErrorException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
