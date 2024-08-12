using EnvManager.Cli.Common.IO.Internal;
using EnvManager.Cli.Common.Loggers;
using EnvManager.Cli.Models;
using EnvManager.Cli.Models.Fs;
using EnvManager.Common;
using Newtonsoft.Json;
using Serilog;
using Serilog.Context;

namespace EnvManager.Cli.Models.Fs.Handlers
{
    public static class CopyAllHandler
    {
        public static void Run(CopyAllStep setting, StepContext context)
        {
            var sourceFolder = setting.SourceFolder
                .FixWindowsDisk()
                .FixUserPath()
                .GetFullPath();

            var targetFolder = setting.TargetFolder
                .FixWindowsDisk()
                .FixUserPath()
                .GetFullPath();

            if (!Directory.Exists(sourceFolder))
            {
                Log.Information($"The source folder does not exist ({sourceFolder}). Skipping...");
                return;
            }

            Log.Information($"# Copying files\n");

            var filesJson = JsonConvert.SerializeObject(setting.Files, Formatting.Indented);
            var ignoreJson = JsonConvert.SerializeObject(setting.IgnoreList, Formatting.Indented);
            Log.Information(
$"""
Parameters:
Source folder: '{sourceFolder}'
Target folder: '{targetFolder}'
Overwrite files: '{setting.FileExistsAction}'
Include patterns: {filesJson}
Exclude patterns: {ignoreJson}
""");

            var files = StaticFileMatcher.GetFiles(sourceFolder, setting.Files, setting.IgnoreList);

            using (LogContext.Push(LogCtx.StepFileOnly))
            {
                LogStepFile(setting, sourceFolder, targetFolder, files);
            }
        }

        private static void LogStepFile(CopyAllStep setting, string sourceFolder, string targetFolder, List<FileMatch> files)
        {
            Log.Information("\nMatched files:");

            foreach (var file in files.Where(e => e.Matched))
            {
                var sourcePath = Path.Combine(sourceFolder, file.Path);
                var targetPath = Path.Combine(targetFolder, file.Path);
                var fileTargetFolder = Path.GetDirectoryName(targetPath);

                Directory.CreateDirectory(fileTargetFolder);

                if (File.Exists(targetPath))
                {
                    if (setting.FileExistsAction is CopyAllStep.OverwriteAction.Throw)
                        throw new IOException($"The file '{targetPath}' already exists");

                    if (setting.FileExistsAction is CopyAllStep.OverwriteAction.Ignore)
                    {
                        Log.Information($"File not copied because it already exists. Source='{sourcePath}' Target='{targetPath}'");
                        continue;
                    }

                    File.SetAttributes(targetPath, FileAttributes.Normal);
                }

                File.Copy(sourcePath, targetPath, true);

                var attributes = File.GetAttributes(sourcePath);
                File.SetAttributes(targetPath, attributes);

                Log.Information($"File copied. Source='{sourcePath}' Target='{targetPath}'");
            }

            Log.Information("\nIgnored Files:");

            foreach (var file in files.Where(e => !e.Matched))
            {
                var sourcePath = Path.Combine(sourceFolder, file.Path);
                Log.Information($"Ignoring file. '{sourcePath}");
            }
        }
    }
}
