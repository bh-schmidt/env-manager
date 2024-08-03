using EnvManager.Cli.Common;
using ImprovedConsole.CommandRunners.Arguments;
using System.Text;

namespace EnvManager.Cli.LuaContexts.Models
{
    public class Pipeline(IEnumerable<Stage> stages)
    {
        private readonly IEnumerable<Stage> stages = stages;

        public void Run(CommandArguments commandArguments)
        {
            var logger = new PipeLogger();

            foreach (var stage in stages)
            {
                logger
                    .WriteLine("------------------------------------------------------------------------------------------------------------------------")
                    .WriteLine($"# Stage started: {stage.Name ?? stage.Id.ToString()}")
                    .WriteLine();

                var internalLogger = new PipeLogger();
                stage.Run(commandArguments, internalLogger);
                logger.WriteLine(internalLogger.Output.PadLinesLeft(4));

                logger
                    .WriteLine("------------------------------------------------------------------------------------------------------------------------")
                    .WriteLine()
                    .WriteLine();
            }

            Console.Write(logger.Output);
        }
    }
}
