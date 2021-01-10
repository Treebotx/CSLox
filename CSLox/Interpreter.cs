using System;
using System.Collections.Generic;

namespace CSLox
{
    public class Interpreter : Expr.IVisitor<object>, Stmt.IVisitor<object>
    {
        private IErrorReporter _errorReporter;
        private LoxEnvironment Globals { get; } = new LoxEnvironment();
        private readonly IDictionary<Expr, int> _locals = new Dictionary<Expr, int>();

        private LoxEnvironment _environment;

        public Interpreter(IErrorReporter errorReporter)
        {
            _errorReporter = errorReporter;
            _environment = Globals;
            Globals.Define("clock", new Clock());
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

            if (_locals.TryGetValue(expr, out var distance))
            {
                _environment.AssignAt(distance, expr.name, value);
            }
            else
            {
                Globals.Assign(expr.name, value);
            }

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

        public object VisitCallExpr(Expr.Call expr)
        {
            var callee = Evaluate(expr.callee);

            var arguments = new List<object>();
            foreach (var argument in expr.arguments)
            {
                arguments.Add(Evaluate(argument));
            }

            if (!(callee is ILoxCallable function))
            {
                throw new LoxRuntimeErrorException(expr.paren, "Can only call functions and classes.");
            }

            if (arguments.Count != function.Arity)
            {
                throw new LoxRuntimeErrorException(expr.paren,
                    $"Expected {function.Arity} arguments but got {arguments.Count}.");
            }

            return function.Call(this, arguments);
        }

        public object VisitGetExpr(Expr.Get expr)
        {
            var obj = Evaluate(expr.obj);
            if (obj is LoxInstance loxInstance)
            {
                return loxInstance.Get(expr.name);
            }

            throw new LoxRuntimeErrorException(expr.name, "Only instances have properties.");
        }

        public object VisitSetExpr(Expr.Set expr)
        {
            var obj = Evaluate(expr.obj);

            var loxInstance = obj as LoxInstance;

            if (obj is null)
            {
                throw new LoxRuntimeErrorException(expr.name, "Only instances have fields.");
            }

            var value = Evaluate(expr.value);
            loxInstance.Set(expr.name, value);
            return value;
        }

        public object VisitSuperExpr(Expr.Super expr)
        {
            var distance = _locals[expr];
            var superClass = _environment.GetAt(distance, "super") as LoxClass;
            var obj = _environment.GetAt(distance - 1, "this") as LoxInstance;

            var method = superClass.FindMethod(expr.method.Lexeme);

            if (method is null) throw new LoxRuntimeErrorException(expr.method, $"Undefined property '{expr.method.Lexeme}'.");

            return method.Bind(obj);
        }

        public object VisitThisExpr(Expr.This expr)
        {
            return LookUpVariable(expr.keyword, expr);
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

        public object VisitLogicalExpr(Expr.Logical expr)
        {
            object left = Evaluate(expr.left);

            if (expr.oper.Type == TokenType.OR)
            {
                if (IsTruthy(left)) return left;
            }
            else
            {
                if (!IsTruthy(left)) return left;
            }

            return Evaluate(expr.right);
        }

        public object VisitVariableExpr(Expr.Variable expr)
        {
            return LookUpVariable(expr.name, expr);
        }

        private object LookUpVariable(Token name, Expr expr)
        {
            if (_locals.TryGetValue(expr, out var distance))
            {
                return _environment.GetAt(distance, name.Lexeme);
            }
            else
            {
                return Globals.Get(name);
            }
        }

        public object VisitExpressionStmt(Stmt.Expression stmt)
        {
            Evaluate(stmt.expression);

            return null;
        }

        public object VisitFunctionStmt(Stmt.Function stmt)
        {
            var function = new LoxFunction(stmt, _environment);
            _environment.Define(stmt.name.Lexeme, function);

            return null;
        }

        public object VisitPrintStmt(Stmt.Print stmt)
        {
            object value = Evaluate(stmt.expression);
            Console.WriteLine(Stringify(value));

            return null;
        }

        public object VisitReturnStmt(Stmt.Return stmt)
        {
            object value = null;
            if (stmt.value != null) value = Evaluate(stmt.value);

            throw new Return(value);
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

        public object VisitClassStmt(Stmt.Class stmt)
        {
            object superClass = null;
            if (stmt.superClass != null)
            {
                superClass = Evaluate(stmt.superClass);
                if (!(superClass is LoxClass))
                {
                    throw new LoxRuntimeErrorException(stmt.superClass.name, "Superclass must be a class.");
                }
            }

            _environment.Define(stmt.name.Lexeme, null);

            if (stmt.superClass != null)
            {
                _environment = new LoxEnvironment(_environment);
                _environment.Define("super", superClass);
            }

            var methods = new Dictionary<string, LoxFunction>();
            foreach (var method in stmt.methods)
            {
                var functionType = method.name.Lexeme.Equals("init")
                    ? FunctionTypes.INITIALIZER
                    : FunctionTypes.NONE;
                methods[method.name.Lexeme] = new LoxFunction(method, _environment, functionType);
            }

            var loxClass = new LoxClass(stmt.name.Lexeme, superClass as LoxClass, methods);

            if (superClass != null) _environment = _environment.Enclosing;
            _environment.Assign(stmt.name, loxClass);

            return null;
        }

        public object VisitIfStmt(Stmt.If stmt)
        {
            if (IsTruthy(Evaluate(stmt.condition)))
            {
                Execute(stmt.thenBranch);
            }
            else if (stmt.elseBranch != null)
            {
                Execute(stmt.elseBranch);
            }

            return null;
        }

        public object VisitWhileStmt(Stmt.While stmt)
        {
            while (IsTruthy(Evaluate(stmt.condition)))
            {
                Execute(stmt.body);
            }

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

        public void Resolve(Expr expr, int depth)
        {
            _locals.Add(expr, depth);
        }

        public void ExecuteBlock(IList<Stmt> statements, LoxEnvironment environment)
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