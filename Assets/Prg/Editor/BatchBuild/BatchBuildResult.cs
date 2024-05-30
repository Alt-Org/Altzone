using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using Prg.Scripts.Common.Util;
using UnityEngine;

namespace Prg.Editor.BatchBuild
{
    /// <summary>
    /// UNITY <c>BuildReport</c> combined with other build output data combined to JSON formattable classes
    /// for analyzing files used or not used in the build.
    /// </summary>
    public static class BatchBuildResult
    {
        private static readonly Encoding Encoding = PlatformUtil.Encoding;

        private const string JsPrefix = "const buildResult = ";
        public static int TestCountLimiter;

        public static void SaveBuildResult(BuildReportAssets buildReportAssets, BuildReportLog buildReportLog,
            ProjectFiles projectFiles, string jsOutputFilename)
        {
            var buildReportData = new BuildReportData(buildReportAssets, buildReportLog, projectFiles);

            var unusedFilenames = new HashSet<string>(projectFiles.Lines.Select(x => x.FilePath));
            foreach (var logLine in buildReportLog.Lines)
            {
                var foundAssets = buildReportAssets.Lines.Where(x => x.AssetPath == logLine.FilePath).ToList();
                buildReportData.Lines.Add(new BuildReportLine(logLine, foundAssets));
                if (TestCountLimiter > 0 && buildReportData.Lines.Count >= TestCountLimiter)
                {
                    break;
                }
                unusedFilenames.Remove(logLine.FilePath);
            }
            var unusedFiles = projectFiles.Lines.Where(x => unusedFilenames.Contains(x.FilePath));
            foreach (var unusedFile in unusedFiles)
            {
                buildReportData.Lines.Add(new BuildReportLine(unusedFile));
                if (TestCountLimiter > 0 && buildReportData.Lines.Count >= 2 * TestCountLimiter)
                {
                    break;
                }
            }
            SaveAsJavaScript(buildReportData, jsOutputFilename);
        }

        private static void SaveAsJavaScript(BuildReportData buildReportData, string outputFilename)
        {
            var jsText = $"{JsPrefix}{JsonUtility.ToJson(buildReportData, TestCountLimiter > 0)};";
            File.WriteAllText(outputFilename, jsText, Encoding);
        }
    }

    /// <summary>
    /// <c>BuildReportData</c> for JSON formatter.
    /// </summary>
    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
    public class BuildReportData
    {
        // From BuildReport
        public string BuildName;
        public string BuildVersion;
        public string BuildDate;
        public string BuildTime;
        public string BuildTarget;
        public long BuildFileSize;

        // From parsed log file.
        public long LogFileSize;

        // From file system.
        public long TotalFileSize;
        public long UnusedFileSize;

        // Report lines contain data from all applicable sources.
        public List<BuildReportLine> Lines = new();

        public BuildReportData()
        {
        }

        public BuildReportData(BuildReportAssets buildReportAssets,
            BuildReportLog buildReportLog,
            ProjectFiles projectFiles)
        {
            BuildName = buildReportAssets.BuildName;
            BuildVersion = buildReportAssets.BuildVersion;
            BuildDate = buildReportAssets.BuildDate;
            BuildTime = buildReportAssets.BuildTime;
            BuildTarget = buildReportAssets.BuildTarget;
            BuildFileSize = buildReportAssets.TotalFileSize;
            LogFileSize = (long)(buildReportLog.TotalFileSizeKb * 1024.0);
            TotalFileSize = projectFiles.TotalFileSize;
            UnusedFileSize = projectFiles.UnUsedFileSize;
        }
    }

    public enum AssetCategory
    {
        Unknown = 0,
        Project = 1,
        Plugin = 2,
        Package = 3,
        Other = 4
    }

    /// <summary>
    /// <c>BuildReportLine</c> for JSON formatter.
    /// </summary>
    [Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
    public class BuildReportLine
    {
        public long PackedSize;
        public long FileSize;
        public double LogPercentage;
        public string AssetName;
        public string AssetExtension;
        public string AssetFolder;
        public AssetCategory AssetCategory;
        public FileCategory FileCategory;
        public AssetType AssetType = new();

        public BuildReportLine()
        {
        }

        public BuildReportLine(BuildReportLogLine logLine, List<MyPackedAssetInfo> packedAssets)
        {
            PackedSize = packedAssets.Sum(x => x.PackedSize);
            FileSize = File.Exists(logLine.FilePath)
                ? new FileInfo(logLine.FilePath).Length
                : (long)(logLine.FileSizeKb * 1024.0);
            if (PackedSize == 0 && FileSize > 0)
            {
                // Fix some 'built in' stuff size.
                PackedSize = FileSize;
                FileSize = 0;
            }
            LogPercentage = logLine.Percentage;
            AssetName = Path.GetFileName(logLine.FilePath);
            AssetExtension = GetExtension(logLine.FilePath);
            AssetFolder = Path.GetDirectoryName(logLine.FilePath) ?? string.Empty;
            if (AssetName.Length == 0 && AssetFolder.Length > 0)
            {
                // Fix some 'built in' stuff name.
                AssetName = AssetFolder;
                AssetFolder = string.Empty;
            }
            AssetCategory = GetAssetCategory(AssetFolder);
            FileCategory = FileCategory.UsedInLog;
            if (packedAssets.Count > 0)
            {
                FileCategory |= FileCategory.UsedInReport;
            }
            AssetType.Update(this, packedAssets);
        }

        public BuildReportLine(ProjectFile unusedFile)
        {
            PackedSize = 0;
            FileSize = unusedFile.FileSize;
            LogPercentage = 0;
            AssetName = Path.GetFileName(unusedFile.FilePath);
            AssetExtension = GetExtension(unusedFile.FilePath);
            AssetFolder = Path.GetDirectoryName(unusedFile.FilePath) ?? string.Empty;
            AssetCategory = GetAssetCategory(AssetFolder);
            FileCategory = FileCategory.UnUsed;
            AssetType.MainType = "unused";
            AssetType.SubType = AssetExtension;
        }

        private static AssetCategory GetAssetCategory(string folder)
        {
            if (folder.StartsWith("Packages"))
            {
                return AssetCategory.Package;
            }
            if (folder.StartsWith("Assets/Plugins"))
            {
                return AssetCategory.Plugin;
            }
            if (folder.StartsWith("Assets"))
            {
                return AssetCategory.Project;
            }
            return AssetCategory.Other;
        }

        private static string GetExtension(string file)
        {
            return (Path.GetExtension(file) ?? string.Empty).Replace(".", string.Empty);
        }
    }

    /// <summary>
    /// <c>AssetType</c> for JSON formatter.
    /// </summary>
    [Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
    public class AssetType
    {
        private const string EmptyExtensionMarker = "(null)";
        private const string ControllerExtension = "controller";

        private static string[] multiComponent =
            { "controller", "inputactions", "overrideController", "prefab", "ttf" };

        public string MainType;
        public string SubType;
        public string SizeInfo;

        public AssetType()
        {
        }

        public void Update(BuildReportLine line, List<MyPackedAssetInfo> packedAssets)
        {
            MainType = string.IsNullOrWhiteSpace(line.AssetExtension) ? EmptyExtensionMarker : line.AssetExtension;
            if (MainType == "physicsMaterial2D")
            {
                // Categorize physics 2D material as normal material.
                MainType = "mat";
            }
            if (multiComponent.Contains(line.AssetExtension))
            {
                if (MainType == "overrideController")
                {
                    // Try to group all animator controllers together.
                    MainType = ControllerExtension;
                }
                SubType = GetSubType(packedAssets);
                return;
            }
            if (line.AssetExtension == "asset")
            {
                if (line.AssetFolder.Contains("\\Fonts"))
                {
                    SubType = GetSubType(packedAssets);
                    return;
                }
                if (packedAssets.Count == 1)
                {
                    var type = packedAssets[0].AssetType;
                    if (type == "MonoBehaviour")
                    {
                        SubType = GetSubType(packedAssets);
                        return;
                    }
                }
            }
            if (line.AssetName == "unity_builtin_extra")
            {
                SubType = GetSubType(packedAssets);
                return;
            }
            var builder = new StringBuilder();
            var result = packedAssets
                .GroupBy(x => x.AssetType)
                .OrderBy(x => x.Key);
            foreach (var group in result)
            {
                builder.Append(group.Key);
                var count = group.Count();
                if (count > 1)
                {
                    builder.Append(' ').Append('[').Append(count).Append(']');
                }
                builder.Append(' ');
            }
            if (builder.Length > 1)
            {
                builder.Length -= 1;
            }
            SubType = builder.ToString();
            if (SubType.Contains("Texture2D") || SubType.Contains("Sprite") || SubType.Contains("Cubemap"))
            {
                MainType = $"img {MainType}";
            }
            else if (SubType.Contains("AudioClip"))
            {
                MainType = $"snd {MainType}";
            }
        }

        private static string GetSubType(List<MyPackedAssetInfo> packedAssets)
        {
            return packedAssets.Count == 1
                ? packedAssets[0].AssetType
                : $"[{packedAssets.Count}]";
        }
    }
}
