using EnvManager.Cli.Common;
using EnvManager.Cli.Models;
using ImprovedConsole.CommandRunners.Arguments;
using MoonSharp.Interpreter;
using System.Text;

namespace EnvManager.Cli.LuaContexts.Models
{
    [MoonSharpUserData]
    public class Stage
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Closure Body { get; set; }
        public List<ITask> Tasks { get; } = [];

        public void Run(CommandArguments commandArguments, PipeLogger logger)
        {
            for (int i = 0; i < Tasks.Count; i++)
            {
                var task = Tasks[i];

                logger.WriteLine($"# Task started: {task.Name ?? task.Id.ToString()}")
                    .WriteLine();

                var internalLogger = new PipeLogger();
                task.Run(commandArguments, internalLogger);
                logger.WriteLine(internalLogger.Output.PadLinesLeft(4));

                if (i != Tasks.Count - 1)
                    logger.WriteLine().WriteLine();
            }
        }

        public void AddTask(ITask task)
        {
            Tasks.Add(task);
        }
    }
}
