using System.Collections.Generic;

namespace CSLox
{
    public class Resolver : Expr.IVisitor<object>, Stmt.IVisitor<object>
    {
        private enum VarInitilized
        {
            IS_INITILIZED,
            IS_NOT_INITILIZED
        }

        private enum FunctionType
        {
            NONE,
            FUNCTION
        }


        private readonly Interpreter _interpreter;
        private readonly IErrorReporter _errorReporter;
        private readonly IList<IDictionary<string, VarInitilized>> _scopes = new List<IDictionary<string, VarInitilized>>();

        private FunctionType CurrentFunction { get; set; } = FunctionType.NONE;

        public Resolver(Interpreter interpreter, IErrorReporter errorReporter)
        {
            _interpreter = interpreter;
            _errorReporter = errorReporter;
        }

        public object VisitBlockStmt(Stmt.Block stmt)
        {
            BeginScope();
            Resolve(stmt.statements);
            EndScope();

            return null;
        }

        public object VisitExpressionStmt(Stmt.Expression stmt)
        {
            Resolve(stmt.expression);
            return null;
        }

        public object VisitFunctionStmt(Stmt.Function stmt)
        {
            Declare(stmt.name);
            Define(stmt.name);

            ResolveFunction(stmt, FunctionType.FUNCTION);

            return null;
        }

        public object VisitIfStmt(Stmt.If stmt)
        {
            Resolve(stmt.condition);
            Resolve(stmt.thenBranch);
            if (stmt.elseBranch != null) Resolve(stmt.elseBranch);

            return null;
        }

        public object VisitPrintStmt(Stmt.Print stmt)
        {
            Resolve(stmt.expression);
            return null;
        }

        public object VisitReturnStmt(Stmt.Return stmt)
        {
            if (CurrentFunction == FunctionType.NONE) _errorReporter.Error(stmt.keyword, "Cannot return from top-level code.");

            if (stmt.value != null) Resolve(stmt.value);
            return null;
        }

        public object VisitWhileStmt(Stmt.While stmt)
        {
            Resolve(stmt.condition);
            Resolve(stmt.body);

            return null;
        }

        public object VisitVarStmt(Stmt.Var stmt)
        {
            Declare(stmt.name);
            if (stmt.initilizer != null) Resolve(stmt.initilizer);
            Define(stmt.name);

            return null;
        }

        private void Declare(Token name)
        {
            if (_scopes.IsEmpty()) return;

            var scope = _scopes.Peek();
            if (scope.ContainsKey(name.Lexeme))
            {
                _errorReporter.Error(name, "There is already a variable with this name in this scope.");
                return;
            }

            scope.Add(name.Lexeme, VarInitilized.IS_NOT_INITILIZED);
        }

        public void Define(Token name)
        {
            if (_scopes.IsEmpty()) return;

            _scopes.Peek()[name.Lexeme] = VarInitilized.IS_INITILIZED;
        }

        public object VisitAssignExpr(Expr.Assign expr)
        {
            Resolve(expr.value);
            ResolveLocal(expr, expr.name);

            return null;
        }

        public object VisitVariableExpr(Expr.Variable expr)
        {
            if (_scopes.IsNotEmpty() && _scopes.Peek().TryGetValue(expr.name.Lexeme, out var initilized))
            {
                if (initilized == VarInitilized.IS_NOT_INITILIZED)
                {
                    _errorReporter.Error(expr.name, "Cannot read local variable in it's own initilizer.");
                }
            }

            ResolveLocal(expr, expr.name);

            return null;
        }

        public object VisitBinaryExpr(Expr.Binary expr)
        {
            Resolve(expr.left);
            Resolve(expr.right);

            return null;
        }

        public object VisitCallExpr(Expr.Call expr)
        {
            Resolve(expr.callee);

            foreach (var argument in expr.arguments) Resolve(argument);

            return null;
        }

        public object VisitGroupingExpr(Expr.Grouping expr)
        {
            Resolve(expr.expression);

            return null;
        }

        public object VisitLiteralExpr(Expr.Literal expr)
        {
            return null;
        }

        public object VisitLogicalExpr(Expr.Logical expr)
        {
            Resolve(expr.left);
            Resolve(expr.right);

            return null;
        }

        public object VisitUnaryExpr(Expr.Unary expr)
        {
            Resolve(expr.right);

            return null;
        }

        public void Resolve(IList<Stmt> statements)
        {
            foreach (var statement in statements) Resolve(statement);
        }

        private void Resolve(Stmt stmt)
        {
            stmt.Accept(this);
        }

        private void Resolve(Expr expr)
        {
            expr.Accept(this);
        }

        private void ResolveLocal(Expr expr, Token name)
        {
            for (var i = _scopes.Count - 1; i >= 0; i--)
            {
                if (_scopes[i].ContainsKey(name.Lexeme))
                {
                    _interpreter.Resolve(expr, _scopes.Count - 1 - i);
                    return;
                }
            }
        }

        private void ResolveFunction(Stmt.Function function, FunctionType type)
        {
            var enclosingFunction = CurrentFunction;
            CurrentFunction = type;

            BeginScope();

            foreach (var parameter in function.parameters)
            {
                Declare(parameter);
                Define(parameter);
            }

            Resolve(function.body);

            EndScope();

            CurrentFunction = enclosingFunction;
        }

        private void BeginScope()
        {
            _scopes.Push(new Dictionary<string, VarInitilized>());
        }

        private void EndScope()
        {
            _scopes.Pop();
        }
    }
}
