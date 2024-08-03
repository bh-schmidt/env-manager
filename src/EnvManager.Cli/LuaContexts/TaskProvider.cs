using EnvManager.Cli.LuaContexts.Models;
using EnvManager.Cli.Models.Tasks;
using MoonSharp.Interpreter;

namespace EnvManager.Cli.LuaContexts
{
    public class TaskProvider
    {
        private Stage current;

        public TaskProvider()
        {
            Tasks = new Dictionary<string, object>();
            Tasks.Add("CopyFiles", (DynValue value) =>
            {
                var task = DynValueParser.Parse<CopyFilesTask>(value);

                if (current is null)
                    throw new Exception("Tasks should only be added inside stages.");

                current.AddTask(task);
                task.Id = Guid.NewGuid();

                return task.Id.ToString();
            });
        }

        public Dictionary<string, object> Tasks { get; }

        public void SetCurrentStage(Stage stage)
        {
            current = stage;
        }
    }
}
