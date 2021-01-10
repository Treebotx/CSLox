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

        private enum ClassType
        {
            NONE,
            CLASS,
            SUBCLASS
        }

        private ClassType _currentClass = ClassType.NONE;

        private readonly Interpreter _interpreter;
        private readonly IErrorReporter _errorReporter;
        private readonly IList<IDictionary<string, VarInitilized>> _scopes = new List<IDictionary<string, VarInitilized>>();

        private FunctionTypes CurrentFunctionType { get; set; } = FunctionTypes.NONE;

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

        public object VisitClassStmt(Stmt.Class stmt)
        {
            var enclosingClass = _currentClass;
            _currentClass = ClassType.CLASS;

            Declare(stmt.name);
            Define(stmt.name);

            if ((stmt.superClass != null) &&
                stmt.name.Lexeme.Equals(stmt.superClass.name.Lexeme))
            {
                _errorReporter.Error(stmt.superClass.name, "A class cannot inherit from itself.");
            }

            if (stmt.superClass != null)
            {
                _currentClass = ClassType.SUBCLASS;
                Resolve(stmt.superClass);
            }

            if (stmt.superClass != null)
            {
                BeginScope();
                _scopes.Peek()["super"] = VarInitilized.IS_INITILIZED;
            }

            BeginScope();

            _scopes.Peek()["this"] = VarInitilized.IS_INITILIZED;

            foreach (var method in stmt.methods)
            {
                var declaration = FunctionTypes.METHOD;
                if (method.name.Lexeme.Equals("init")) declaration = FunctionTypes.INITIALIZER;
                ResolveFunction(method, declaration);
            }

            EndScope();

            if (stmt.superClass != null) EndScope();

            _currentClass = enclosingClass;

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

            ResolveFunction(stmt, FunctionTypes.FUNCTION);

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
            if (CurrentFunctionType == FunctionTypes.NONE) _errorReporter.Error(stmt.keyword, "Cannot return from top-level code.");

            if (stmt.value != null)
            {
                if (CurrentFunctionType == FunctionTypes.INITIALIZER)
                {
                    _errorReporter.Error(stmt.keyword, "Cannot return a value from an initializer.");
                }

                Resolve(stmt.value);
            }

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

        public object VisitGetExpr(Expr.Get expr)
        {
            Resolve(expr.obj);

            return null;
        }

        public object VisitSetExpr(Expr.Set expr)
        {
            Resolve(expr.value);
            Resolve(expr.obj);

            return null;
        }

        public object VisitSuperExpr(Expr.Super expr)
        {
            if (_currentClass == ClassType.NONE) _errorReporter.Error(expr.keyword, "Cannot use 'super' outside of a class.");
            else if (_currentClass != ClassType.SUBCLASS)
                _errorReporter.Error(expr.keyword, "Cannot use 'super' in a class with no superclass.");
            ResolveLocal(expr, expr.keyword);

            return null;
        }

        public object VisitThisExpr(Expr.This expr)
        {
            if (_currentClass == ClassType.NONE)
            {
                _errorReporter.Error(expr.keyword, "Cannot use 'this' outside of a class.");
                return null;
            }

            ResolveLocal(expr, expr.keyword);

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

        private void ResolveFunction(Stmt.Function function, FunctionTypes type)
        {
            var enclosingFunction = CurrentFunctionType;
            CurrentFunctionType = type;

            BeginScope();

            foreach (var parameter in function.parameters)
            {
                Declare(parameter);
                Define(parameter);
            }

            Resolve(function.body);

            EndScope();

            CurrentFunctionType = enclosingFunction;
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
