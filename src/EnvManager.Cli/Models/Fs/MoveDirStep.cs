using EnvManager.Cli.Common.IO;
using EnvManager.Common;
using Serilog;

namespace EnvManager.Cli.Models.Fs
{
    public class MoveDirStep : ITask
    {
        public string Code => "fs.move_dir";
        public string Source { get; set; }
        public string Target { get; set; }

        public void Run(StepContext context)
        {
            var verbose = context.Arguments.Options.Contains("-v");

            ArgumentException.ThrowIfNullOrWhiteSpace(Source);
            ArgumentException.ThrowIfNullOrWhiteSpace(Target);

            Source = Source
                .FixUserPath()
                .FixWindowsDisk()
                .GetFullPath();

            Target = Target
                .FixUserPath()
                .FixWindowsDisk()
                .GetFullPath();

            if (!Source.DirectoryExists())
            {
                Log.Information(
$"""
Source directory does not exist.
Source: '{Source}'
""");
                return;
            }

            if (Target.DirectoryExists())
            {
                Log.Information(
$"""
Target directory already exists.
Target: '{Target}'
""");
                return;
            }

            Log.Information(
$"""
Copying directory.
Source: '{Source}'
Target: '{Target}'
""");

            try
            {
                Log.Information("\nCopy started.");

                if (verbose)
                    DirHelper.Copy(Source, Target, true, true);
                else
                    DirHelper.Copy(Source, Target);

                Log.Information("Copy finished.");
            }
            catch (Exception ex)
            {
                Log.Information($"An error ocurred copying the directory.");
                Log.Information($"Message: {ex.Message}");

                DirHelper.Delete(Target);
                throw;
            }

            try
            {
                Log.Information("Deleting source directory.");

                DirHelper.Delete(Source);

                Log.Information("Source directory deleted.");
            }
            catch (Exception ex)
            {
                Log.Information(
$"""

An error ocurred deleting the directory {Source}.
Message: {ex.Message}
""");
            }
        }
    }
}
