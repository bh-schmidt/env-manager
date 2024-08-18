namespace EnvManager.Cli.Common.IO.Internal
{
    public record FileSystemMatch()
    {
        private bool? hasAccess;

        public string Path { get; init; }
        public bool IsDirectory { get; init; }
        public bool Excluded { get; init; }

        public bool HasAccess
        {
            get { return hasAccess ??= GetAccess(Path, IsDirectory); }
            init { hasAccess = value; }
        }

        private static bool GetAccess(string path, bool isDir)
        {
            if (isDir)
                return DirHelper.TryGetDirectories(path, out _);

            return true;
        }
    }
}

