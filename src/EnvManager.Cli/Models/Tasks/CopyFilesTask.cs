﻿using EnvManager.Cli.Common;
using EnvManager.Cli.Tasks;
using ImprovedConsole.CommandRunners.Arguments;

namespace EnvManager.Cli.Models.Tasks
{
    public class CopyFilesTask : ITask
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Code { get; } = "CopyFiles";

        public string SourceFolder { get; set; }
        public string TargetFolder { get; set; }
        public IEnumerable<string> Files { get; set; }
        public IEnumerable<string> IgnoreList { get; set; }
        public OverwriteAction FileExistsAction { get; set; }

        public void Run(CommandArguments arguments, PipeLogger logger)
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

            CopyFilesHandler.Run(this, arguments, logger);
        }

        public enum OverwriteAction
        {
            Ignore = 1,
            Overwrite = 2,
            Throw = 3
        }
    }
}
