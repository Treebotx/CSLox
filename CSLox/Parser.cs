using System.Collections.Generic;

namespace CSLox
{
    public class Parser
    {
        /*
         * expression    -> equality ;
         * equality      -> comparison ( ( "!=" | "==") comparison )* ;
         * comparison    -> term ( ( ">" | ">=" | "<" | "<=" ) term )* ;
         * term          -> factor ( ( "-" | "+" ) factor )* ;
         * factor        -> unary ( ( "/" | "*" ) unary )* ;
         * unary         -> ( "!" | "-" ) unary | primary ;
         * primary       -> NUMBER | STRING | "true" | "false" | "nil" | "(" expression ")" ;
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

        public Expr Parse()
        {
            try
            {
                return Expression();
            }
            catch (ParseErrorException error)
            {
                return null;
            }
        }

        // expression    -> equality ;
        private Expr Expression()
        {
            return Equality();
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
        // primary       -> NUMBER | STRING | "true" | "false" | "nil" | "(" expression ")" ;
        private Expr Primary()
        {
            if (Match(TokenType.FALSE)) return new Expr.Literal(false);
            if (Match(TokenType.TRUE)) return new Expr.Literal(true);
            if (Match(TokenType.NIL)) return new Expr.Literal(null);

            if (Match(TokenType.NUMBER, TokenType.STRING)) return new Expr.Literal(Previous().Literal);

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

        private ParseErrorException Error(Token token, string message)
        {
            _errorReporter.Error(token, message);
            return new ParseErrorException();
        }
    }
}
