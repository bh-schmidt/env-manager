using EnvManager.Cli.Common.IO;
using EnvManager.Common;
using Serilog;

namespace EnvManager.Cli.Models.Bkp
{
    public class RestoreBackupTask : ITask
    {
        public string Code => "bkp.restore";

        public string Source { get; set; }
        public string Target { get; set; }

        public void Run(StepContext context)
        {
            if (Source[^1] is not '/' or '\\')
                Source += '/';

            if (Target[^1] is not '/' or '\\')
                Target += '/';

            Source = Source.FixWindowsDisk()
                .FixUserPath()
                .GetFullPath();

            Target = Target.FixWindowsDisk()
                .FixUserPath()
                .GetFullPath();

            Log.Information(
$"""
Parameters:
Source: {Source}
Target: {Target}

""");

            if (!Source.DirectoryExists())
            {
                Log.Information("The source directory does not exist. Skipping...");
                return;
            }

            if (Target.DirectoryExists())
            {
                Log.Information("The target directory already exists. Skipping...");
                return;
            }

            var latest = Directory.GetDirectories(Source)
                .OrderDescending()
                .FirstOrDefault();

            if (latest is null)
            {
                Log.Information("There is no backup in the source directory. Skipping...");
                return;
            }

            var sourceDir = latest.GetFullPath();

            Log.Information($"Restoring the backup ({sourceDir}) to target directory ({Target})");

            DirHelper.Copy(sourceDir, Target);

            Log.Information($"Backup restored.");
        }
    }
}
