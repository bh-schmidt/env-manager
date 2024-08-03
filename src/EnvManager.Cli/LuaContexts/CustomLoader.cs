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
                .GetDirectoryName()
                .GetFullPath();

            var scriptsDir = Path.Combine(baseDir, "scripts");

            ModulePaths = [
                Path.Combine(baseDir, "?.lua"),
                Path.Combine(scriptsDir, "?.lua"),
                Path.Combine(scriptsDir, "?/init.lua"),
                Path.Combine(scriptsDir, "?/_init.lua"),
            ];
        }

        public override object LoadFile(string file, Table globalContext)
        {
            return File.ReadAllBytes(file);
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
