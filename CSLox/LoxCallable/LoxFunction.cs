using System.Collections.Generic;

namespace CSLox
{
    public partial class LoxFunction : ILoxCallable
    {

        public LoxFunction(Stmt.Function declaration, LoxEnvironment closure)
            : this(declaration, closure, FunctionTypes.NONE)
        {
            _closure = closure;
            _declaration = declaration;
        }

        public LoxFunction(Stmt.Function declaration, LoxEnvironment closure, FunctionTypes functionType)
        {
            _closure = closure;
            _declaration = declaration;
            _functionType = functionType;
        }

        public int Arity => _declaration.parameters.Count;

        public object Call(Interpreter interpreter, IList<object> arguments)
        {
            LoxEnvironment environment = new LoxEnvironment(_closure);

            for (var i = 0; i < _declaration.parameters.Count; i++)
            {
                environment.Define(_declaration.parameters[i].Lexeme, arguments[i]);
            }

            try
            {
                interpreter.ExecuteBlock(_declaration.body, environment);
            }
            catch (Return returnValue)
            {
                if (_functionType == FunctionTypes.INITIALIZER) return _closure.GetAt(0, "this");
                return returnValue.Value;
            }

            if (_functionType == FunctionTypes.INITIALIZER) return _closure.GetAt(0, "this");
            return null;
        }

        public LoxFunction Bind(LoxInstance loxInstance)
        {
            var environment = new LoxEnvironment(_closure);
            environment.Define("this", loxInstance);
            return new LoxFunction(_declaration, environment, _functionType);
        }

        public override string ToString()
        {
            return $"<fn {_declaration.name.Lexeme}>";
        }

        private readonly Stmt.Function _declaration;
        private readonly LoxEnvironment _closure;
        private readonly FunctionTypes _functionType;
    }
}
