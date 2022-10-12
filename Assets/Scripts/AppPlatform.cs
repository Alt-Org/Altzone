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
    /// Check if we are running a device simulator mode inside UNITY Editor.
    /// </summary>
    public static bool IsSimulator
    {
        get
        {
            if (UnityEngine.Device.Application.installMode != ApplicationInstallMode.Editor)
            {
                // Simulator can run only inside Editor.
                return false;
            }
            if (UnityEngine.Device.SystemInfo.deviceType != DeviceType.Handheld)
            {
                // Simulator simulates handheld devices (for now).
                return false;
            }
            // Only Editor platforms can be simulated!
            return UnityEngine.Device.Application.platform is not
                (RuntimePlatform.WindowsEditor or RuntimePlatform.LinuxEditor or RuntimePlatform.OSXEditor);
        }
    }

    /// <summary>
    ///  Mobile platform (for consistency).
    /// </summary>
    public static bool IsMobile => Application.isMobilePlatform;

    /// <summary>
    /// Desktop platform is everything but mobile or console.
    /// </summary>
    public static bool IsDesktop => !(Application.isMobilePlatform || Application.isConsolePlatform);

    /// <summary>
    /// Windows platform can be editor, player or server.
    /// </summary>
    public static bool IsWindows { get; } = UnityEngine.Device.Application.platform is
        RuntimePlatform.WindowsEditor or RuntimePlatform.WindowsPlayer or RuntimePlatform.WindowsServer;

    /// <summary>
    /// Converts (UNITY) path separators to windows style (only on windows platform where we can have two directory separators).
    /// </summary>
    public static string ConvertToWindowsPath(string path) =>
        path.Replace(Path.AltDirectorySeparatorChar.ToString(), Path.DirectorySeparatorChar.ToString());
}