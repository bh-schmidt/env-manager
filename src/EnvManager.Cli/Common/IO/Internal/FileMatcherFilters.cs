namespace EnvManager.Cli.Common.IO.Internal
{
    public class FileMatcherFilters
    {
        public required string SourceDir { get; init; }
        public IEnumerable<string> Patterns { get; init; } = ["**/*"];
        public IEnumerable<string> IgnorePatterns { get; init; } = [];
    }
}
