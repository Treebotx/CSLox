using System;

namespace CSLox
{
    public class ErrorReporter : IErrorReporter
    {
        public bool HadError { get; private set; }

        public void Error(int line, string message)
        {
            Report(line, "", message);
        }

        public void Reset()
        {
            HadError = false;
        }

        private void Report(int line, string where, string message)
        {
            Console.Error.WriteLine($"[line {line}] Error{where}: {message}");
            HadError = true;
        }
    }
}
