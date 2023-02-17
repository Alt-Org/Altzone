using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public static class AssetHistory
    {
        public const string AssetHistoryFilename = "m_Build_AssetHistory.txt";
        public const string AssetPath = "Assets";
        public const string DayNumberKey = "AssetHistory.DayNumber";
        public static readonly int MetaExtensionLength = ".meta".Length;
        public static readonly Encoding Encoding = new UTF8Encoding(false, false);
    }

    /// <summary>
    /// Keeps a list of files (assets) we have ever seen for a later case when files has been deleted or renamed and
    /// we need to find out what was the original name or location.<br />
    /// Initially files are in the order OS reports them and later additions are appended as they are found.<br />
    /// This facilitates tracking renamed files unambiguously.
    /// </summary>
    /// <remarks>
    /// File format (for lines) is: &lt;asset_name&gt; \t &lt;asset_guid&gt; \t &lt;asset_extension&gt;<br />
    /// We try to run this once a day when UNITY Editor is started first time.
    /// </remarks>
    public static class AssetHistoryUpdater
    {
        [InitializeOnLoadMethod]
        private static void InitializeOnLoadMethod()
        {
            EditorApplication.delayCall += OnDelayCall;
        }

        private static void OnDelayCall()
        {
            EditorApplication.delayCall -= OnDelayCall;

            var dayOfYear = DateTime.Now.DayOfYear;
            if (dayOfYear == PlayerPrefs.GetInt(AssetHistory.DayNumberKey, 0) && File.Exists(AssetHistory.AssetHistoryFilename))
            {
                return;
            }
            UpdateAssetHistory();
            PlayerPrefs.SetInt(AssetHistory.DayNumberKey, dayOfYear);
        }

        public static void UpdateAssetHistory()
        {
            var lines = File.Exists(AssetHistory.AssetHistoryFilename)
                ? File.ReadAllLines(AssetHistory.AssetHistoryFilename, AssetHistory.Encoding)
                : Array.Empty<string>();
            var hasLines = lines.Length > 0;
            var fileHistory = new HashSet<string>(lines);
            var files = Directory.GetFiles(AssetHistory.AssetPath, "*.meta", SearchOption.AllDirectories);
            var currentStatus =
                $"{RichText.Magenta("UpdateAssetHistory")} {AssetHistory.AssetHistoryFilename} with {fileHistory.Count} entries and {files.Length} meta files";
            var newFileCount = 0;
            var isShowNewFiles = Math.Abs(fileHistory.Count - files.Length) < 100;
            var newLines = new StringBuilder();
            foreach (var file in files)
            {
                if (string.IsNullOrWhiteSpace(file))
                {
                    continue;
                }
                var assetPath = file.Substring(0, file.Length - AssetHistory.MetaExtensionLength);
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
                using var streamWriter = File.AppendText(AssetHistory.AssetHistoryFilename);
                // Add CR-LF
                streamWriter.WriteLine();
                streamWriter.Write(newLines.ToString());
            }
            else
            {
                File.WriteAllText(AssetHistory.AssetHistoryFilename, newLines.ToString(), AssetHistory.Encoding);
            }
            UnityEngine.Debug.Log($"{currentStatus} {RichText.Yellow($"updated with {newFileCount} new entries")}");
        }
    }
}