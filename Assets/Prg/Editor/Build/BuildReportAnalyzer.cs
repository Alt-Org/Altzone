using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Editor
{
    internal static class BuildReportAnalyzerMenu
    {
        [MenuItem("Altzone/Show Last Build Report", false, 10)]
        private static void ShowLastBuildReport() => BuildReportAnalyzer.ShowLastBuildReport();
    }

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
            if (scenesUsingAssets.Length > 0)
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

            var packedAssets = GetPackedAssets(buildReport.packedAssets);
            Debug.Log("*");
            Debug.Log($"Selected PackedAssets count {packedAssets.Count}");
            packedAssets = packedAssets.OrderBy(x => x.packedSize).Reverse().ToList();
            foreach (var assetInfo in packedAssets)
            {
                var packedSize = assetInfo.packedSize;
                var fileSize = (ulong)new FileInfo(assetInfo.sourceAssetPath).Length;
                var marker =
                    packedSize < fileSize ? "<color=white><b><</b></color>"
                    : packedSize > fileSize ? "<color=yellow><b>></b></color>"
                    : "=";
                Debug.Log(
                    $"{FormatSize(assetInfo.packedSize)} {marker} {FormatSize(fileSize)} {assetInfo.type.Name} {assetInfo.sourceAssetPath} {assetInfo.sourceAssetGUID}");
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

        private static List<PackedAssetInfo> GetPackedAssets(PackedAssets[] allPackedAssets)
        {
            var packedAssets = new List<PackedAssetInfo>();
            foreach (var packedAsset in allPackedAssets)
            {
                var contents = packedAsset.contents;
                foreach (var assetInfo in contents)
                {
                    if (assetInfo.packedSize < 1024)
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
                    var sourceAssetPath = assetInfo.sourceAssetPath;
                    if (sourceAssetPath.StartsWith("Packages/") ||
                        sourceAssetPath.StartsWith("Assets/Photon/")
                        || sourceAssetPath.StartsWith("Assets/Plugins/"))
                    {
                        continue;
                    }
                    packedAssets.Add(assetInfo);
                }
            }
            return packedAssets;
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
    }
}
