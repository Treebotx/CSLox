using System;

namespace CSLox
{
    [Serializable]
    public class LoxRuntimeErrorException : Exception
    {
        public LoxRuntimeErrorException() : base() { }
        public LoxRuntimeErrorException(string message) : base(message) { }
        public LoxRuntimeErrorException(string message, Exception innerException) : base(message, innerException) { }

        public LoxRuntimeErrorException(Token token, string message) : base(message)
        {
            ErrorToken = token;
        }

        protected LoxRuntimeErrorException(System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context) : base(info, context) { }

        public Token ErrorToken { get; private set; }
    }
}
