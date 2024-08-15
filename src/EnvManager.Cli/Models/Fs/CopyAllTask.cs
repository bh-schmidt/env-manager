using EnvManager.Cli.Models.Fs.Handlers;

namespace EnvManager.Cli.Models.Fs
{
    public class CopyAllTask : ITask
    {
        public string Code { get; } = "fs.copy_all";

        public string Source { get; set; }
        public string Target { get; set; }
        public IEnumerable<string> Files { get; set; }
        public IEnumerable<string> IgnoreList { get; set; }
        public OverwriteAction FileExistsAction { get; set; }

        public void Run(StepContext context)
        {
            if (Files is null || !Files.Any())
                Files = ["**/*"];

            if (Source[^1] is not '/' or '\\')
                Source += '/';

            if (Target[^1] is not '/' or '\\')
                Target += '/';

            IgnoreList ??= [];
            FileExistsAction = Enum.IsDefined(FileExistsAction) ?
                FileExistsAction :
                OverwriteAction.Throw;

            CopyAllHandler.Run(this, context);
        }

        public enum OverwriteAction
        {
            Ignore = 1,
            Overwrite = 2,
            Throw = 3
        }
    }
}
