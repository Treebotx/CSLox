using Moq;
using Xunit;

namespace CSLox.Tests
{
    public class ScannerTests
    {
        static Mock<IErrorReporter> erMock = new Mock<IErrorReporter>();
        static IErrorReporter _errorReporter = erMock.Object;

        [Fact]
        public void Scanner_ReturnsAnEofTokenIfInputIsEmpty()
        {
            var scanner = new Scanner("", _errorReporter);

            var tokens = scanner.ScanTokens();

            Assert.Single(tokens);
            Assert.Equal(TokenType.EOF, tokens[0].Type);
        }

        [Theory]
        [InlineData("(", TokenType.LEFT_PAREN)]
        [InlineData(")", TokenType.RIGHT_PAREN)]
        [InlineData("{", TokenType.LEFT_BRACE)]
        [InlineData("}", TokenType.RIGHT_BRACE)]
        [InlineData(",", TokenType.COMMA)]
        [InlineData(".", TokenType.DOT)]
        [InlineData("-", TokenType.MINUS)]
        [InlineData("+", TokenType.PLUS)]
        [InlineData(";", TokenType.SEMICOLON)]
        [InlineData("/", TokenType.SLASH)]
        [InlineData("*", TokenType.STAR)]
        [InlineData("!", TokenType.BANG)]
        [InlineData("!=", TokenType.BANG_EQUAL)]
        [InlineData("=", TokenType.EQUAL)]
        [InlineData("==", TokenType.EQUAL_EQUAL)]
        [InlineData(">", TokenType.GREATER)]
        [InlineData(">=", TokenType.GREATER_EQUAL)]
        [InlineData("<", TokenType.LESS)]
        [InlineData("<=", TokenType.LESS_EQUAL)]
        public void Scanner_ReturnsCorrectTokenType(string token, TokenType expected)
        {
            var scanner = new Scanner(token, _errorReporter);

            var tokens = scanner.ScanTokens();

            Assert.Equal(2, tokens.Count);
            Assert.Equal(expected, tokens[0].Type);
            Assert.Equal(token, tokens[0].Lexeme);
            Assert.Null(tokens[0].Literal);
            Assert.Equal(TokenType.EOF, tokens[1].Type);
        }

        [Theory]
        [InlineData("string")]
        [InlineData("string\n")]
        [InlineData("\nstring")]
        [InlineData("str\ning")]
        [InlineData("\nstr\ning\n")]
        public void Scanner_ReturnsAString(string str)
        {
            string expected = $"\"{str}\"";
            var scanner = new Scanner(expected, _errorReporter);

            var tokens = scanner.ScanTokens();

            Assert.Equal(2, tokens.Count);
            Assert.Equal(TokenType.STRING, tokens[0].Type);
            Assert.Equal(expected, tokens[0].Lexeme);
            Assert.Equal(str, tokens[0].Literal);
            Assert.Equal(TokenType.EOF, tokens[1].Type);
        }

        [Theory]
        [InlineData("1.5", 1.5)]
        [InlineData("1", 1.0)]
        [InlineData("0", 0)]
        [InlineData("1.1234567", 1.1234567)]
        public void Scanner_ReturnsANumber(string str, double expected)
        {
            var scanner = new Scanner(str, _errorReporter);

            var tokens = scanner.ScanTokens();

            Assert.Equal(2, tokens.Count);
            Assert.Equal(TokenType.NUMBER, tokens[0].Type);
            Assert.Equal(str, tokens[0].Lexeme);
            Assert.Equal(expected, tokens[0].Literal);
            Assert.Equal(TokenType.EOF, tokens[1].Type);
        }

        [Theory]
        [InlineData("xxx")]
        [InlineData("_x_x")]
        [InlineData("x1111111")]
        [InlineData("anda")]
        [InlineData("classa")]
        [InlineData("elsex")]
        [InlineData("false1")]
        [InlineData("funt")]
        [InlineData("fort")]
        [InlineData("ifz")]
        [InlineData("nilzy")]
        [InlineData("orz")]
        [InlineData("printlsefgsdrefgbgldslgjh")]
        [InlineData("returnsssss")]
        [InlineData("supers")]
        [InlineData("thisx")]
        [InlineData("truethy")]
        [InlineData("vart")]
        [InlineData("whilex")]
        public void Scanner_ReturnsAnIdentifier(string str)
        {
            var scanner = new Scanner(str, _errorReporter);

            var tokens = scanner.ScanTokens();

            Assert.Equal(2, tokens.Count);
            Assert.Equal(TokenType.IDENTIFIER, tokens[0].Type);
            Assert.Equal(str, tokens[0].Lexeme);
            Assert.Null(tokens[0].Literal);
            Assert.Equal(TokenType.EOF, tokens[1].Type);
        }

        [Theory]
        [InlineData("and", TokenType.AND)]
        [InlineData("class", TokenType.CLASS)]
        [InlineData("else", TokenType.ELSE)]
        [InlineData("false", TokenType.FALSE)]
        [InlineData("fun", TokenType.FUN)]
        [InlineData("for", TokenType.FOR)]
        [InlineData("if", TokenType.IF)]
        [InlineData("nil", TokenType.NIL)]
        [InlineData("or", TokenType.OR)]
        [InlineData("print", TokenType.PRINT)]
        [InlineData("return", TokenType.RETURN)]
        [InlineData("super", TokenType.SUPER)]
        [InlineData("this", TokenType.THIS)]
        [InlineData("true", TokenType.TRUE)]
        [InlineData("var", TokenType.VAR)]
        [InlineData("while", TokenType.WHILE)]
        public void Scanner_ReturnsAKeyword(string str, TokenType expected)
        {
            var scanner = new Scanner(str, _errorReporter);

            var tokens = scanner.ScanTokens();

            Assert.Equal(2, tokens.Count);
            Assert.Equal(expected, tokens[0].Type);
            Assert.Equal(str, tokens[0].Lexeme);
            Assert.Null(tokens[0].Literal);
            Assert.Equal(TokenType.EOF, tokens[1].Type);
        }

        [Theory]
        [InlineData("//")]
        [InlineData("//comment")]
        [InlineData("/**/")]
        [InlineData("/*comment*/")]
        [InlineData("/*\n*/")]
        [InlineData("/*\ncomment*/")]
        [InlineData("/* comment\n*/")]
        [InlineData("/* comment \n after*/")]
        public void Scanner_IgnoresComments(string str)
        {
            var scanner = new Scanner(str, _errorReporter);

            var tokens = scanner.ScanTokens();

            Assert.Single(tokens);
            Assert.Equal(TokenType.EOF, tokens[0].Type);
        }

        [Theory]
        [InlineData("and and", 3)]
        [InlineData("\"string\" class", 3)]
        [InlineData("if a > 5.05", 5)]
        [InlineData("5+5", 4)]
        [InlineData("var a=\"newline\n\";", 6)]
        [InlineData("var // ignored comment \n literal", 3)]
        [InlineData("/* comment with newline \n after newline */", 1)]
        public void Scanner_ReturnsMultipleTokensPlusEOF(string str, int expected)
        {
            var scanner = new Scanner(str, _errorReporter);

            var tokens = scanner.ScanTokens();

            Assert.Equal(expected, tokens.Count);
            Assert.Equal(TokenType.EOF, tokens[expected - 1].Type);
        }

        [Fact]
        public void Scanner_ReportsAnErrorIfStringIsUnterminated()
        {
            var scanner = new Scanner("\"unterminated", erMock.Object);

            scanner.ScanTokens();

            erMock.Verify(m => m.Error(It.IsAny<int>(), It.Is<string>(s => s == "Unterminated string.")));
        }

        [Fact]
        public void Scanner_ReportsAnErrorIfAnUnexpectedCharacterIsUsed()
        {
            var scanner = new Scanner("un@un", erMock.Object);

            scanner.ScanTokens();

            erMock.Verify(m => m.Error(It.IsAny<int>(), It.Is<string>(s => s == "Unexpected character.")));
        }

        [Fact]
        public void Scanner_ReportsAnErrorIfACommentIsUnterminated()
        {
            var scanner = new Scanner("unterminated /* aaaaa", erMock.Object);

            scanner.ScanTokens();

            erMock.Verify(m => m.Error(It.IsAny<int>(), It.Is<string>(s => s == "Unterminated comment.")));
        }
    }
}
