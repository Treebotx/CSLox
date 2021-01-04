using System.Collections.Generic;

namespace CSLox
{
    public class LoxFunction : ILoxCallable
    {
        public LoxFunction(Stmt.Function declaration, LoxEnvironment closure)
        {
            _closure = closure;
            _declaration = declaration;
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
                return returnValue.Value;
            }

            return null;
        }

        public override string ToString()
        {
            return $"<fn {_declaration.name.Lexeme}>";
        }

        private readonly Stmt.Function _declaration;
        private readonly LoxEnvironment _closure;
    }
}
