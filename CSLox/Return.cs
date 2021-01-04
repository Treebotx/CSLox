namespace CSLox
{
    public class Return : LoxRuntimeErrorException
    {
        public object Value { get; }

        public Return(object value) : base()
        {
            Value = value;
        }
    }
}
