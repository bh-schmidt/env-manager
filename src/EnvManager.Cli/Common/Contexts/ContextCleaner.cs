namespace EnvManager.Cli.Common.Contexts
{
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
            lock (context.Stacks)
            {
                if (stack is null)
                {
                    context.Stacks.Remove(key);
                    return;
                }

                var local = context.Stacks[key];
                if (local.Value.Id > stack.Id)
                    local.Value = stack;

                OnDispose(local.Value);
            }
        }
    }
}
