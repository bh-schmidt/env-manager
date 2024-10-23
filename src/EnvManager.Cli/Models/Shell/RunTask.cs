using EnvManager.Cli.Common.EventLoggers;
using EnvManager.Cli.Common.Loggers;
using EnvManager.Common;
using Newtonsoft.Json;
using Serilog;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace EnvManager.Cli.Models.Shell
{
    public class RunTask : ITask
    {
        public string Code => "sh.run";

        public IEnumerable<string> Files { get; set; }
        public string Script { get; set; }
        public bool IgnoreErrors { get; set; }

        public void Run(StepContext context)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var path = Environment.GetEnvironmentVariable("PATH");
                var bashPath = "C:\\Program Files\\Git\\bin";

                Console.WriteLine($"PATH={path}"); //remove

                if (!path.Contains(bashPath, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (path.EndsWith(';'))
                        Environment.SetEnvironmentVariable("PATH", $"{path}{bashPath};", EnvironmentVariableTarget.Process);
                    else
                        Environment.SetEnvironmentVariable("PATH", $"{path};{bashPath};", EnvironmentVariableTarget.Process);
                }
            }

            var files = Files?
                .Select(e => e.FixUserPath()
                    .FixWindowsDisk()
                    .FixCurrentPath(context.Step.Direrctory)
                    .GetFullPath());

            Log.Information(
$"""
Parameters:
Files: {JsonConvert.SerializeObject(files)}
Inline Script: 
---
{Script}
---

""");
            if (files?.Any() == true)
            {
                Log.Information("Files informed. Ignoring inline scripts.");
                foreach (var file in files)
                    Run(file);

                return;
            }

            ProcessStartInfo info = new()
            {
                FileName = "sh",
                Arguments = $"-c \"{Script.Replace("\"", "\\\"")}\"",
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            };

            var process = Process.Start(info);
            Handle("inline script", process);
        }

        private void Run(string file)
        {
            ProcessStartInfo info = new()
            {
                FileName = "sh",
                Arguments = file,
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            };

            var process = Process.Start(info);
            Handle(file, process);
        }

        private void Handle(string name, Process process)
        {
            var logger = new CommonEventLogger();

            ArgumentNullException.ThrowIfNull(process);

            logger.Queue(process.StandardOutput);
            logger.Queue(process.StandardError);
            logger.Start();

            Log.Information($"Running script: '{name}'");

            process.WaitForExit();
            logger.Finish();

            if (process.ExitCode == 0)
            {
                Log.Information($"Script finished: '{name}'");
                return;
            }

            Log.Information($"Script finished with errors: '{name}'. ExitCode: {process.ExitCode}.");

            if (IgnoreErrors)
                return;

            throw new Exception("The program finished with errors");
        }
    }
}
