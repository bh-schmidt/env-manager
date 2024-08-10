using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;

namespace EnvManager.Cli.Common.Loggers
{
    public partial class FileSink : ILogEventSink, IDisposable
    {
        private Dictionary<string, string> properties;
        private string _path;
        private readonly ITextFormatter _textFormatter;
        private readonly Dictionary<string, FileContext> _contexts = [];

        public FileSink(string path, ITextFormatter textFormatter)
        {
            _path = path;
            _textFormatter = textFormatter;
            var props = Properties().Matches(path);
            properties = props.ToDictionary(e => e.Groups[1].Value, e => e.Value);
        }

        public void Emit(LogEvent logEvent)
        {
            if (properties.Count > logEvent.Properties.Count)
                return;

            if (!properties.Keys.All(logEvent.Properties.ContainsKey))
                return;

            var props = properties
                .ToDictionary(e => e.Value, e => logEvent.Properties[e.Key] as ScalarValue);

            var builder = new StringBuilder(_path);
            foreach (var prop in props)
                builder.Replace(prop.Key, prop.Value.Value.ToString());

            var path = builder.ToString();

            var fileContext = GetContext(path);

            _textFormatter.Format(logEvent, fileContext.Writer);
            fileContext.Writer.Flush();
        }

        private FileContext GetContext(string path)
        {
            lock (_contexts)
            {
                ref FileContext fileContext = ref CollectionsMarshal.GetValueRefOrAddDefault(_contexts, path, out var contextExists);

                if (!contextExists)
                {
                    fileContext = new FileContext(path)
                    {
                        Kill = context =>
                        {
                            lock (_contexts)
                            {
                                _contexts.Remove(context.Path);
                            }

                            context.Dispose();
                        }
                    };

                    fileContext.DebounceKill();
                }

                return fileContext;
            }
        }


        [GeneratedRegex("[{]([^}]+)[}]")]
        private static partial Regex Properties();

        public void Dispose()
        {
            lock (_contexts)
            {
                foreach (var item in _contexts)
                    item.Value.Dispose();

                _contexts.Clear();
            }
        }
    }
}
