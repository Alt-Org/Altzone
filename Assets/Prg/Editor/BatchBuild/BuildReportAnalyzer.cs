using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.Tilemaps;
using Object = UnityEngine.Object;

namespace Prg.Editor.BatchBuild
{
    /// <summary>
    /// Read and analyze UNITY Build Report (in binary format) by converting it to internal YAML format and saving as an asset.
    /// </summary>
    /// <remarks>
    /// Idea and tech is from UNITY Build Report Inspector.
    /// </remarks>
    internal static class BuildReportAnalyzer
    {
        private const ulong MinPackedSize = 1024;

        private const string HtmlReportName = "Assets/BuildReports/BuildReport.html";

        public static void HtmlBuildReportFast(bool useJavaScriptSort = true)
        {
            Debug.Log("*");
            HtmlBuildReport(false, false, useJavaScriptSort);
        }

        public static void HtmlBuildReportFull(bool useJavaScriptSort = true)
        {
            Debug.Log("*");
            HtmlBuildReport(true, false, useJavaScriptSort);
        }

        private static void HtmlBuildReport(bool includeUnused, bool logDetails, bool useJavaScriptSort)
        {
            Debug.Log("*");
            BuildReport buildReport = null;
            Timed("Load Last Build Report", () =>
                buildReport = UnityBuildReport.GetOrCreateLastBuildReport());
            if (buildReport == null)
            {
                return;
            }
            AnalyzeLastBuildReport(buildReport, includeUnused, logDetails, useJavaScriptSort);
        }

        private static void AnalyzeLastBuildReport(BuildReport buildReport, bool includeUnused, bool logDetails,
            bool useJavaScriptSort = false)
        {
            var summary = buildReport.summary;
            var buildTargetName = BuildPipeline.GetBuildTargetName(summary.platform);
            var buildStartedAt = $"{summary.buildStartedAt:yyyy-MM-dd HH:mm:ss}";
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
            Timed("Select large assets",
                () => largeAssets = allBuildAssets.Where(x => x.PackedSize >= MinPackedSize).ToList());

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
                        $"{FormatSize(packedSize)} {marker} {FormatSize(fileSize)} {assetInfo.AssetType} {assetInfo.AssetPath} {assetInfo.AssetGuid}");
                }
                if (includeUnused)
                {
                    Debug.Log("*");
                    Debug.Log($"Unused Assets count {unusedAssets.Count}");
                    unusedAssets = unusedAssets.OrderBy(x => x.MaxSize).Reverse().ToList();
                    foreach (var assetInfo in unusedAssets)
                    {
                        Debug.Log(
                            $"{FormatSize(assetInfo.PackedSize)} <color=magenta><b>u</b></color> {FormatSize(assetInfo.FileSize)} {assetInfo.AssetType} {assetInfo.AssetPath} {assetInfo.AssetGuid}");
                    }
                }
            }
            Timed("HTML report", () =>
                HtmlReporter.CreateBuildReportHtmlPage(unusedAssets, largeAssets, summary, useJavaScriptSort));
        }

        private static void AnalyzeLastScenesUsingAssets(BuildReport buildReport, bool logDetails)
        {
            var scenesUsingAssets = buildReport.scenesUsingAssets;
            if (scenesUsingAssets.Length == 0)
            {
                Debug.Log(
                    $"Scenes in build not available and it requires '<b>BuildOptions.DetailedBuildReport</b>' to be set!");
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
                    if (IsAssetExcluded(sourceAssetPath))
                    {
                        continue;
                    }
                    if (assetInfo.type == typeof(MonoBehaviour))
                    {
                        continue;
                    }
                    var sourceAssetGuid = assetInfo.sourceAssetGUID.ToString();
                    if (sourceAssetGuid == "00000000000000000000000000000000" ||
                        sourceAssetGuid == "0000000000000000f000000000000000")
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
                if (IsAssetExcluded(assetPath))
                {
                    continue;
                }
                unusedAssets.Add(new BuildAssetInfo(assetPath, assetGuid));
            }
            return unusedAssets;
        }

        private static bool IsAssetExcluded(string path)
        {
            // Note that
            // - TextMesh Pro *is* included here because it can contain fonts
            // - scenes are not included in Build Report as other assets
            // - animation controllers are ignored silently
            // - inputactions can not be detected for now and we ignore them silently
            return path.StartsWith("Packages/") ||
                   path.StartsWith("Assets/BuildReport") ||
                   path.EndsWith(".asmdef") ||
                   path.EndsWith(".asmref") ||
                   path.EndsWith(".controller") ||
                   path.EndsWith(".cs") ||
                   path.EndsWith(".inputactions") ||
                   path.EndsWith(".preset") ||
                   path.EndsWith(".unity");
        }

        private static bool IsUnusedAssetExcluded(string path)
        {
            return path.StartsWith("Assets/Astar/") ||
                   path.StartsWith("Assets/Photon/") ||
                   path.StartsWith("Assets/Plugins/") ||
                   path.StartsWith("Assets/Tests/") ||
                   path.Contains("/Editor/") ||
                   path.Contains("/Test/");
        }

        #region Utilities

        private static void Timed(string message, Action action, double minTimeToLog = 0.1)
        {
            var stopwatch = Stopwatch.StartNew();
            action();
            stopwatch.Stop();
            var totalSeconds = stopwatch.Elapsed.TotalSeconds;
            if (totalSeconds < minTimeToLog)
            {
                return;
            }
            if (totalSeconds < 0.1)
            {
                Debug.Log($"{message} took {totalSeconds:0.000} s");
                return;
            }
            Debug.Log($"{message} took {totalSeconds:0.0} s");
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
            // Lower case extension is from file system and CamelCase is asset type from build system.
            private static readonly string[] KnownAssetExtensions =
            {
                "asset",
                "AudioClip",
                "dll",
                "Font",
                "guiskin",
                "jpg",
                "Material",
                "mdb",
                "mp3",
                "png",
                "psb", // Adobe Photoshop Large Document Format
                "psd",
                "Shader",
                "shader",
                "Sprite",
                "Texture2D",
                "ttf",
                "wav",
                ""
            };

            private static readonly string[] IgnoredAssetExtensions =
            {
                "anim",
                "AnimationClip",
                "csv",
                "fbx",
                "cginc", // Shader
                "html",
                "json",
                "mat",
                "md",
                "mixer",
                "ParticleSystem",
                "pdf",
                "prefab",
                "Tilemap",
                "TextAsset",
                "tsv",
                "txt",
                "xml",
                ""
            };

            public readonly string GroupByTypeKey;
            public readonly string AssetTypeTag;
            public readonly string AssetSizeTag;
            public readonly bool IsTexture;
            public readonly bool IsFont;
            public readonly bool IsAudioClip;
            public readonly bool IsRecommendedFormat;
            public readonly bool IsNPOT;

            public AssetInfoDetails(BuildAssetInfo assetInfo) : base(assetInfo)
            {
                GroupByTypeKey = AssetType;
                AssetTypeTag = AssetType;

                if (FileSize == 0)
                {
                    return;
                }
                if (!KnownAssetExtensions.Contains(AssetType))
                {
                    if (IgnoredAssetExtensions.Contains(AssetType))
                    {
                        return;
                    }
                    Debug.LogWarning($"skip '{AssetType}' in {assetInfo.AssetPath}, it is unknown");
                    return;
                }
                var asset = AssetDatabase.LoadAssetAtPath<Object>(assetInfo.AssetPath);
                if (asset == null)
                {
                    Debug.Log($"deleted '{AssetType}' in {assetInfo.AssetPath}");
                    return;
                }
                if (asset is Texture2D texture2D)
                {
                    // https://docs.unity3d.com/ScriptReference/Texture2D.html
                    // Recommended, default, and supported texture formats, by platform
                    // https://docs.unity3d.com/Manual/class-TextureImporterOverride.html
                    // ETC1, DXT1 - RGB texture
                    // ETC2, DXT5 - RGBA texture
                    var assetFormat = texture2D.format.ToString();
                    var width = texture2D.width;
                    var height = texture2D.height;
                    var extension = GetExtension(AssetPath);

                    // Use asset extension and texture type for grouping.
                    GroupByTypeKey = $"{extension} {assetFormat}";
                    AssetTypeTag = assetFormat;
                    AssetSizeTag = $"{width}x{height}";
                    IsTexture = true;
                    IsRecommendedFormat = assetFormat.Contains("ETC1") || assetFormat.Contains("ETC2") ||
                                          assetFormat.Contains("DXT1") || assetFormat.Contains("DXT5");
                    IsNPOT = !IsPowerOfTwo(width) || !IsPowerOfTwo(height);

                    // Try to guess if asset is font.
                    if (extension != "asset")
                    {
                        return;
                    }
                    var isFont = AssetPath.Contains("Font") || AssetPath.Contains("TextMesh Pro");
                    if (!isFont)
                    {
                        return;
                    }
                    IsFont = true;
                    // Should be font atlas (greyscale Alpha8).
                    GroupByTypeKey = $"font {GroupByTypeKey}";
                    AssetTypeTag = $"Font {AssetTypeTag}";
                    return;
                }
                if (asset is TMP_FontAsset font)
                {
                    IsFont = true;
                    var extension = GetExtension(AssetPath);
                    var assetFormat = font.atlasTexture.format.ToString();
                    var width = font.atlasWidth;
                    var height = font.atlasHeight;
                    GroupByTypeKey = $"font {extension} {assetFormat}";
                    AssetTypeTag = $"Font {assetFormat}";
                    AssetSizeTag = $"{width}x{height}";
                    return;
                }
                if (asset is AudioClip audioClip)
                {
                    // https://docs.unity3d.com/ScriptReference/AudioClip.html
                    var extension = GetExtension(AssetPath);
                    GroupByTypeKey = $"{AssetType} {extension}";
                    AssetTypeTag = extension;
                    var timeSpan = TimeSpan.FromSeconds(audioClip.length);
                    var timeText = timeSpan.Minutes < 1
                        ? $"{timeSpan.Seconds}.{timeSpan.Milliseconds:000}"
                        : $"{timeSpan.Hours * 60 + timeSpan.Minutes}:{timeSpan.Seconds:00}.{timeSpan.Milliseconds:000}";
                    AssetSizeTag = $"{GetFrequency(audioClip.frequency)} {timeText}";
                    IsAudioClip = true;
                    return;
                }
                if (asset is Tile tile)
                {
                    if (tile.sprite != null)
                    {
                        GroupByTypeKey = "tile sprite";
                        AssetTypeTag = "tile sprite";
                    }
                    else
                    {
                        GroupByTypeKey = "tile unk";
                        AssetTypeTag = "tile unk";
                    }
                    return;
                }
                if (asset is Shader)
                {
                    // Nothing to add, shader name could be added on report?
                    return;
                }
                var fullName = asset.GetType().FullName ?? string.Empty;
                var isUnity = fullName.StartsWith("Unity.") || fullName.StartsWith("UnityEditor.");
                if (!isUnity)
                {
                    return;
                }
                // ignore dll UnityEditor.DefaultAsset Assets/Plugins/Demigiant/DOTween/Editor/DOTweenEditor.dll
                // ignore mdb UnityEditor.DefaultAsset Assets/Plugins/Demigiant/DOTween/DOTween.dll.mdb
                var assetPath = assetInfo.AssetPath;
                if (assetPath.StartsWith("Assets/Plugins/") &&
                    (assetPath.EndsWith(".dll") || assetPath.EndsWith(".mdb")))
                {
                    return;
                }
                Debug.Log($"ignore {AssetType} f={fullName} a={assetPath}");
                return;

                bool IsPowerOfTwo(int x)
                {
                    // https://stackoverflow.com/questions/600293/how-to-check-if-a-number-is-a-power-of-2
                    return (x != 0) && ((x & (x - 1)) == 0);
                }

                string GetFrequency(float frequency)
                {
                    if (frequency < 1000.0)
                    {
                        return $"{frequency:0} Hz";
                    }
                    return $"{frequency / 1000:0} KHz";
                }
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
            public readonly string AssetType;
            public readonly bool IsTest;
            public readonly bool IsUnused;

            protected BuildAssetInfo(BuildAssetInfo other)
            {
                AssetPath = other.AssetPath;
                AssetGuid = other.AssetGuid;
                PackedSize = other.PackedSize;
                FileSize = other.FileSize;
                MaxSize = other.MaxSize;
                AssetType = other.AssetType;
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
                AssetType = fileExists ? assetInfo.type.Name : "deleted";
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
                AssetType = GetExtension(AssetPath);
                IsTest = AssetPath.Contains("Test");
                IsUnused = true;
            }

            protected static string GetExtension(string assetPath)
            {
                return Path.GetExtension(assetPath).Replace(".", string.Empty).ToLower(CultureInfo.InvariantCulture);
            }
        }

        #endregion

        #region HTML Report

        /// <summary>
        /// Creates an HTML page with details and statistics from given asset files.
        /// </summary>
        private static class HtmlReporter
        {
            public static void CreateBuildReportHtmlPage(List<BuildAssetInfo> unusedAssets,
                List<BuildAssetInfo> largeAssets, BuildSummary summary,
                bool useJavaScriptSort)
            {
                #region HTML Templates

                // Putting padding between the columns using CSS:
                // https://stackoverflow.com/questions/11800975/html-table-needs-spacing-between-columns-not-rows

                // HTML color names:
                // https://htmlcolorcodes.com/color-names/
                const string htmlStart = @"<!DOCTYPE html>
<html>
<!-- @Build_Comment@-->
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
th.prod {
  color: DodgerBlue;
}
th.test {
  color: Coral;
}
th.unused {
  color: Crimson;
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
.megabytes, .special {
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
<title>@Build_Title@</title>
</head>
<body>";
                const string htmlEnd = @"</body>
</html>";
                const string javaScript = @"
// https://www.w3schools.com/howto/howto_js_sort_table.asp
const sortDir = [false,false,false,false,false,false];
function sortTable(index) {
  table = document.getElementById(""dataTable"");
  sortingUp = !sortDir[index];
  sortDir[index] = sortingUp;
  switching = true;
  /*Make a loop that will continue until
  no switching has been done:*/
  while (switching) {
    //start by saying: no switching is done:
    switching = false;
    rows = table.rows;
    /*Loop through all table rows (except the
    first, which contains table headers):*/
    for (i = 1; i < (rows.length - 1); i++) {
      //start by saying there should be no switching:
      shouldSwitch = false;
      /*Get the two elements you want to compare,
      one from current row and one from the next:*/
      x = rows[i].getElementsByTagName(""TD"")[index];
      y = rows[i + 1].getElementsByTagName(""TD"")[index];
      //check if the two rows should switch place:
      if (sortingUp) {
        if (x.innerText.toLowerCase() > y.innerText.toLowerCase()) {
          //if so, mark as a switch and break the loop:
          shouldSwitch = true;
          break;
        }
      } else {
        if (x.innerText.toLowerCase() < y.innerText.toLowerCase()) {
          //if so, mark as a switch and break the loop:
          shouldSwitch = true;
          break;
        }
      }
    }
    if (shouldSwitch) {
      /*If a switch has been marked, make the switch
      and mark that a switch has been done:*/
      rows[i].parentNode.insertBefore(rows[i + 1], rows[i]);
      switching = true;
    }
  }
}";

                const string excludeFilesWarning =
                    "Note that source code, text file types, scenes and other components can be excluded form this report for various reasons";

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
                var buildInfo = $"{Application.productName} {buildName} Build Report";
                var fixedHtmlStart = htmlStart
                    .Replace("@Build_Comment@", buildInfo)
                    .Replace("@Build_Title@", buildInfo);
                var builder = new StringBuilder()
                    .Append(fixedHtmlStart).AppendLine()
                    .Append(@"<table id=""dataTable"">").AppendLine();
                if (useJavaScriptSort)
                {
                    builder
                        .Append("<tr>")
                        .Append(@"<th><button onclick=""sortTable(0)"">Packed Size</button></th>")
                        .Append(@"<th><button onclick=""sortTable(1)"">Check</button></th>")
                        .Append(@"<th><button onclick=""sortTable(2)"">File Size</button></th>")
                        .Append(@"<th><button onclick=""sortTable(3)"">AssetType Info</button></th>")
                        .Append(@"<th><button onclick=""sortTable(4)"">Asset Name</button></th>")
                        .Append(@"<th><button onclick=""sortTable(5)"">Asset Path</button></th>")
                        .Append("</tr>").AppendLine();
                }
                else
                {
                    builder
                        .Append("<tr>")
                        .Append($"<th>PackedSize</th>")
                        .Append($"<th>Check</th>")
                        .Append($"<th>FileSize</th>")
                        .Append($"<th>AssetType</th>")
                        .Append($"<th>Name</th>")
                        .Append($"<th>Path</th>")
                        .Append("</tr>").AppendLine();
                }
                var ignoredAssets = 0;
                foreach (var a in finalAssets)
                {
                    if (IsUnusedAssetExcluded(a.AssetPath) ||
                        (a.IsUnused && a.AssetTypeTag.ToLower() == "shader"))
                    {
                        ignoredAssets += 1;
                        continue;
                    }
                    UpdateFileTypeStatistics(a);
                    var marker = a.IsUnused ? @"<span class=""unused"">unused</span>"
                        : a.PackedSize < a.FileSize ? @"<span class=""less"">less</span>"
                        : a.PackedSize > a.FileSize ? @"<span class=""more"">more</span>"
                        : @"<span class=""same"">same</span>";
                    var name = Path.GetFileName(a.AssetPath)
                        .Replace("Test", @"<span class=""special""><b>Test</b></span>");
                    var folder = (Path.GetDirectoryName(a.AssetPath) ?? "")
                        .Replace("Resources", @"<span class=""special""><b>Resources</b></span>")
                        .Replace("Test", @"<span class=""special""><b>Test</b></span>");
                    var filetype = a.AssetTypeTag;
                    if (a.IsTexture)
                    {
                        // Special formatting for Texture details
                        filetype = a.IsRecommendedFormat
                            ? $"<b>{a.AssetTypeTag}</b>"
                            : a.AssetTypeTag;
                        filetype = @$"<span class=""texture"">{filetype} {a.AssetSizeTag}</span>";
                        if (a.IsNPOT)
                        {
                            filetype = @$"{filetype} <span class=""npot"">NPOT</span>";
                        }
                    }
                    else if (a.IsAudioClip || a.IsFont)
                    {
                        filetype = $"{filetype} {a.AssetSizeTag}";
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
                var visibleAssets = tempAssets.Count - ignoredAssets;
                builder
                    .Append("</table>").AppendLine();
                builder
                    .Append(
                        @$"<p class=""smaller"">Table row count is {visibleAssets}. Ignored unused assets {ignoredAssets}. Refresh page (F5) for default sort by largest size</p>")
                    .AppendLine()
                    .Append(@$"<p class=""smaller"">Build for {buildName} platform" +
                            $" on {summary.buildEndedAt:yyyy-MM-dd HH:mm:ss}" +
                            $" output size is {FormatSize(summary.totalSize)}</p>").AppendLine();

                // FileType statistics
                var keys = new HashSet<string>();
                keys.UnionWith(prodFileTypes.Keys);
                keys.UnionWith(testFileTypes.Keys);
                keys.UnionWith(unusedFileTypes.Keys);
                var sortedKeys = keys.OrderBy(x => x).ToList();
                builder
                    .Append(@"<table id=""stats"">").AppendLine()
                    .Append("<tr>")
                    .Append(@"<th>File</th>")
                    .Append(@"<th class=""prod"" colspan=""2"">Prod</th>")
                    .Append(@"<th class=""test"" colspan=""2"">Test</th>")
                    .Append(@"<th class=""unused"" colspan=""2"">Unused</th>")
                    .Append("</tr>").AppendLine()
                    .Append("<tr>")
                    .Append("<th>AssetType</th>")
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
                    .Append(
                        @$"<td{GetStyleFromFileSize(totUnusedSize, "right")}>{FormatSizeOrEmpty(totUnusedSize)}</td>")
                    .Append("</tr>").AppendLine()
                    .Append("</table>").AppendLine();

                builder
                    .Append(
                        @$"<p class=""smaller"">Page created on {DateTime.Now:yyyy-MM-dd HH:mm:ss}. <i>{excludeFilesWarning}</i></p>")
                    .AppendLine();
                if (useJavaScriptSort)
                {
                    builder
                        .Append(@$"<script>{javaScript}</script>").AppendLine();
                }
                builder
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
                    var counterDictionary = assetInfo.IsUnused ? unusedFileTypes :
                        assetInfo.IsTest ? testFileTypes : prodFileTypes;
                    if (!counterDictionary.TryAdd(fileTypeKey, 1))
                    {
                        counterDictionary[fileTypeKey] += 1;
                    }
                    var fileSizeDictionary = assetInfo.IsUnused ? unusedFileSizes :
                        assetInfo.IsTest ? testFileSizes : prodFileSizes;
                    var fileSize = assetInfo.IsUnused ? assetInfo.FileSize : assetInfo.PackedSize;
                    if (!fileSizeDictionary.TryAdd(fileTypeKey, fileSize))
                    {
                        fileSizeDictionary[fileTypeKey] += fileSize;
                    }
                }
            }
        }

        #endregion
    }
}
