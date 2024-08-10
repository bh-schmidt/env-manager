using ImprovedConsole;

namespace EnvManager.Cli.Common.IO
{
    public class DirHelper
    {
        public static void Copy(string sourceDir, string destinationDir)
        {
            Copy(new DirectoryInfo(sourceDir), destinationDir, true, false);
        }

        public static void Copy(string sourceDir, string destinationDir, bool recursive, bool log)
        {
            Copy(new DirectoryInfo(sourceDir), destinationDir, recursive, log);
        }

        public static void Copy(DirectoryInfo sourceDir, string destinationDir, bool recursive, bool log)
        {
            Directory.CreateDirectory(destinationDir);

            void fileHandler(FileInfo file)
            {
                if (log)
                    ConsoleWriter.WriteLine($"Copying file: {file.FullName}");

                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath);
            }

            void value(DirectoryInfo dir)
            {
                string newDestinationDir = Path.Combine(destinationDir, dir.Name);
                Copy(dir, newDestinationDir, true, log);
            }

            Action<DirectoryInfo> dirHandler = recursive ?
                value :
                null;

            Iterate(
                sourceDir,
                fileHandler,
                dirHandler);
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
