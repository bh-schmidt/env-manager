using EnvManager.Cli.Common;
using ImprovedConsole;
using ImprovedConsole.CommandRunners.Arguments;
using System.Text;

namespace EnvManager.Cli.Models
{
    public class Pipeline(List<Stage> stages)
    {
        private readonly List<Stage> stages = stages;

        public void Run(CommandArguments commandArguments)
        {
            for (var i = 0; i < stages.Count; i++)
            {
                var stage = stages[i];

                ConsoleWriter
                    .WriteLine("------------------------------------------------------------------------------------------------------------------------")
                    .WriteLine($"Stage started: {stage.Name ?? stage.Id.ToString()}");

                using (CustomLogger.AddPadding(4))
                {
                    stage.Run(commandArguments);
                }

                ConsoleWriter
                    .WriteLine("\r------------------------------------------------------------------------------------------------------------------------")
                    .WriteLine();

                if (i != stages.Count - 1)
                {
                    ConsoleWriter
                        .WriteLine()
                        .WriteLine()
                        .WriteLine();
                }
            }
        }
    }
}
