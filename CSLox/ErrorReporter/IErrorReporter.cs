namespace CSLox
{
    public interface IErrorReporter
    {
        bool HadError { get; }

        void Error(int line, string message);
        void Error(Token token, string message);
        void Reset();
    }
}