using Moq;
using Xunit;

namespace CSLox.Tests
{
    public class ParserTests
    {
        static Mock<IErrorReporter> erMock = new Mock<IErrorReporter>();
        static IErrorReporter _errorReporter = erMock.Object;

        [Theory]
        [InlineData("1 == 2", "==", 1D, 2D)]
        [InlineData("\"yyy\" != \"x\"", "!=", "yyy", "x")]
        [InlineData("560.50 >= 5.4", ">=", 560.50D, 5.4D)]
        [InlineData("9 < 10000", "<", 9D, 10000D)]
        [InlineData("\"asdfghjkl\" <= 0", "<=", "asdfghjkl", 0D)]
        [InlineData("1 - \"qwertyuiop\"", "-", 1D, "qwertyuiop")]
        [InlineData("1 + 1", "+", 1D, 1D)]
        [InlineData("1 / 1", "/", 1D, 1D)]
        [InlineData("1 * 1", "*", 1D, 1D)]
        public void Parser_ReturnsBinaryExpression(string exprString, string expOper, object expLeft, object expRight)
        {
            var tokens = new Scanner(exprString, _errorReporter).ScanTokens();

            var p = new Parser(tokens, _errorReporter);

            var expr = p.Parse();

            Assert.IsType<Expr.Binary>(expr);

            var binaryExpr = expr as Expr.Binary;
            var left = binaryExpr.left as Expr.Literal;
            var oper = binaryExpr.oper;
            var right = binaryExpr.right as Expr.Literal;

            Assert.True(expLeft.Equals(left.value));
            Assert.Equal(expOper, oper.Lexeme);
            Assert.True(expRight.Equals(right.value));
        }

        [Theory]
        [InlineData("!1", "!", 1D)]
        [InlineData("-2", "-", 2D)]
        public void Parser_ReturnsUnaryExpression(string exprString, string expOper, object expRight)
        {
            var tokens = new Scanner(exprString, _errorReporter).ScanTokens();

            var p = new Parser(tokens, _errorReporter);

            var expr = p.Parse();

            Assert.IsType<Expr.Unary>(expr);

            var unaryExpr = expr as Expr.Unary;
            var oper = unaryExpr.oper;
            var right = unaryExpr.right as Expr.Literal;
            Assert.Equal(expOper, oper.Lexeme);
            Assert.True(expRight.Equals(right.value));
        }

        [Theory]
        [InlineData("1", 1D)]
        [InlineData("2", 2D)]
        [InlineData("\"string\"", "string")]
        [InlineData("true", true)]
        [InlineData("false", false)]
        public void Parser_ReturnsLiteralExpression(string exprString, object expLiteral)
        {
            var tokens = new Scanner(exprString, _errorReporter).ScanTokens();

            var p = new Parser(tokens, _errorReporter);

            var expr = p.Parse();

            Assert.IsType<Expr.Literal>(expr);

            var literalExpr = expr as Expr.Literal;
            Assert.True(expLiteral.Equals(literalExpr.value));
        }

        [Fact]
        public void Parser_NilReturnsLiteralWithANullValue()
        {
            var tokens = new Scanner("nil", _errorReporter).ScanTokens();

            var p = new Parser(tokens, _errorReporter);

            var expr = p.Parse();

            Assert.IsType<Expr.Literal>(expr);

            var literalExpr = expr as Expr.Literal;
            Assert.Null(literalExpr.value);
        }

        [Theory]
        [InlineData("1 + 2 * 3", TokenType.PLUS)]
        [InlineData("1 * 2 + 3", TokenType.PLUS)]
        [InlineData("1 * (2 + 3)", TokenType.STAR)]
        [InlineData("1 == 2 + 3", TokenType.EQUAL_EQUAL)]
        [InlineData("1 != 2 * 3", TokenType.BANG_EQUAL)]
        [InlineData("1 != !2", TokenType.BANG_EQUAL)]
        [InlineData("1 + 2 - 3", TokenType.MINUS)]
        [InlineData("1 - 2 + 3", TokenType.PLUS)]
        [InlineData("1 * 2 / 3", TokenType.SLASH)]
        [InlineData("1 / 2 * 3", TokenType.STAR)]
        public void Parser_LowestPrecedenceOperatorShouldBeRootOperator(string exprString, TokenType expType)
        {
            var tokens = new Scanner(exprString, _errorReporter).ScanTokens();

            var p = new Parser(tokens, _errorReporter);

            var expr = p.Parse();

            var binaryExpr = expr as Expr.Binary;

            Assert.Equal(expType, binaryExpr.oper.Type);
        }
    }
}
