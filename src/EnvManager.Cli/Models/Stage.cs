using EnvManager.Cli.Common;
using ImprovedConsole;
using ImprovedConsole.CommandRunners.Arguments;
using MoonSharp.Interpreter;
using System.Text;

namespace EnvManager.Cli.Models
{
    [MoonSharpUserData]
    public class Stage
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Closure Body { get; set; }
        public List<IStep> Steps { get; } = [];

        public void Run(CommandArguments commandArguments)
        {
            for (int i = 0; i < Steps.Count; i++)
            {
                var task = Steps[i];

                ConsoleWriter.WriteLine($"Task started: {task.Name ?? task.Id.ToString()}");

                using (CustomLogger.AddPadding(4))
                {
                    task.Run(commandArguments);
                }

                if (i != Steps.Count - 1)
                    ConsoleWriter.WriteLine().WriteLine();
            }
        }

        public void AddTask(IStep task)
        {
            Steps.Add(task);
        }
    }
}
