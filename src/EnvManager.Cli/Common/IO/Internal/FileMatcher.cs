using EnvManager.Cli.Common.Concurrency;
using EnvManager.Common;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;

namespace EnvManager.Cli.Common.IO.Internal
{
    public class FileMatcher(FileMatcherFilters options)
    {
        private ConcurrentCollection<PathMatcher> matchers;

        private readonly string sourceDir = options.SourceDir
            .FixUserPath()
            .FixWindowsDisk();

        private readonly IEnumerable<string> patterns = options.IncludePatterns;

        private readonly Regex[] ignoreFiles = options.ExcludePatterns
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

            matchers.Foreach(matcher =>
            {
                if (!DirHelper.TryGetDirectories(matcher.CurrentDir, out var dirs))
                {
                    AddDir(matcher.CurrentDir, false);
                    return;
                }

                AddDir(matcher.CurrentDir, true);

                if (matcher.Parts.IsEmpty)
                    return;

                var currentPart = matcher.Parts.Span[0];
                if (currentPart == "**")
                {
                    SearchAnyDirs(matcher, dirs);
                    return;
                }

                if (matcher.Parts.Length == 1)
                {
                    SearchLast(matcher);
                    return;
                }

                SearchDirs(matcher);
            });
        }

        private void AddFiles(IEnumerable<string> paths)
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
                        IsDirectory = false,
                        Path = path,
                        Excluded = true
                    });
                    continue;
                }

                Matches.Add(new()
                {
                    IsDirectory = false,
                    Path = path,
                    Excluded = false
                });
            }
        }

        private void AddDirs(IEnumerable<string> paths)
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
                        IsDirectory = true,
                        Path = path,
                        Excluded = true,
                    });
                    continue;
                }

                Matches.Add(new()
                {
                    IsDirectory = true,
                    Path = path,
                    Excluded = false,
                });
            }
        }

        private void AddDir(string path, bool hasAccess)
        {
            lock (HandledPaths)
            {
                if (HandledPaths.Contains(path))
                    return;

                HandledPaths.Add(path);
            }

            if (ignoreFiles.Any(e => e.IsMatch(path)))
            {
                Matches.Add(new()
                {
                    IsDirectory = true,
                    Path = path,
                    Excluded = true,
                    HasAccess = hasAccess
                });
                return;
            }

            Matches.Add(new()
            {
                IsDirectory = true,
                Path = path,
                Excluded = false,
                HasAccess = hasAccess
            });
        }

        private void SearchAnyDirs(PathMatcher matcher, string[] dirs)
        {
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
        }

        private void SearchLast(PathMatcher matcher)
        {
            var currentPart = matcher.Parts.Span[0];
            var foundDirs = Directory.GetDirectories(matcher.CurrentDir, currentPart);
            AddDirs(foundDirs);

            var foundFiles = Directory.GetFiles(matcher.CurrentDir, currentPart);
            AddFiles(foundFiles);
        }

        private void SearchDirs(PathMatcher matcher)
        {
            var currentPart = matcher.Parts.Span[0];
            var directories = Directory.GetDirectories(matcher.CurrentDir, currentPart);

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
