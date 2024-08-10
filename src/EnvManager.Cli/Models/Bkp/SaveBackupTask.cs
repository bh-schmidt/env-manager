using EnvManager.Cli.Models.Bkp.Handlers;
using EnvManager.Common;

namespace EnvManager.Cli.Models.Bkp
{
    public class SaveBackupTask : ITask
    {
        public string Code => "bkp.save";

        public string Source { get; set; }
        public string Target { get; set; }
        public IEnumerable<string> IncludePatterns { get; set; }
        public IEnumerable<string> ExcludePatterns { get; set; }

        public void Run(StepContext context)
        {
            if (IncludePatterns is null || !IncludePatterns.Any())
                IncludePatterns = ["**/*"];

            if (Source[^1] is not '/' or '\\')
                Source += '/';

            if (Target[^1] is not '/' or '\\')
                Target += '/';

            ExcludePatterns ??= [];

            Source = Source.FixWindowsDisk()
                .FixUserPath()
                .GetFullPath();

            Target = Target.FixWindowsDisk()
                .FixUserPath()
                .GetFullPath();

            new SaveBackupTaskHandler(this).Run();
        }
    }
}
