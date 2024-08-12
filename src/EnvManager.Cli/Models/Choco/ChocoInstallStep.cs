using EnvManager.Cli.Models.Choco.Handlers;

namespace EnvManager.Cli.Models.Choco
{
    public class ChocoInstallStep : ITask
    {
        public string Code => "choco.install";

        public Guid Id { get; set; }
        public string Name { get; set; }

        public List<string> Packages { get; set; }
        public bool IgnoreErrors { get; set; }

        public void Run(StepContext context)
        {
            ChocoInstallHandler.Run(this);
        }
    }
}
