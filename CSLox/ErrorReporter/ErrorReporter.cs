using System;

namespace CSLox
{
    public class ErrorReporter : IErrorReporter
    {
        public bool HadError { get; private set; } = false;
        public bool HadRuntimeError { get; private set; } = false;

        public void Error(int line, string message)
        {
            Report(line, "", message);
        }

        public void Error(Token token, string message)
        {
            if (token.Type == TokenType.EOF)
            {
                Report(token.Line, " at end", message);
            }
            else
            {
                Report(token.Line, $" at '{token.Lexeme}'", message);
            }
        }

        public void RuntimeError(LoxRuntimeErrorException error)
        {
            Console.Error.WriteLine($"{error.Message}\n[line {error.ErrorToken.Line}]");
            HadRuntimeError = true;
        }

        public void Reset()
        {
            HadError = false;
            HadRuntimeError = false;
        }

        private void Report(int line, string where, string message)
        {
            Console.Error.WriteLine($"[line {line}] Error{where}: {message}");
            HadError = true;
        }
    }
}
