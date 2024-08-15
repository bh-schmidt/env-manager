using EnvManager.Cli.Common.Loggers;
using EnvManager.Common;
using Newtonsoft.Json;
using Serilog;
using System.Diagnostics;

namespace EnvManager.Cli.Models.Program
{
    public class RunTask : ITask
    {
        public string Code => "program.run";

        public IEnumerable<string> Files { get; set; }
        public bool WaitForExit { get; set; } = true;
        public bool IgnoreErrors { get; set; }
        public int MaxParallelism { get; set; } = 1;

        public void Run(StepContext context)
        {
            var files = Files?
                .Select(e => e.FixUserPath()
                    .FixWindowsDisk()
                    .FixCurrentPath(context.Step.Direrctory)
                    .GetFullPath());

            Log.Information(
$"""
Parameters:
Files: {JsonConvert.SerializeObject(files)}
Wait For Exit: {WaitForExit}
Ignore Errors: {IgnoreErrors}
Max Parallelism: {MaxParallelism}

""");

            Parallel.ForEach(
                files,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = MaxParallelism,
                },
                Run);
        }

        private void Run(string file)
        {
            var info = new ProcessStartInfo()
            {
                FileName = file,
                UseShellExecute = true
            };
            var process = Process.Start(info);

            Log.Information($"Program started: '{file}'");

            if (!WaitForExit)
            {
                return;
            }

            if (process is null)
            {
                Log.Information($"Can't wait exit of: '{file}'");
                return;
            }

            process.WaitForExit();

            if (process.ExitCode == 0)
            {
                Log.Information($"Program finished: '{file}'");
                return;
            }

            Log.Information($"Program finished with errors: '{file}'. ExitCode: {process.ExitCode}.");

            if (IgnoreErrors)
                return;

            throw new Exception("The program finished with errors");
        }
    }
}
