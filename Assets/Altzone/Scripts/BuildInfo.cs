using System;
using UnityEngine;

namespace Altzone.Scripts
{
    /// <summary>
    /// Machine generated code below!<br />
    /// Current Android BundleVersionCode, Patch (number as in Sem Ver) and CompiledOnDate.<br />
    /// BuildTagOrLabel can be used in development/debug builds to show this info.
    /// IsMuteOtherAudioSources is copied from <c>PlayerSettings.muteOtherAudioSources</c> on mobile platforms.<br />
    /// See: https://docs.unity3d.com/ScriptReference/PlayerSettings-muteOtherAudioSources.html
    /// </summary>
    /// <remarks>Patch value is reset to zero when BundleVersionCode is incremented.</remarks>
    internal static class BuildInfo
    {
        private const string BundleVersionCodeValue = "116";
        private const string PatchValue = "1";
        private const string CompiledOnDateValue = "2024-14-06 09:09";
        private const string BuildTagOrLabelValue = "test/build";
        private const bool IsMuteOtherAudioSourcesValue = false;

        public static string Version => $"{Application.version}.{BundleVersionCodeValue}.{PatchValue}";

        public static int BundleVersionCode => int.Parse(BundleVersionCodeValue);
        public static int Patch => int.Parse(PatchValue);
#if UNITY_EDITOR
        public static string CompiledOnDate => DateTime.Now.FormatMinutes();
        public static string BuildTagOrLabel => string.Empty;
#else
        public static string CompiledOnDate => CompiledOnDateValue;
        public static string BuildTagOrLabel => BuildTagOrLabelValue;
#endif
        public static bool IsMuteOtherAudioSources => IsMuteOtherAudioSourcesValue;
    }
}
