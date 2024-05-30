using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using Prg.Scripts.Common.Util;
using UnityEngine;

namespace Prg.Editor.BatchBuild
{
    /// <summary>
    /// Creates a list of files in the project using UNITY <c>BuildReport</c> and file system scan to JSON formattable classes.<br />
    /// UNITY build output log is ignored here for now.
    /// </summary>
    public static class BatchBuildFiles
    {
        private static readonly Encoding Encoding = PlatformUtil.Encoding;

        private const string JsPrefix = "const buildFiles = ";
        public static int TestCountLimiter;

        public static ProjectFiles SaveProjectFiles(BuildReportAssets buildReport, BuildReportLog buildReportLog,
            string tsvOutputFilename, string jsOutputFilename)
        {
            var packedAssets = buildReport.Lines;
            var logLines = buildReportLog.Lines;
            var fileList = GetProjectFilesListFromMetaFiles();
            var projectFiles = new ProjectFiles();
            foreach (var projectFile in fileList)
            {
                var fileCategory = FileCategory.UnUsed;
                if (packedAssets.Any(x => x.AssetPath == projectFile))
                {
                    fileCategory |= FileCategory.UsedInReport;
                }
                if (logLines.Any(x => x.FilePath == projectFile))
                {
                    fileCategory |= FileCategory.UsedInLog;
                }
                projectFiles.Lines.Add(new ProjectFile()
                {
                    FilePath = projectFile,
                    FileSize = new FileInfo(projectFile).Length,
                    FileCategory = fileCategory
                });
            }
            projectFiles.TotalFileSize = projectFiles.Lines.Sum(x => x.FileSize);
            projectFiles.UnUsedFileSize =
                projectFiles.Lines.Sum(x => x.FileCategory == FileCategory.UnUsed ? x.FileSize : 0);
            SaveAsTsv(projectFiles, tsvOutputFilename);
            SaveAsJavaScript(projectFiles, jsOutputFilename);
            return projectFiles;
        }

        public static ProjectFiles LoadFromFile(string filename)
        {
            // Remove variable declaration and semicolon.
            var buffer = new StringBuilder(File.ReadAllText(filename, Encoding));
            buffer.Remove(0, JsPrefix.Length);
            buffer.Length -= 1;
            return JsonUtility.FromJson<ProjectFiles>(buffer.ToString());
        }

        private static void SaveAsTsv(ProjectFiles projectFiles, string outputFilename)
        {
            var files = projectFiles.Lines;
            // Build tsv file.
            var builder = new StringBuilder()
                .Append(
                    $"Name\tSize\tCat\t\tFiles\t{files.Count}\tTotal Size\t{projectFiles.TotalFileSize}")
                .AppendLine();
            foreach (var file in files)
            {
                builder.Append(file.FilePath).Append('\t')
                    .Append(file.FileSize).Append('\t')
                    .Append((int)file.FileCategory)
                    .AppendLine();
            }
            // Remove last CR-LF.
            builder.Length -= 2;
            File.WriteAllText(outputFilename, builder.ToString(), Encoding);
        }

        private static void SaveAsJavaScript(ProjectFiles projectFiles, string outputFilename)
        {
            var jsText = $"{JsPrefix}{JsonUtility.ToJson(projectFiles, TestCountLimiter > 0)};";
            File.WriteAllText(outputFilename, jsText, Encoding);
        }

        private static List<string> GetProjectFilesListFromMetaFiles()
        {
            const string assetFolder = "Assets";
            var folderName = Path.GetFullPath(assetFolder);
            var prefixLen = folderName.Length - assetFolder.Length;
            var folderMetaFiles = Directory.GetFiles(folderName, "*.meta", SearchOption.AllDirectories);
            var result = new List<string>();
            foreach (var metaFile in folderMetaFiles)
            {
                var fullPath = metaFile[..^5];
                if (Directory.Exists(fullPath))
                {
                    continue;
                }
                var filename = fullPath.Substring(prefixLen, fullPath.Length - prefixLen);
                result.Add(filename.Replace('\\', '/'));
                if (TestCountLimiter > 0 && result.Count >= TestCountLimiter)
                {
                    break;
                }
            }
            return result;
        }
    }

    [Flags]
    public enum FileCategory
    {
        UnUsed = 0,
        UsedInLog = 1,
        UsedInReport = 2,
        UsedInBoth = 3,
    }

    [SuppressMessage("ReSharper", "FieldCanBeMadeReadOnly.Global")]
    public class ProjectFiles
    {
        public long TotalFileSize;
        public long UnUsedFileSize;
        public List<ProjectFile> Lines = new();

        public ProjectFiles()
        {
        }
    }

    [Serializable, SuppressMessage("ReSharper", "InconsistentNaming")]
    public class ProjectFile
    {
        public string FilePath;
        public long FileSize;
        public FileCategory FileCategory;

        public ProjectFile()
        {
        }
    }
}
