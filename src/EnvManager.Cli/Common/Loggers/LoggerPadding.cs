using EnvManager.Cli.Common.Contexts;
using Serilog.Context;

namespace EnvManager.Cli.Common.Loggers
{
    public class LoggerPadding(CustomStack stack, IDisposable logContextDisposable) : IDisposable
    {
        static readonly AsyncLocal<CustomStack> Padding = new();

        public static IDisposable AddPadding(int padding)
        {
            var previous = Padding.Value;

            if (Padding.Value == null)
            {
                Padding.Value = CustomStack.New(padding);
            }
            else
            {
                padding += Padding.Value.Get<int>();
                Padding.Value = Padding.Value.Push(padding);
            }

            var context = LogContext.PushProperty(LogCtx.LogPaddingName, new string(' ', padding));
            return new LoggerPadding(previous, context);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);

            logContextDisposable.Dispose();

            if (stack is null)
            {
                Padding.Value = null;
                return;
            }

            if (Padding.Value.Id > stack.Id)
                Padding.Value = stack;
        }
    }
}
