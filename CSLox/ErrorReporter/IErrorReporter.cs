namespace CSLox
{
    public interface IErrorReporter
    {
        bool HadError { get; }
        bool HadRuntimeError { get; }

        void Error(int line, string message);
        void Error(Token token, string message);
        void RuntimeError(LoxRuntimeErrorException error);
        void Reset();
    }
}