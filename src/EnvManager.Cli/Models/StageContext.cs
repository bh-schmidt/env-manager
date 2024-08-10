using EnvManager.Common;
using ImprovedConsole.CommandRunners.Arguments;

namespace EnvManager.Cli.Models
{
    public class StageContext(PipelineContext context, Stage stage)
    {
        public CommandArguments Arguments { get; } = context.Arguments;
        public Pipeline Pipeline { get; } = context.Pipeline;
        public Stage Stage { get; } = stage;
        public string LogDirectory { get; } = context.LogDirectory.CombinePathWith(stage.Id.ToString());
    }
}
