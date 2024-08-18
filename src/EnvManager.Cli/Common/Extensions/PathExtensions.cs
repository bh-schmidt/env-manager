using System.Diagnostics;
using System.Text.RegularExpressions;

namespace EnvManager.Common
{
    public static partial class PathExtensions
    {
        public static string FixWindowsDisk(this string path)
        {
            if (OperatingSystem.IsWindows() && WindowsDiskRegex().IsMatch(path))
                return WindowsDiskRegex().Replace(path, "$1:/");

            return path;
        }

        public static string FixUserPath(this string path)
        {
            if (path.StartsWith('~'))
            {
                var userPath = Environment.GetEnvironmentVariable("USERPROFILE");
                return userPath + path[1..];
            }

            return path;
        }

        public static string FixCurrentPath(this string path, string newPath)
        {
            return newPath.CombinePathWith(path);
        }

        public static string CombinePathWith(this string source, string path)
        {
            return Path.Combine(source, path);
        }


        public static bool DirectoryExists(this string path)
        {
            return Directory.Exists(path);
        }

        public static string GetParentDirectory(this string path)
        {
            if (!Path.IsPathRooted(path))
            {
                var fullPath = path.GetFullPath();
                var name = fullPath.GetParentDirectory();
                return Path.GetRelativePath(".", name);
            }

            if (path[^1] is '\\' or '/')
            {
                return Path.GetDirectoryName(path)
                    .GetParentDirectory();
            }

            return Path.GetDirectoryName(path);
        }

        public static string GetFullPath(this string path)
        {
            return Path.GetFullPath(path);
        }

        public static string ToRelativePath(this string path, string relativeTo)
        {
            return Path.GetRelativePath(relativeTo, path);
        }

        public static string[] SplitPath(this string path)
        {
            return SplitPathRegex()
                .Split(path);
        }

        [GeneratedRegex(@"^/([a-zA-Z])/")]
        private static partial Regex WindowsDiskRegex();

        [GeneratedRegex(@"[\/\\]")]
        private static partial Regex SplitPathRegex();
    }
}
