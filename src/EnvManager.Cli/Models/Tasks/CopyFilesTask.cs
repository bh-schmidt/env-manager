using EnvManager.Tasks;
using ImprovedConsole.CommandRunners.Arguments;

namespace EnvManager.Models.Tasks
{
    public class CopyFilesTask : ITask
    {
        public string Name { get; set; }
        public string SourceFolder { get; set; }
        public string TargetFolder { get; set; }
        public IEnumerable<string> Files { get; set; }
        public IEnumerable<string> IgnoreList { get; set; }
        public bool Overwrite { get; set; }

        public void Run(CommandArguments arguments)
        {
            if (Files is null || !Files.Any())
                Files = ["**/*"];

            if (SourceFolder[^1] is not '/' or '\\')
                SourceFolder += '/';

            if (TargetFolder[^1] is not '/' or '\\')
                TargetFolder += '/';

            IgnoreList ??= [];

            CopyFilesHandler.Run(this, arguments);
        }
    }
}
