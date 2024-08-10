using System.Text.RegularExpressions;

namespace EnvManager.Cli.Common
{
    public static partial class StringExtensions
    {
        public static string PadLinesLeft(this string source, int padSize)
        {
            if(padSize == 0)
                return source;

            var x1 = "\r" + new string(' ', padSize);
            var x2 = "\n" + new string(' ', padSize);

            return PaddingRegex()
                .Replace(source, match =>
                {
                    if (match.Value == "\r")
                        return x1;

                    return x2;
                });
        }

        [GeneratedRegex("([\r](?![\n])|[\n])", RegexOptions.Multiline)]
        private static partial Regex PaddingRegex();
    }
}
