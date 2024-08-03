using EnvManager.Cli.Models.Files;
using EnvManager.Common;
using ImprovedConsole;
using ImprovedConsole.CommandRunners.Arguments;
using System.Text.RegularExpressions;

namespace EnvManager.Cli.Handlers.Files
{
    public static class CopyFilesHandler
    {
        public static void Run(CopyFilesStep setting, CommandArguments arguments)
        {
            var verbose = arguments.Options["-v"] is not null;

            var sourceFolder = setting.SourceFolder
                .FixWindowsDisk()
                .FixUserPath()
                .GetFullPath();

            var targetFolder = setting.TargetFolder
                .FixWindowsDisk()
                .FixUserPath()
                .GetFullPath();

            if (!Directory.Exists(sourceFolder))
            {
                ConsoleWriter
                    .WriteLine($"The source folder does not exist ({sourceFolder})")
                    .WriteLine("Skipping...");
                return;
            }

            ConsoleWriter
                .WriteLine("Copying files.")
                .WriteLine($"Source folder: '{sourceFolder}'")
                .WriteLine($"Target folder: '{targetFolder}'")
                .WriteLine($"Overwrite files: '{setting.FileExistsAction}'")
                .WriteLine($"\nInclude patterns:");

            foreach (var item in setting.Files)
                ConsoleWriter.WriteLine($"\t{item}");

            ConsoleWriter.WriteLine($"\nExclude patterns:");
            foreach (var item in setting.IgnoreList)
                ConsoleWriter.WriteLine($"\t{item}");

            var files = GetFiles(sourceFolder, targetFolder, setting);

            if (verbose)
            {
                ConsoleWriter.Write("\n\n");
                if (files.Any())
                    ConsoleWriter.WriteLine("Copying the following files:");
                else
                    ConsoleWriter.WriteLine("No files matched");
            }

            foreach (var file in files)
            {
                var sourcePath = Path.Combine(sourceFolder, file);
                var targetPath = Path.Combine(targetFolder, file);
                var fileTargetFolder = Path.GetDirectoryName(targetPath);

                Directory.CreateDirectory(fileTargetFolder);

                if (File.Exists(targetPath))
                {
                    if (setting.FileExistsAction is CopyFilesStep.OverwriteAction.Throw)
                        throw new IOException($"The file '{targetPath}' already exists");

                    if (setting.FileExistsAction is CopyFilesStep.OverwriteAction.Ignore)
                    {
                        if (verbose)
                            ConsoleWriter.WriteLine($"File already exists (ignored): '{sourcePath}");
                        continue;
                    }

                    File.SetAttributes(targetPath, FileAttributes.Normal);
                }

                if (verbose)
                    ConsoleWriter.WriteLine($"Copying file: '{sourcePath}");

                File.Copy(sourcePath, targetPath, true);
            }
        }

        public static List<string> GetFiles(string sourceFolder, string targetFolder, CopyFilesStep setting)
        {
            var sourceFiles = GetIncludedFiles(sourceFolder, setting)
                .Select(file => file.ToRelativePath(sourceFolder))
                .Distinct()
                .ToArray();

            var ignoreFiles = setting.IgnoreList
                .Select(file => Path.Combine(sourceFolder, file))
                .Select(Path.GetFullPath)
                .Select(file => file.ToRelativePath(sourceFolder))
                .Distinct()
                .Select(file =>
                {
                    var regex = RegexBuilder.BuildPathRegex(file);
                    return new Regex(regex, RegexOptions.RightToLeft);
                })
                .ToArray();

            List<string> files = [];

            foreach (var file in sourceFiles)
            {
                if (ignoreFiles.Any(e => e.IsMatch(file)))
                    continue;

                files.Add(file);
            }

            return files;
        }

        private static List<string> GetIncludedFiles(string sourceFolder, CopyFilesStep setting)
        {
            List<string> files = [];
            foreach (var file in setting.Files)
            {
                var matcher = new PathMatcher
                {
                    Pattern = file,
                    SourceFolder = sourceFolder,
                    Parts = Path
                         .Combine(sourceFolder, file)
                         .GetFullPath()
                         .ToRelativePath(sourceFolder)
                         .SplitPath()
                };

                var foundFiles = HandleMatcher(sourceFolder, matcher);
                files.AddRange(foundFiles);
            }

            return files;
        }

        private static IEnumerable<string> HandleMatcher(string currentDir, PathMatcher matcher)
        {
            if (matcher.Parts.IsEmpty)
                return [];

            var currentPart = matcher.Parts[0];

            if (matcher.Parts.Length == 1)
            {
                var foundFiles = Directory.GetFiles(currentDir, currentPart);
                return foundFiles;
            }

            List<string> files = [];

            if (currentPart == "**")
            {
                var dirs = Directory.GetDirectories(currentDir);

                PathMatcher newMatcher = new()
                {
                    Pattern = matcher.Pattern,
                    SourceFolder = matcher.SourceFolder,
                    Parts = matcher.Parts[1..]
                };

                var found1 = HandleMatcher(currentDir, newMatcher);
                files.AddRange(found1);

                foreach (var dir in dirs)
                {
                    var found2 = HandleMatcher(dir, newMatcher);
                    var found3 = HandleMatcher(dir, matcher);

                    files.AddRange(found2);
                    files.AddRange(found3);
                }

                return files;
            }

            var directories = Directory.GetDirectories(currentDir, currentPart);
            foreach (var directory in directories)
            {
                PathMatcher newMatcher = new()
                {
                    Pattern = matcher.Pattern,
                    SourceFolder = matcher.SourceFolder,
                    Parts = matcher.Parts[1..]
                };
                var foundFiles = HandleMatcher(directory, newMatcher);
                files.AddRange(foundFiles);
            }

            return files;
        }

        ref struct PathMatcher
        {
            public string Pattern { get; set; }
            public string SourceFolder { get; set; }
            public ReadOnlySpan<string> Parts { get; set; }
        }
    }
}
