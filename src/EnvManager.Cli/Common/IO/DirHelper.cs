using EnvManager.Cli.Common.IO.Internal;
using EnvManager.Common;

namespace EnvManager.Cli.Common.IO
{
    public class DirHelper
    {
        public static async Task CopyAsync(string sourceDir, string destinationDir, CancellationToken cancellationToken = default)
        {
            var matcher = new FileMatcher(sourceDir, ["**/*"], []);
            matcher.Match();

            await CopyAsync(matcher.Matches, sourceDir, destinationDir, true, cancellationToken);
        }

        public static async Task CopyAsync(string sourceDir, string destinationDir, bool replaceFiles, CancellationToken cancellationToken = default)
        {
            var matcher = new FileMatcher(sourceDir, ["**/*"], []);
            matcher.Match();

            await CopyAsync(matcher.Matches, sourceDir, destinationDir, replaceFiles, cancellationToken);
        }

        public static async Task CopyAsync(IEnumerable<FileSystemMatch> matches, string baseDir, string destinationDir, bool replaceFiles, CancellationToken cancellationToken = default)
        {
            if (cancellationToken == default)
            {
                var cts = new CancellationTokenSource();
                cts.CancelAfter(60 * 60 * 1000);
                cancellationToken = cts.Token;
            }

            var files = matches
                .Where(e => !e.IsDirectory)
                .Select(e => e.Path)
                .ToArray();

            await EnsureDownloadedAsync(files, cancellationToken);

            Directory.CreateDirectory(destinationDir);
            foreach (var match in matches)
            {
                var relativePath = match.Path.ToRelativePath(baseDir);
                var targetPath = destinationDir.CombinePathWith(relativePath);

                if (match.IsDirectory)
                {
                    Directory.CreateDirectory(targetPath);
                    continue;
                }

                if (File.Exists(targetPath))
                {
                    if (!replaceFiles)
                        continue;

                    File.SetAttributes(targetPath, FileAttributes.Normal);
                }

                File.Copy(match.Path, targetPath, true);

                var sourceAtt = File.GetAttributes(match.Path);
                File.SetAttributes(targetPath, sourceAtt);
            }
        }

        public static void Delete(string sourceDir)
        {
            Delete(new DirectoryInfo(sourceDir));
        }

        public static void Delete(DirectoryInfo directory)
        {
            static void FileHandler(FileInfo file)
            {
                file.Attributes = FileAttributes.Normal;
                file.Delete();
            }

            static void DirHandler(DirectoryInfo directory)
            {
                Delete(directory);
            }

            Iterate(
                directory,
                FileHandler,
                DirHandler);

            directory.Attributes = FileAttributes.Normal;
            directory.Delete();
        }

        private static async Task EnsureDownloadedAsync(IEnumerable<string> files, CancellationToken cancellationToken)
        {
        Start:
            var pendingFiles = files
                .Where(IsPlaceholder);

            if (!pendingFiles.Any())
                return;

            List<Task> tasks = [];

            foreach (var file in pendingFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var task = Task.Factory.StartNew(() =>
                {
                    Download(file);
                }, cancellationToken);
                tasks.Add(task);
            }

            await Task.WhenAll(tasks);

            await Task.Delay(5000, cancellationToken);
            goto Start;
        }

        private static void Download(string file)
        {
            try
            {
                var buffer = new char[1];
                using var stream = File.OpenRead(file);
                using var reader = new StreamReader(stream);
                reader.ReadBlock(buffer, 0, 1);
            }
            catch { }
        }

        static bool IsPlaceholder(string filename)
        {
            var att = File.GetAttributes(filename);
            return (((int)att) & 0x00400000) != 0;
        }

        private static void Iterate(DirectoryInfo directory, Action<FileInfo> fileHandler, Action<DirectoryInfo> dirHandler)
        {
            if (!directory.Exists)
                return;

            DirectoryInfo[] dirs = directory.GetDirectories();

            foreach (FileInfo file in directory.GetFiles())
                fileHandler?.Invoke(file);

            if (dirHandler is not null)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    dirHandler(subDir);
                }
            }

        }
    }
}
