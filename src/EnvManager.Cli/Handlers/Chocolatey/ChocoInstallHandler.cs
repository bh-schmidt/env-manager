using EnvManager.Cli.Common;
using EnvManager.Cli.Models.Chocolatey;
using ImprovedConsole;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Principal;

namespace EnvManager.Cli.Handlers.Chocolatey
{
    public class ChocoInstallHandler
    {
        private static readonly CliEventLogger _eventLogger = new();

        public static void Run(ChocoInstallStep step)
        {
            for (int i = 0; i < step.Packages.Count; i++)
            {
                var package = step.Packages[i];

                ConsoleWriter.WriteLine($"Installing package {package}");

                using (CustomLogger.AddPadding(4))
                {
                    Install(package, step.IgnoreErrors);
                }
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

            if (!ignoreErrors && proc.ExitCode != 0)
                throw new Exception($"An error ocurred installing the package. Status code: {proc.ExitCode}.");
        }
    }
}
