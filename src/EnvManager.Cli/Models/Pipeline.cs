using EnvManager.Cli.Common.Loggers;
using ImprovedConsole.CommandRunners.Arguments;
using Serilog;

namespace EnvManager.Cli.Models
{
    public class Pipeline(List<Stage> stages)
    {
        public string Id { get; } = $"{DateTime.Now:yyyy_MM_dd_HH_mm_ss}";
        private readonly List<Stage> stages = stages;

        public void Run(CommandArguments commandArguments)
        {
            var context = new PipelineContext(commandArguments, this);
            using var _c = LogCtx.SetPipelineLogPath(context.LogFilePath);

            Log.Information(
$"""
Starting pipeline
Id: {Id}
Log Path: {context.LogFilePath}
Date: {LogCtx.GetCurrentDate()}
""");

            for (var i = 0; i < stages.Count; i++)
            {
                var stage = stages[i];

                Log.Information("------------------------------------------------------------------------------------------------------------------------");

                stage.Run(context);
            }

            Log.Information("------------------------------------------------------------------------------------------------------------------------");
        }
    }
}
