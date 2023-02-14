using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    /// <summary>
    /// Keeps a list of files (assets) we have ever seen for a later case when files has been deleted or renamed and
    /// we need to find out what was the original name or location.
    /// </summary>
    /// <remarks>
    /// We try to run this once a day when UNITY Editor is started first time.
    /// </remarks>
    public static class AssetHistoryUpdater
    {
        private const string AssetHistoryFilename = "m_Build_AssetHistory.txt";
        private const string AssetPath = "Assets";
        private const string DayNumberKey = "AssetHistory.DayNumber";
        private static readonly int MetaExtensionLength = ".meta".Length;
        private static readonly Encoding Encoding = Encoding.UTF8;

        /*[MenuItem("Window/ALT-Zone/Update Asset History", false, 55)]
        private static void UpdateAssetHistoryMenu() => OnDelayCall();*/

        [InitializeOnLoadMethod]
        private static void InitializeOnLoadMethod()
        {
            EditorApplication.delayCall += OnDelayCall;
        }

        private static void OnDelayCall()
        {
            EditorApplication.delayCall -= OnDelayCall;

            var dayOfYear = DateTime.Now.DayOfYear;
            if (dayOfYear == PlayerPrefs.GetInt(DayNumberKey, 0) && File.Exists(AssetHistoryFilename))
            {
                return;
            }
            UpdateAssetHistory();
            PlayerPrefs.SetInt(DayNumberKey, dayOfYear);
        }

        public static void CheckDeletedGuids(List<string> folderNames)
        {
            var assetLines = File.Exists(AssetHistoryFilename) ? File.ReadAllLines(AssetHistoryFilename, Encoding) : Array.Empty<string>();

            Debug.Log($"Checking {folderNames.Count} folders against {assetLines.Length} assets in {AssetHistoryFilename}");

            foreach (var folderName in folderNames)
            {
                CheckDeletedGuids(folderName, assetLines);
            }
        }

        private static void CheckDeletedGuids(string folderName, string[] assetLines)
        {
            // This works only on Windows for now :-(
            folderName = folderName.Replace('/', '\\');

            var assetHistory = new List<Tuple<string, string>>();
            var folderNamePrefix = folderName + '\\';
            var didRename = false;
            foreach (var assetLine in assetLines)
            {
                if (assetLine.StartsWith(folderNamePrefix))
                {
                    var tokens = assetLine.Split('\t');
                    // Check if asset has been renamed - and remove previous names as guid is the same all the time.
                    var previousNameIndex = assetHistory.FindIndex(x => x.Item2 == tokens[1]);
                    if (previousNameIndex != -1)
                    {
                        Debug.Log($"{assetHistory[previousNameIndex].Item1} -> {tokens[0]} guid: {tokens[1]}");
                        assetHistory.RemoveAt(previousNameIndex);
                        didRename = true;
                    }
                    assetHistory.Add(new Tuple<string, string>(tokens[0], tokens[1]));
                }
            }
            if (didRename)
            {
                Debug.Log("");
            }

            var metaFileArray = Directory.GetFiles(folderName, "*.meta", SearchOption.AllDirectories);
            var allMetaFilesArray = AssetPath == folderName
                ? metaFileArray
                : Directory.GetFiles(AssetPath, "*.meta", SearchOption.AllDirectories);
            var fileList = CreateFileList(metaFileArray);
            Debug.Log($"Check {folderName} with {fileList.Count} asset files and {assetHistory.Count} history");
            if (!SanityCheck())
            {
                return;
            }

            // Check that a history file exists - to find deleted files.
            var missingAssets = new List<Tuple<string, string>>();
            foreach (var tuple in assetHistory)
            {
                var assetFilename = tuple.Item1;
                if (!fileList.Contains(assetFilename))
                {
                    missingAssets.Add(tuple);
                }
            }
            if (missingAssets.Count > 0)
            {
                Debug.Log("No missing assets");
                return;
            }
            // Check if deleted files are used by existing files.
            var useCount = CheckIfGuidIsUsed(missingAssets, allMetaFilesArray);
            Debug.Log($"Deleted asset count {missingAssets.Count} with {useCount} invalid references");

            #region Local helper functions

            List<string> CreateFileList(string[] metaFilenames)
            {
                var list = new List<string>();
                foreach (var file in metaFilenames)
                {
                    var filename = file.Substring(0, file.Length - MetaExtensionLength);
                    list.Add(filename);
                }
                return list;
            }

            bool SanityCheck()
            {
                // Just a sanity check that history file is up-to-date with file system.
                foreach (var filename in fileList)
                {
                    if (Directory.Exists(filename))
                    {
                        continue;
                    }
                    //var index = Array.FindIndex(files, x => x.StartsWith(filename));
                    var index = assetHistory.FindIndex(x => x.Item1 == filename);
                    if (index == -1)
                    {
                        Debug.Log($"{RichText.Yellow(filename)} not found in asset history");
                        return false;
                    }
                }
                return true;
            }

            #endregion
        }

        private static int CheckIfGuidIsUsed(List<Tuple<string, string>> missingAssets, string[] metaFileArray)
        {
            var validExtensions = new[]
            {
                // These are YAML files that can(?) have references to other files.
                ".anim",
                ".asset",
                ".controller",
                ".lightning",
                ".mat",
                ".prefab",
                ".unity",
            };

            var stopwatch = Stopwatch.StartNew();
            var foundCount = 0;
            foreach (var metaFilename in metaFileArray)
            {
                var metaContentFilename = metaFilename.Substring(0, metaFilename.Length - MetaExtensionLength);
                if (Directory.Exists(metaContentFilename))
                {
                    continue;
                }
                var contentExtension = Path.GetExtension(metaContentFilename);
                if (!validExtensions.Contains(contentExtension))
                {
                    continue;
                }
                var text = File.ReadAllText(metaContentFilename, Encoding);
                foreach (var tuple in missingAssets)
                {
                    var missingFilename = tuple.Item1;
                    if (metaContentFilename == missingFilename)
                    {
                        continue;
                    }
                    var guid = tuple.Item2;
                    if (text.Contains(guid))
                    {
                        foundCount += 1;
                        var asset = AssetDatabase.LoadMainAssetAtPath(metaContentFilename);
                        Debug.Log($"{RichText.White(missingFilename)} guid: {guid} is deleted but referenced in");
                        Debug.Log($"{RichText.Yellow(metaFilename)} ", asset);
                    }
                }
            }
            stopwatch.Stop();
            Debug.Log($"Check all files took {stopwatch.ElapsedMilliseconds / 1000d:0.000} s");
            return foundCount;
        }

        private static void UpdateAssetHistory()
        {
            var lines = File.Exists(AssetHistoryFilename) ? File.ReadAllLines(AssetHistoryFilename, Encoding) : Array.Empty<string>();
            var hasLines = lines.Length > 0;
            var fileHistory = new HashSet<string>(lines);
            var files = Directory.GetFiles(AssetPath, "*.meta", SearchOption.AllDirectories);
            var currentStatus =
                $"{RichText.Magenta("UpdateAssetHistory")} {AssetHistoryFilename} with {fileHistory.Count} entries and {files.Length} meta files";
            var newFileCount = 0;
            var isShowNewFiles = Math.Abs(fileHistory.Count - files.Length) < 100;
            var newLines = new StringBuilder();
            foreach (var file in files)
            {
                if (string.IsNullOrWhiteSpace(file))
                {
                    continue;
                }
                var assetPath = file.Substring(0, file.Length - MetaExtensionLength);
                var guid = AssetDatabase.GUIDFromAssetPath(assetPath);
                var line = $"{assetPath}\t{guid}";
                if (fileHistory.Add(line))
                {
                    newFileCount += 1;
                    newLines.Append(line).AppendLine();
                    if (isShowNewFiles)
                    {
                        UnityEngine.Debug.Log(line);
                    }
                }
            }
            if (newFileCount == 0)
            {
                UnityEngine.Debug.Log($"{currentStatus} {RichText.White("ok")}");
                return;
            }
            // Remove last CR-LF
            newLines.Length -= 2;
            if (hasLines)
            {
                using var streamWriter = File.AppendText(AssetHistoryFilename);
                // Add CR-LF
                streamWriter.WriteLine();
                streamWriter.Write(newLines.ToString());
            }
            else
            {
                File.WriteAllText(AssetHistoryFilename, newLines.ToString(), Encoding);
            }
            UnityEngine.Debug.Log($"{currentStatus} {RichText.Yellow($"updated with {newFileCount} new entries")}");
        }
    }
}