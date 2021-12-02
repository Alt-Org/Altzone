using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Editor.Prg.Util
{
    public class MenuBuildReport : MonoBehaviour
    {
        private static readonly string[] excludedFolders =
        {
            "Assets/Photon",
            "Assets/Editor",
        };

        [MenuItem("Window/ALT-Zone/Build/Check Build Report")]
        private static void WindowReport()
        {
            var buildTargetName = TeamCity.CommandLine.BuildTargetNameFrom(EditorUserBuildSettings.activeBuildTarget);
            Debug.Log($"Build target is {buildTargetName}");
            var buildReport = $"m_Build_{buildTargetName}.log";
            if (!File.Exists(buildReport))
            {
                Debug.LogWarning($"Build report {buildReport} not found");
                return;
            }

            var allFiles = parseBuildReport(buildReport, out var totalSize);
            Debug.Log($"Build contains {allFiles.Count} asset files, total size is {totalSize:### ### ###.0} kb");
            var usedAssets = new HashSet<string>();
            int[] fileCount = { 0, 0, 0 };
            double[] fileSize = { 0, 0, 0 };
            double[] filePercent = { 0, 0, 0 };
            foreach (var assetLine in allFiles)
            {
                if (assetLine.isAsset)
                {
                    usedAssets.Add(assetLine.filePath);
                    fileCount[0] += 1;
                    fileSize[0] += assetLine.fileSizeKb;
                    filePercent[0] += assetLine.percentage;
                }
                else if (assetLine.isPackage)
                {
                    fileCount[1] += 1;
                    fileSize[1] += assetLine.fileSizeKb;
                    filePercent[1] += assetLine.percentage;
                }
                else if (assetLine.isResource)
                {
                    fileCount[2] += 1;
                    fileSize[2] += assetLine.fileSizeKb;
                    filePercent[2] += assetLine.percentage;
                }
                else
                {
                    Debug.LogError("Unknown asset line: " + assetLine.filePath);
                    return;
                }
            }
            Debug.Log($"Build contains {fileCount[0]} ASSET files, their size is {fileSize[0]:### ### ###.0} kb ({filePercent[0]:0.0}%)");
            Debug.Log($"Build contains {fileCount[1]} PACKAGE files, their size is {fileSize[1]:### ### ###.0} kb ({filePercent[1]:0.0}%)");
            Debug.Log($"Build contains {fileCount[2]} RESOURCE files, their size is {fileSize[2]:### ### ###.0} kb ({filePercent[2]:0.0}%)");
            var unusedAssets = checkUnusedAssets(usedAssets);
            Debug.Log($"Project contains {unusedAssets.Count} unused assets for {buildTargetName} build");
            if (excludedAssetCount > 0)
            {
                Debug.Log($"Excluded {excludedAssetCount} files or folders");
            }
            double unusedAssetSizeTotal = unusedAssets.Select(x => x.fileSizeKb).Sum();
            Debug.Log($"Unused assets total size is {unusedAssetSizeTotal:### ### ###.0} kb");
            foreach (var unusedAsset in unusedAssets.OrderBy(x => x.fileSizeKb).Reverse())
            {
                Debug.Log($"UNUSED {unusedAsset.filePath} {unusedAsset.fileSizeKb} kb");
            }
        }

        private static int excludedAssetCount;

        private static List<AssetLine> checkUnusedAssets(HashSet<string> usedAssets)
        {
            void handleSubFolder(string parent, List<Regex> _excluded, ref List<AssetLine> _result)
            {
                if (excludedFolders.Contains(parent))
                {
                    excludedAssetCount += 1;
                    return;
                }
                string[] guids = AssetDatabase.FindAssets(null, new[] { parent });
                foreach (var guid in guids)
                {
                    var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                    var isExclude = false;
                    foreach (var regex in _excluded)
                    {
                        if (regex.IsMatch(assetPath))
                        {
                            Debug.Log($"skip {assetPath} ({regex})");
                            isExclude = true;
                            break;
                        }
                    }
                    if (isExclude)
                    {
                        excludedAssetCount += 1;
                    }
                    else
                    {
                        var isUsed = usedAssets.Contains(assetPath);
                        if (!isUsed)
                        {
                            if (Directory.Exists(assetPath))
                            {
                                continue; // Ignore folders
                            }
                            var assetLine = new AssetLine(assetPath, isFile: true);
                            _result.Add(assetLine);
                        }
                    }
                }
            }

            var excluded = new List<Regex>();
            foreach (var excludedFolder in excludedFolders)
            {
                excluded.Add(new Regex(excludedFolder));
            }
            excludedAssetCount = 0;
            var result = new List<AssetLine>();
            var folders = AssetDatabase.GetSubFolders("Assets");
            foreach (var folder in folders)
            {
                handleSubFolder(folder, excluded, ref result);
            }
            return result;
        }

        private static List<AssetLine> parseBuildReport(string buildReport, out double totalSize)
        {
            const string markerLine = "-------------------------------------------------------------------------------";
            const string assetsLine = "Used Assets and files from the Resources folder, sorted by uncompressed size:";
            var result = new List<AssetLine>();
            var processing = false;
            totalSize = 0;
            foreach (var line in File.ReadAllLines(buildReport))
            {
                if (processing)
                {
                    if (line == markerLine)
                    {
                        break;
                    }
                    var assetLine = new AssetLine(line);
                    totalSize += assetLine.fileSizeKb;
                    result.Add(assetLine);
                }
                if (line == assetsLine)
                {
                    processing = true;
                }
            }
            return result;
        }

        private class AssetLine
        {
            private static readonly CultureInfo culture = CultureInfo.GetCultureInfo("en-US");
            private static readonly char[] separators1 = { '%' };
            private static readonly char[] separators2 = { ' ' };

            public readonly double fileSizeKb;
            public readonly double percentage;
            public readonly string filePath;

            public bool isAsset => filePath.StartsWith("Assets/");
            public bool isPackage => filePath.StartsWith("Packages/");
            public bool isResource => filePath.StartsWith("Resources/");

            public AssetLine(string line, bool isFile = false)
            {
                if (isFile)
                {
                    filePath = line;
                    fileSizeKb = (int) (new FileInfo(filePath).Length / 1024);
                    return;
                }
                var tokens = line.Split(separators1);
                if (tokens.Length != 2)
                {
                    filePath = line;
                    return;
                }
                filePath = tokens[1].Trim();
                tokens = tokens[0].Split(separators2, StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length != 3)
                {
                    return;
                }
                fileSizeKb = double.Parse(tokens[0], culture);
                percentage = double.Parse(tokens[2], culture);
            }
        }
    }
}