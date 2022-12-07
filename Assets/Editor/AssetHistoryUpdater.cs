using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;

namespace Editor
{
    public static class AssetHistoryUpdater
    {
        private const string AssetHistoryFilename = "m_Build_AssetHistory.txt";
        private const string AssetPath = "Assets";

        [InitializeOnLoadMethod]
        private static void InitializeOnLoadMethod()
        {
            EditorApplication.delayCall += OnDelayCall;
        }

        private static void OnDelayCall()
        {
            UnityEngine.Debug.Log(RichText.Magenta("AssetHistoryUpdater working"));
            UnityEngine.Debug.Log(Path.GetFullPath(AssetHistoryFilename));
            if (!File.Exists(AssetHistoryFilename))
            {
                File.WriteAllText(AssetHistoryFilename, "");
            }
            UpdateAssetHistory();
        }

        private static void UpdateAssetHistory()
        {
            var lines = File.ReadAllLines(AssetHistoryFilename);
            var fileHistory = new HashSet<string>(lines);
            var files = Directory.GetFiles(AssetPath, "*.meta", SearchOption.AllDirectories);
            UnityEngine.Debug.Log($"UpdateAssetHistory {AssetHistoryFilename} with {fileHistory.Count} entries and {files.Length} meta files");
            var newFileCount = 0;
            var newLines = new StringBuilder();
            foreach (var file in files)
            {
                if (string.IsNullOrWhiteSpace(file))
                {
                    continue;
                }
                if (fileHistory.Add(file))
                {
                    newFileCount += 1;
                    if (newFileCount > 1)
                    {
                        newLines.AppendLine();
                    }
                    var guid = GetAssetGuid(file);
                    newLines.Append($"{file}\t{guid}");
                }
            }
            if (newFileCount == 0)
            {
                UnityEngine.Debug.Log("ok");
            }
            using var sw = File.AppendText(AssetHistoryFilename);
            sw.WriteLine(newLines.ToString());
            UnityEngine.Debug.Log($"UpdateAssetHistory {AssetHistoryFilename} with {newFileCount} new entries");
        }

        private static string GetAssetGuid(string metaPath)
        {
            var assetPath = Path.GetFileNameWithoutExtension(metaPath);
            var guid = AssetDatabase.GUIDFromAssetPath(assetPath);
            return guid.ToString();
        }
    }
}