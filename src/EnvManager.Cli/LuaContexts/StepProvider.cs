using EnvManager.Cli.Models;
using EnvManager.Cli.Models.Chocolatey;
using EnvManager.Cli.Models.Files;
using MoonSharp.Interpreter;

namespace EnvManager.Cli.LuaContexts
{
    public class StepProvider
    {
        private Stage current;

        public StepProvider()
        {
            Steps["files"] = new Dictionary<string, object>
            {
                ["copy"] = GetFunction<CopyFilesStep>()
            };

            Steps["choco"] = new Dictionary<string, object>
            {
                ["install"] = GetFunction<ChocoInstallStep>()
            };
        }

        public Dictionary<string, object> Steps { get; } = [];

        public void SetCurrentStage(Stage stage)
        {
            current = stage;
        }

        private Func<DynValue, string> GetFunction<T>()
            where T : class, IStep, new()
        {
            return (DynValue value) =>
            {
                var task = DynValueParser.Parse<T>(value);

                if (current is null)
                    throw new Exception("Steps should only be used inside stages.");

                current.AddTask(task);
                task.Id = Guid.NewGuid();

                return task.Id.ToString();
            };
        }
    }
}
