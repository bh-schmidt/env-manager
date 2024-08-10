using EnvManager.Cli.Common;
using EnvManager.Cli.Common.Loggers;
using ImprovedConsole;
using ImprovedConsole.CommandRunners.Arguments;
using MoonSharp.Interpreter;
using Serilog;
using System.Text;

namespace EnvManager.Cli.Models
{
    [MoonSharpUserData]
    public class Stage
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public Closure Body { get; set; }
        public List<Step> Steps { get; } = [];

        public void Run(PipelineContext pipelineContext)
        {
            var stageContext = new StageContext(pipelineContext, this);
            Log.Information(
$"""
Stage started
Id = {Id}
Name = {Name}
Log Dir = {stageContext.LogDirectory}
Date: {LogCtx.GetCurrentDate()}
""");

            using (LogCtx.AddPadding(4))
            {
                for (int i = 0; i < Steps.Count; i++)
                {
                    var step = Steps[i];

                    step.Run(stageContext);

                    if (i != Steps.Count - 1)
                        Log.Information("\n");
                }
            }
        }

        public void AddStep(Step step)
        {
            Steps.Add(step);
        }
    }
}
