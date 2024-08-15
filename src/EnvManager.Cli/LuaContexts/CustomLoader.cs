using EnvManager.Common;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Loaders;

namespace EnvManager.Cli.LuaContexts
{
    public class CustomLoader : ScriptLoaderBase
    {
        private readonly string baseDir;

        public CustomLoader(string firstFile)
        {
            baseDir = firstFile
                .GetParentDirectory()
                .GetFullPath();

            var scriptsDir = Path.Combine(baseDir, "scripts");

            ModulePaths = [
                Path.Combine(baseDir, "?.lua"),
                Path.Combine(baseDir, "?/init.lua"),
                Path.Combine(baseDir, "?/_init.lua"),
            ];
        }

        public override object LoadFile(string file, Table globalContext)
        {
            var lines = GetText(file);
            return string.Join('\n', lines);
        }

        private IEnumerable<string> GetText(string file)
        {
            var threatedFile = file.Replace("\\", "/");
            var lines = File.ReadAllLines(file);

            yield return $"__set_current_file('{threatedFile}')";
            foreach (var line in lines)
            {
                yield return line;
                if (line.Trim().StartsWith("require"))
                    yield return $"__set_current_file('{threatedFile}')";
            }
        }

        public override string ResolveFileName(string filename, Table globalContext)
        {
            var path = filename
                .FixUserPath()
                .FixWindowsDisk();

            path = Path.Combine(baseDir, path);

            return base.ResolveFileName(path, globalContext);
        }

        public override bool ScriptFileExists(string name)
        {
            return File.Exists(name);
        }
    }
}
