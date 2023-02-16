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
            var assetHistoryFilename = AssetHistory.AssetHistoryFilename;
            var assetLines = File.Exists(assetHistoryFilename) ? File.ReadAllLines(assetHistoryFilename, AssetHistory.Encoding) : Array.Empty<string>();

            Debug.Log($"Checking {folderNames.Count} folders against {assetLines.Length} assets in {assetHistoryFilename}");

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
            var allMetaFilesArray = AssetHistory.AssetPath == folderName
                ? metaFileArray
                : Directory.GetFiles(AssetHistory.AssetPath, "*.meta", SearchOption.AllDirectories);
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
                    var filename = file.Substring(0, file.Length - AssetHistory.MetaExtensionLength);
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