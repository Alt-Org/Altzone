using System;
using System.Globalization;

namespace Editor.Prg
{
    /// <summary>
    /// <c>CultureInfo</c> for custom editor code for sorting strings.
    /// </summary>
    public static class EditorCultureInfo
    {
        public static readonly CultureInfo forSorting = new CultureInfo("fi-FI");

        public static StringComparer sortComparer => StringComparer.Create(forSorting, ignoreCase: false);
    }
}
