using System.Collections.Generic;

namespace CSLox
{
    public class LoxEnvironment
    {
        private IDictionary<string, object> _values = new Dictionary<string, object>();
        private LoxEnvironment _enclosing = null;

        public LoxEnvironment()
        {
            _enclosing = null;
        }

        public LoxEnvironment(LoxEnvironment enclosing)
        {
            _enclosing = enclosing;
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

            if (_enclosing != null)
            {
                _enclosing.Assign(name, value);
                return;
            }

            throw new LoxRuntimeErrorException(name, $"Undefined variable {name.Lexeme}.");
        }

        public object Get(Token name)
        {
            if (_values.TryGetValue(name.Lexeme, out object value))
            {
                return value;
            }

            if (_enclosing != null) return _enclosing.Get(name);

            throw new LoxRuntimeErrorException(name, $"Undefined variable {name.Lexeme}.");
        }
    }
}