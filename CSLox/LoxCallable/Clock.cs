using System.Collections.Generic;

namespace CSLox
{
    public class Clock : ILoxCallable
    {
        public int Arity => 0;

        public object Call(Interpreter interpreter, IList<object> arguments)
        {
            return System.DateTime.UtcNow.Millisecond;
        }

        public override string ToString()
        {
            return "<native fn Clock>";
        }
    }
}
