using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Editor.Build
{
    internal static class BuildReportAnalyzer
    {
        private const string LastBuildReport = "Library/LastBuild.buildreport";
        private const string BuildReportDir = "Assets/BuildReports";

        public static void ShowLastBuildReport()
        {
            Debug.Log("*");
            var buildReport = GetOrCreateLastBuildReport();
            if (buildReport == null)
            {
                Debug.Log($"{LastBuildReport} NOT FOUND");
                return;
            }
            AnalyzeLastBuildReport(buildReport);
        }

        private static void AnalyzeLastBuildReport(BuildReport buildReport)
        {
            var summary = buildReport.summary;
            var buildTargetName = BuildPipeline.GetBuildTargetName(summary.platform);
            var buildStartedAt = $"{summary.buildStartedAt:yyyy-dd-MM HH:mm:ss}";
            var buildText = summary.result == BuildResult.Succeeded
                ? $"{buildStartedAt} {buildTargetName} {summary.result} {FormatSize(summary.totalSize)}"
                : $"{buildStartedAt} {buildTargetName} {summary.result}";
            Debug.Log($"Build {buildText} <color=orange><b>*</b></color>", buildReport);

            // Requires BuildOptions.DetailedBuildReport to be true for this data to be populated during build!
            var scenesUsingAssets = buildReport.scenesUsingAssets;
            if (scenesUsingAssets.Length == 0)
            {
                Debug.Log($"Scenes in build not available and it requires '<b>BuildOptions.DetailedBuildReport</b>' to be set!");
            }
            else
            {
                // Bill Of Materials for scenes: key is scene 'name' and content is list of assets used in this scene.
                var bom = new Dictionary<string, HashSet<string>>();
                GetScenesUsingAssets(scenesUsingAssets, bom);

                Debug.Log("*");
                Debug.Log($"Scenes in build {bom.Count}");
                foreach (var entry in bom)
                {
                    Debug.Log($"{entry.Key} has {entry.Value.Count} dependencies");
                }
            }

            var allBuildAssets = new List<BuildAssetInfo>();
            var largeAssets = GetLargeAndAllAssets(buildReport.packedAssets, ref allBuildAssets);

            var unusedAssets = GetUnusedAssets(allBuildAssets);
            Debug.Log("*");
            Debug.Log($"Unused Assets count {unusedAssets.Count}");
            unusedAssets = unusedAssets.OrderBy(x => x.MaxSize).Reverse().ToList();
            foreach (var assetInfo in unusedAssets)
            {
                Debug.Log(
                    $"{FormatSize(assetInfo.PackedSize)} <color=magenta><b>u</b></color> {FormatSize(assetInfo.FileSize)} {assetInfo.Type} {assetInfo.AssetPath} {assetInfo.AssetGuid}");
            }

            Debug.Log("*");
            Debug.Log($"Large Assets count {largeAssets.Count}");
            largeAssets = largeAssets.OrderBy(x => x.PackedSize).Reverse().ToList();
            foreach (var assetInfo in largeAssets)
            {
                var packedSize = assetInfo.PackedSize;
                var fileSize = assetInfo.FileSize;
                var marker =
                    packedSize < fileSize ? "<color=white><b><</b></color>"
                    : packedSize > fileSize ? "<color=yellow><b>></b></color>"
                    : "=";
                Debug.Log(
                    $"{FormatSize(packedSize)} {marker} {FormatSize(fileSize)} {assetInfo.Type} {assetInfo.AssetPath} {assetInfo.AssetGuid}");
            }
        }

        private static void GetScenesUsingAssets(ScenesUsingAssets[] scenesUsingAssets, Dictionary<string, HashSet<string>> bom)
        {
            // Plural - ScenesUsingAssets
            foreach (var assets in scenesUsingAssets)
            {
                // Singular - ScenesUsingAsset
                foreach (var asset in assets.list)
                {
                    foreach (var scenePath in asset.scenePaths)
                    {
                        if (!bom.TryGetValue(scenePath, out var assetList))
                        {
                            assetList = new HashSet<string>();
                            bom.Add(scenePath, assetList);
                        }
                        assetList.Add(asset.assetPath);
                    }
                }
            }
        }

        private static List<BuildAssetInfo> GetLargeAndAllAssets(PackedAssets[] allPackedAssets, ref List<BuildAssetInfo> allBuildAssets)
        {
            var largeAssets = new List<BuildAssetInfo>();
            foreach (var packedAsset in allPackedAssets)
            {
                var contents = packedAsset.contents;
                foreach (var assetInfo in contents)
                {
                    var sourceAssetPath = assetInfo.sourceAssetPath;
                    if (IsPathExcluded(sourceAssetPath))
                    {
                        continue;
                    }
                    if (assetInfo.type == typeof(MonoBehaviour))
                    {
                        continue;
                    }
                    var sourceAssetGuid = assetInfo.sourceAssetGUID.ToString();
                    if (sourceAssetGuid == "00000000000000000000000000000000" || sourceAssetGuid == "0000000000000000f000000000000000")
                    {
                        continue;
                    }
                    // Add to all build assets we want to analyze.
                    var buildAssetInfo = new BuildAssetInfo(assetInfo);
                    allBuildAssets.Add(buildAssetInfo);
                    if (assetInfo.packedSize < 1024)
                    {
                        continue;
                    }
                    // Add to large build assets we want to analyze.
                    largeAssets.Add(buildAssetInfo);
                }
            }
            return largeAssets;
        }

        private static List<BuildAssetInfo> GetUnusedAssets(List<BuildAssetInfo> usedAssets)
        {
            var allAssetGuids = AssetDatabase.FindAssets(string.Empty);
            Debug.Log($"allAssets {allAssetGuids.Length}");
            var unusedAssets = new List<BuildAssetInfo>();
            foreach (var assetGuid in allAssetGuids)
            {
                var isAssetInUse = usedAssets.Any(x => x.AssetGuid == assetGuid);
                if (isAssetInUse)
                {
                    continue;
                }
                var assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
                if (Directory.Exists(assetPath))
                {
                    continue;
                }
                if (IsPathExcluded(assetPath))
                {
                    continue;
                }
                unusedAssets.Add(new BuildAssetInfo(assetPath, assetGuid));
            }
            return unusedAssets;
        }

        private static bool IsPathExcluded(string path)
        {
            return path.StartsWith("Packages/") ||
                   path.StartsWith("Assets/BuildReport") ||
                   path.StartsWith("Assets/Photon/") ||
                   path.StartsWith("Assets/Plugins/") ||
                   path.StartsWith("Assets/TextMesh Pro/") ||
                   path.Contains("/Editor/");
        }

        private static BuildReport GetOrCreateLastBuildReport()
        {
            if (!File.Exists(LastBuildReport))
            {
                Debug.Log($"Last Build Report NOT FOUND: {LastBuildReport}");
                return null;
            }
            if (!Directory.Exists(BuildReportDir))
            {
                Directory.CreateDirectory(BuildReportDir);
            }

            var date = File.GetLastWriteTime(LastBuildReport);
            var name = $"Build_{date:yyyy-dd-MM_HH.mm.ss}";
            var assetPath = $"{BuildReportDir}/{name}.buildreport";

            // Load last Build Report.
            var buildReport = AssetDatabase.LoadAssetAtPath<BuildReport>(assetPath);
            if (buildReport != null && buildReport.name == name)
            {
                return buildReport;
            }
            // Create new last Build Report.
            File.Copy("Library/LastBuild.buildreport", assetPath, true);
            AssetDatabase.ImportAsset(assetPath);
            buildReport = AssetDatabase.LoadAssetAtPath<BuildReport>(assetPath);
            buildReport.name = name;
            AssetDatabase.SaveAssets();
            return buildReport;
        }

        private static string FormatSize(ulong bytes)
        {
            // https://www.atatus.com/blog/what-is-a-kibibyte/
            if (bytes < 1024)
            {
                return $"{bytes} B";
            }
            if (bytes < 1024 * 1024)
            {
                return $"{bytes / 1024.0:0.0} KiB";
            }
            return $"{bytes / 1024.0 / 1024.0:0.0} MiB";
        }

        /// <summary>
        /// Asset info for both used assets (<c>PackedAssetInfo</c>) and unused assets.
        /// </summary>
        private class BuildAssetInfo
        {
            public readonly string AssetPath;
            public readonly string AssetGuid;
            public readonly ulong PackedSize;
            public readonly ulong FileSize;
            public readonly ulong MaxSize;
            public readonly string Type;

            public BuildAssetInfo(PackedAssetInfo assetInfo)
            {
                AssetPath = assetInfo.sourceAssetPath;
                AssetGuid = assetInfo.sourceAssetGUID.ToString();
                PackedSize = assetInfo.packedSize;
                FileSize = (ulong)new FileInfo(AssetPath).Length;
                MaxSize = Math.Max(PackedSize, FileSize);
                Type = assetInfo.type.Name;
            }

            public BuildAssetInfo(string assetPath, string assetGuid)
            {
                AssetPath = assetPath;
                AssetGuid = assetGuid;
                PackedSize = 0;
                FileSize = (ulong)new FileInfo(AssetPath).Length;
                MaxSize = FileSize;
                Type = Path.GetExtension(AssetPath).Replace(".", string.Empty);
            }
        }
    }
}
