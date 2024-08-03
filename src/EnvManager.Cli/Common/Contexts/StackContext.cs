using ImprovedConsole;

namespace EnvManager.Cli.Common.Contexts
{
    public class StackContext
    {
        private readonly Dictionary<string, AsyncLocal<CustomStack>> _stacks = [];

        public ContextCleaner Push(string key, object obj)
        {
            AsyncLocal<CustomStack> local;
            ContextCleaner cleaner;
            lock (_stacks)
            {
                if (_stacks.TryGetValue(key, out local))
                {
                    cleaner = new(this, key, local.Value);
                    if (local.Value is null)
                    {
                        local.Value = CustomStack.New(obj);
                        return cleaner;
                    }

                    local.Value = local.Value.Push(obj);
                    return cleaner;
                }

                local = new AsyncLocal<CustomStack>();
                local.Value = CustomStack.New(obj);
                cleaner = new(this, key);
                _stacks[key] = local;

                return cleaner;
            }
        }

        public T Get<T>(string key, T defaultValue = default)
        {
            if (_stacks.TryGetValue(key, out AsyncLocal<CustomStack> value))
            {
                if (value.Value == null)
                    return defaultValue;

                return value.Value.Get<T>();
            }

            return defaultValue;
        }

        public class ContextCleaner(StackContext context, string key) : IDisposable
        {
            private readonly StackContext context = context;
            private readonly string key = key;
            private readonly CustomStack stack;

            public ContextCleaner(StackContext context, string key, CustomStack stack) : this(context, key)
            {
                this.stack = stack;
            }

            public event Action<CustomStack> OnDispose;

            public void Dispose()
            {
                lock (context._stacks)
                {
                    if (stack is null)
                    {
                        context._stacks.Remove(key);
                        return;
                    }

                    var local = context._stacks[key];
                    if (local.Value.Id > stack.Id)
                        local.Value = stack;

                    OnDispose(local.Value);
                }
            }
        }
    }
}
