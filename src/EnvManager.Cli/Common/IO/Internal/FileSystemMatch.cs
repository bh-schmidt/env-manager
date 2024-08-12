namespace EnvManager.Cli.Common.IO.Internal
{
    public record FileSystemMatch()
    {
        public string Path { get; init; }
        public bool IsDirectory { get; init; }
        public bool Excluded { get; init; }
    }
}

