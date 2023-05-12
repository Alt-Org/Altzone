using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

/// <summary>
/// Formats <c>DateTime</c> using <c>CultureInfo.InvariantCulture</c> for consistent results.<br />
/// https://blog.mzikmund.com/2019/11/using-proper-culture-with-c-string-interpolation/
/// </summary>
/// <remarks>Default <c>CultureInfo</c> can be operating system dependent and user configurable.</remarks>
[SuppressMessage("ReSharper", "CheckNamespace")]
public static class DateFormat
{
    public static string FormatMinutes(this DateTime dateTime)
    {

        FormattableString formattable = $"{dateTime:yyyy-dd-MM HH:mm}";
        return formattable.ToString(CultureInfo.InvariantCulture);
    }

    public static string FormatSeconds(this DateTime dateTime)
    {

        FormattableString formattable = $"{dateTime:yyyy-dd-MM HH:mm:ss}";
        return formattable.ToString(CultureInfo.InvariantCulture);
    }
}
