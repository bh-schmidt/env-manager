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

        public static string GetDirectoryName(this string path)
        {
            if (Path.IsPathRooted(path))
                return Path.GetDirectoryName(path);

            var fullPath = path.GetFullPath();
            var name = Path.GetDirectoryName(fullPath);
            return Path.GetRelativePath(".", name);
        }

        public static string GetFullPath(this string path)
        {
            return Path.GetFullPath(path);
        }

        public static string ToRelativePath(this string path, string relativeTo)
        {
            int size = relativeTo.Length;
            if (path[size] is '\\' or '/')
                size++;

            return path[size..];

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
