using System;
using System.Diagnostics;
using Prg.Scripts.Common.Util;
using UnityEditor;

namespace Prg.Editor.BatchBuild
{
    internal static class BatchBuildMenu
    {
        private const string MenuRoot = "Prg/";
        private const string MenuItem = MenuRoot + "Build/";

        [MenuItem(MenuItem + "Show Build Report in browser", false, 10)]
        private static void HtmlBuildReportBrowser() => Logged(() => BuildReportAnalyzer.HtmlBuildReportFast());

        [MenuItem(MenuItem + "Show Build Report with unused Assets", false, 11)]
        private static void HtmlBuildReportBrowserFull() => Logged(() => BuildReportAnalyzer.HtmlBuildReportFull());

        [MenuItem(MenuItem + @"Delete Old Build Reports", false, 12)]
        private static void DeleteOldBuildReports() =>
            Logged(() => UnityBuildReport.DeleteOldReports(DateTime.Today, true));

        [MenuItem(MenuItem + @"Test .\etc\secretKeys Folder", false, 13)]
        private static void TestDumpSecretKeysFolder() => Logged(() =>
        {
            foreach (var buildTarget in new[]
                         { BuildTarget.Android, BuildTarget.WebGL, BuildTarget.StandaloneWindows64 })
            {
                Debug.Log("*");
                try
                {
                    var secretKeys = BatchBuild.LoadSecretKeys(@".\etc\secretKeys", buildTarget);
                    Debug.Log($"* BuildTarget {buildTarget}: keys {secretKeys.Count}");
                    foreach (var pair in secretKeys)
                    {
                        Debug.Log($"{RichText.White(pair.Key)}={pair.Value}");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"* BuildTarget {buildTarget}: {RichText.Red(e.Message)}");
                }
            }
        });

        private static void Logged(Action action)
        {
            LogFileWriter.CreateLogFileWriter();
            var stopwatch = Stopwatch.StartNew();
            action();
            stopwatch.Stop();
            Debug.Log($"Command took {stopwatch.Elapsed.TotalSeconds:0.0} s");
        }
    }
}
