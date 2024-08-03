using System.Text;

namespace EnvManager.Cli.Common
{
    public class PipeLogger : ILogger
    {
        public StringBuilder Output { get; } = new();

        public ILogger Write(object obj)
        {
            Output.Append(obj?.ToString());
            return this;
        }

        public ILogger WriteLine()
        {
            Output.AppendLine();
            return this;
        }

        public ILogger WriteLine(object obj)
        {
            Output.AppendLine(obj?.ToString());
            return this;
        }
    }
}
