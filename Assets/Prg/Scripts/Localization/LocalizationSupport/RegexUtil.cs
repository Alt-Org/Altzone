using System.Text.RegularExpressions;

namespace Prg.Util
{
    public static class RegexUtil
    {
        private const RegexOptions CultureInvariant = RegexOptions.CultureInvariant | RegexOptions.Compiled;
        private const RegexOptions MultiLine = RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.Compiled;

        private static readonly Regex _emptyLines = new(@"^\s+$[\r\n|\n\r|\r|\n]*", MultiLine);
        private static readonly Regex _tagsRegex = new(@"<[^>]*>", CultureInvariant);

        public static string RemoveAllTags(string text, string replacement = "") => _tagsRegex.Replace(text, replacement);

        public static string RemoveAllEmptyLines(string text) => _emptyLines.Replace(text, string.Empty);

        public static string ReplaceCrLf(string text, string replacement) => Regex.Replace(text, @"\r\n|\n\r|\r|\n", replacement);
    }
}
