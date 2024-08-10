using System.Text;

namespace EnvManager.Cli.Common.Extensions
{
    public static class StringBuilderExtensions
    {
        public static StringBuilder PadLinesLeft(this StringBuilder source, int padSize)
        {
            var pad = string.Empty.PadLeft(padSize);
            var linePad = Environment.NewLine + pad;

            return source
                .Insert(0, pad)
                .Replace("\r", "$\r$")
                .Replace("\n", "$\n$")
                .Replace("$\r$$\n$", linePad)
                .Replace("$\r$", linePad)
                .Replace("$\n$", linePad);
        }
    }
}
