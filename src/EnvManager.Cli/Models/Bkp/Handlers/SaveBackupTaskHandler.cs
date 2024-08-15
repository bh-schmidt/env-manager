using EnvManager.Cli.Common.IO.Internal;
using EnvManager.Cli.Common.Loggers;
using EnvManager.Common;
using Newtonsoft.Json;
using Serilog;
using Serilog.Context;

namespace EnvManager.Cli.Models.Bkp.Handlers
{
    public class SaveBackupTaskHandler(SaveBackupTask task)
    {
        public void Run()
        {
            var filesJson = JsonConvert.SerializeObject(task.IncludePatterns, Formatting.Indented);
            var ignoreJson = JsonConvert.SerializeObject(task.ExcludePatterns, Formatting.Indented);
            Log.Information(
$"""
Parameters:
Source dir: '{task.Source}'
Target dir: '{task.Target}'
Include patterns: {filesJson}
Exclude patterns: {ignoreJson}

""");

            if (!Directory.Exists(task.Source))
            {
                Log.Information($"The source folder does not exist ({task.Source}). Skipping...");
                return;
            }

            Log.Information($"Backup started");

            var files = StaticFileMatcher.GetFiles(task.Source, task.IncludePatterns, task.ExcludePatterns);

            if (files.All(e => !e.Matched))
            {
                Log.ForContext(LogCtx.PipelineOnly)
                    .Information("No files matched.");
            }

            using (LogContext.Push(LogCtx.StepFileOnly))
            {
                LogStepFile(files);
            }

            Log.Information($"Backup completed");
        }

        private void LogStepFile(List<FileMatch> files)
        {
            Log.Information("\nMatched files:");

            var targetDir = task.Target
                .CombinePathWith($"{DateTime.Now:yyyy_MM_dd_HH_mm_ss}");

            foreach (var file in files.Where(e => e.Matched))
            {
                var sourceFile = Path.Combine(task.Source, file.Path);
                var targetFile = Path.Combine(targetDir, file.Path);
                var fileTargetFolder = Path.GetDirectoryName(targetFile);

                Directory.CreateDirectory(fileTargetFolder);

                File.Copy(sourceFile, targetFile, true);

                var attributes = File.GetAttributes(sourceFile);
                File.SetAttributes(targetFile, attributes);

                Log.Information($"File copied. Source='{sourceFile}' Target='{targetFile}'");
            }

            Log.Information("\nIgnored Files:");

            foreach (var file in files.Where(e => !e.Matched))
            {
                var sourceFile = Path.Combine(task.Source, file.Path);
                Log.Information($"Ignoring file. '{sourceFile}");
            }

            Log.Information(string.Empty);
        }
    }
}
