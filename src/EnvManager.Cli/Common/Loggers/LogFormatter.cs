using Serilog.Events;
using Serilog.Formatting;

namespace EnvManager.Cli.Common.Loggers
{
    public class LogFormatterOptions
    {
        public bool UsePadding { get; set; } = true;
    }

    public class LogFormatter(LogFormatterOptions options) : ITextFormatter
    {
        public LogFormatter() : this(new LogFormatterOptions()) { }

        public void Format(LogEvent logEvent, TextWriter output)
        {
            string message = logEvent.RenderMessage();

            if (options.UsePadding && logEvent.Properties.TryGetValue(LogCtx.LogPaddingName, out var pd))
            {
                var x = (ScalarValue)pd;
                var padding = (string)x.Value;
                output.Write(padding);

                message = message.PadLinesLeft(padding.Length);
            }

            if (logEvent.Properties.ContainsKey(LogCtx.DateName))
            {
                output.Write($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss:fff}] ");
            }

            if (logEvent.Properties.ContainsKey(LogCtx.NoLFName))
            {
                output.Write(message);
                return;
            }

            output.WriteLine(message);
        }
    }
}
