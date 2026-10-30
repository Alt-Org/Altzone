using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Prg;
using Prg.Localization;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
#if PRG_DEBUG
using Prg.Util;
using Debug = Prg.Debug;
#else
using Debug = UnityEngine.Debug;
#endif

namespace Editor.Prg.Localization
{
    /// <summary>
    /// UNITY Editor menu commands for localization.
    /// </summary>
    public static class LocalizationMenu
    {
        private static bool _hasLogger;

        [Conditional("PRG_DEBUG")]
        private static void SetEditorEnvironment()
        {
#if PRG_DEBUG
            if (string.IsNullOrEmpty(Localized.NoLocalizeTag))
            {
                // We can not reference Tags.NoLocalize here :-(
                Localized.NoLocalizeTag = "NoLocalize";
            }
            if (_hasLogger)
            {
                return;
            }
            _hasLogger = true;
            LogConfig.Create();
            LogConfig.ForceLogging(
                typeof(CheckLocalization),
                typeof(LocaleWords),
                typeof(LocalizedEditor),
                typeof(Localizer));
            LogFileWriter.CreateLogFileWriter();
#endif
        }

        private const string MenuRoot = "Prg/";
        private const string MenuItemRoot = MenuRoot + "Localization/";

        private const string AddLocalizationName = "Add Prefab Localization!";
        private const string UpdateLocalizationName = "Update Prefab Localization!";
        private const string RemoveLocalizationName = "Remove Prefab Localization!";
        private const string AddNoLocalizeTagName = "Add Prefab NoLocalize Tag!";

        private const string LocalizationReportClipboardName = "Create Report to Clipboard";
        private const string LocalizationReportCsvSelected = "Create Report to _local tsv (Selected)";
        private const string LocalizationReportCsvFull = "Create Report to _local tsv (All)";

        private const string OpenLocalizationsTsvName = "Open Game Localizations TSV file";
        private const string CopyMissingKeysClipboardName = "Copy Missing Keys to Clipboard";
        private const string CopyMissingKeysCsvName = "Copy Missing Keys to _local tsv";
        private const string LoadLocalizationsName = "Load Game Localizations from TSV File";

        [MenuItem(MenuItemRoot + AddLocalizationName, true, 0)]
        private static bool MenuAddLocalizationEnabled() => Selection.gameObjects.Length > 0;

        // Group 10

        [MenuItem(MenuItemRoot + AddLocalizationName, false, 10)]
        private static void MenuAddLocalization()
        {
            SetEditorEnvironment();
            Debug.Log("*");
            foreach (var gameObject in Selection.gameObjects)
            {
                LocalizedEditorUtil.AddLocalization(gameObject);
            }
        }

        [MenuItem(MenuItemRoot + UpdateLocalizationName, true, 0)]
        private static bool MenuUpdateLocalizationEnabled() => Selection.gameObjects.Length > 0;

        [MenuItem(MenuItemRoot + UpdateLocalizationName, false, 11)]
        private static void MenuUpdateLocalization()
        {
            SetEditorEnvironment();
            Debug.Log("*");
            foreach (var gameObject in Selection.gameObjects)
            {
                LocalizedEditorUtil.UpdateLocalization(gameObject);
            }
        }

        [MenuItem(MenuItemRoot + RemoveLocalizationName, true, 0)]
        private static bool RemoveLocalizationEnabled() => Selection.gameObjects.Length > 0;

        [MenuItem(MenuItemRoot + RemoveLocalizationName, false, 12)]
        private static void RemoveLocalization()
        {
            SetEditorEnvironment();
            Debug.Log("*");
            foreach (var gameObject in Selection.gameObjects)
            {
                LocalizedEditorUtil.RemoveLocalization(gameObject);
            }
        }

        [MenuItem(MenuItemRoot + AddNoLocalizeTagName, true, 0)]
        private static bool AddNoLocalizeTagEnabled() => Selection.gameObjects.Length > 0;

        [MenuItem(MenuItemRoot + AddNoLocalizeTagName, false, 13)]
        private static void AddNoLocalizeTag()
        {
            SetEditorEnvironment();
            Debug.Log("*");
            foreach (var gameObject in Selection.gameObjects)
            {
                LocalizedEditorUtil.AddNoLocalizeTag(gameObject);
            }
        }

        // Group 30

        [MenuItem(MenuItemRoot + LocalizationReportClipboardName, true, 0)]
        private static bool LocalizationReportClipboardEnabled() => Selection.assetGUIDs.Length > 0;

        [MenuItem(MenuItemRoot + LocalizationReportClipboardName, false, 30)]
        private static void LocalizationReportClipboard()
        {
            SetEditorEnvironment();
            Debug.Log("*");
            CheckLocalization.CheckLocalizationInPrefabs(Selection.assetGUIDs, isCsvReport: false);
        }

        [MenuItem(MenuItemRoot + LocalizationReportCsvSelected, true, 0)]
        private static bool LocalizationReportCsvLocalEnabled() => Selection.assetGUIDs.Length > 0;

        [MenuItem(MenuItemRoot + LocalizationReportCsvSelected, false, 31)]
        private static void LocalizationReportCsvLocal()
        {
            SetEditorEnvironment();
            Debug.Log("*");
            CheckLocalization.CheckLocalizationInPrefabs(Selection.assetGUIDs, isCsvReport: true);
        }

        [MenuItem(MenuItemRoot + LocalizationReportCsvFull, true, 0)]
        private static bool LocalizationReportCsvWorkEnabled() => !Application.isPlaying && Selection.assetGUIDs.Length > 0;

        [MenuItem(MenuItemRoot + LocalizationReportCsvFull, false, 32)]
        private static void LocalizationReportCsvWork()
        {
            SetEditorEnvironment();
            Debug.Log("*");
            CheckLocalization.CheckLocalizationInAllPrefabs();
        }

        // Group 50

        [MenuItem(MenuItemRoot + OpenLocalizationsTsvName, true, 0)]
        private static bool OpenLocalizationsTsvEnabled() => !Application.isPlaying;

        [MenuItem(MenuItemRoot + OpenLocalizationsTsvName, false, 50)]
        private static void OpenLocalizationsTsv()
        {
            SetEditorEnvironment();
            Debug.Log("*");
            var path = Localizer.LocalizationTsvFilename;
            if (!File.Exists(path))
            {
                Debug.Log($"{RichText.Brown("FILE NOT FOUND")}: {path}");
                return;
            }
            Process.Start(path);
        }

        [MenuItem(MenuItemRoot + CopyMissingKeysClipboardName, true, 0)]
        private static bool CopyMissingKeysClipboardEnabled() => Localizer.HasMissingKeys;

        [MenuItem(MenuItemRoot + CopyMissingKeysClipboardName, false, 51)]
        private static void CopyMissingKeysClipboard()
        {
            SetEditorEnvironment();
            Debug.Log("*");
            CheckLocalization.WriteReport(GetMissingKeys(forClipboard: true), isCsvReport: false);
        }

        [MenuItem(MenuItemRoot + CopyMissingKeysCsvName, true, 0)]
        private static bool CopyMissingKeysCsvEnabled() => Localizer.HasMissingKeys;

        [MenuItem(MenuItemRoot + CopyMissingKeysCsvName, false, 52)]
        private static void CopyMissingKeysCsv()
        {
            SetEditorEnvironment();
            Debug.Log("*");
            CheckLocalization.WriteReport(GetMissingKeys(), isCsvReport: true);
        }

        [MenuItem(MenuItemRoot + LoadLocalizationsName, true, 0)]
        private static bool TsvLocalizationsEnabled() => !Application.isPlaying;

        [MenuItem(MenuItemRoot + LoadLocalizationsName, false, 59)]
        private static void TsvLocalizations()
        {
            SetEditorEnvironment();
            Debug.Log("*");
            var localeWords = LocaleWords.Get();
            localeWords.SaveWordsInEditor(Localizer.LocalizationTsvFilename);
        }

        private static string GetMissingKeys(bool forClipboard = false)
        {
            var keys = Localizer.GetMissingKeys();
            Debug.Log($"key count {keys.Count}");
            var builder = new StringBuilder();
            foreach (var key in keys.OrderBy(x => x))
            {
                Assert.IsFalse(key.Contains('\r'), $"key must not contain CR character: {key}");
                // Unfortunately it seems that pasting text from clipboard causes LF to CR-LF expansion.
                var keyText = forClipboard
                    ? key.Replace("\n", "\n@\t")
                    : key;
                builder.Append(keyText).Append('\t').AppendLine();
            }
            return builder.ToString();
        }
    }
}
