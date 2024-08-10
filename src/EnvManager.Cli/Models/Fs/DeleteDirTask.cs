using EnvManager.Cli.Common.IO;
using EnvManager.Common;
using Serilog;

namespace EnvManager.Cli.Models.Fs
{
    public class DeleteDirTask : ITask
    {
        public string Code => "fs.delete_dir";
        public string Path { get; set; }

        public void Run(StepContext context)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(Path);

            Path = Path
                .FixUserPath()
                .FixWindowsDisk()
                .GetFullPath();

            if (!Path.DirectoryExists())
            {
                Log.Information(
$"""
Directory does not exist. Ignoring.
Path: {Path}
""");
                return;
            }

            Log.Information(
$"""
Deleting directory
Path: {Path}
""");

            DirHelper.Delete(Path);

            Log.Information("Directory deleted");
        }
    }
}
