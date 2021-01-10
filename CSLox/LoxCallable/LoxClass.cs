using System.Collections.Generic;

namespace CSLox
{
    public class LoxClass : ILoxCallable
    {
        public string Name { get; }

        private readonly Dictionary<string, LoxFunction> _methods;
        private readonly LoxClass _superClass;

        public int Arity
        {
            get
            {
                var initilizer = FindMethod("init");
                if (initilizer == null) return 0;
                return initilizer.Arity;
            }
        }

        public LoxClass(string name, LoxClass superClass, Dictionary<string, LoxFunction> methods)
        {
            Name = name;
            _methods = methods;
            _superClass = superClass;
        }

        public object Call(Interpreter interpreter, IList<object> arguments)
        {
            var instance = new LoxInstance(this);

            var initilizer = FindMethod("init");
            if (!(initilizer is null))
            {
                initilizer.Bind(instance).Call(interpreter, arguments);
            }

            return instance;
        }

        public LoxFunction FindMethod(string name)
        {
            if (_methods.TryGetValue(name, out var loxFunction)) return loxFunction;

            if (_superClass != null) return _superClass.FindMethod(name);

            return null;
        }

        public override string ToString()
        {
            return $"LoxClass: {Name}";
        }
    }
}
