using EnvManager.Cli.Common.IO;
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

            FileMatcherFilters filter = new()
            {
                SourceDir = task.Source,
                IncludePatterns = task.IncludePatterns,
                ExcludePatterns = task.ExcludePatterns,
            };

            var matcher = new FileMatcher(filter)
            {
                MaxConcurrency = task.MaxConcurrency,
            };

            matcher.Match();

            if (matcher.Matches.All(e => !e.Excluded))
            {
                Log.ForContext(LogCtx.PipelineOnly)
                    .Information("No files matched.");
            }

            var matches = matcher.Matches
                .OrderBy(e => e.Path.Length)
                .ThenBy(e => e.Path)
                .ToArray();

            using (LogContext.Push(LogCtx.StepFileOnly))
            {
                LogStepFile(matches);
            }

            var targetDir = task.Target
                .CombinePathWith($"{DateTime.Now:yyyy_MM_dd_HH_mm_ss}");

            Log.Information($"Copy started");

            DirHelper
                .CopyAsync(matches, task.Source, targetDir, false, task.MaxConcurrency)
                .Wait();

            Log.Information($"Backup completed");
        }

        private void LogStepFile(IEnumerable<FileSystemMatch> paths)
        {
            Log.Information("\nCopying paths:");

            foreach (var fs in paths.Where(e => !e.Excluded && e.HasAccess))
            {
                var path = Path.Combine(task.Source, fs.Path);
                Log.Information($"Copying: '{path}'");
            }

            Log.Information("\nExcluded paths:");

            foreach (var fs in paths.Where(e => e.Excluded))
            {
                var path =
                    fs.IsDirectory ?
                    Path.Combine(task.Source, fs.Path, "*") :
                    Path.Combine(task.Source, fs.Path);

                Log.Information($"Excluded: '{path}");
            }

            Log.Information("\nForbidden paths:");

            foreach (var fs in paths.Where(e => !e.HasAccess))
            {
                var path =
                    fs.IsDirectory ?
                    Path.Combine(task.Source, fs.Path, "*") :
                    Path.Combine(task.Source, fs.Path);

                Log.Information($"Forbidden: '{path}");
            }


            Log.Information(string.Empty);
        }
    }
}
