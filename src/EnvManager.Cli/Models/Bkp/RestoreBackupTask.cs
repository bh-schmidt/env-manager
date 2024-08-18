using EnvManager.Cli.Common.IO;
using EnvManager.Cli.Enums;
using EnvManager.Common;
using Serilog;

namespace EnvManager.Cli.Models.Bkp
{
    public class RestoreBackupTask : ITask
    {
        public string Code => "bkp.restore";

        public string Source { get; set; }
        public string Target { get; set; }
        public DirectoryExistsAction DirExists { get; set; }
        public FileExistsAction FileExists { get; set; }
        public int MaxConcurrency { get; set; }

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
                if (DirExists == DirectoryExistsAction.Throw)
                    throw new Exception("The target directory already exists.");

                if (DirExists == DirectoryExistsAction.Ignore)
                {
                    Log.Information("The target directory already exists. Skipping...");
                    return;
                }

                Log.Information("The target directory already exists.");
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

            var replaceFiles = FileExists == FileExistsAction.Replace;
            DirHelper.CopyAsync(sourceDir, Target, replaceFiles, MaxConcurrency).Wait();

            Log.Information($"Backup restored.");
        }
    }
}
