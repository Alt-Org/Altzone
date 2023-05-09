using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Prg.Editor.Build
{
    /// <summary>
    /// Read and analyze UNITY Build Report (in binary format) by converting it to internal YAML format and saving as an asset.
    /// </summary>
    /// <remarks>
    /// Idea and tech is from UNITY Build Report Inspector.
    /// </remarks>
    internal static class BuildReportAnalyzer
    {
        private const string LastBuildReportPath = "Library/LastBuild.buildreport";
        private const string BuildReportDir = "Assets/BuildReports";
        private const ulong MinPackedSize = 1024;

        private const string HtmlReportName = "Assets/BuildReports/BuildReport.html";

        public static void HtmlBuildReportFast()
        {
            Debug.Log("*");
            BuildReport buildReport = null;
            Timed("Load Last Build Report", () =>
                buildReport = GetOrCreateLastBuildReport());
            if (buildReport == null)
            {
                Debug.Log($"{LastBuildReportPath} NOT FOUND");
                return;
            }
            AnalyzeLastBuildReport(buildReport, false, false);
        }

        public static void HtmlBuildReportFull()
        {
            Debug.Log("*");
            BuildReport buildReport = null;
            Timed("Load Last Build Report", () =>
                buildReport = GetOrCreateLastBuildReport());
            if (buildReport == null)
            {
                Debug.Log($"{LastBuildReportPath} NOT FOUND");
                return;
            }
            AnalyzeLastBuildReport(buildReport, true, false);
        }

        private static void AnalyzeLastBuildReport(BuildReport buildReport, bool includeUnused, bool logDetails)
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
            AnalyzeLastScenesUsingAssets(buildReport, logDetails);

            string[] databaseAssets = null;
            Timed("Get all assets from AssetDatabase", () => databaseAssets = GetAllAssetDatabaseAssets());

            var allBuildAssets = new List<BuildAssetInfo>();
            Timed("Load used assets from Build Report",
                () => allBuildAssets = LoadAssetsFromBuildReport(buildReport.packedAssets));

            List<BuildAssetInfo> largeAssets = null;
            Timed("Select large assets", () => largeAssets = allBuildAssets.Where(x => x.PackedSize >= MinPackedSize).ToList());

            List<BuildAssetInfo> unusedAssets = new List<BuildAssetInfo>();
            if (includeUnused)
            {
                Timed("Get unused assets", () => unusedAssets = GetUnusedAssets(databaseAssets, allBuildAssets));
            }

            Debug.Log($"All Assets count {databaseAssets.Length}");
            if (!logDetails)
            {
                Debug.Log($"Large Assets count {largeAssets.Count}");
                if (includeUnused)
                {
                    Debug.Log($"Unused Assets count {unusedAssets.Count}");
                }
            }
            else
            {
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
                if (includeUnused)
                {
                    Debug.Log("*");
                    Debug.Log($"Unused Assets count {unusedAssets.Count}");
                    unusedAssets = unusedAssets.OrderBy(x => x.MaxSize).Reverse().ToList();
                    foreach (var assetInfo in unusedAssets)
                    {
                        Debug.Log(
                            $"{FormatSize(assetInfo.PackedSize)} <color=magenta><b>u</b></color> {FormatSize(assetInfo.FileSize)} {assetInfo.Type} {assetInfo.AssetPath} {assetInfo.AssetGuid}");
                    }
                }
            }
            Timed("HTML report", () =>
                HtmlReporter.CreateBuildReportHtmlPage(unusedAssets, largeAssets, summary));
        }

        private static void AnalyzeLastScenesUsingAssets(BuildReport buildReport, bool logDetails)
        {
            var scenesUsingAssets = buildReport.scenesUsingAssets;
            if (scenesUsingAssets.Length == 0)
            {
                Debug.Log($"Scenes in build not available and it requires '<b>BuildOptions.DetailedBuildReport</b>' to be set!");
            }
            else
            {
                // Bill Of Materials for scenes: key is scene 'name' and content is list of assets used in this scene.
                var bom = new Dictionary<string, HashSet<string>>();
                Timed("Get Scenes Using Assets", () => PopulateDataLocal(ref bom));

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

            void PopulateDataLocal(ref Dictionary<string, HashSet<string>> bom)
            {
                // Plural - ScenesUsingAssets
                foreach (var assets in buildReport.scenesUsingAssets)
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
        }

        private static List<BuildAssetInfo> LoadAssetsFromBuildReport(PackedAssets[] allPackedAssets)
        {
            List<BuildAssetInfo> allBuildAssets = new List<BuildAssetInfo>();
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
                }
            }
            return allBuildAssets;
        }

        private static string[] GetAllAssetDatabaseAssets()
        {
            return AssetDatabase.FindAssets(string.Empty);
        }

        private static List<BuildAssetInfo> GetUnusedAssets(string[] allAssetGuids, List<BuildAssetInfo> usedAssets)
        {
            var unusedAssets = new List<BuildAssetInfo>();
            foreach (var assetGuid in allAssetGuids)
            {
                var isAssetInUse = usedAssets.Any(x => x.AssetGuid == assetGuid);
                if (isAssetInUse)
                {
                    continue;
                }
                // This can be quite slow because it needs to go to the file system (or cache?).
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
            // Note that
            // - scenes are not included in Build Report as other assets
            // - inputactions can not be detected for now and we ignore them silently
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
                   path.EndsWith(".inputactions") ||
                   path.EndsWith(".unity");
        }

        /// <summary>
        /// Gets last UNITY Build Report from file.
        /// </summary>
        /// <remarks>
        /// This code is based on UNITY Build Report Inspector<br />
        /// https://docs.unity3d.com/Packages/com.unity.build-report-inspector@0.1/manual/index.html<br />
        /// https://github.com/Unity-Technologies/BuildReportInspector/blob/master/com.unity.build-report-inspector/Editor/BuildReportInspector.cs
        /// </remarks>
        /// <returns>the last <c>BuildReport</c> instance or <c>null</c> if one is not found</returns>
        private static BuildReport GetOrCreateLastBuildReport()
        {
            if (!File.Exists(LastBuildReportPath))
            {
                Debug.Log($"Last Build Report NOT FOUND: {LastBuildReportPath}");
                return null;
            }
            if (!Directory.Exists(BuildReportDir))
            {
                Directory.CreateDirectory(BuildReportDir);
            }

            var date = File.GetLastWriteTime(LastBuildReportPath);
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

        #region Utilities

        private static void Timed(string message, Action action)
        {
            var stopwatch = Stopwatch.StartNew();
            action();
            stopwatch.Stop();
            Debug.Log($"{message} took {stopwatch.Elapsed.TotalSeconds:0.0} s");
        }

        private static string FormatSizeOrEmpty(ulong bytes, string empty = "&nbsp;")
        {
            if (bytes == 0)
            {
                return empty;
            }
            return FormatSize(bytes);
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

        #endregion

        #region Classes for asset files

        /// <summary>
        /// Extended <c>BuildAssetInfo</c> for statistics and detailed reporting.
        /// </summary>
        private class AssetInfoDetails : BuildAssetInfo
        {
            public readonly string GroupByTypeKey;
            public readonly string TextureType;
            public readonly string TextureSize;
            public readonly bool IsTexture;
            public readonly bool IsRecommendedFormat;
            public readonly bool IsNPOT;

            public AssetInfoDetails(BuildAssetInfo assetInfo) : base(assetInfo)
            {
                bool IsPowerOfTwo(int x)
                {
                    // https://stackoverflow.com/questions/600293/how-to-check-if-a-number-is-a-power-of-2
                    return (x != 0) && ((x & (x - 1)) == 0);
                }

                GroupByTypeKey = Type;
                TextureType = Type;

                var asset = Type == "Texture2D" ? AssetDatabase.LoadAssetAtPath<Texture2D>(assetInfo.AssetPath) : null;
                if (asset == null)
                {
                    return;
                }
                // Recommended, default, and supported texture formats, by platform
                // https://docs.unity3d.com/Manual/class-TextureImporterOverride.html
                // ETC1, DXT1 - RGB texture
                // ETC2, DXT5 - RGBA texture
                var assetFormat = asset.format.ToString();
                var width = asset.width;
                var height = asset.height;

                // Use asset extension and texture type for grouping.
                GroupByTypeKey = $"{GetExtension(AssetPath)} {assetFormat}";
                TextureType = assetFormat;
                TextureSize = $"{width}x{height}";
                IsTexture = true;
                IsRecommendedFormat = assetFormat.Contains("ETC1") || assetFormat.Contains("ETC2") ||
                                      assetFormat.Contains("DXT1") || assetFormat.Contains("DXT5");
                IsNPOT = !IsPowerOfTwo(width) || !IsPowerOfTwo(height);
            }
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
            public readonly bool IsUnused;

            protected BuildAssetInfo(BuildAssetInfo other)
            {
                AssetPath = other.AssetPath;
                AssetGuid = other.AssetGuid;
                PackedSize = other.PackedSize;
                FileSize = other.FileSize;
                MaxSize = other.MaxSize;
                Type = other.Type;
                IsTest = other.IsTest;
                IsUnused = other.IsUnused;
            }

            public BuildAssetInfo(PackedAssetInfo assetInfo)
            {
                // Build Report can be old and related assets deleted.
                AssetPath = assetInfo.sourceAssetPath;
                var fileExists = File.Exists(AssetPath);
                AssetGuid = assetInfo.sourceAssetGUID.ToString();
                PackedSize = assetInfo.packedSize;
                FileSize = fileExists ? (ulong)new FileInfo(AssetPath).Length : 0;
                MaxSize = Math.Max(PackedSize, FileSize);
                Type = fileExists ? assetInfo.type.Name : "deleted";
                IsTest = AssetPath.Contains("Test");
                IsUnused = false;
            }

            public BuildAssetInfo(string assetPath, string assetGuid)
            {
                // This should be for existing and un-used assets.
                AssetPath = assetPath;
                AssetGuid = assetGuid;
                PackedSize = 0;
                FileSize = (ulong)new FileInfo(AssetPath).Length;
                MaxSize = FileSize;
                Type = GetExtension(AssetPath);
                IsTest = AssetPath.Contains("Test");
                IsUnused = true;
            }

            protected static string GetExtension(string assetPath)
            {
                return Path.GetExtension(assetPath).Replace(".", string.Empty).ToLower();
            }
        }

        #endregion

        /// <summary>
        /// Creates an HTML page with details and statistics from given asset files.
        /// </summary>
        private static class HtmlReporter
        {
            public static void CreateBuildReportHtmlPage(List<BuildAssetInfo> unusedAssets, List<BuildAssetInfo> largeAssets, BuildSummary summary)
            {
                #region HTML Templates

                // Putting padding between the columns using CSS:
                // https://stackoverflow.com/questions/11800975/html-table-needs-spacing-between-columns-not-rows

                // HTML color names:
                // https://htmlcolorcodes.com/color-names/
                const string htmlStart = @"<!DOCTYPE html>
<html>
<head>
<style>
html * {
  font-family: Arial, Helvetica, sans-serif;
}
body {
  background-color: FloralWhite;
}
tr:nth-child(even) {
  background-color: linen;
}
tr:nth-child(odd) {
  background-color: linen;
}
tr > * + * {
  padding-left: .5em;
}
th {
  text-align: left;
}
td.center {
  text-align: center;
}
td.right {
  text-align: right;
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
  color: MediumPurple;
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
.texture {
  color: DarkRed;
}
.npot {
  color: OrangeRed;
}
.for-test {
  color: CadetBlue;
}
</style>
<title>@Build_Report@</title>
</head>
<body>";
                const string htmlEnd = @"</body>
</html>";
                const string tableStart = @"<table>";
                const string tableEnd = @"</table>";

                const string pathExcludedWarning = "Scenes, C# source code and input actions are excluded form the report";

                #endregion

                var tempAssets = new List<BuildAssetInfo>(unusedAssets);
                tempAssets.AddRange(largeAssets);
                tempAssets = tempAssets.OrderBy(x => x.MaxSize).Reverse().ToList();

                // Convert assets to have more details for report statistics.
                var finalAssets = tempAssets.ConvertAll(x => new AssetInfoDetails(x));

                // Statistics by FileType.
                var prodFileTypes = new Dictionary<string, int>();
                var testFileTypes = new Dictionary<string, int>();
                var unusedFileTypes = new Dictionary<string, int>();
                var prodFileSizes = new Dictionary<string, ulong>();
                var testFileSizes = new Dictionary<string, ulong>();
                var unusedFileSizes = new Dictionary<string, ulong>();

                // Actual Build Report.
                var buildName = BuildPipeline.GetBuildTargetName(summary.platform);
                var fixedHtmlStart = htmlStart.Replace("@Build_Report@", $"{Application.productName} {buildName} Build Report");
                var builder = new StringBuilder()
                    .Append(fixedHtmlStart).AppendLine()
                    .Append(tableStart).AppendLine()
                    .Append("<tr>")
                    .Append($"<th>PackedSize</th>")
                    .Append($"<th>Check</th>")
                    .Append($"<th>FileSize</th>")
                    .Append($"<th>Type</th>")
                    .Append($"<th>Name</th>")
                    .Append($"<th>Path</th>")
                    .Append("</tr>").AppendLine();

                foreach (var a in finalAssets)
                {
                    UpdateFileTypeStatistics(a);

                    var marker = a.IsUnused ? @"<span class=""unused"">unused</span>"
                        : a.PackedSize < a.FileSize ? @"<span class=""less"">less</span>"
                        : a.PackedSize > a.FileSize ? @"<span class=""more"">more</span>"
                        : @"<span class=""same"">same</span>";
                    var name = Path.GetFileName(a.AssetPath);
                    if (a.IsTest)
                    {
                        name = $"<span class=\"for-test\">{name}</span>";
                    }
                    var folder = Path.GetDirectoryName(a.AssetPath);
                    var filetype = a.Type;
                    if (a.IsTexture)
                    {
                        // Special formatting for Texture details
                        filetype = a.IsRecommendedFormat
                            ? $"<b>{a.TextureType}</b>"
                            : a.TextureType;
                        filetype = $"<span class=\"texture\">{filetype} {a.TextureSize}</span>";
                        if (a.IsNPOT)
                        {
                            filetype = $"{filetype} <span class=\"npot\">NPOT</span>";
                        }
                    }
                    builder
                        .Append("<tr>")
                        .Append($"<td{GetStyleFromFileSize(a.PackedSize)}>{FormatSize(a.PackedSize)}</td>")
                        .Append($"<td>{marker}</td>")
                        .Append($"<td{GetStyleFromFileSize(a.FileSize)}>{FormatSize(a.FileSize)}</td>")
                        .Append($"<td>{filetype}</td>")
                        .Append($"<td>{name}</td>")
                        .Append($"<td>{folder}</td>")
                        .Append("</tr>").AppendLine();
                }
                builder
                    .Append(tableEnd).AppendLine()
                    .Append($"<p>Table row count is {tempAssets.Count}</p>").AppendLine()
                    .Append($"<p>Build for {buildName} platform" +
                            $" on {summary.buildEndedAt:yyyy-dd-MM HH:mm:ss}" +
                            $" output size is {FormatSize(summary.totalSize)}</p>").AppendLine();

                // FileType statistics
                var keys = new HashSet<string>();
                keys.UnionWith(prodFileTypes.Keys);
                keys.UnionWith(testFileTypes.Keys);
                keys.UnionWith(unusedFileTypes.Keys);
                var sortedKeys = keys.OrderBy(x => x).ToList();
                builder
                    .Append(tableStart).AppendLine()
                    .Append("<tr>")
                    .Append("<th>File</th>")
                    .Append(@"<th colspan=""2"">Prod</th>")
                    .Append(@"<th colspan=""2"">Test</th>")
                    .Append(@"<th colspan=""2"">Unused</th>")
                    .Append("</tr>").AppendLine()
                    .Append("<tr>")
                    .Append("<th>Type</th>")
                    .Append("<th>Count</th>")
                    .Append("<th>PackedSize</th>")
                    .Append("<th>Count</th>")
                    .Append("<th>PackedSize</th>")
                    .Append("<th>Count</th>")
                    .Append("<th>FileSize</th>")
                    .Append("</tr>").AppendLine();
                var totProdCount = 0;
                var totTestCount = 0;
                var totUnusedCount = 0;
                var totProdSize = 0UL;
                var totTestSize = 0UL;
                var totUnusedSize = 0UL;
                foreach (var key in sortedKeys)
                {
                    if (prodFileTypes.TryGetValue(key, out var prodCount))
                    {
                        totProdCount += prodCount;
                    }
                    if (testFileTypes.TryGetValue(key, out var testCount))
                    {
                        totTestCount += testCount;
                    }
                    if (unusedFileTypes.TryGetValue(key, out var unusedCount))
                    {
                        totUnusedCount += unusedCount;
                    }
                    if (prodFileSizes.TryGetValue(key, out var prodSize))
                    {
                        totProdSize += prodSize;
                    }
                    if (testFileSizes.TryGetValue(key, out var testSize))
                    {
                        totTestSize += testSize;
                    }
                    if (unusedFileSizes.TryGetValue(key, out var unusedSize))
                    {
                        totUnusedSize += unusedSize;
                    }
                    builder
                        .Append("<tr>")
                        .Append($"<td>{key}</td>")
                        .Append(@$"<td class=""right"">{prodCount}</td>")
                        .Append(@$"<td{GetStyleFromFileSize(prodSize, "right")}>{FormatSize(prodSize)}</td>")
                        .Append(@$"<td class=""right"">{(testCount > 0 ? testCount.ToString() : "&nbsp;")}</td>")
                        .Append(@$"<td{GetStyleFromFileSize(testSize, "right")}>{FormatSizeOrEmpty(testSize)}</td>")
                        .Append(@$"<td class=""right"">{(unusedCount > 0 ? unusedCount.ToString() : "&nbsp;")}</td>")
                        .Append(@$"<td{GetStyleFromFileSize(unusedSize, "right")}>{FormatSizeOrEmpty(unusedSize)}</td>")
                        .Append("</tr>").AppendLine();
                }
                builder
                    .Append("<tr>")
                    .Append(@"<td><b>Total</b></td>")
                    .Append(@$"<td class=""right"">{totProdCount}</td>")
                    .Append(@$"<td{GetStyleFromFileSize(totProdSize, "right")}>{FormatSize(totProdSize)}</td>")
                    .Append(@$"<td class=""right"">{(totTestCount > 0 ? totTestCount.ToString() : "&nbsp;")}</td>")
                    .Append(@$"<td{GetStyleFromFileSize(totTestSize, "right")}>{FormatSizeOrEmpty(totTestSize)}</td>")
                    .Append(@$"<td class=""right"">{(totUnusedCount > 0 ? totUnusedCount.ToString() : "&nbsp;")}</td>")
                    .Append(@$"<td{GetStyleFromFileSize(totUnusedSize, "right")}>{FormatSizeOrEmpty(totUnusedSize)}</td>")
                    .Append("</tr>").AppendLine()
                    .Append(tableEnd).AppendLine();

                builder
                    .Append($"<p class=\"smaller\">Page created on {DateTime.Now:yyyy-dd-MM HH:mm:ss}. <i>{pathExcludedWarning}</i></p>").AppendLine()
                    .Append(htmlEnd);

                var content = builder.ToString();
                File.WriteAllText(HtmlReportName, content);
                var htmlPath = Path.GetFullPath(HtmlReportName);
                Debug.Log($"Application.OpenURL {htmlPath}");
                Application.OpenURL(htmlPath);

                string GetStyleFromFileSize(ulong fileSize, string otherClassNames = null)
                {
                    if (!string.IsNullOrEmpty(otherClassNames))
                    {
                        otherClassNames = $" {otherClassNames}";
                    }
                    if (fileSize < 1024)
                    {
                        return @$" class=""bytes{otherClassNames}""";
                    }
                    if (fileSize < 1024 * 1024)
                    {
                        return @$" class=""kilobytes{otherClassNames}""";
                    }
                    return @$" class=""megabytes{otherClassNames}""";
                }

                void UpdateFileTypeStatistics(AssetInfoDetails assetInfo)
                {
                    var fileTypeKey = assetInfo.GroupByTypeKey;
                    var counterDictionary = assetInfo.IsUnused ? unusedFileTypes : assetInfo.IsTest ? testFileTypes : prodFileTypes;
                    if (!counterDictionary.TryAdd(fileTypeKey, 1))
                    {
                        counterDictionary[fileTypeKey] += 1;
                    }
                    var fileSizeDictionary = assetInfo.IsUnused ? unusedFileSizes : assetInfo.IsTest ? testFileSizes : prodFileSizes;
                    var fileSize = assetInfo.IsUnused ? assetInfo.FileSize : assetInfo.PackedSize;
                    if (!fileSizeDictionary.TryAdd(fileTypeKey, fileSize))
                    {
                        fileSizeDictionary[fileTypeKey] += fileSize;
                    }
                }
            }
        }
    }
}
