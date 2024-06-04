using System.Text;
using System.Text.RegularExpressions;

namespace EnvManager.Common
{
    public static partial class RegexBuilder
    {
        static readonly string[] specialChars = ["\\", "/", ".", "^", "$", "*", "+", "?", "(", ")", "[", "]", "{", "}", "|",];

        public static string ReplaceSpecialCharacters(string value)
        {
            StringBuilder builder = new(value);

            foreach (string @char in specialChars)
                builder.Replace(@char, $@"\{@char}");

            return builder.ToString();
        }

        public static string BuildPathRegex(string value)
        {
            var baseRegex = ReplacePathRegex()
                .Replace(value, delegate (Match match)
                {
                    if (match.Groups[1].Success)
                        return @"(.+[\/\\])?";

                    if (match.Groups[2].Success)
                        return @"[^\/\\]+";

                    return $@"\{match.Groups[3].Value}";
                });

            return $"^{baseRegex}$";
        }

        [GeneratedRegex(@"([*]{2}[\/\\])|([*])|([\/\\.\^\$\*\+\?\(\)\[\]\{\}\|])")]
        private static partial Regex ReplacePathRegex();
    }
}
