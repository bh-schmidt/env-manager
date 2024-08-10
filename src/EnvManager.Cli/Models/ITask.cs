using EnvManager.Cli.Common;
using ImprovedConsole.CommandRunners.Arguments;

namespace EnvManager.Cli.Models
{
    public interface ITask
    {
        string Code { get; }
        void Run(StepContext context);
    }
}
