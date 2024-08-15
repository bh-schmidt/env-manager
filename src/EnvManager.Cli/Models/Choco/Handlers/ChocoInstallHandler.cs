using EnvManager.Cli.Common.EventLoggers;
using EnvManager.Cli.Common.Loggers;
using Serilog;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace EnvManager.Cli.Models.Choco.Handlers
{
    public class ChocoInstallHandler
    {
        private static readonly ChocoEventLogger _eventLogger = new();

        public static void Run(ChocoInstallStep step)
        {
            for (int i = 0; i < step.Packages.Count; i++)
            {
                var package = step.Packages[i];

                Install(package, step.IgnoreErrors);
            }
        }

        private static void Install(string package, bool ignoreErrors)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                throw new Exception("Chocolatey is a windows only tool.");

            using (WindowsIdentity identity = WindowsIdentity.GetCurrent())
            {
                WindowsPrincipal principal = new(identity);

                if (!principal.IsInRole(WindowsBuiltInRole.Administrator))
                    throw new Exception("Admin privileges are required to install chocolatey packages.");
            }

            Log.Information($"Installing '{package}'...");

            ProcessStartInfo startInfo = new()
            {
                FileName = "choco",
                Arguments = $"install -y {package}",
                CreateNoWindow = true,
                RedirectStandardError = true,
                RedirectStandardOutput = true,
            };

            var proc = Process.Start(startInfo);
            ArgumentNullException.ThrowIfNull(proc);

            _eventLogger.Queue(proc.StandardOutput);
            _eventLogger.Queue(proc.StandardError);

            _eventLogger.Start();

            proc.WaitForExit();

            _eventLogger.Finish();

            if (proc.ExitCode == 0)
            {
                Log.Information($"Installation complete.");
                return;
            }

            if (ignoreErrors)
            {
                Log.Information($"An error ocurred during the installation.");
                return;
            }

            throw new Exception($"An error ocurred installing the package. Status code: {proc.ExitCode}.");
        }
    }
}
