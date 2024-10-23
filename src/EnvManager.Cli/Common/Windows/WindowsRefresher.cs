using Microsoft.Win32;
using System.Collections;
using System.Runtime.Versioning;

namespace EnvManager.Cli.Common.Windows
{
    [SupportedOSPlatform("windows")]
    public class WindowsRefresher
    {
        private readonly static HashSet<string> mergedVars = new(StringComparer.OrdinalIgnoreCase)
        {
            "Path",
            "PATHEXT"
        };

        public static void Refresh()
        {
            var machineVariables = GetVariables(Registry.LocalMachine, @"SYSTEM\CurrentControlSet\Control\Session Manager\Environment");
            var userVariables = GetVariables(Registry.CurrentUser, @"Environment");
            var processVariables = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Process)
                .Cast<DictionaryEntry>()
                .ToDictionary(e => e.Key.ToString(), e => e.Value.ToString(), StringComparer.OrdinalIgnoreCase);

            var variables = machineVariables
                .ToDictionary();

            foreach (var uv in userVariables
                .Where(e => !mergedVars.Contains(e.Key)))
            {
                variables[uv.Key] = uv.Value;
            }

            foreach (var pv in processVariables
                .Where(e => !mergedVars.Contains(e.Key)))
            {
                variables[pv.Key] = pv.Value;
            }

            foreach (var mv in mergedVars)
            {
                var value = Merge(
                    machineVariables.GetValueOrDefault(mv, string.Empty),
                    userVariables.GetValueOrDefault(mv, string.Empty),
                    processVariables.GetValueOrDefault(mv, string.Empty));
                variables[mv] = value;
            }

            foreach (var variable in variables)
            {
                Environment.SetEnvironmentVariable(variable.Key, variable.Value, EnvironmentVariableTarget.Process);
            }

            var variablesToRemove = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Process)
                .Cast<DictionaryEntry>()
                .ToDictionary(e => e.Key.ToString(), e => e.Value.ToString())
                .Where(e => !variables.ContainsKey(e.Key));

            foreach (var variable in variablesToRemove)
                Environment.SetEnvironmentVariable(variable.Key, null, EnvironmentVariableTarget.Process);
        }

        private static string Merge(params string[] values)
        {
            var items = new HashSet<string>();
            foreach (var value in values)
            {
                var split = value.Split(';');
                if (split.Length == 0)
                    continue;

                foreach (var item in split)
                {
                    if (item.Length == 0)
                        continue;

                    if (items.Contains(item))
                        continue;

                    items.Add(item);
                }
            }

            return string.Join(';', items);
        }

        private static Dictionary<string, string> GetVariables(RegistryKey registryKey, string path)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var subKey = registryKey.OpenSubKey(path);
            var names = subKey.GetValueNames();

            foreach (var name in names)
            {
                var value = (string)subKey.GetValue(name);
                result[name] = value;
            }

            return result;
        }

    }
}
