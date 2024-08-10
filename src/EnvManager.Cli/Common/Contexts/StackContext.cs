using ImprovedConsole;

namespace EnvManager.Cli.Common.Contexts
{
    public class StackContext
    {
        internal readonly Dictionary<string, AsyncLocal<CustomStack>> Stacks = [];

        public ContextCleaner Push(string key, object obj)
        {
            AsyncLocal<CustomStack> local;
            ContextCleaner cleaner;
            lock (Stacks)
            {
                if (Stacks.TryGetValue(key, out local))
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
                Stacks[key] = local;

                return cleaner;
            }
        }

        public T Get<T>(string key, T defaultValue = default)
        {
            if (Stacks.TryGetValue(key, out AsyncLocal<CustomStack> value))
            {
                if (value.Value == null)
                    return defaultValue;

                return value.Value.Get<T>();
            }

            return defaultValue;
        }
    }
}
