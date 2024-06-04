using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;

namespace Prg.Editor.BatchBuild
{
    /// <summary>
    /// Encapsulates UNITY <c>BuildReport</c> object as used in "Build Report Inspector".<br />
    /// See https://docs.unity3d.com/Packages/com.unity.build-report-inspector@0.1/manual/index.html
    /// </summary>
    /// <remarks>
    /// UNITY build pipeline returns this object but does not allow to save it for later use.<br />
    /// This utility is used to re-create it from file system 'binary file' that contains this information in proprietary format.
    /// </remarks>
    public static class UnityBuildReport
    {
        // UNITY build pipeline creates the during build.
        private const string LastBuildReportFilename = "Library/LastBuild.buildreport";
        // Our folder for collected BuildReport assets.
        private const string BuildReportAssetFolder = "Assets/BuildReports";
        private const string AssetFileExtension = ".buildreport";
        private const string AssetFilter = "t:BuildReport";

        /// <summary>
        /// Gets last UNITY Build Report from file.
        /// </summary>
        /// <remarks>
        /// This code is based on UNITY Build Report Inspector<br />
        /// https://docs.unity3d.com/Packages/com.unity.build-report-inspector@0.1/manual/index.html<br />
        /// https://github.com/Unity-Technologies/BuildReportInspector/blob/master/com.unity.build-report-inspector/Editor/BuildReportInspector.cs
        /// </remarks>
        /// <returns>the last <c>BuildReport</c> instance or <c>null</c> if one is not found</returns>
        public static BuildReport GetOrCreateLastBuildReport(bool deleteOldReports = true)
        {
            if (!File.Exists(LastBuildReportFilename))
            {
                Debug.Log($"UNITY Last Build Report file NOT FOUND: {LastBuildReportFilename}");
                Debug.Log("Remember to use BuildOptions.DetailedBuildReport when starting " +
                          "BuildPipeline.BuildPlayer() to get all data for the report.");
                return null;
            }
            if (!Directory.Exists(BuildReportAssetFolder))
            {
                Directory.CreateDirectory(BuildReportAssetFolder);
            }
            else if (deleteOldReports)
            {
                DeleteOldReports(DateTime.Now);
            }

            var date = File.GetLastWriteTime(LastBuildReportFilename);
            var name = $"Build_{date:yyyy-MM-dd_HH.mm.ss}";
            var buildreportAssetPath = $"{BuildReportAssetFolder}/{name}{AssetFileExtension}";

            // Load last Build Report.
            var buildReport = AssetDatabase.LoadAssetAtPath<BuildReport>(buildreportAssetPath);
            if (buildReport != null && buildReport.name == name)
            {
                return buildReport;
            }
            // Create new last Build Report.
            File.Copy(LastBuildReportFilename, buildreportAssetPath, true);
            AssetDatabase.ImportAsset(buildreportAssetPath);
            buildReport = AssetDatabase.LoadAssetAtPath<BuildReport>(buildreportAssetPath);
            buildReport.name = name;
            AssetDatabase.SaveAssets();
            return buildReport;
        }

        public static void DeleteOldReports(DateTime dateLimit, bool isVerbose = false)
        {
            if (isVerbose) Debug.Log("*");
            var assetGuids = AssetDatabase.FindAssets(AssetFilter, new[] { BuildReportAssetFolder });
            if (isVerbose) Debug.Log($"found build reports: {assetGuids.Length}");

            var assetPaths = new List<string>();
            foreach (var assetGuid in assetGuids)
            {
                var relativePath = AssetDatabase.GUIDToAssetPath(assetGuid);
                if (string.IsNullOrEmpty(relativePath))
                {
                    continue;
                }
                var fileDate = File.GetCreationTime(relativePath);
                var dateDifference = fileDate - dateLimit;
                if (dateDifference.Days >= 0)
                {
                    // Keep very new ones.
                    continue;
                }
                assetPaths.Add(relativePath);
            }
            var failed = new List<string>();
            // It seems that AssetDatabase itself logs all failed assets on console log.
            AssetDatabase.DeleteAssets(assetPaths.ToArray(), failed);
            if (isVerbose) Debug.Log($"deleted build reports: {assetPaths.Count - failed.Count}");
        }

        #region BuildReport Extension methods

        public static List<PackedAssetInfo> GetPackedAssets(this BuildReport buildReport)
        {
            List<PackedAssetInfo> packedAssets = new List<PackedAssetInfo>();
            foreach (var packedAsset in buildReport.packedAssets)
            {
                var contents = packedAsset.contents;
                foreach (var assetInfo in contents)
                {
                    var sourceAssetGuid = assetInfo.sourceAssetGUID.ToString();
                    if (sourceAssetGuid == "00000000000000000000000000000000" ||
                        sourceAssetGuid == "0000000000000000f000000000000000")
                    {
                        continue;
                    }
                    packedAssets.Add(assetInfo);
                }
            }
            return packedAssets;
        }

        #endregion
    }
}
