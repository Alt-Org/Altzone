using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace Altzone.Scripts
{
    /// <summary>
    /// Runtime compilation of some product version info.<br />
    /// <b>Version</b> should contain comprehensible version string for end users to see inside application.<br />
    /// <b>BundleVersionCode</b> is AndroidBundleVersionCode copied here because is can not be access otherwise at runtime.<br />
    /// <b>Patch</b> is for non-semantic versioning schemes to have an optional patch value.<br />
    /// <b>IsMuteOtherAudioSources</b> is relevant only on mobile platforms (optionally).
    /// </summary>
    /// <remarks>
    /// IsMuteOtherAudioSources is copied from <c>PlayerSettings.muteOtherAudioSources</c> on all platforms.<br />
    /// See: https://docs.unity3d.com/ScriptReference/PlayerSettings-muteOtherAudioSources.html
    /// </remarks>
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    internal static class BuildInfo
    {
        public const string CompiledOnDate =
#if UNITY_EDITOR
                "today"
#else
                MachineGeneratedBuildInfo.CompiledOnDateValue
#endif
            ;

        public static readonly string Version =
            $"{Application.version}.{MachineGeneratedBuildInfo.BundleVersionCodeValue}";

        public const int BundleVersionCode = MachineGeneratedBuildInfo.BundleVersionCodeValue;
        public const int Patch = MachineGeneratedBuildInfo.PatchValue;
        public const bool IsMuteOtherAudioSources = MachineGeneratedBuildInfo.IsMuteOtherAudioSourcesValue;
    }
}
