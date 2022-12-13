using System;
using System.Collections.Generic;
using System.IO;
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
            if (File.Exists(AssetHistoryFilename))
            {
                var dayNumber = PlayerPrefs.GetInt(DayNumberKey, 0);
                if (dayNumber == dayOfYear)
                {
                    return;
                }
            }
            else
            {
                File.WriteAllText(AssetHistoryFilename, "");
            }
            UpdateAssetHistory();
            PlayerPrefs.SetInt(DayNumberKey, dayOfYear);
        }

        private static void UpdateAssetHistory()
        {
            var lines = File.ReadAllLines(AssetHistoryFilename);
            var hasLines = lines.Length > 0 && lines[0].Length > 0;
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
                File.WriteAllText(AssetHistoryFilename, newLines.ToString());
            }
            UnityEngine.Debug.Log($"{currentStatus} {RichText.Yellow($"updated with {newFileCount} new entries")}");
        }
    }
}