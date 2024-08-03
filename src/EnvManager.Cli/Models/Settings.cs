using EnvManager.Cli.Models.Tasks;
using Newtonsoft.Json.Linq;

namespace EnvManager.Cli.Models
{
    internal class Settings
    {
        public IEnumerable<JObject> Tasks { get; set; }
        public IEnumerable<ITask> InternalTasks { get; set; }

        public void TransformTasks()
        {
            List<ITask> tasks = new();

            foreach (var obj in Tasks)
            {
                var name = obj["name"].Value<string>();

                ITask task = name switch
                {
                    "CopyFiles" => obj.ToObject<CopyFilesTask>(),
                    _ => throw new Exception()
                };

                tasks.Add(task);
            }

            InternalTasks = tasks;
        }
    }
}
