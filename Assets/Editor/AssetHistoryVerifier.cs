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

            try
            {
                foreach (var folderName in folderNames)
                {
                    CheckDeletedGuids(folderName, assetHistory, state, timer);
                }
            }
            finally
            {
                timer.Stop();
            }
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
            var isFullCheck = AssetHistory.AssetPath == folderName;
            var allMetaFiles = isFullCheck
                ? folderMetaFiles
                : Directory.GetFiles(AssetHistory.AssetPath, "*.meta", SearchOption.AllDirectories);
            var folderFiles = new List<string>();
            foreach (var file in folderMetaFiles)
            {
                // Using Windows path separator!  
                var filename = file.Substring(0, file.Length - AssetHistory.MetaExtensionLength)
                    .Replace('/', '\\');
                folderFiles.Add(filename);
            }
            Debug.Log(
                $"Check {folderName} with {folderFiles.Count} asset files against {allMetaFiles.Count()}/{assetHistory.Count} actual/history");
            if (!SanityCheck(folderFiles, assetHistory))
            {
                return;
            }
            // Read all project YAML files for checking that all GUIDs in them are valid.
            var referencedGuids = new HashSet<string>();
            var otherGuids = new HashSet<string>();
            var guidFileCount = 0;
            foreach (var metaFilename in allMetaFiles)
            {
                var contentFilename = metaFilename.Substring(0, metaFilename.Length - AssetHistory.MetaExtensionLength);
                if (Directory.Exists(contentFilename))
                {
                    continue;
                }
                var contentExtension = Path.GetExtension(contentFilename);
                if (state.OtherExtensions.Contains(contentExtension))
                {
                    continue;
                }
                // https://docs.unity3d.com/Manual/YAMLSceneExample.html
                var textLines = File.ReadAllLines(contentFilename, AssetHistory.Encoding);
                if (textLines.Length < 2)
                {
                    continue;
                }
                var isValid = textLines[0].StartsWith("%YAML ") && textLines[1].StartsWith("%TAG !u! ");
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
                guidFileCount += CheckFileReferences(contentFilename, textLines, referencedGuids, otherGuids);
            }
            Debug.Log($"Found {referencedGuids.Count} different guids and {otherGuids.Count} other guids in {guidFileCount} files");
            var guidList = otherGuids.ToList();
            guidList.Sort();
            File.WriteAllText("m_Build_AssetHistory_GUIDs.txt", string.Join("\r\n", guidList));

            // Find all current existing file guids in given folder
            var folderGuids = new HashSet<string>();
            foreach (var assetFile in folderFiles)
            {
                var guid = AssetDatabase.GUIDFromAssetPath(assetFile);
                folderGuids.Add(guid.ToString());
            }
            Debug.Log($"Check folderGuids {folderGuids.Count}");
            var unusedReferenceCount = 0;
            foreach (var folderGuid in folderGuids)
            {
                if (!referencedGuids.Contains(folderGuid))
                {
                    unusedReferenceCount += 1;
                    var tuple = assetHistory.FirstOrDefault(x => x.Item2 == folderGuid);
                    Debug.Log($"Unused reference to {folderGuid} {tuple?.Item1}");
                }
            }
            Debug.Log($"UnusedReferenceCount {unusedReferenceCount}");
            if (!isFullCheck)
            {
                return;
            }
            var missingReferenceCount = 0;
            foreach (var tuple in assetHistory)
            {
                var guid = tuple.Item2;
                if (folderGuids.Contains(guid))
                {
                    continue;
                }
                if (referencedGuids.Contains(guid))
                {
                    missingReferenceCount += 1;
                    Debug.Log($"Missing reference {guid} {tuple.Item1}");
                }
            }
            Debug.Log($"MissingReferenceCount {missingReferenceCount}");
            return;
            foreach (var referencedGuid in referencedGuids)
            {
                if (!folderGuids.Contains(referencedGuid))
                {
                    missingReferenceCount += 1;
                    var tuple = assetHistory.FirstOrDefault(x => x.Item2 == referencedGuid);
                    Debug.Log($"Missing reference {referencedGuid} {tuple?.Item1}");
                }
            }
        }

        private static int CheckFileReferences(string contentFilename, string[] textLines, HashSet<string> referencedGuids,
            HashSet<string> otherGuids)
        {
            var guidCount = 0;
            foreach (var textLine in textLines)
            {
                if (textLine.Contains("guid") || textLine.Contains("GUID"))
                {
                    if (textLine.Contains("guid: 0000000000000000"))
                    {
                        continue;
                    }
                    guidCount += 1;
                    var pos1 = textLine.IndexOf(", guid:", StringComparison.Ordinal);
                    if (pos1 > 0)
                    {
                        var pos2 = textLine.LastIndexOf(",", StringComparison.Ordinal);
                        if (pos2 > pos1 && pos2 - pos1 == 40)
                        {
                            var realGuid = textLine.Substring(pos1 + 8, 32);
                            referencedGuids.Add(realGuid);
                            continue;
                        }
                    }
                    otherGuids.Add(textLine.Trim());
                }
            }
            //var asset = AssetDatabase.LoadAssetAtPath<Object>(contentFilename.Replace('\\', '/'));
            //Debug.Log($"{Path.GetFileName(contentFilename)} lines {textLines.Length} guidCount {guidCount}", asset);
            return guidCount;
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