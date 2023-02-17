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
            var state = AssetHistoryState.Load();
            var assetLines = AssetHistory.Load();
            var assetHistory = RemoveRenamedEntries(assetLines);
            Debug.Log($"Checking {folderNames.Count} folders against {assetHistory.Count} unique assets in {AssetHistory.AssetHistoryFilename}");

            foreach (var folderName in folderNames)
            {
                CheckDeletedGuids(folderName, assetHistory, state, timer);
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

        private static void CheckDeletedGuids(string folderName, List<Tuple<string, string>> assetHistory,
            AssetHistoryState state, Stopwatch timer)
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
            // Read all project YAML files for checking that all GUIDs in them are valid-
            foreach (var metaFilename in allMetaFiles)
            {
                var metaContentFilename = metaFilename.Substring(0, metaFilename.Length - AssetHistory.MetaExtensionLength);
                if (Directory.Exists(metaContentFilename))
                {
                    continue;
                }
                var contentExtension = Path.GetExtension(metaContentFilename);
                if (state.OtherExtensions.Contains(contentExtension))
                {
                    continue;
                }
                // https://docs.unity3d.com/Manual/YAMLSceneExample.html
                var text = File.ReadAllText(metaContentFilename, AssetHistory.Encoding);
                var isValid = text.StartsWith("%YAML ") && text.Contains("%TAG !u! ");
                if (!isValid)
                {
                    if (!state.OtherExtensions.Contains(contentExtension))
                    {
                        state.OtherExtensions.Add(contentExtension);
                        state.Save();
                        Debug.Log($"New asset extension {RichText.White(contentExtension)} found");
                    }
                    continue;
                }
                if (!state.YamlExtensions.Contains(contentExtension))
                {
                    state.YamlExtensions.Add(contentExtension);
                    state.Save();
                    Debug.Log($"New YAML asset extension {RichText.Yellow(contentExtension)} found");
                }
            }
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
    }
}