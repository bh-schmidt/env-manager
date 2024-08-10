using EnvManager.Common;

namespace EnvManager.Cli.Common.Loggers
{
    public class FileContext : IDisposable
    {
        private long last = long.MinValue;

        public FileContext(string path)
        {
            var dir = path.GetParentDirectory();
            Directory.CreateDirectory(dir);
            Stream = File.Open(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
            Stream.Seek(0, SeekOrigin.End);
            Writer = new StreamWriter(Stream);
            Path = path;
        }

        public Stream Stream { get; }
        public StreamWriter Writer { get; }
        public required Action<FileContext> Kill { get; init; }
        public string Path { get; }

        public void DebounceKill()
        {
            var current = last++;
            _ = Task.Delay(30_000)
                .ContinueWith(task =>
                {
                    if (current == last)
                        Kill(this);

                    task.Dispose();
                    return Task.CompletedTask;
                });
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Writer.Dispose();
            Stream.Dispose();
        }
    }
}
