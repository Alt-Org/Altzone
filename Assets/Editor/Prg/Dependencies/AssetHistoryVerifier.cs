using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
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
                    verifier.CheckDeletedGuids(folderName);
                }
            }
            finally
            {
                verifier.Timer.Stop();
            }
            Debug.Log($"Check took {verifier.Timer.ElapsedMilliseconds / 1000f:0.000} s");
            if (verifier.InvalidGuidCount == 0)
            {
                Debug.Log("<b>No missing references found</b>");
                return;
            }
            Debug.Log($"<b>Missing references count: {verifier.InvalidGuidCount} in {verifier.MissingReferencesCount} asset(s)</b>");
        }

        public static void CheckUnusedReferences(List<string> folderNames)
        {
            
        }
        private class MissingReferences
        {
            private readonly AssetHistoryState _state;
            private readonly string[] _assetLines;
            private readonly HashSet<string> _invalidGuids = new();
            private readonly HashSet<string> _assetsWithMissingReferences = new();

            public Stopwatch Timer { get; }
            public int InvalidGuidCount { get; private set; }
            public int MissingReferencesCount => _assetsWithMissingReferences.Count;

            public MissingReferences()
            {
                Timer = Stopwatch.StartNew();
                _state = AssetHistoryState.Load();
                _assetLines = AssetHistory.Load();
            }

            public void CheckDeletedGuids(string folderName)
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

            private void CheckGuid(string contentFilename, string textLine, string guid, ref Object currentAsset)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);
                if (!string.IsNullOrEmpty(assetPath))
                {
                    return;
                }
                InvalidGuidCount += 1;
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
    }
}