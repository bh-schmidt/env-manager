using EnvManager.Common;
using System.Text.RegularExpressions;

namespace EnvManager.Cli.Common.IO.Internal
{
    public class FileMatcher
    {
        private readonly string sourceDir;
        private readonly IEnumerable<string> patterns;
        private readonly Regex[] ignoreFiles;

        public FileMatcher(string sourceDir, IEnumerable<string> patterns, IEnumerable<string> ignorePatterns)
        {
            this.sourceDir = sourceDir;
            this.patterns = patterns;
            ignoreFiles = ignorePatterns
                .Select(file => Path.Combine(sourceDir, file))
                .Select(Path.GetFullPath)
                .Select(file => file.ToRelativePath(sourceDir))
                .Distinct()
                .Select(file =>
                {
                    var regex = RegexBuilder.BuildPathRegex(file);
                    return new Regex(regex, RegexOptions.RightToLeft);
                })
                .ToArray();
        }

        public List<FileSystemMatch> Matches { get; } = [];
        public HashSet<string> HandledPaths { get; } = [];

        public void Match()
        {
            foreach (var file in patterns)
            {
                var matcher = new PathMatcher
                {
                    Pattern = file,
                    SourceFolder = sourceDir,
                    Parts = Path
                         .Combine(sourceDir, file)
                         .GetFullPath()
                         .ToRelativePath(sourceDir)
                         .SplitPath()
                };

                Search(sourceDir, matcher);
            }
        }

        private void Add(IEnumerable<string> paths, bool isDirectory)
        {
            foreach (var path in paths)
            {
                if (HandledPaths.Contains(path))
                    continue;

                HandledPaths.Add(path);

                if (ignoreFiles.Any(e => e.IsMatch(path)))
                {
                    Matches.Add(new()
                    {
                        IsDirectory = isDirectory,
                        Path = path,
                        Excluded = true
                    });
                    continue;
                }

                Matches.Add(new()
                {
                    IsDirectory = isDirectory,
                    Path = path,
                    Excluded = false
                });
            }
        }

        private void Search(string currentDir, PathMatcher matcher)
        {
            if (matcher.Parts.IsEmpty)
                return;

            var currentPart = matcher.Parts[0];

            if (currentPart == "**")
            {
                var dirs = Directory.GetDirectories(currentDir);
                Add(dirs, true);

                PathMatcher newMatcher = new()
                {
                    Pattern = matcher.Pattern,
                    SourceFolder = matcher.SourceFolder,
                    Parts = matcher.Parts[1..]
                };

                Search(currentDir, newMatcher);

                foreach (var dir in dirs)
                {
                    Search(dir, newMatcher);
                    Search(dir, matcher);
                }

                return;
            }

            if (matcher.Parts.Length == 1)
            {
                var foundDirs = Directory.GetDirectories(currentDir, currentPart);
                Add(foundDirs, true);

                var foundFiles = Directory.GetFiles(currentDir, currentPart);
                Add(foundFiles, false);
                return;
            }

            var directories = Directory.GetDirectories(currentDir, currentPart);
            Add(directories, true);

            foreach (var directory in directories)
            {
                PathMatcher newMatcher = new()
                {
                    Pattern = matcher.Pattern,
                    SourceFolder = matcher.SourceFolder,
                    Parts = matcher.Parts[1..]
                };
                Search(directory, newMatcher);
            }
        }

        ref struct PathMatcher
        {
            public string Pattern { get; set; }
            public string SourceFolder { get; set; }
            public ReadOnlySpan<string> Parts { get; set; }
        }
    }
}
