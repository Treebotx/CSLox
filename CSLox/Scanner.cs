using System.Collections.Generic;

namespace CSLox
{
    public class Scanner
    {
        private string _source;
        private readonly IErrorReporter _errorReporter;
        private List<Token> _tokens = new List<Token>();
        private int _start = 0;
        private int _current = 0;
        private int _length = 0;
        private int _line = 1;

        private static readonly Dictionary<string, TokenType> _keywords = new Dictionary<string, TokenType>
        {
            { "and",    TokenType.AND },
            { "class",  TokenType.CLASS },
            { "else",   TokenType.ELSE },
            { "false",  TokenType.FALSE },
            { "for",    TokenType.FOR },
            { "fun",    TokenType.FUN },
            { "if",     TokenType.IF },
            { "nil",    TokenType.NIL },
            { "or",     TokenType.OR },
            { "print",  TokenType.PRINT },
            { "return", TokenType.RETURN },
            { "super",  TokenType.SUPER },
            { "this",   TokenType.THIS },
            { "true",   TokenType.TRUE },
            { "var",    TokenType.VAR },
            { "while",  TokenType.WHILE }
        };

        public Scanner(string source, IErrorReporter errorReporter)
        {
            _source = source;
            _errorReporter = errorReporter;
        }

        private bool IsAtEnd => _current >= _source.Length;

        public List<Token> ScanTokens()
        {
            while (!IsAtEnd)
            {
                _start = _current;
                _length = 0;
                ScanToken();
            }

            _tokens.Add(new Token(TokenType.EOF, "", null, _line));
            return _tokens;
        }

        private void ScanToken()
        {
            var c = Advance();

            switch (c)
            {
                case '(': AddToken(TokenType.LEFT_PAREN); break;
                case ')': AddToken(TokenType.RIGHT_PAREN); break;
                case '{': AddToken(TokenType.LEFT_BRACE); break;
                case '}': AddToken(TokenType.RIGHT_BRACE); break;
                case ',': AddToken(TokenType.COMMA); break;
                case '.': AddToken(TokenType.DOT); break;
                case '-': AddToken(TokenType.MINUS); break;
                case '+': AddToken(TokenType.PLUS); break;
                case ';': AddToken(TokenType.SEMICOLON); break;
                case '*': AddToken(TokenType.STAR); break;
                case '!':
                    AddToken(Match('=') ? TokenType.BANG_EQUAL : TokenType.BANG);
                    break;
                case '=':
                    AddToken(Match('=') ? TokenType.EQUAL_EQUAL : TokenType.EQUAL);
                    break;
                case '<':
                    AddToken(Match('=') ? TokenType.LESS_EQUAL : TokenType.LESS);
                    break;
                case '>':
                    AddToken(Match('=') ? TokenType.GREATER_EQUAL : TokenType.GREATER);
                    break;
                case '/':
                    if (Match('/'))
                    {
                        while (Peek() != '\n' && !IsAtEnd) Advance();
                    }
                    else if (Match('*'))
                    {
                        ScanComments();
                    }
                    else
                    {
                        AddToken(TokenType.SLASH);
                    }
                    break;

                case ' ': break;
                case '\r': break;
                case '\t': break;

                case '\n': break;

                case '"': ScanString(); break;

                default:
                    if (IsDigit(c))
                    {
                        ScanNumber();
                    } else if (IsAlpha(c))
                    {
                        ScanIdentifier();
                    } else
                    {
                        _errorReporter.Error(_line, "Unexpected character.");
                    }
                    break;
            }
        }

        private char Advance()
        {
            var c = _source[_current];
            _current++;
            _length++;
            if (c == '\n') _line++;

            return c;
        }

        private char Peek()
        {
            if (IsAtEnd) return '\0';
            return _source[_current];
        }

        private char PeekNext()
        {
            if (_current + 1 >= _source.Length) return '\0';
            return _source[_current + 1];
        }

        private void ScanString()
        {
            while (Peek() != '"' && !IsAtEnd)
            {
                Advance();
            }

            if (IsAtEnd)
            {
                _errorReporter.Error(_line, "Unterminated string.");
                return;
            }

            Advance();

            var value = _source.Substring(_start + 1, _length - 2);
            AddToken(TokenType.STRING, value);
        }

        private void ScanNumber()
        {
            while (IsDigit(Peek())) Advance();

            if (Peek() == '.' && IsDigit(PeekNext()))
            {
                Advance();
                while (IsDigit(Peek())) Advance();
            }

            var value = _source.Substring(_start, _length);
            AddToken(TokenType.NUMBER, double.Parse(value));
        }

        private void ScanIdentifier()
        {
            while (IsAlphaNumeric(Peek())) Advance();

            string text = _source.Substring(_start, _length);

            if (!_keywords.TryGetValue(text, out TokenType type)) type = TokenType.IDENTIFIER;
            AddToken(type);
        }

        private void ScanComments()
        {
            while ( !IsAtEnd )
            {
                var c = Advance();
                if (c == '*')
                {
                    if (Peek() == '/')
                    {
                        Advance();
                        return;
                    }
                }
            }

            _errorReporter.Error(_line, "Unterminated comment.");
        }

        private void AddToken(TokenType type)
        {
            AddToken(type, null);
        }

        private void AddToken(TokenType type, object literal)
        {
            var text = _source.Substring(_start, _length);
            _tokens.Add(new Token(type, text, literal, _line));
        }

        private bool Match(char expected)
        {
            if (IsAtEnd) return false;
            if (_source[_current] != expected) return false;

            Advance();
            return true;
        }

        private bool IsDigit(char c)
        {
            return (c >= '0' && c <= '9');
        }

        private bool IsAlpha(char c)
        {
            return (c >= 'a' && c <= 'z') ||
                   (c >= 'A' && c <= 'Z') ||
                   (c == '_');
        }

        private bool IsAlphaNumeric(char c)
        {
            return (IsDigit(c) || IsAlpha(c));
        }
    }
}
