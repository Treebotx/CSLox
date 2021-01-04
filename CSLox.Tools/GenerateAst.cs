using System;
using System.Collections.Generic;
using System.IO;

namespace CSLox.Tools
{
    class GenerateAst
    {
        static int Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.Error.WriteLine("Usage: generateast <output directory>");
                return 64;
            }

            string outputDir = args[0];

            DefineAst(outputDir, "Expr", new List<string>
            {
                "Assign   : Token name, Expr value",
                "Binary   : Expr left, Token oper, Expr right",
                "Call     : Expr callee, Token paren, IList<Expr> arguments",
                "Grouping : Expr expression",
                "Literal  : object value",
                "Logical  : Expr left, Token oper, Expr right",
                "Unary    : Token oper, Expr right",
                "Variable : Token name"
            });

            DefineAst(outputDir, "Stmt", new List<string>
            {
                "Block      : IList<Stmt> statements",
                "Function   : Token name, IList<Token> parameters," +
                            " IList<Stmt> body",
                "If         : Expr condition, Stmt thenBranch," +
                            " Stmt elseBranch",
                "Expression : Expr expression",
                "Print      : Expr expression",
                "Return     : Token keyword, Expr value",
                "Var        : Token name, Expr initilizer",
                "While      : Expr condition, Stmt body"
            });

            return 0;
        }

        private static void DefineAst(string outputDir, string baseName, List<string> types)
        {
            string path = $"{outputDir}/{baseName}.cs";

            using (StreamWriter writer = new StreamWriter(path))
            {
                writer.WriteLine($"using System.Collections.Generic;\n");
                writer.WriteLine($"{Indent()}namespace CSLox{OpenBrace()}");

                writer.WriteLine($"{Indent()}public abstract class {baseName}{OpenBrace()}");

                DefineVisitor(writer, baseName, types);

                // The AST classes.
                foreach (var type in types)
                {
                    string className = type.Split(":")[0].Trim();
                    string fields = type.Split(":")[1].Trim();
                    DefineType(writer, baseName, className, fields);
                }

                // The base accept() method.
                writer.WriteLine($"{Indent()}public abstract R Accept<R>(IVisitor<R> visitor);");

                writer.WriteLine($"{CloseBrace()}"); // end abstract class.

                writer.WriteLine($"{CloseBrace()}"); // End namespace.
            }
        }

        private static void DefineVisitor(StreamWriter writer, string baseName, List<string> types)
        {
            writer.WriteLine($"{Indent()}public interface IVisitor<R>{OpenBrace()}");

            foreach (var type in types)
            {
                string typeName = type.Split(":")[0].Trim();
                writer.WriteLine($"{Indent()}R Visit{typeName}{baseName} ( {typeName} {baseName.ToLower()} );");
            }

            writer.WriteLine($"{CloseBrace()}");
            writer.WriteLine();
        }

        private static void DefineType(StreamWriter writer, string baseName, string className, string fieldList)
        {
            writer.WriteLine($"{Indent()}public class {className} : {baseName}{OpenBrace()}");

            // Constructor.
            writer.WriteLine($"{Indent()}public {className} ( {fieldList} ){OpenBrace()}");

            // Store parameters in fields.
            string[] fields = fieldList.Split(", ");
            foreach (var field in fields)
            {
                var name = field.Split(" ")[1];
                writer.WriteLine($"{Indent()}this.{name} = {name};");
            }

            writer.WriteLine($"{CloseBrace()}"); // End constructer.

            // Visitor pattern.
            writer.WriteLine();
            writer.WriteLine($"{Indent()}public override R Accept<R>(IVisitor<R> visitor){OpenBrace()}");
            writer.WriteLine($"{Indent()}return visitor.Visit{className}{baseName}(this);");
            writer.WriteLine($"{CloseBrace()}");

            // Fields.
            writer.WriteLine();
            foreach (var field in fields)
            {
                writer.WriteLine($"{Indent()}public {field};");
            }

            writer.WriteLine($"{CloseBrace()}"); // End class declaration.
            writer.WriteLine();
        }

        private static int _indentCounter = 0;
        private static string _indentation = "\t";

        private static string OpenBrace()
        {
            string indent = $"\n{Indent()}{{";
            _indentCounter++;
            return indent;
        }

        private static string Indent()
        {
            string indent = string.Empty;

            for (var i = 0; i < _indentCounter; i++)
            {
                indent += _indentation;
            }
            return indent;
        }

        private static string CloseBrace()
        {

            if (_indentCounter > 0) _indentCounter--;
            string indent = $"{Indent()}}}";
            return indent;
        }
    }
}
