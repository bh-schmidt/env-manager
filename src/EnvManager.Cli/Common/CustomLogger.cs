using EnvManager.Cli.Common.Contexts;
using ImprovedConsole;
using System.Runtime.CompilerServices;

namespace EnvManager.Cli.Common
{
    public class CustomLogger : ConsoleInstance
    {
        private static readonly AsyncLocal<StackContext> _contextBox = new();

        public override ConsoleInstance Write(object obj)
        {
            var value = obj?.ToString();
            if (value is null)
                return this;

            var padding = GetPadding();
            if (padding > 0)
            {
                var builder = value.PadLinesLeft(padding);
                return base.Write(builder);
            }

            return base.Write(value);
        }

        public override ConsoleInstance WriteLine(object obj)
        {
            var value = obj?.ToString();
            if (value is null)
                return this;

            var padding = GetPadding();
            if (padding > 0)
            {
                var builder = value.PadLinesLeft(padding);
                return base.WriteLine(builder)
                    .Write(new string(' ', padding));
            }

            return base.WriteLine(obj);
        }

        public override ConsoleInstance WriteLine()
        {
            var padding = GetPadding();
            if (padding > 0)
            {
                return base.WriteLine()
                    .Write(new string(' ', padding));
            }

            return base.WriteLine();
        }

        public static IDisposable AddPadding(int padding)
        {
            var context = GetOrCreateContext();

            var current = context.Get<int>("padding");
            int newPadding = current + padding;

            var cleaner = context.Push("padding", newPadding);
            ConsoleWriter.WriteLine();

            cleaner.OnDispose += (stack) =>
            {
                ConsoleWriter.WriteLine();
            };

            return cleaner;
        }

        private static int GetPadding()
        {
            var context = GetOrCreateContext();
            return context.Get<int>("padding");
        }

        private static StackContext GetOrCreateContext()
        {
            if (_contextBox.Value == null)
                _contextBox.Value = new();

            return _contextBox.Value;
        }
    }
}
