using System;
using System.Collections.Generic;
using System.IO;

namespace CSLox
{
    class Program
    {
        static readonly IErrorReporter errorReporter = new ErrorReporter();
        private static Interpreter interpreter = new Interpreter(errorReporter);

        static int Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.Error.WriteLine("Usage: cslox [script]");
                return (int)ExitCodes.INVALID_ARGUMENT;
            }
            else if (args.Length == 1)
            {
                RunFile(args[0]);
            }
            else
            {
                RunPrompt();
            }

            return (int)ExitCodes.SUCCESS;
        }

        private static void RunPrompt()
        {
            for (; ; )
            {
                Console.Write("> ");
                var line = Console.ReadLine();
                if (string.IsNullOrEmpty(line)) break;
                Run(line);
                errorReporter.Reset();
            }
        }

        private static void RunFile(string path)
        {
            if (path == null) throw new ArgumentNullException();
            if (path.Length == 0) throw new ArgumentException();

            string file = string.Empty;
            using (StreamReader streamReader = new StreamReader(path))
            {
                file = streamReader.ReadToEnd();
            }

            Run(file);

            if (errorReporter.HadError) Environment.Exit((int)ExitCodes.ERROR_IN_CODE);
            if (errorReporter.HadRuntimeError) Environment.Exit((int)ExitCodes.RUNTIME_ERROR);
        }

        private static void Run(string source)
        {
            Scanner scanner = new Scanner(source, errorReporter);
            List<Token> tokens = scanner.ScanTokens();

            Parser parser = new Parser(tokens, errorReporter);
            Expr expr = parser.Parse();

            //if (errorReporter.HadError) return;
            if (expr == null) return;

            //Console.WriteLine(new AstPrinter().Print(expr));
            interpreter.Interpret(expr);
        }
    }
}
