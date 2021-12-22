using System;
using System.Globalization;

namespace Editor.Prg.Editors
{
    /// <summary>
    /// <c>CultureInfo</c> for custom editor code for sorting strings.
    /// </summary>
    public static class EditorCultureInfo
    {
        private static readonly CultureInfo ForSorting = new CultureInfo("fi-FI");

        public static StringComparer SortComparer => StringComparer.Create(ForSorting, ignoreCase: false);
    }
}
