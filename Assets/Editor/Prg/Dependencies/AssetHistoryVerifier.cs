using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace Editor.Prg.Dependencies
{
    public static class AssetHistoryVerifier
    {
        public static void CheckMissingReferences(List<string> folderNames)
        {
            Debug.Log($"Checking {folderNames.Count} folders");

            var verifier = new MissingReferences();
            try
            {
                foreach (var folderName in folderNames)
                {
                    verifier.CheckMissingReferences(folderName);
                }
            }
            finally
            {
                verifier.Timer.Stop();
            }
            Debug.Log($"Check took {verifier.Timer.ElapsedMilliseconds / 1000f:0.000} s");
            if (verifier.MissingGuidCount == 0)
            {
                Debug.Log("<b>No missing references found</b>");
                return;
            }
            Debug.Log($"<b>Missing references count: {verifier.MissingGuidCount} in {verifier.MissingFileCount} asset file</b>");
        }

        public static void CheckUnusedReferences(List<string> folderNames)
        {
            Debug.Log($"Checking {folderNames.Count} folders");

            var verifier = new UnusedReferences();
            try
            {
                foreach (var folderName in folderNames)
                {
                    verifier.CheckUnusedReferences(folderName);
                }
            }
            finally
            {
                verifier.Timer.Stop();
            }
            Debug.Log($"Check took {verifier.Timer.ElapsedMilliseconds / 1000f:0.000} s");
            if (verifier.UnusedGuidCount == 0)
            {
                Debug.Log("<b>No unused assets found</b>");
                return;
            }
            Debug.Log($"<b>Unused asset count: {verifier.UnusedGuidCount}</b>");
            var unusedGuids = verifier.UnusedGuids;
            unusedGuids.Sort();
            foreach (var unusedGuid in unusedGuids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(unusedGuid);
                var asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);
                Debug.Log($"Unused {RichText.Yellow(assetPath)} {unusedGuid}", asset);
            }
        }

        private class MissingReferences
        {
            private readonly AssetHistoryState _state;
            private readonly string[] _assetLines;
            private readonly HashSet<string> _invalidGuids = new();
            private readonly HashSet<string> _assetsWithMissingReferences = new();

            public Stopwatch Timer { get; }
            public int MissingGuidCount { get; private set; }
            public int MissingFileCount => _assetsWithMissingReferences.Count;

            public MissingReferences()
            {
                Timer = Stopwatch.StartNew();
                _state = AssetHistoryState.Load();
                _assetLines = AssetHistory.Load();
            }

            public void CheckMissingReferences(string folderName)
            {
                var folderMetaFiles = Directory.GetFiles(folderName, "*.meta", SearchOption.AllDirectories);
                Debug.Log($"Check {folderName} with {folderMetaFiles.Length} asset files");
                foreach (var metaFile in folderMetaFiles)
                {
                    // Using Windows path separator!  
                    var contentFilename = metaFile.Substring(0, metaFile.Length - AssetHistory.MetaExtensionLength)
                        .Replace('/', '\\');
                    if (Directory.Exists(contentFilename))
                    {
                        continue;
                    }
                    var contentExtension = Path.GetExtension(contentFilename);
                    if (_state.OtherExtensions.Contains(contentExtension))
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
                        if (!_state.OtherExtensions.Contains(contentExtension))
                        {
                            _state.OtherExtensions.Add(contentExtension);
                            _state.Save();
                            Debug.Log($"New other asset extension {RichText.White(contentExtension)} found");
                        }
                        continue;
                    }
                    if (!_state.YamlExtensions.Contains(contentExtension))
                    {
                        _state.YamlExtensions.Add(contentExtension);
                        _state.Save();
                        Debug.Log($"New YAML asset extension {RichText.Yellow(contentExtension)} found");
                    }
                    CheckFileReferences(contentFilename, textLines);
                }
            }

            private void CheckFileReferences(string contentFilename, string[] textLines)
            {
                Object currentAsset = null;
                foreach (var textLine in textLines)
                {
                    if (!(textLine.Contains("guid") || textLine.Contains("GUID")))
                    {
                        continue;
                    }
                    if (textLine.Contains("guid: 0000000000000000"))
                    {
                        continue;
                    }
                    var pos1 = textLine.IndexOf(", guid:", StringComparison.Ordinal);
                    if (pos1 > 0)
                    {
                        var pos2 = textLine.LastIndexOf(",", StringComparison.Ordinal);
                        if (pos2 > pos1 && pos2 - pos1 == 40)
                        {
                            var realGuid = textLine.Substring(pos1 + 8, 32);
                            CheckGuid(contentFilename, textLine, realGuid, ref currentAsset);
                        }
                    }
                }
            }

            /// <summary>
            /// Checks that given <c>GUID</c> is a valid reference to existing asset.
            /// </summary>
            private void CheckGuid(string contentFilename, string textLine, string guid, ref Object currentAsset)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    return;
                }
                MissingGuidCount += 1;
                _assetsWithMissingReferences.Add(contentFilename);
                if (_invalidGuids.Contains(guid))
                {
                    return;
                }
                _invalidGuids.Add(guid);
                if (currentAsset == null)
                {
                    currentAsset = AssetDatabase.LoadAssetAtPath<Object>(contentFilename);
                }
                Debug.Log($"{RichText.Yellow(contentFilename)} GUID not found {guid}", currentAsset);
                var lines = _assetLines.Where(x => x.EndsWith(guid)).ToList();
                if (lines.Count == 0)
                {
                    Debug.Log(textLine.Trim());
                    return;
                }
                var lastSeen = lines.Last().Split('\t')[0];
                Debug.Log($"Last seen in {RichText.White(lastSeen)}");
            }
        }

        private class UnusedReferences
        {
            public Stopwatch Timer { get; }
            private readonly AssetHistoryState _state;
            private readonly List<string> _scenesForBuild = new();
            private readonly HashSet<string> _unusedGuids = new();

            public int UnusedGuidCount => _unusedGuids.Count;
            public List<string> UnusedGuids => _unusedGuids.ToList();

            public UnusedReferences()
            {
                Timer = Stopwatch.StartNew();
                _state = AssetHistoryState.Load();
                _scenesForBuild.AddRange(EditorBuildSettings.scenes
                    .Where(x => x.enabled)
                    .Select(x => x.guid.ToString()));
            }

            public void CheckUnusedReferences(string folderName)
            {
                var folderGuids = GetInterestingFolderGuids(folderName);
                Debug.Log($"Check {folderName} with {folderGuids.Count} asset files");

                CheckUnusedReferences(folderGuids);
                if (folderGuids.Count > 0)
                {
                    _unusedGuids.UnionWith(folderGuids);
                }
            }

            private void CheckUnusedReferences(HashSet<string> folderGuids)
            {
                // Read all files that can have a GUID reference in it.
                var allMetaFiles = Directory.GetFiles(AssetHistory.AssetPath, "*.meta", SearchOption.AllDirectories);
                foreach (var metaFile in allMetaFiles)
                {
                    // Using Windows path separator!  
                    var contentFilename = metaFile.Substring(0, metaFile.Length - AssetHistory.MetaExtensionLength)
                        .Replace('/', '\\');
                    if (Directory.Exists(contentFilename))
                    {
                        continue;
                    }
                    var contentExtension = Path.GetExtension(contentFilename);
                    if (_state.OtherExtensions.Contains(contentExtension))
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
                        if (!_state.OtherExtensions.Contains(contentExtension))
                        {
                            _state.OtherExtensions.Add(contentExtension);
                            _state.Save();
                            Debug.Log($"New other asset extension {RichText.White(contentExtension)} found");
                        }
                        continue;
                    }
                    if (!_state.YamlExtensions.Contains(contentExtension))
                    {
                        _state.YamlExtensions.Add(contentExtension);
                        _state.Save();
                        Debug.Log($"New YAML asset extension {RichText.Yellow(contentExtension)} found");
                    }
                    CheckFileReferences(textLines, folderGuids);
                }
            }

            private HashSet<string> GetInterestingFolderGuids(string folderName)
            {
                string[] excludedDirectories =
                {
                    @"Assets\Photon\",
                    @"Assets\Photon Unity Networking",
                    @"Assets\TextMesh Pro\",
                };

                // These files are rarely dragged in Editor to create a reference to it.
                // - and if it is and there is error it should be found during runtime when testing the app.
                string[] excludedExtensions =
                {
                    ".asmdef",
                    ".bytes",
                    ".cs",
                    ".csv",
                    ".txt",
                };

                var folderMetaFiles = Directory.GetFiles(folderName, "*.meta", SearchOption.AllDirectories);
                var folderGuids = new HashSet<string>();
                foreach (var metaFile in folderMetaFiles)
                {
                    // Using Windows path separator!  
                    var contentFilename = metaFile.Substring(0, metaFile.Length - AssetHistory.MetaExtensionLength)
                        .Replace('/', '\\');
                    if (Directory.Exists(contentFilename))
                    {
                        continue;
                    }
                    if (excludedDirectories.Any(x => contentFilename.StartsWith(x)))
                    {
                        continue;
                    }
                    var contentExtension = Path.GetExtension(contentFilename);
                    if (excludedExtensions.Contains(contentExtension))
                    {
                        continue;
                    }
                    var textLines = File.ReadAllLines(metaFile, AssetHistory.Encoding);
                    if (textLines.Length < 3)
                    {
                        continue;
                    }
                    // Check for labels.
                    if (textLines.Contains("labels:"))
                    {
                        continue;
                    }
                    // Some meta files can contain empty lines we have to skip.
                    // - typically due to version control line end conversion errors when using not suitable settings
                    var line2 = textLines[1] == string.Empty ? textLines[2] : textLines[1];
                    var guid = line2.Split(':')[1].Trim();
                    if (_scenesForBuild.Contains(guid))
                    {
                        // Scene in build settings.
                        continue;
                    }
                    Assert.IsFalse(folderGuids.Contains(guid));
                    folderGuids.Add(guid);
                }
                return folderGuids;
            }

            private void CheckFileReferences(string[] textLines, HashSet<string> folderGuids)
            {
                foreach (var textLine in textLines)
                {
                    if (!(textLine.Contains("guid") || textLine.Contains("GUID")))
                    {
                        continue;
                    }
                    if (textLine.Contains("guid: 0000000000000000"))
                    {
                        continue;
                    }
                    var pos1 = textLine.IndexOf(", guid:", StringComparison.Ordinal);
                    if (pos1 > 0)
                    {
                        var pos2 = textLine.LastIndexOf(",", StringComparison.Ordinal);
                        if (pos2 > pos1 && pos2 - pos1 == 40)
                        {
                            var realGuid = textLine.Substring(pos1 + 8, 32);
                            if (folderGuids.Contains(realGuid))
                            {
                                // What is left here is unused.
                                folderGuids.Remove(realGuid);
                            }
                        }
                    }
                }
            }
        }
    }
}