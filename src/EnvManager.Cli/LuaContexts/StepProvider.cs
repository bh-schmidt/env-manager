using EnvManager.Cli.Models;
using EnvManager.Common;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.CoreLib;
using MoonSharp.Interpreter.Debugging;
using System.Reflection;

namespace EnvManager.Cli.LuaContexts
{
    public class StepProvider
    {
        private Stage currentStage;
        private string currentFile;

        public StepProvider()
        {
            var taskType = typeof(ITask);
            var types = Assembly.GetExecutingAssembly().GetTypes()
                .Where(e => e.IsClass && taskType.IsAssignableFrom(e));

            foreach (var type in types)
            {
                var instance = (ITask)Activator.CreateInstance(type);
                var split = instance.Code.Split('.');

                var current = Steps;
                foreach (var key in split.SkipLast(1))
                {
                    if (current.TryGetValue(key, out var obj))
                    {
                        if (obj is Dictionary<string, object> dic)
                        {
                            current = dic;
                            continue;
                        }

                        throw new InvalidCastException($"Can't convert {obj.GetType().FullName} to dictionary.");
                    }

                    var newDic = new Dictionary<string, object>();
                    current[key] = newDic;
                    current = newDic;
                }

                var method = GetType().GetMethod(nameof(GetFunction), BindingFlags.NonPublic | BindingFlags.Instance);
                var func = method.MakeGenericMethod(type);
                var lastKey = split.Last();
                current[lastKey] = func.Invoke(this, []);
            }
        }

        public Dictionary<string, object> Steps { get; } = [];

        public void SetCurrentStage(Stage stage)
        {
            currentStage = stage;
        }

        public void SetCurrentFile(string file)
        {
            currentFile = file;
        }

        private Func<DynValue, string> GetFunction<T>()
            where T : class, ITask, new()
        {
            return (DynValue value) =>
            {
                if (currentStage is null)
                    throw new Exception("Steps should only be used inside stages.");

                var step = DynValueParser.ParseStep<T>(value);
                step.File = currentFile.GetFullPath();

                var number = currentStage.Steps.Count + 1;
                step.Id = $"{currentStage.Id}_STEP_{number:000}";

                currentStage.AddStep(step);

                return step.Id;
            };
        }
    }
}
