using System.IO;
using UnityEngine;

/// <summary>
/// Convenience class to access some platform specific features.<br />
/// Note that we have distinct separation of <c>IsEditor</c> and <c>IsDevelopmentBuild</c>, UNITY considers that they are "same".
/// </summary>
/// <remarks>
/// Most of these are getters because
/// static code analysis will otherwise complain about using compile time constants that are always <c>true</c> or <c>false</c>.
/// </remarks>
public static class AppPlatform
{
    public static bool IsEditor
    {
        get
        {
#if UNITY_EDITOR
            return true;
#else
            return false;
#endif
        }
    }

    public static bool IsDevelopmentBuild
    {
        get
        {
#if DEVELOPMENT_BUILD
            return true;
#else
            return false;
#endif
        }
    }

    public static bool IsWindows { get; } = Application.platform.ToString().ToLower().Contains("windows");

    /// <summary>
    /// Converts (UNITY) path separators to windows style (obviously on windows platform).
    /// </summary>
    public static string ConvertToWindowsPath(string path) =>
        path.Replace(Path.AltDirectorySeparatorChar.ToString(), Path.DirectorySeparatorChar.ToString());
}