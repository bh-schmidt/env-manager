using EnvManager.Cli.Common;
using EnvManager.Cli.Handlers.Chocolatey;
using ImprovedConsole.CommandRunners.Arguments;

namespace EnvManager.Cli.Models.Chocolatey
{
    public class ChocoInstallStep : IStep
    {
        public string Code => "install";

        public Guid Id { get; set; }
        public string Name { get; set; }

        public List<string> Packages { get; set; }
        public bool IgnoreErrors { get; set; } = true;

        public void Run(CommandArguments arguments)
        {
            ChocoInstallHandler.Run(this);
        }
    }
}
