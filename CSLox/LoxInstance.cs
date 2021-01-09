using System.Collections.Generic;

namespace CSLox
{
    public class LoxInstance
    {
        private LoxClass _loxClass;
        private readonly IDictionary<string, object> _fields = new Dictionary<string, object>();

        public LoxInstance(LoxClass loxClass)
        {
            _loxClass = loxClass;
        }

        public object Get(Token name)
        {
            if (_fields.TryGetValue(name.Lexeme, out var value)) return value;
            //if (_fields.ContainsKey(name.Lexeme))
            //{
            //    return _fields[name.Lexeme];
            //}

            var method = _loxClass.FindMethod(name.Lexeme);
            if (method != null) return method.Bind(this);

            throw new LoxRuntimeErrorException(name, $"Undefined property {name.Lexeme}.");
        }

        public void Set(Token name, object value)
        {
            _fields[name.Lexeme] = value;
        }

        public override string ToString()
        {
            return $"Instance of {_loxClass}";
        }
    }
}