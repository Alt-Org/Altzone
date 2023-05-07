using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Editor.Build
{
    internal static class BuildReportAnalyzer
    {
        private const string LastBuildReport = "Library/LastBuild.buildreport";
        private const string BuildReportDir = "Assets/BuildReports";

        private const string HtmlFilename = "Assets/BuildReports/BuildReport.html";

        public static void ShowLastBuildReport(bool logDetails = false)
        {
            Debug.Log("*");
            var buildReport = GetOrCreateLastBuildReport();
            if (buildReport == null)
            {
                Debug.Log($"{LastBuildReport} NOT FOUND");
                return;
            }
            AnalyzeLastBuildReport(buildReport, logDetails);
        }

        private static void AnalyzeLastBuildReport(BuildReport buildReport, bool logDetails)
        {
            var summary = buildReport.summary;
            var buildTargetName = BuildPipeline.GetBuildTargetName(summary.platform);
            var buildStartedAt = $"{summary.buildStartedAt:yyyy-dd-MM HH:mm:ss}";
            var buildText = summary.result == BuildResult.Succeeded
                ? $"{buildStartedAt} {buildTargetName} {summary.result} {FormatSize(summary.totalSize)}"
                : $"{buildStartedAt} {buildTargetName} {summary.result}";
            Debug.Log($"Build {buildText} <color=orange><b>*</b></color>", buildReport);
            Debug.Log("*");

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

                Debug.Log($"Scenes in build {bom.Count}");
                if (logDetails)
                {
                    Debug.Log("*");
                    foreach (var entry in bom)
                    {
                        Debug.Log($"{entry.Key} has {entry.Value.Count} dependencies");
                    }
                }
            }

            var allBuildAssets = new List<BuildAssetInfo>();
            var largeAssets = GetLargeAndAllAssets(buildReport.packedAssets, ref allBuildAssets);

            var unusedAssets = GetUnusedAssets(allBuildAssets);
            Debug.Log($"Unused Assets count {unusedAssets.Count}");
            if (logDetails)
            {
                Debug.Log("*");
                unusedAssets = unusedAssets.OrderBy(x => x.MaxSize).Reverse().ToList();
                foreach (var assetInfo in unusedAssets)
                {
                    Debug.Log(
                        $"{FormatSize(assetInfo.PackedSize)} <color=magenta><b>u</b></color> {FormatSize(assetInfo.FileSize)} {assetInfo.Type} {assetInfo.AssetPath} {assetInfo.AssetGuid}");
                }
            }
            Debug.Log($"Large Assets count {largeAssets.Count}");
            if (logDetails)
            {
                Debug.Log("*");
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
            CreateBuildReportHtmlPage(unusedAssets, largeAssets, summary);
        }

        private static void CreateBuildReportHtmlPage(List<BuildAssetInfo> unusedAssets, List<BuildAssetInfo> largeAssets, BuildSummary summary)
        {
            // Putting padding between the columns using CSS:
            // https://stackoverflow.com/questions/11800975/html-table-needs-spacing-between-columns-not-rows
            const string htmlStart = @"<!DOCTYPE html>
<html>
<head>
<style>
html * {
  font-family: Arial, Helvetica, sans-serif;
}
body {
  background-color: linen;
}
tr > * + * {
  padding-left: .5em;
}
th {
  text-align: left;
}
.smaller {
  font-size: smaller;
}
.unused {
  color: Coral;
}
.less {
  color: DarkSeaGreen;
}
.more {
  color: DarkSalmon;
}
.same {
  color: DarkGray;
}
.megabytes {
  color: DarkBlue;
}
.kilobytes {
  color: DarkSlateGray;
}
.bytes {
  color: Silver;
}
.texture2d {
  color: DarkRed;
}
.for-test {
  color: CadetBlue;
}
</style>
<title>@Build_Report@</title>
</head>
<body>
<table>";
            const string htmlEnd = @"</body>
</html>";
            const string tableEnd = @"</table>";

            var allAssets = new List<BuildAssetInfo>(unusedAssets);
            allAssets.AddRange(largeAssets);
            allAssets = allAssets.OrderBy(x => x.MaxSize).Reverse().ToList();

            var buildName = BuildPipeline.GetBuildTargetName(summary.platform);
            var fixedHtmlStart = htmlStart.Replace("@Build_Report@", $"Altzone {buildName} Build Report");
            var builder = new StringBuilder()
                .Append(fixedHtmlStart).AppendLine()
                .Append("<tr>")
                .Append($"<th>PackedSize</th>")
                .Append($"<th>Check</th>")
                .Append($"<th>FileSize</th>")
                .Append($"<th>Type</th>")
                .Append($"<th>Name</th>")
                .Append($"<th>Path</th>")
                .Append("</tr>").AppendLine();

            foreach (var a in allAssets)
            {
                var marker = a.PackedSize == 0 ? @"<span class=""unused"">unused</span>"
                    : a.PackedSize < a.FileSize ? @"<span class=""less"">less</span>"
                    : a.PackedSize > a.FileSize ? @"<span class=""more"">more</span>"
                    : @"<span class=""same"">same</span>";
                var name = Path.GetFileName(a.AssetPath);
                if (a.IsTest)
                {
                    name = $"<span class=\"for-test\">{name}</span>";
                }
                var folder = Path.GetDirectoryName(a.AssetPath);
                builder
                    .Append("<tr>")
                    .Append($"<td{GetStyleFromFileSize(a.PackedSize)}>{FormatSize(a.PackedSize)}</td>")
                    .Append($"<td>{marker}</td>")
                    .Append($"<td{GetStyleFromFileSize(a.FileSize)}>{FormatSize(a.FileSize)}</td>")
                    .Append($"<td>{GetTypeExplained(a)}</td>")
                    .Append($"<td>{name}</td>")
                    .Append($"<td>{folder}</td>")
                    .Append("</tr>").AppendLine();
            }
            builder
                .Append(tableEnd).AppendLine()
                .Append($"<p>Table row count is {allAssets.Count}</p>").AppendLine()
                .Append($"<p>Build for {buildName} platform" +
                        $" on {summary.buildEndedAt:yyyy-dd-MM HH:mm:ss}" +
                        $" output size is {FormatSize(summary.totalSize)}</p>").AppendLine()
                .Append($"<p class=\"smaller\">Page created on {DateTime.Now:yyyy-dd-MM HH:mm:ss}</p>").AppendLine()
                .Append(htmlEnd);

            var content = builder.ToString();
            File.WriteAllText(HtmlFilename, content);
            var htmlPath = Path.GetFullPath(HtmlFilename);
            Debug.Log($"Application.OpenURL {htmlPath}");
            Application.OpenURL(htmlPath);

            string GetStyleFromFileSize(ulong fileSize)
            {
                if (fileSize < 1024)
                {
                    return @" class=""bytes""";
                }
                if (fileSize < 1024 * 1024)
                {
                    return @" class=""kilobytes""";
                }
                return @" class=""megabytes""";
            }
        }

        private static string GetTypeExplained(BuildAssetInfo assetInfo)
        {
            if (assetInfo.Type != "Texture2D")
            {
                return assetInfo.Type;
            }
            var asset = AssetDatabase.LoadAssetAtPath<Texture2D>(assetInfo.AssetPath);
            if (asset == null)
            {
                return assetInfo.Type;
            }
            // Recommended, default, and supported texture formats, by platform
            // https://docs.unity3d.com/Manual/class-TextureImporterOverride.html
            var assetFormat = asset.format.ToString();
            var format = assetFormat.Contains("ETC2") || assetFormat.Contains("DXT5")
                ? $"<b>{asset.format}</b>"
                : asset.format.ToString();
            return $"<span class=\"texture2d\">{format} {asset.width}x{asset.height}</span>";
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
            // Note that scenes are not included in Build Report as other assets.
            return path.StartsWith("Packages/") ||
                   path.StartsWith("Assets/BuildReport") ||
                   path.StartsWith("Assets/Photon/") ||
                   path.StartsWith("Assets/Plugins/") ||
                   path.StartsWith("Assets/Tests/") ||
                   path.StartsWith("Assets/TextMesh Pro/") ||
                   path.Contains("/Editor/") ||
                   path.Contains("/Test/") ||
                   path.EndsWith(".asmdef") ||
                   path.EndsWith(".asmref") ||
                   path.EndsWith(".cs") ||
                   path.EndsWith(".unity");
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
            public readonly bool IsTest;

            public BuildAssetInfo(PackedAssetInfo assetInfo)
            {
                AssetPath = assetInfo.sourceAssetPath;
                AssetGuid = assetInfo.sourceAssetGUID.ToString();
                PackedSize = assetInfo.packedSize;
                FileSize = (ulong)new FileInfo(AssetPath).Length;
                MaxSize = Math.Max(PackedSize, FileSize);
                Type = assetInfo.type.Name;
                IsTest = AssetPath.Contains("Test");
            }

            public BuildAssetInfo(string assetPath, string assetGuid)
            {
                AssetPath = assetPath;
                AssetGuid = assetGuid;
                PackedSize = 0;
                FileSize = (ulong)new FileInfo(AssetPath).Length;
                MaxSize = FileSize;
                Type = Path.GetExtension(AssetPath).Replace(".", string.Empty);
                IsTest = AssetPath.Contains("Test");
            }
        }
    }
}
