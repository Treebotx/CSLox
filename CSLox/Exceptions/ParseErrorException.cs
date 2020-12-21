using System;

namespace CSLox
{
    [Serializable]
    public class ParseErrorException : Exception
    {
        public ParseErrorException() : base() { }
        public ParseErrorException(string message) : base(message) { }
        public ParseErrorException(string message, Exception innerException) : base(message, innerException) { }

        protected ParseErrorException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
