using ImprovedConsole.CommandRunners.Arguments;

namespace EnvManager.Models
{
    public interface ITask
    {
        string Name { get; set; }
        void Run(CommandArguments arguments);
    }
}
