using System;
using UnityEditor;

namespace Prg
{
    /// <summary>
    /// Machine generated code!<br />
    /// Last used Android BundleVersionCode and CompiledOnDate for any build. We go mobile first.
    /// </summary>
    public static class BundleVersionCode
    {
        private const string BundleVersionCodeValue = ".57";
        private const string CompiledOnDateValue = "2023-12-05 09:47";

#if UNITY_EDITOR
        public static string Get => PlayerSettings.Android.bundleVersionCode.ToString();
        public static string CompiledOnDate => DateTime.Now.FormatMinutes();
#else
        public static string Get => BundleVersionCodeValue;
        public static string CompiledOnDate => CompiledOnDateValue;
#endif
    }
}
