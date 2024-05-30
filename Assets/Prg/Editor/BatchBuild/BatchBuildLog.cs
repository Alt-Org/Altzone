using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Prg.Scripts.Common.Util;
using UnityEngine;
using UnityEngine.Assertions;

namespace Prg.Editor.BatchBuild
{
    /// <summary>
    /// Creates a list of files in the UNITY build output log to JSON formattable classes.
    /// </summary>
    public static class BatchBuildLog
    {
        private static readonly CultureInfo Culture = PlatformUtil.Culture;
        private static readonly Encoding Encoding = PlatformUtil.Encoding;

        private const string JsPrefix = "const buildLog = ";
        public static int TestCountLimiter;

        public static BuildReportLog SaveBuildReportLog(string buildLogFilename, string tsvOutputFilename,
            string jsOutputFilename)
        {
            var buildReportLog = CreateBuildReportLog_(buildLogFilename);
            if (buildReportLog == null)
            {
                return null;
            }
            SaveAsTsv(buildReportLog, tsvOutputFilename);
            SaveAsJavaScript(buildReportLog, jsOutputFilename);
            return buildReportLog;
        }

        public static BuildReportLog CreateBuildReportLog(string buildLogFilename)
        {
            return CreateBuildReportLog_(buildLogFilename);
        }

        public static BuildReportLog LoadFromFile(string filename)
        {
            // Remove variable declaration and semicolon.
            var buffer = new StringBuilder(File.ReadAllText(filename, Encoding));
            buffer.Remove(0, JsPrefix.Length);
            buffer.Length -= 1;
            return JsonUtility.FromJson<BuildReportLog>(buffer.ToString());
        }

        private static void SaveAsTsv(BuildReportLog buildReportLog, string outputFilename)
        {
            var logLines = buildReportLog.Lines;

            // Build tsv file.
            var builder = new StringBuilder()
                .Append(
                    $"Name\tSize Kb\t%\t\tFiles\t{logLines.Count}\tReported Size\t{buildReportLog.TotalFileSizeKb.ToString("0.0", Culture)} Kb")
                .AppendLine();
            foreach (var line in logLines)
            {
                builder.Append(line.FilePath).Append('\t')
                    .Append(line.FileSizeKb.ToString("0.0", Culture)).Append('\t')
                    .Append(line.Percentage > 0 ? line.Percentage.ToString("\t0.0", Culture) : null)
                    .AppendLine();
            }
            // Remove last CR-LF.
            builder.Length -= 2;
            File.WriteAllText(outputFilename, builder.ToString(), Encoding);
        }

        private static void SaveAsJavaScript(BuildReportLog buildReportLog, string outputFilename)
        {
            var jsText = $"{JsPrefix}{JsonUtility.ToJson(buildReportLog, TestCountLimiter > 0)};";
            File.WriteAllText(outputFilename, jsText, Encoding);
        }

        private static BuildReportLog CreateBuildReportLog_(string buildLogFilename)
        {
            const string buildReportLine = "Build Report";
            const string usedAssetsLine =
                "Used Assets and files from the Resources folder, sorted by uncompressed size:";
            const string noDataMarkerLine =
                "Information on used Assets is not available, since player data was not rebuilt.";
            const string endMarkerLine =
                "-------------------------------------------------------------------------------";
            // Example lines:
            //  1.4 mb	 0.2% Assets/Altzone/Graphics/Logo/ALT ZONE logo.png
            //  341.5 kb	 0.1% Assets/TextMesh Pro/Sprites/EmojiOne.png
            //  0.1 kb	 0.0% Assets/MenuUi/Scripts/Shop.cs

            var lines = File.ReadAllLines(buildLogFilename);
            var lastLine = lines.Length - 1;
            var currentLine = 0;
            // Find Build Report line.
            for (; currentLine < lastLine; ++currentLine)
            {
                var line = lines[currentLine];
                if (line == buildReportLine)
                {
                    break;
                }
            }
            if (currentLine == lastLine)
            {
                Debug.LogWarning($"Report file {buildLogFilename} does not have a 'Build Report'");
                return null;
            }
            // Find Used Assets line.
            currentLine += 1;
            for (; currentLine < lastLine; ++currentLine)
            {
                var line = lines[currentLine];
                if (line == usedAssetsLine)
                {
                    break;
                }
                if (line == noDataMarkerLine)
                {
                    Debug.LogWarning($"Report file {buildLogFilename} did not have data" +
                                     $" for 'Used Assets' because *player data* was *not rebuilt* on last build!");
                    return null;
                }
            }
            if (currentLine == lastLine)
            {
                Debug.LogWarning($"Report file {buildLogFilename} does not have valid 'Build Report'");
                return null;
            }
            // Read all Used Assets lines.
            var totalSize = 0D;
            var result = new List<BuildReportLogLine>();
            currentLine += 1;
            for (; currentLine < lastLine; ++currentLine)
            {
                var line = lines[currentLine];
                if (line == endMarkerLine)
                {
                    break;
                }
                var assetLine = new BuildReportLogLine(line);
                totalSize += assetLine.FileSizeKb;
                result.Add(assetLine);
                if (TestCountLimiter > 0 && result.Count >= TestCountLimiter)
                {
                    break;
                }
            }
            var buildReportLog = new BuildReportLog(totalSize, result);
            return buildReportLog;
        }
    }

    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
    public class BuildReportLog
    {
        public double TotalFileSizeKb;
        public List<BuildReportLogLine> Lines = new();

        public BuildReportLog()
        {
        }

        public BuildReportLog(double totalFileSizeKb, List<BuildReportLogLine> buildReportLogLines)
        {
            TotalFileSizeKb = Math.Round(totalFileSizeKb, 3);
            Lines = (buildReportLogLines ?? new List<BuildReportLogLine>())
                .OrderBy(x => x.SortKey)
                .ToList();
        }
    }

    [Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
    public class BuildReportLogLine
    {
        private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("en-US");
        private static readonly char[] Separators1 = { '%' };
        private static readonly char[] Separators2 = { ' ', '\t' };

        // Properties can not be readonly for JSON serializer to work :-(
        public double FileSizeKb;
        public double Percentage;
        public string FilePath;

        public readonly string SortKey;

        public BuildReportLogLine()
        {
        }

        public BuildReportLogLine(string logLine)
        {
            //  1.4 mb	 1.6% Assets/Sounds/Altzone_battle_version_2.mp3
            //  712.4 kb	 0.8% Assets/Sounds/10-minutes-of-silence.mp3

            Assert.IsTrue(!string.IsNullOrWhiteSpace(logLine));
            // Guaranteed split of the line onto two parts - filename can have spaces in it!
            var tokens = logLine.Split(Separators1);
            Assert.AreEqual(2, tokens.Length);
            // Last part is the file name.
            FilePath = tokens[1].Trim();
            // First part contains three tokens.
            tokens = tokens[0].Split(Separators2, StringSplitOptions.RemoveEmptyEntries);
            Assert.AreEqual(3, tokens.Length);
            FileSizeKb = double.Parse(tokens[0], Culture);
            if (tokens[1] == "mb")
            {
                FileSizeKb *= 1024.0;
            }
            else
            {
                Assert.AreEqual("kb", tokens[1]);
            }
            Percentage = double.Parse(tokens[2], Culture);

            SortKey = $"{9999999.9 - FileSizeKb:0000000.000}.{FilePath}";
        }
    }
}
