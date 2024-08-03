namespace EnvManager.Cli.Common.Contexts
{
    public class CustomStack
    {
        private readonly CustomStack _previous;
        private readonly object _value;

        CustomStack(object value)
        {
            _value = value;
        }

        CustomStack(CustomStack previous, object value)
        {
            _previous = previous;
            _value = value;

            Id = previous.Id + 1;
        }

        internal int Id { get; } = 1;

        public T Get<T>() => (T)_value;

        public CustomStack Push(object value) => new(this, value);
        public static CustomStack New(object value) => new(value);
    }
}
