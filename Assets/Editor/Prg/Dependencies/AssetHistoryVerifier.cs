using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using Object = UnityEngine.Object;

namespace Editor.Prg.Dependencies
{
    public class AssetHistoryVerifier
    {
        public static void CheckMissingReferences(List<string> folderNames)
        {
            Debug.Log($"Checking {folderNames.Count} folders");

            var verifier = new AssetHistoryVerifier();
            try
            {
                foreach (var folderName in folderNames)
                {
                    verifier.CheckDeletedGuids(folderName);
                }
            }
            finally
            {
                verifier._timer.Stop();
            }
            Debug.Log($"Check took {verifier._timer.ElapsedMilliseconds / 1000f:0.000} s");
            if (verifier._invalidGuidCount == 0)
            {
                Debug.Log("<b>No missing references found</b>");
                return;
            }
            Debug.Log($"<b>Missing references count: {verifier._invalidGuidCount} in {verifier._assetsWithMissingReferences.Count} asset(s)</b>");
        }

        private readonly Stopwatch _timer;
        private readonly AssetHistoryState _state;
        private readonly string[] _assetLines;
        private readonly HashSet<string> _invalidGuids = new();
        private readonly HashSet<string> _assetsWithMissingReferences = new();
        private int _invalidGuidCount;

        private AssetHistoryVerifier()
        {
            _timer = Stopwatch.StartNew();
            _state = AssetHistoryState.Load();
            _assetLines = AssetHistory.Load();
        }

        private void CheckDeletedGuids(string folderName)
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
            _invalidGuidCount += 1;
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