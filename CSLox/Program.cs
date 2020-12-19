using System;
using System.Collections.Generic;
using System.IO;

namespace CSLox
{
    class Program
    {
        static readonly IErrorReporter errorReporter = new ErrorReporter();

        static int Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("UsageL cslox [script]");
                return (int)ExitCodes.INVALID_ARGUMENT;
            }
            else if (args.Length == 1)
            {
                runFile(args[0]);
            }
            else
            {
                runPrompt();
            }

            return (int)ExitCodes.SUCCESS;
        }

        private static void runPrompt()
        {
            for (; ; )
            {
                Console.Write("> ");
                var line = Console.ReadLine();
                if (string.IsNullOrEmpty(line)) break;
                run(line);
                errorReporter.Reset();
            }
        }

        private static void runFile(string path)
        {
            if (path == null) throw new ArgumentNullException();
            if (path.Length == 0) throw new ArgumentException();

            string file = string.Empty;
            using (StreamReader streamReader = new StreamReader(path))
            {
                file = streamReader.ReadToEnd();
            }

            run(file);

            if (errorReporter.HadError) Environment.Exit((int)ExitCodes.ERROR_IN_CODE);
        }

        private static void run(string source)
        {
            Scanner scanner = new Scanner(source, errorReporter);
            List<Token> tokens = scanner.ScanTokens();

            foreach (var token in tokens)
            {
                Console.WriteLine(token);
            }
        }
    }
}
