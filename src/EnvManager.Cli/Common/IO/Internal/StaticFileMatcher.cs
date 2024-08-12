using EnvManager.Common;
using System.Text.RegularExpressions;

namespace EnvManager.Cli.Common.IO.Internal
{
    public static class StaticFileMatcher
    {
        public static List<FileMatch> GetFiles(string source, IEnumerable<string> patterns, IEnumerable<string> ignorePatterns)
        {
            var sourceFiles = GetIncludedFiles(source, patterns)
                .Select(file => file.ToRelativePath(source))
                .Distinct()
                .ToArray();

            var ignoreFiles = ignorePatterns
                .Select(file => Path.Combine(source, file))
                .Select(Path.GetFullPath)
                .Select(file => file.ToRelativePath(source))
                .Distinct()
                .Select(file =>
                {
                    var regex = RegexBuilder.BuildPathRegex(file);
                    return new Regex(regex, RegexOptions.RightToLeft);
                })
                .ToArray();

            List<FileMatch> files = [];

            foreach (var file in sourceFiles)
            {
                if (ignoreFiles.Any(e => e.IsMatch(file)))
                {
                    files.Add(new FileMatch(file, false));
                    continue;
                }

                files.Add(new FileMatch(file));
            }

            return files;
        }

        private static List<string> GetIncludedFiles(string sourceFolder, IEnumerable<string> patterns)
        {
            List<string> files = [];
            foreach (var file in patterns)
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
