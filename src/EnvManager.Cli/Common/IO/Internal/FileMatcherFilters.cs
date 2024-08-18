namespace EnvManager.Cli.Common.IO.Internal
{
    public class FileMatcherFilters
    {
        public required string SourceDir { get; init; }
        public IEnumerable<string> IncludePatterns { get; init; } = ["**/*"];
        public IEnumerable<string> ExcludePatterns { get; init; } = [];
    }
}
