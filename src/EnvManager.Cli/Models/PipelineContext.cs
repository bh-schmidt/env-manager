using EnvManager.Common;
using ImprovedConsole.CommandRunners.Arguments;

namespace EnvManager.Cli.Models
{
    public class PipelineContext
    {
        public CommandArguments Arguments { get; }
        public Pipeline Pipeline { get; }
        public string LogDirectory { get; }
        public string LogFilePath { get; }

        public PipelineContext(CommandArguments arguments, Pipeline pipeline)
        {
            Arguments = arguments;
            Pipeline = pipeline;
            LogDirectory = $"~/.logs/envm/{pipeline.Id}".FixUserPath().GetFullPath();
            LogFilePath = LogDirectory.CombinePathWith($"{pipeline.Id}.txt");
        }
    }
}
