using System;
using System.Collections.Generic;

namespace CSLox
{
    public class Interpreter : Expr.IVisitor<object>, Stmt.IVisitor<object>
    {
        private IErrorReporter _errorReporter;
        private LoxEnvironment _environment = new LoxEnvironment();

        public Interpreter(IErrorReporter errorReporter)
        {
            _errorReporter = errorReporter;
        }

         public void Interpret(IList<Stmt> statements)
        {
            try
            {
                foreach (var statement in statements)
                {
                    Execute(statement);
                }
            }
            catch (LoxRuntimeErrorException error)
            {
                _errorReporter.RuntimeError(error);
            }
        }

        public object VisitAssignExpr(Expr.Assign expr)
        {
            object value = Evaluate(expr.value);
            _environment.Assign(expr.name, value);

            return value;
        }

        public object VisitBinaryExpr(Expr.Binary expr)
        {
            object left = Evaluate(expr.left);
            object right = Evaluate(expr.right);

            switch (expr.oper.Type)
            {
                case TokenType.GREATER:
                    CheckNumberOperands(expr.oper, left, right);
                    return (double)left > (double)right;
                case TokenType.GREATER_EQUAL:
                    CheckNumberOperands(expr.oper, left, right);
                    return (double)left >= (double)right;
                case TokenType.LESS:
                    CheckNumberOperands(expr.oper, left, right);
                    return (double)left < (double)right;
                case TokenType.LESS_EQUAL:
                    CheckNumberOperands(expr.oper, left, right);
                    return (double)left <= (double)right;
                case TokenType.BANG_EQUAL:
                    return !IsEqual(left, right);
                case TokenType.EQUAL_EQUAL:
                    return IsEqual(left, right);
                case TokenType.MINUS:
                    CheckNumberOperands(expr.oper, left, right);
                    return (double)left - (double)right;
                case TokenType.PLUS:
                    if (left is double && right is double)
                    {
                        return (double)left + (double)right;
                    }

                    if (left is string || right is string)
                    {
                        return left.ToString() + right.ToString();
                    }
                    throw new LoxRuntimeErrorException(expr.oper,
                        "Operands must be two numbers or two strings.");
                case TokenType.SLASH:
                    CheckNumberOperands(expr.oper, left, right);
                    if ((double)right == 0d) throw new LoxRuntimeErrorException(expr.oper, "Attempted divide by zero.");
                    return (double)left / (double)right;
                case TokenType.STAR:
                    CheckNumberOperands(expr.oper, left, right);
                    return (double)left * (double)right;
            }

            // Unreachable
            return null;
        }

        public object VisitGroupingExpr(Expr.Grouping expr)
        {
            return Evaluate(expr.expression);
        }

        public object VisitLiteralExpr(Expr.Literal expr)
        {
            return expr.value;
        }

        public object VisitUnaryExpr(Expr.Unary expr)
        {
            object right = Evaluate(expr.right);

            switch (expr.oper.Type)
            {
                case TokenType.BANG:
                    return !IsTruthy(right);
                case TokenType.MINUS:
                    CheckNumberOperand(expr.oper, right);
                    return -(double)right;
            }

            // Unreachable
            return null;
        }

        public object VisitVariableExpr(Expr.Variable expr)
        {
            return _environment.Get(expr.name);
        }

        public object VisitExpressionStmt(Stmt.Expression stmt)
        {
            Evaluate(stmt.expression);

            return null;
        }

        public object VisitPrintStmt(Stmt.Print stmt)
        {
            object value = Evaluate(stmt.expression);
            Console.WriteLine(Stringify(value));

            return null;
        }

        public object VisitVarStmt(Stmt.Var stmt)
        {
            object value = null;
            if (stmt.initilizer != null)
            {
                value = Evaluate(stmt.initilizer);
            }

            _environment.Define(stmt.name.Lexeme, value);

            return null;
        }

        public object VisitBlockStmt(Stmt.Block stmt)
        {
            ExecuteBlock(stmt.statements, new LoxEnvironment(_environment));
            return null;
        }

        private bool IsTruthy(object obj)
        {
            if (obj == null) return false;
            if (obj is bool) return (bool)obj;
            return true;
        }

        private bool IsEqual(object left, object right)
        {
            if (left == null && right == null) return true;
            if (left == null) return false;

            return left.Equals(right);
        }

        private string Stringify(object obj)
        {
            if (obj == null) return "nil";

            if (obj is double)
            {
                string text = obj.ToString();
                return text;
            }

            if (obj is bool) return obj.ToString().ToLower();

            return obj.ToString();
        }

        private object Evaluate(Expr expr)
        {
            return expr.Accept(this);
        }

        private void Execute(Stmt stmt)
        {
            stmt.Accept(this);
        }

        private void ExecuteBlock(IList<Stmt> statements, LoxEnvironment environment)
        {
            LoxEnvironment previous = _environment;

            try
            {
                _environment = environment;

                foreach (var statement in statements)
                {
                    Execute(statement);
                }
            }
            finally
            {
                _environment = previous;
            }
        }

        private void CheckNumberOperand(Token oper, object operand)
        {
            if (operand is double) return;

            throw new LoxRuntimeErrorException(oper, "Operand must be a number.");
        }

        private void CheckNumberOperands(Token oper, object left, object right)
        {
            if (left is double && right is double) return;

            throw new LoxRuntimeErrorException(oper, "Operands must be a numbers.");
        }
    }
}