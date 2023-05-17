using System;

namespace Prg
{
    /// <summary>
    /// Machine generated code below!<br />
    /// Last used Android BundleVersionCode and CompiledOnDate for any build. We go mobile first.
    /// </summary>
    public static class BuildProperties
    {
        private const string BundleVersionCodeValue = "57";
        private const string CompiledOnDateValue = "2023-16-05 08:57";

        public static string BundleVersionCode => BundleVersionCodeValue;

#if UNITY_EDITOR
        public static string CompiledOnDate => DateTime.Now.FormatMinutes();
#else
        public static string CompiledOnDate => CompiledOnDateValue;
#endif
    }
}