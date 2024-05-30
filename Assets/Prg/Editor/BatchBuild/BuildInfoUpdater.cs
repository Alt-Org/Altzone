using System;
using System.IO;
using System.Text;
using Prg.Scripts.Common.Util;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace Prg.Editor.BatchBuild
{
    /// <summary>
    /// Utility to update <b>BuildInfo.cs</b> C# source file for latest build info by rewriting it on the file system.
    /// </summary>
    public static class BuildInfoUpdater
    {
        private const string BuildInfoFilename = "BuildInfo.cs";

        private static readonly Encoding Encoding = PlatformUtil.Encoding;

#if false
        [MenuItem("Prg/Write SourceCode Changes", false, 10)]
        public static void WriteSourceCodeChanges()
        {
            UpdateFile(PlayerSettings.Android.bundleVersionCode, "");
        }
#endif

        public static void UpdateFile(int bundleVersionCode, string buildTagOrLabel = null)
        {
            const string tagBundleVersionCodeValue = "BundleVersionCodeValue = \"";
            const string tagPatchValue = "PatchValue = \"";
            const string tagCompiledOnDateValue = "CompiledOnDateValue = \"";
            const string tagBuildTagOrLabelValue = "BuildTagOrLabelValue = \"";
            const string tagIsMuteOtherAudioSourcesValue = "IsMuteOtherAudioSourcesValue = \"";
            const string endTag = "\";";

            var sourceCodeFilename = FindFile();
            Assert.IsNotNull(sourceCodeFilename);
            var oldSource = File.ReadAllText(sourceCodeFilename, Encoding);
            var index1 = 0;
            int index2;
            var newSource = oldSource;

            // Update BundleVersionCode
            var bundleVersionText = bundleVersionCode.ToString();
            var isResetPatch = false;
            if (UpdateIndexesFor(tagBundleVersionCodeValue))
            {
                var bundleText = GetCurrentText();
                if (bundleText != bundleVersionText)
                {
                    isResetPatch = true;
                    ReplaceCurrentTextWith(int.Parse(bundleVersionText).ToString());
                }
            }
            // Update Patch
            if (UpdateIndexesFor(tagPatchValue))
            {
                if (isResetPatch)
                {
                    ReplaceCurrentTextWith("0");
                }
                else
                {
                    var patchText = GetCurrentText();
                    if (int.TryParse(patchText, out var patchValue))
                    {
                        patchValue += 1;
                        ReplaceCurrentTextWith(patchValue.ToString());
                    }
                }
            }
            // Update CompiledOnDate
            if (UpdateIndexesFor(tagCompiledOnDateValue))
            {
                ReplaceCurrentTextWith(DateTime.Now.FormatMinutes());
            }
            // Update BuildTagOrLabel (if given, but can be empty)
            if (buildTagOrLabel != null)
            {
                UpdateIndexesFor(tagBuildTagOrLabelValue);
                if (index2 >= index1 && index1 >= 0)
                {
                    ReplaceCurrentTextWith(buildTagOrLabel);
                }
            }
            // Update (mobile) IsMuteOtherAudioSources.
            // https://docs.unity3d.com/ScriptReference/PlayerSettings-muteOtherAudioSources.html
#if UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_WIN
            if (UpdateIndexesFor(tagIsMuteOtherAudioSourcesValue))
            {
                ReplaceCurrentTextWith(PlayerSettings.muteOtherAudioSources.ToString());
            }
#endif
            if (newSource == oldSource)
            {
                return;
            }
            File.WriteAllText(sourceCodeFilename, newSource, Encoding);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            return;

            bool UpdateIndexesFor(string startTag)
            {
                index1 = newSource.IndexOf(startTag, index1, StringComparison.Ordinal);
                if (index1 == -1)
                {
                    index2 = -1;
                    return false;
                }
                index2 = newSource.IndexOf(endTag, index1, StringComparison.Ordinal);
                if (index2 == -1)
                {
                    return false;
                }
                index1 += startTag.Length;
                return true;
            }

            string GetCurrentText()
            {
                return index2 > index1 ? newSource.Substring(index1, index2 - index1) : string.Empty;
            }

            void ReplaceCurrentTextWith(string newText)
            {
                var part3 = GetCurrentText();
                if (part3 == newText)
                {
                    return;
                }
                var part1 = newSource[..index1];
                var part2 = newSource[index2..];
                newSource = part1 + newText + part2;
            }

            string FindFile()
            {
                // Try to find 'build info' file in the project by its filename.
                string filePath = null;
                var files = Directory.GetFiles(Application.dataPath, BuildInfoFilename,
                    SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    if (file.EndsWith(BuildInfoFilename))
                    {
                        filePath = AppPlatform.IsWindows ? AppPlatform.ConvertToWindowsPath(file) : file;
                        break;
                    }
                }
                return filePath;
            }
        }
    }
}
