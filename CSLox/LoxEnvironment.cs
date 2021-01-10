using System.Collections.Generic;

namespace CSLox
{
    public class LoxEnvironment
    {
        private IDictionary<string, object> _values = new Dictionary<string, object>();
        public LoxEnvironment Enclosing { get; private set; } = null;

        public LoxEnvironment()
        {
            Enclosing = null;
        }

        public LoxEnvironment(LoxEnvironment enclosing)
        {
            Enclosing = enclosing;
        }

        public void Define(string name, object value)
        {
            //_values.Add(name, value);
            _values[name] = value;
        }

        public void Assign(Token name, object value)
        {
            if (_values.ContainsKey(name.Lexeme))
            {
                _values[name.Lexeme] = value;
                return;
            }

            if (Enclosing != null)
            {
                Enclosing.Assign(name, value);
                return;
            }

            throw new LoxRuntimeErrorException(name, $"Undefined variable {name.Lexeme}.");
        }

        public void AssignAt(int distance, Token name, object value)
        {
            Ancestor(distance)._values[name.Lexeme] = value;
        }

        public object Get(Token name)
        {
            if (_values.TryGetValue(name.Lexeme, out object value))
            {
                return value;
            }

            if (Enclosing != null) return Enclosing.Get(name);

            throw new LoxRuntimeErrorException(name, $"Undefined variable {name.Lexeme}.");
        }

        public object GetAt(int distance, string name)
        {
            return Ancestor(distance)._values[name];
        }

        private LoxEnvironment Ancestor(int distance)
        {
            var environment = this;
            for (var i = 0; i < distance; i++)
            {
                environment = environment.Enclosing;
            }

            return environment;
        }
    }
}