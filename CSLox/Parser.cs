using System;
using System.Collections.Generic;

namespace CSLox
{
    public class Parser
    {
        /*
         * program       -> declaration* EOF ;
         * declaration   -> funDecl
         *                | varDecl
         *                | statement ;
         * funDecl       -> "fun" function ;
         * function      -> IDENTIFIER "(" parameters? ")" block ;
         * parameters    -> IDENTIFIER ( "," IDENTIFIER )* ;
         * varDecl       -> "var" IDENTIFIER ( "=" expression)? ";" ;
         * statement     -> exprStmt
         *                | forStmt
         *                | ifStmt
         *                | forStmt
         *                | printStmt
         *                | returnStmt
         *                | whileStmt
         *                | block ;
         * returnStmt    -> "return" expression? ";" ;
         * forStmt        | "for" "(" ( varDecl | exprStmt | ";" )
         *                  expression? ";"
         *                  expression? ";" statement ;
         * whileStmt     -> "while" "(" expression ")" statement ;
         * ifStmt        -> "if" "(" expression ")" statement
         *                ( "else" statement )? ;
         * block         -> "{" declaration* "}" ;
         * exprStmt      -> expression ";" ;
         * printStmt     -> "print" expression ";" ;
         * arguments     -> expression ( "," expression )* ;
         * expression    -> assignment ;
         * assignment    -> IDENTIFIER "=" assignment
         *                | logic_or ;
         * logic_or      -> logic_and ( "or" logic_and )* ;
         * logic_and     -> equality ( "and" equality )* ;
         * equality      -> comparison ( ( "!=" | "==") comparison )* ;
         * comparison    -> term ( ( ">" | ">=" | "<" | "<=" ) term )* ;
         * term          -> factor ( ( "-" | "+" ) factor )* ;
         * factor        -> unary ( ( "/" | "*" ) unary )* ;
         * unary         -> ( "!" | "-" ) unary | call ;
         * call          -> primary ( "(" arguments? ") )* ;
         * primary       -> "true" | "false" | "nil"
         *                | NUMBER | STRING
         *                | "(" expression ")"
         *                | IDENTIFIER ;
         */

        private List<Token> _tokens;
        private readonly IErrorReporter _errorReporter;
        private readonly TokenType[] _binaryOperators =
        {
            TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL,
            TokenType.GREATER, TokenType.GREATER_EQUAL,
            TokenType.LESS, TokenType.LESS_EQUAL,
            TokenType.MINUS, TokenType.PLUS,
            TokenType.SLASH, TokenType.STAR
        };
        private int _current = 0;

        private bool IsAtEnd => Peek().Type == TokenType.EOF;

        public Parser(List<Token> tokens, IErrorReporter errorReporter)
        {
            _tokens = tokens;
            _errorReporter = errorReporter;
        }

        public IList<Stmt> Parse()
        {
            var statements = new List<Stmt>();
            
            while (!IsAtEnd)
            {
                statements.Add(Declaration());
            }

            return statements;
        }

        // declaration   -> funDecl
        //                | varDecl
        //                | statement ;
        private Stmt Declaration()
        {
            try
            {
                if (Match(TokenType.FUN)) return Function("function");
                if (Match(TokenType.VAR)) return VarDeclaration();

                return Statement();
            }
            catch (LoxParseErrorException error)
            {
                Synchronise();
                return null;
            }
        }

        // funDecl       -> "fun" function ;
        // function      -> IDENTIFIER "(" parameters? ")" block ;
        private Stmt Function(string kind)
        {
            var name = Consume(TokenType.IDENTIFIER, $"Expected {kind} name.");
            Consume(TokenType.LEFT_PAREN, $"Expected '(' after {kind} name.");

            var paramaters = new List<Token>();

            if (!Check(TokenType.RIGHT_PAREN))
            {
                do
                {
                    if (paramaters.Count > 255)
                    {
                        Error(Peek(), "Can't have more than 255 parameters.");
                    }

                    paramaters.Add(Consume(TokenType.IDENTIFIER, "Expected parameter name."));
                } while (Match(TokenType.COMMA));
            }

            Consume(TokenType.RIGHT_PAREN, "Expected ')' after parameters.");

            Consume(TokenType.LEFT_BRACE, $"Expected '{{' before {kind} body.");
            var body = Block();

            return new Stmt.Function(name, paramaters, body);
        }

        // varDecl       -> "var" IDENTIFIER( "=" expression)? ";" ;
        private Stmt VarDeclaration()
        {
            Token name = Consume(TokenType.IDENTIFIER, "Expect variable name.");

            Expr initilizer = null;
            if (Match(TokenType.EQUAL))
            {
                initilizer = Expression();
            }

            Consume(TokenType.SEMICOLON, "Expect ';' after variable declaration.");
            return new Stmt.Var(name, initilizer);
        }

        // statement     -> exprStmt
        //                | forStmt
        //                | ifStmt
        //                | printStmt
        //                | returnStmt
        //                | whileStmt
        //                | block ;
        private Stmt Statement()
        {
            if (Match(TokenType.FOR)) return ForStatement();
            if (Match(TokenType.IF)) return IfStatement();
            if (Match(TokenType.PRINT)) return PrintStatement();
            if (Match(TokenType.RETURN)) return ReturnStatement();
            if (Match(TokenType.WHILE)) return WhileStatement();
            if (Match(TokenType.LEFT_BRACE)) return new Stmt.Block(Block());

            return ExpressionStatement();
        }

        // forStmt        | "for" "(" (varDecl | exprStmt | ";" )
        //                  expression? ";"
        //                  expression? ";" statement ;
        private Stmt ForStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expected '(' after 'for'.");

            Stmt initializer;
            if (Match(TokenType.SEMICOLON)) initializer = null;
            else if (Match(TokenType.VAR)) initializer = VarDeclaration();
            else initializer = ExpressionStatement();

            Expr condition = null;
            if (!Check(TokenType.SEMICOLON))
            {
                condition = Expression();
            }
            Consume(TokenType.SEMICOLON, "Expected ';' after loop condition.");

            Expr increment = null;
            if (!Check(TokenType.RIGHT_PAREN))
            {
                increment = Expression();
            }
            Consume(TokenType.RIGHT_PAREN, "Expected ')' after for clauses.");

            Stmt body = Statement();

            if (increment != null)
            {
                body = new Stmt.Block(new List<Stmt>
                {
                    body,
                    new Stmt.Expression(increment)
                });
            }

            if (condition == null) condition = new Expr.Literal(true);
            body = new Stmt.While(condition, body);

            if (initializer != null)
            {
                body = new Stmt.Block(new List<Stmt>
                {
                    initializer,
                    body
                });
            }

            return body;
        }

        // ifStmt        -> "if" "(" expression ")" statement
        //                ( "else" statement )? ;
        private Stmt IfStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after 'if'.");
            Expr condition = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after if condition.");

            Stmt thenBranch = Statement();

            Stmt elseBranch = null;
            if (Match(TokenType.ELSE))
            {
                elseBranch = Statement();
            }

            return new Stmt.If(condition, thenBranch, elseBranch);
        }

        // block         -> "{" declaration* "}" ;
        private IList<Stmt> Block()
        {
            IList<Stmt> statements = new List<Stmt>();

            while (!Check(TokenType.RIGHT_BRACE) && !IsAtEnd)
            {
                statements.Add(Declaration());
            }

            Consume(TokenType.RIGHT_BRACE, "Expect '}' after block.");

            return statements;
        }

        // exprStmt      -> expression ";" ;
        private Stmt ExpressionStatement()
        {
            Expr expr = Expression();
            Consume(TokenType.SEMICOLON, "Expect ';' after expression.");

            return new Stmt.Expression(expr);
        }

        // printStmt     -> "print" expression ";" ;
        private Stmt PrintStatement()
        {
            Expr value = Expression();
            Consume(TokenType.SEMICOLON, "Expect ';' after value.");

            return new Stmt.Print(value);
        }

        // returnStmt    -> "return" expression? ";" ;
        private Stmt ReturnStatement()
        {
            Token keyword = Previous();
            Expr value = null;
            if (!Check(TokenType.SEMICOLON))
            {
                value = Expression();
            }

            Consume(TokenType.SEMICOLON, "Expected ';' after return value.");

            return new Stmt.Return(keyword, value);
        }

        // whileStmt     -> "while" "(" expression ")" statement ;
        private Stmt WhileStatement()
        {
            Consume(TokenType.LEFT_PAREN, "Expect '(' after 'while'.");
            Expr condition = Expression();
            Consume(TokenType.RIGHT_PAREN, "Expect ')' after condition.");
            Stmt body = Statement();

            return new Stmt.While(condition, body);
        }

        // expression    -> assignment ;
        private Expr Expression()
        {
            return Assignment();
        }

        // assignment    -> IDENTIFIER "=" assignment
        //                | logic_or ;
        private Expr Assignment()
        {
            Expr expr = Or();

            if (Match(TokenType.EQUAL))
            {
                Token equals = Previous();
                Expr value = Assignment();

                if (expr is Expr.Variable variable)
                {
                    Token name = variable.name;
                    return new Expr.Assign(name, value);
                }

                Error(equals, "Invalid assignment target.");
            }

            return expr;
        }

        // logic_or      -> logic_and( "or" logic_and )* ;
        private Expr Or()
        {
            Expr expr = And();

            while (Match(TokenType.OR))
            {
                Token oper = Previous();
                Expr right = And();
                expr = new Expr.Logical(expr, oper, right);
            }

            return expr;
        }

        // logic_and     -> equality( "and" equality )* ;

        private Expr And()
        {
            Expr expr = Equality();

            while (Match(TokenType.AND))
            {
                Token oper = Previous();
                Expr right = Equality();
                expr = new Expr.Logical(expr, oper, right);
            }

            return expr;
        }

        // equality      -> comparison ( ( "!=" | "==") comparison )* ;
        private Expr Equality()
        {
            Expr expr = Comparison();

            while (Match(TokenType.BANG_EQUAL, TokenType.EQUAL_EQUAL))
            {
                Token oper = Previous();
                Expr right = Comparison();
                expr = new Expr.Binary(expr, oper, right);
            }

            return expr;
        }

        // comparison    -> term ( ( ">" | ">=" | "<" | "<=" ) term )* ;
        private Expr Comparison()
        {
            Expr expr = Term();

            while (Match(TokenType.GREATER, TokenType.GREATER_EQUAL,
                         TokenType.LESS,     TokenType.LESS_EQUAL))
            {
                Token oper = Previous();
                Expr right = Term();
                expr = new Expr.Binary(expr, oper, right);
            }

            return expr;
        }

        // term          -> factor ( ( "-" | "+" ) factor )* ;
        private Expr Term()
        {
            Expr expr = Factor();

            while (Match(TokenType.MINUS, TokenType.PLUS))
            {
                Token oper = Previous();
                Expr right = Factor();
                expr = new Expr.Binary(expr, oper, right);
            }

            return expr;
        }

        // factor        -> unary ( ( "/" | "*" ) unary )* ;
        private Expr Factor()
        {
            Expr expr = Unary();

            while (Match(TokenType.SLASH, TokenType.STAR))
            {
                Token oper = Previous();
                Expr right = Unary();
                expr = new Expr.Binary(expr, oper, right);
            }

            return expr;
        }

        // unary         -> ( "!" | "-" ) unary | call ;
        private Expr Unary()
        {
            if (Match(TokenType.BANG, TokenType.MINUS))
            {
                Token oper = Previous();
                Expr right = Unary();
                return new Expr.Unary(oper, right);
            }

            return Call();
        }

        // call          -> primary( "(" arguments? ") )* ;
        private Expr Call()
        {
            Expr expr = Primary();

            while (true)
            {
                if (Match(TokenType.LEFT_PAREN))
                {
                    expr = FinishCall(expr);
                }
                else
                {
                    break;
                }
            }

            return expr;
        }

        private Expr FinishCall(Expr callee)
        {
            var arguments = new List<Expr>();
            if (!Check(TokenType.RIGHT_PAREN))
            {
                do
                {
                    if (arguments.Count > 255)
                    {
                        Error(Peek(), "Can't have more than 255 arguments.");
                    }

                    arguments.Add(Expression());
                } while (Match(TokenType.COMMA)) ;
            }

            var paren = Consume(TokenType.RIGHT_PAREN, "Expected ')' after arguments.");

            return new Expr.Call(callee, paren, arguments);
        }

        // primary       -> "true" | "false" | "nil"
        //                | NUMBER | STRING
        //                | "(" expression ")"
        //                | IDENTIFIER ;
        private Expr Primary()
        {
            if (Match(TokenType.FALSE)) return new Expr.Literal(false);
            if (Match(TokenType.TRUE)) return new Expr.Literal(true);
            if (Match(TokenType.NIL)) return new Expr.Literal(null);

            if (Match(TokenType.NUMBER, TokenType.STRING)) return new Expr.Literal(Previous().Literal);

            if (Match(TokenType.IDENTIFIER)) return new Expr.Variable(Previous());

            if (Match(TokenType.LEFT_PAREN))
            {
                Expr expr = Expression();
                Consume(TokenType.RIGHT_PAREN, "Expected ')' after expression.");
                return new Expr.Grouping(expr);
            }

            if (Match(_binaryOperators))
            {
                Error(Previous(), "Unexpected binary operator.");
                return Expression();
            }

            throw Error(Peek(), "Expected expression.");
        }

        private Token Peek()
        {
            return _tokens[_current];
        }

        private Token Advance()
        {
            if (!IsAtEnd) _current++;
            return Previous();
        }

        private Token Previous()
        {
            return _tokens[_current - 1];
        }

        private bool Match(params TokenType[] types)
        {
            foreach (var type in types)
            {
                if (Check(type))
                {
                    Advance();
                    return true;
                }
            }

            return false;
        }

        private Token Consume(TokenType type, string message)
        {
            if (Check(type)) return Advance();

            throw Error(Peek(), message);
        }

        private bool Check(TokenType type)
        {
            if (IsAtEnd) return false;
            return Peek().Type == type;
        }

        private void Synchronise()
        {
            Advance();

            while (!IsAtEnd)
            {
                if (Previous().Type == TokenType.SEMICOLON) return;

                switch (Peek().Type)
                {
                    case TokenType.CLASS:
                        return;
                    case TokenType.FUN:
                        return;
                    case TokenType.VAR:
                        return;
                    case TokenType.FOR:
                        return;
                    case TokenType.IF:
                        return;
                    case TokenType.WHILE:
                        return;
                }

                Advance();
            }
        }

        private LoxParseErrorException Error(Token token, string message)
        {
            _errorReporter.Error(token, message);
            return new LoxParseErrorException();
        }
    }
}