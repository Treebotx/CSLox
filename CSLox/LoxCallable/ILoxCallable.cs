using System.Collections.Generic;

namespace CSLox
{
    public interface ILoxCallable
    {
        int Arity { get; }

        object Call(Interpreter interpreter, IList<object> arguments);
    }
}
