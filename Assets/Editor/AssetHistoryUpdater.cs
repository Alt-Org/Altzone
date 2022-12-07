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
        private static readonly int MetaExtensionLength = ".meta".Length;

        /*[MenuItem("Window/ALT-Zone/Update Asset History", false, 55)]
        private static void UpdateAssetHistoryMenu() => OnDelayCall();*/

        [InitializeOnLoadMethod]
        private static void InitializeOnLoadMethod()
        {
            EditorApplication.delayCall += OnDelayCall;
        }

        private static void OnDelayCall()
        {
            UnityEngine.Debug.Log(RichText.Magenta("AssetHistoryUpdater working"));
            if (!File.Exists(AssetHistoryFilename))
            {
                File.WriteAllText(AssetHistoryFilename, "");
            }
            UpdateAssetHistory();
        }

        private static void UpdateAssetHistory()
        {
            var lines = File.ReadAllLines(AssetHistoryFilename);
            var hasLines = lines.Length > 0 && lines[0].Length > 0;
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
                var assetPath = file.Substring(0, file.Length - MetaExtensionLength);
                var guid = AssetDatabase.GUIDFromAssetPath(assetPath);
                var line = $"{assetPath}\t{guid}";
                if (fileHistory.Add(line))
                {
                    newFileCount += 1;
                    newLines.Append(line).AppendLine();
                    if (hasLines)
                    {
                        UnityEngine.Debug.Log(line);
                    }
                }
            }
            if (newFileCount == 0)
            {
                UnityEngine.Debug.Log(RichText.White("ok"));
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
                File.WriteAllText(AssetHistoryFilename, newLines.ToString());
            }
            var message = $"UpdateAssetHistory {AssetHistoryFilename} with {newFileCount} new entries";
            UnityEngine.Debug.Log(RichText.Yellow(message));
        }
    }
}