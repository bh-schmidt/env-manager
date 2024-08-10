using EnvManager.Cli.Handlers.Chocolatey;

namespace EnvManager.Cli.Models.Chocolatey
{
    public class ChocoInstallStep : ITask
    {
        public string Code => "choco.install";

        public Guid Id { get; set; }
        public string Name { get; set; }

        public List<string> Packages { get; set; }
        public bool IgnoreErrors { get; set; } = true;

        public void Run(StepContext context)
        {
            ChocoInstallHandler.Run(this);
        }
    }
}
