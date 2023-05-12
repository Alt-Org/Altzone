using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

/// <summary>
/// Formats <c>DateTime</c> using Finnish culture info.<br />
/// https://blog.mzikmund.com/2019/11/using-proper-culture-with-c-string-interpolation/
/// </summary>
/// <remarks><c>CultureInfo</c> can be operating system dependent and user configurable.</remarks>
[SuppressMessage("ReSharper", "CheckNamespace")]
public static class DateFormat
{
    private static readonly CultureInfo FinnishCulture = CultureInfo.GetCultureInfo("fi-FI");

    public static string FormatMinutes(this DateTime dateTime)
    {

        FormattableString formattable = $"{dateTime:yyyy-dd-MM HH:mm}";
        return formattable.ToString(FinnishCulture);
    }

    public static string FormatSeconds(this DateTime dateTime)
    {

        FormattableString formattable = $"{dateTime:yyyy-dd-MM HH:mm:ss}";
        return formattable.ToString(FinnishCulture);
    }
}
