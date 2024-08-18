using EnvManager.Cli.Common;
using EnvManager.Cli.Common.Loggers;
using EnvManager.Cli.Common.Windows;
using EnvManager.Common;
using ImprovedConsole;
using MoonSharp.Interpreter;
using Serilog;
using Serilog.Context;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace EnvManager.Cli.Models
{
    public class Step
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool IgnoreErrors { get; set; }
        public DynValue Parameters { get; set; }
        public string File { get; internal set; }
        public string Direrctory => File.GetParentDirectory();
        internal ITask Task { get; set; }

        public void Run(StageContext stageContext)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                WindowsRefresher.Refresh();

            var stepContext = new StepContext(stageContext, this);
            var verbose = stageContext.Arguments.Options.Contains("-v");

            Log.Information(
$"""
Task started
Id: {Id}
Name: {Name}
Log File: {stepContext.LogFilePath}
Date: {LogCtx.GetCurrentDate()}
""");

            using (LogCtx.AddPadding(4))
            using (LogCtx.SetStepLogPath(stepContext.LogFilePath))
            {
                try
                {
                    var sw = new Stopwatch();
                    sw.Start();
                    Task.Run(stepContext);
                    sw.Stop();
                    Log.Information($"Elapsed: {sw.Elapsed}");
                }
                catch (Exception ex)
                {
                    var message = verbose ?
                        ex.Message + '\n' + ex.StackTrace :
                        ex.Message;

                    Log.Information("An error occurred.");
                    Log.Information(message);
                    throw;
                }
            }
        }
    }
}
