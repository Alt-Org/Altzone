using System.IO;
using UnityEngine;

/// <summary>
/// Convenience class for platform detection to access platform specific features.<br />
/// Note that we have distinct separation of <c>IsEditor</c> and <c>IsDevelopmentBuild</c>, UNITY considers that they are "same".
/// </summary>
/// <remarks>
/// Most of these are getters because
/// static code analysis will otherwise complain about using compile time constants that are always <c>true</c> or <c>false</c>.
/// </remarks>
public static class AppPlatform
{
    /// <summary>
    /// Alias for UNITY <c>Application.isEditor</c>.
    /// </summary>
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

    /// <summary>
    /// Replacement for UNITY <c>Debug.isDebugBuild</c> that returns <c>true</c> when running outside UNITY Editor
    /// and check box called "Development Build" is checked.
    /// </summary>
    /// <remarks>
    /// See differences from https://docs.unity3d.com/2021.3/Documentation/ScriptReference/Debug-isDebugBuild.html
    /// </remarks>
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

    /// <summary>
    /// Windows platform can be editor, player and server.
    /// </summary>
    public static bool IsWindows { get; } = Application.platform.ToString().ToLower().StartsWith("windows");

    /// <summary>
    /// Converts (UNITY) path separators to windows style (only on windows platform where we can have two directory separators).
    /// </summary>
    public static string ConvertToWindowsPath(string path) =>
        path.Replace(Path.AltDirectorySeparatorChar.ToString(), Path.DirectorySeparatorChar.ToString());
}