using EnvManager.Cli.Common.Concurrency;
using EnvManager.Common;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace EnvManager.Cli.Common.IO.Internal
{
    public class FileMatcher(FileMatcherFilters options)
    {
        private ConcurrentCollection<PathMatcher> matchers;
        private readonly string sourceDir = options.SourceDir;
        private readonly IEnumerable<string> patterns = options.Patterns;
        private readonly Regex[] ignoreFiles = options.IgnorePatterns
            .Select(file => Path.Combine(options.SourceDir, file))
            .Select(Path.GetFullPath)
            .Select(file => file.ToRelativePath(options.SourceDir))
            .Distinct()
            .Select(file =>
            {
                var regex = RegexBuilder.BuildPathRegex(file);
                return new Regex(regex, RegexOptions.RightToLeft);
            })
            .ToArray();

        public int MaxConcurrency { get; init; } = 1;
        public ConcurrentBag<FileSystemMatch> Matches { get; } = [];
        public HashSet<string> HandledPaths { get; } = [];

        public void Match()
        {
            matchers = new()
            {
                MaxConcurrency = MaxConcurrency
            };

            foreach (var file in patterns)
            {
                var matcher = new PathMatcher
                {
                    Pattern = file,
                    CurrentDir = sourceDir,
                    SourceFolder = sourceDir,
                    Parts = Path
                         .Combine(sourceDir, file)
                         .GetFullPath()
                         .ToRelativePath(sourceDir)
                         .SplitPath()
                };

                matchers.Add(matcher);
            }

            matchers.Foreach(Search);
        }

        private void Add(IEnumerable<string> paths, bool isDirectory)
        {
            foreach (var path in paths)
            {
                lock (HandledPaths)
                {
                    if (HandledPaths.Contains(path))
                        continue;

                    HandledPaths.Add(path);
                }

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

        private void Search(PathMatcher matcher)
        {
            if (matcher.Parts.IsEmpty)
                return;

            var currentPart = matcher.Parts.Span[0];

            if (currentPart == "**")
            {
                var dirs = Directory.GetDirectories(matcher.CurrentDir);
                Add(dirs, true);

                matchers.Add(matcher with
                {
                    Parts = matcher.Parts[1..]
                });

                foreach (var dir in dirs)
                {
                    matchers.Add(matcher with
                    {
                        CurrentDir = dir,
                        Parts = matcher.Parts[1..]
                    });

                    matchers.Add(matcher with
                    {
                        CurrentDir = dir
                    });
                }

                return;
            }

            if (matcher.Parts.Length == 1)
            {
                var foundDirs = Directory.GetDirectories(matcher.CurrentDir, currentPart);
                Add(foundDirs, true);

                var foundFiles = Directory.GetFiles(matcher.CurrentDir, currentPart);
                Add(foundFiles, false);
                return;
            }

            var directories = Directory.GetDirectories(matcher.CurrentDir, currentPart);
            Add(directories, true);

            foreach (var directory in directories)
            {
                matchers.Add(matcher with
                {
                    CurrentDir = directory,
                    Parts = matcher.Parts[1..]
                });
            }
        }

        record PathMatcher
        {
            public string Pattern { get; set; }
            public string CurrentDir { get; set; }
            public string SourceFolder { get; set; }
            public Memory<string> Parts { get; set; }
        }
    }
}
