using System;
using System.Collections.Generic;

namespace CSLox
{
    public class Parser
    {
        /*
         * program       -> declaration* EOF ;
         * declaration   -> varDecl | statement ;
         * varDecl       -> "var" IDENTIFIER ( "=" expression)? ";" ;
         * statement     -> exprStmt | printStmt | block ;
         * block         -> "{" declaration* "}" ;
         * exprStmt      -> expression ";" ;
         * printStmt     -> "print" expression ";" ;
         * expression    -> assignment ;
         * assignment    -> IDENTIFIER "=" assignment | equality ;
         * equality      -> comparison ( ( "!=" | "==") comparison )* ;
         * comparison    -> term ( ( ">" | ">=" | "<" | "<=" ) term )* ;
         * term          -> factor ( ( "-" | "+" ) factor )* ;
         * factor        -> unary ( ( "/" | "*" ) unary )* ;
         * unary         -> ( "!" | "-" ) unary | primary ;
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

        // declaration   -> varDecl | statement ;
        private Stmt Declaration()
        {
            try
            {
                if (Match(TokenType.VAR)) return VarDeclaration();

                return Statement();
            }
            catch (LoxParseErrorException error)
            {
                Synchronise();
                return null;
            }
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

        // statement     -> exprStmt | printStmt | block ;
        private Stmt Statement()
        {
            if (Match(TokenType.PRINT)) return PrintStatement();
            if (Match(TokenType.LEFT_BRACE)) return new Stmt.Block(Block());

            return ExpressionStatement();
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

        // expression    -> assignment ;
        private Expr Expression()
        {
            return Assignment();
        }

        // assignment    -> IDENTIFIER "=" assignment | equality ;
        private Expr Assignment()
        {
            Expr expr = Equality();

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

        // unary         -> ( "!" | "-" ) unary | primary ;
        private Expr Unary()
        {
            if (Match(TokenType.BANG, TokenType.MINUS))
            {
                Token oper = Previous();
                Expr right = Unary();
                return new Expr.Unary(oper, right);
            }

            return Primary();
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