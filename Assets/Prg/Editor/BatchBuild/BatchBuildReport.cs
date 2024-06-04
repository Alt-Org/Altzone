using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Prg.Scripts.Common.Util;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Prg.Editor.BatchBuild
{
    /// <summary>
    /// Creates a list of files in the UNITY <c>BuildReport</c> to JSON formattable classes.
    /// </summary>
    public static class BatchBuildReport
    {
        private static readonly Encoding Encoding = PlatformUtil.Encoding;

        private const string JsPrefix = "const buildReport = ";
        public static int TestCountLimiter;

        public static BuildReportAssets SaveBuildReport(BuildReport buildReport, string tsvOutputFilename,
            string jsOutputFilename)
        {
            var buildReportAssets = CreateBuildReportAssets(buildReport);
            SaveAsTsv(buildReportAssets, tsvOutputFilename);
            SaveAsJavaScript(buildReportAssets, jsOutputFilename);
            return buildReportAssets;
        }

        public static BuildReportAssets LoadFromFile(string filename)
        {
            // Remove variable declaration and semicolon.
            var buffer = new StringBuilder(File.ReadAllText(filename, Encoding));
            buffer.Remove(0, JsPrefix.Length);
            buffer.Length -= 1;
            return JsonUtility.FromJson<BuildReportAssets>(buffer.ToString());
        }

        private static void SaveAsTsv(BuildReportAssets buildReportAssets, string outputFilename)
        {
            var assetLines = buildReportAssets.Lines;

            // Build tsv file.
            var builder = new StringBuilder()
                .Append(
                    $"Name\tSize\t%\t\tFiles\t{assetLines.Count}\tReported Size\t{buildReportAssets.TotalFileSize}")
                .AppendLine();
            foreach (var assetInfo in assetLines)
            {
                builder.Append(assetInfo.AssetPath).Append('\t')
                    .Append(assetInfo.PackedSize).Append('\t')
                    .Append(assetInfo.AssetType).Append('\t')
                    .AppendLine();
            }
            // Remove last CR-LF.
            builder.Length -= 2;
            File.WriteAllText(outputFilename, builder.ToString(), Encoding);
        }

        private static void SaveAsJavaScript(BuildReportAssets buildReportAssets, string outputFilename)
        {
            var jsText = $"{JsPrefix}{JsonUtility.ToJson(buildReportAssets, TestCountLimiter > 0)};";
            File.WriteAllText(outputFilename, jsText, Encoding);
        }

        private static BuildReportAssets CreateBuildReportAssets(BuildReport buildReport)
        {
            List<PackedAssetInfo> packedAssets = buildReport.GetPackedAssets();
            if (TestCountLimiter > 0)
            {
                packedAssets = packedAssets.Take(TestCountLimiter).ToList();
            }
            var reportedSize = packedAssets.Sum(x => (long)x.packedSize);
            return new BuildReportAssets(reportedSize, buildReport.summary, packedAssets);
        }
    }

    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
    public class BuildReportAssets
    {
        public string BuildName;
        public string BuildVersion;
        public string BuildDate;
        public string BuildTime;
        public string BuildTarget;
        public long TotalFileSize;
        public List<MyPackedAssetInfo> Lines = new();

        public BuildReportAssets()
        {
        }

        public BuildReportAssets(long totalFileSize, BuildSummary summary, List<PackedAssetInfo> buildReportAssetLines)
        {
            BuildName = Application.productName;
            BuildVersion = Application.version;
            var buildEndedAt = summary.buildEndedAt;
            BuildDate = buildEndedAt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            BuildTime = buildEndedAt.ToString("HH.mm", CultureInfo.InvariantCulture);
            BuildTarget = BuildPipeline.GetBuildTargetName(summary.platform);
            TotalFileSize = totalFileSize;
            Lines = buildReportAssetLines.Select(x => new MyPackedAssetInfo(x))
                .OrderBy(x => x.SortKey)
                .ToList();
        }
    }

    [Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
    public class MyPackedAssetInfo
    {
        public string AssetPath;
        public long PackedSize;
        public string AssetType;

        public readonly string SortKey;

        public MyPackedAssetInfo()
        {
        }

        public MyPackedAssetInfo(PackedAssetInfo packedAssetInfo)
        {
            AssetPath = packedAssetInfo.sourceAssetPath;
            PackedSize = (long)packedAssetInfo.packedSize;
            AssetType = packedAssetInfo.type.Name;
            // Descend by size, ascend by path.
            SortKey = $"{999999999999 - PackedSize:000000000000}.{AssetPath}";
        }
    }
}
