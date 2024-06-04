﻿using EnvManager.Common;
using EnvManager.Models.Tasks;
using ImprovedConsole.CommandRunners.Arguments;
using System.Text.RegularExpressions;

namespace EnvManager.Tasks
{
    public static class CopyFilesHandler
    {
        public static void Run(CopyFilesTask setting, CommandArguments arguments)
        {
            var preview = arguments.Options["-p"] is not null;
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
                return;

            Console.WriteLine("Copying files.");
            Console.WriteLine($"Source folder: '{sourceFolder}'");
            Console.WriteLine($"Target folder: '{targetFolder}'");
            Console.WriteLine($"Overwrite files: '{setting.Overwrite}'");
            Console.WriteLine($"\nInclude patterns:");
            foreach (var item in setting.Files)
                Console.WriteLine($"\t{item}");

            Console.WriteLine($"\nExclude patterns:");
            foreach (var item in setting.IgnoreList)
                Console.WriteLine($"\t{item}");

            var files = GetFiles(sourceFolder, targetFolder, setting);

            if (verbose)
            {
                Console.Write("\n\n");
                if (files.Any())
                    Console.WriteLine("Copying the following files:");
                else
                    Console.WriteLine("No files matched");
            }

            foreach (var file in files)
            {
                var sourcePath = Path.Combine(sourceFolder, file);
                var targetPath = Path.Combine(targetFolder, file);
                var fileTargetFolder = Path.GetDirectoryName(targetPath);

                if (verbose)
                    Console.WriteLine($"Copying file: '{sourcePath}");

                if (!preview)
                {
                    if (!Directory.Exists(fileTargetFolder))
                        Directory.CreateDirectory(fileTargetFolder);

                    if (File.Exists(targetPath))
                        File.SetAttributes(targetPath, FileAttributes.Normal);

                    File.Copy(sourcePath, targetPath, setting.Overwrite);
                }
            }
        }

        public static List<string> GetFiles(string sourceFolder, string targetFolder, CopyFilesTask setting)
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

        private static List<string> GetIncludedFiles(string sourceFolder, CopyFilesTask setting)
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

                foreach (var dir in dirs)
                {
                    PathMatcher newMatcher = new()
                    {
                        Pattern = matcher.Pattern,
                        SourceFolder = matcher.SourceFolder,
                        Parts = matcher.Parts[1..]
                    };

                    var found1 = HandleMatcher(currentDir, newMatcher);
                    var found2 = HandleMatcher(dir, newMatcher);
                    var found3 = HandleMatcher(dir, matcher);

                    files.AddRange(found1);
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