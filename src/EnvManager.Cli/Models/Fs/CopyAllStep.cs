using EnvManager.Cli.Handlers.Fs;

namespace EnvManager.Cli.Models.Fs
{
    public class CopyAllStep : ITask
    {
        public string Code { get; } = "fs.copy_all";

        public string SourceFolder { get; set; }
        public string TargetFolder { get; set; }
        public IEnumerable<string> Files { get; set; }
        public IEnumerable<string> IgnoreList { get; set; }
        public OverwriteAction FileExistsAction { get; set; }

        public void Run(StepContext context)
        {
            if (Files is null || !Files.Any())
                Files = ["**/*"];

            if (SourceFolder[^1] is not '/' or '\\')
                SourceFolder += '/';

            if (TargetFolder[^1] is not '/' or '\\')
                TargetFolder += '/';

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
