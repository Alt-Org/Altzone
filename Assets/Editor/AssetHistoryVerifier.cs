using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;

namespace Editor
{
    public static class AssetHistoryVerifier
    {
        public static void CheckDeletedGuids(List<string> folderNames)
        {
            var timer = Stopwatch.StartNew();
            var assetLines = AssetHistory.Load();
            var assetHistory = RemoveRenamedEntries(assetLines);
            Debug.Log($"Checking {folderNames.Count} folders against {assetHistory.Count} unique assets in {AssetHistory.AssetHistoryFilename}");

            foreach (var folderName in folderNames)
            {
                CheckDeletedGuids(folderName, assetHistory, timer);
            }
            timer.Stop();
            Debug.Log($"Check took {timer.ElapsedMilliseconds / 1000f:0.000} s");
        }

        private static List<Tuple<string, string>> RemoveRenamedEntries(string[] assetLines)
        {
            var assetHistory = new List<Tuple<string, string>>();
            foreach (var assetLine in assetLines)
            {
                var tokens = assetLine.Split('\t');
                // Check if asset has been renamed - and remove previous names as guid is same all the time.
                var previousNameIndex = assetHistory.FindIndex(x => x.Item2 == tokens[1]);
                if (previousNameIndex != -1)
                {
                    assetHistory.RemoveAt(previousNameIndex);
                }
                assetHistory.Add(new Tuple<string, string>(tokens[0], tokens[1]));
            }
            return assetHistory;
        }
        
        private static void CheckDeletedGuids(string folderName, List<Tuple<string, string>> assetHistory, Stopwatch timer)
        {
            var folderMetaFiles = Directory.GetFiles(folderName, "*.meta", SearchOption.AllDirectories);
            var allMetaFiles = AssetHistory.AssetPath == folderName
                ? folderMetaFiles
                : Directory.GetFiles(AssetHistory.AssetPath, "*.meta", SearchOption.AllDirectories);
            var folderFiles = new List<string>();
            foreach (var file in folderMetaFiles)
            {
                var filename = file.Substring(0, file.Length - AssetHistory.MetaExtensionLength);
                folderFiles.Add(filename);
            }
            Debug.Log($"Check {folderName} with {folderFiles.Count} asset files against {allMetaFiles.Count()}/{assetHistory.Count} actual/history");
            if (!SanityCheck(folderFiles, assetHistory))
            {
                return;
            }

            #region Local helper functions

            List<string> CreateFileList(string[] metaFilenames)
            {
                var list = new List<string>();
                return list;
            }

            #endregion
        }

        private static bool SanityCheck(List<string> fileList, List<Tuple<string, string>> assetHistory)
        {
            // Just a sanity check that asset history (file) is up-to-date with current file system.
            foreach (var filename in fileList)
            {
                if (Directory.Exists(filename))
                {
                    continue;
                }
                var index = assetHistory.FindIndex(x => x.Item1 == filename);
                if (index == -1)
                {
                    Debug.Log($"{RichText.Yellow(filename)} not found in asset history");
                    return false;
                }
            }
            return true;
        }

        private static void OldCode(List<string> fileList, List<Tuple<string, string>> assetHistory)
        {
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
            var useCount = 0;//CheckIfGuidIsUsed(missingAssets, allMetaFiles);
            Debug.Log($"Deleted asset count {missingAssets.Count} with {useCount} invalid references");
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
                var metaContentFilename = metaFilename.Substring(0, metaFilename.Length - AssetHistory.MetaExtensionLength);
                if (Directory.Exists(metaContentFilename))
                {
                    continue;
                }
                var contentExtension = Path.GetExtension(metaContentFilename);
                if (!validExtensions.Contains(contentExtension))
                {
                    continue;
                }
                var text = File.ReadAllText(metaContentFilename, AssetHistory.Encoding);
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

    }
}