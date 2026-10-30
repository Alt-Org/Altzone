using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Prg;
using Prg.Localization;
using Prg.Util;
using UnityEditor;
using UnityEngine;
#if PRG_DEBUG
using Debug = Prg.Debug;
#else
using Debug = UnityEngine.Debug;
#endif
using FileUtil = Prg.Util.FileUtil;

namespace Editor.Prg.Localization
{
    [CustomEditor(typeof(LocaleWords))]
    public class LocaleWordsEditor : UnityEditor.Editor
    {
        private static bool _hasLogger;

        [Conditional("PRG_DEBUG")]
        private static void SetEditorEnvironment()
        {
#if PRG_DEBUG
            if (_hasLogger)
            {
                return;
            }
            _hasLogger = true;
            _hasLogger = true;
            LogConfig.Create();
            LogConfig.ForceLogging(
                typeof(LocaleWords));
            LogFileWriter.CreateLogFileWriter();
#endif
        }

        public override void OnInspectorGUI()
        {
            if (serializedObject.isEditingMultipleObjects)
            {
                DrawDefaultInspector();
                return;
            }
            GUILayout.Space(5);
            if (GUILayout.Button("(1) Open Localizations Tsv file"))
            {
                Debug.Log("*");
                var path = Localizer.LocalizationTsvFilename;
                if (!File.Exists(path))
                {
                    Debug.Log($"{RichText.Brown("FILE NOT FOUND")}: {path}");
                    return;
                }
                Process.Start(path);
            }
            GUILayout.Space(5);
            if (GUILayout.Button("(2) Load Localizations from TSV File"))
            {
                if (serializedObject.targetObject is LocaleWords localeWords)
                {
                    SetEditorEnvironment();
                    localeWords.SaveWordsInEditor(Localizer.LocalizationTsvFilename);
                    EditorUtility.SetDirty(serializedObject.targetObject);
                    AssetDatabase.SaveAssets();
                }
            }
            GUILayout.Space(5);
            if (GUILayout.Button("(3) Check Missing values for Locales"))
            {
                if (serializedObject.targetObject is LocaleWords localeWords)
                {
                    SetEditorEnvironment();
                    localeWords.CheckMissingValues();
                }
            }
            GUILayout.Space(20);
            DrawDefaultInspector();
        }
    }

    public static class LocaleWordsExtensions
    {
        public static void CheckMissingValues(this LocaleWords asset)
        {
            var localeWords = asset.LoadWords();
            foreach (var locale in localeWords.Keys)
            {
                var missingValueCount = 0;
                foreach (var pair in localeWords[locale])
                {
                    if (!string.IsNullOrEmpty(pair.Value))
                    {
                        continue;
                    }
                    missingValueCount += 1;
                    Debug.LogWarning($"locale {locale} missing value for {pair.Key}");
                }
                if (missingValueCount == 0)
                {
                    continue;
                }
                Debug.LogWarning($"locale {locale} missing values count is {missingValueCount}");
            }
        }

        public static void SaveWordsInEditor(this LocaleWords asset, string localizationTsvFilename)
        {
            // Content has both CR-LF and plain LF 'line end' characters to format multiline columns properly.
            // First line contains locale codes starting from column 1.
            // There is not specifix comment characters at start of the line!
            // First token (key) can end with comment char to allow 'section headers'

            const char tab = '\t';
            const char keyEndComment = '#';

            var tsvFileContent = FileUtil.ReadAllText(localizationTsvFilename);

            // Get Locales from first line.
            var localeCodes = new List<string>();
            var lines = tsvFileContent.Split("\r\n");
            {
                // Read locales from left to right until empty colum or eol.
                var localeColumns = lines[0].Split(tab);
                var isEmptyFound = false;
                for (var i = 1; i < localeColumns.Length; ++i)
                {
                    var localeCode = localeColumns[i];
                    if (string.IsNullOrEmpty(localeCode))
                    {
                        isEmptyFound = true;
                        continue;
                    }
                    if (isEmptyFound)
                    {
                        throw new UnityException($"invalid empty columns in first line: '{lines[0]}'");
                    }
                    localeCodes.Add(localeCode);
                }
            }

            // Add word dictionary for each locale.
            var localeWords = new Dictionary<string, Dictionary<string, string>>();
            foreach (var localeCode in localeCodes)
            {
                localeWords.Add(localeCode, new Dictionary<string, string>());
            }

            LoadWords();
            asset.SaveWords(localeWords);
            return;

            void LoadWords()
            {
                var localeCount = localeCodes.Count;
                var errorCount = 0;
                for (var i = 1; i < lines.Length; ++i)
                {
                    var line = lines[i];
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        // Skip empty lines.
                        continue;
                    }
                    var words = line.Split(tab);
                    if (words.Length < 1 + localeCount)
                    {
                        Debug.LogError($"invalid line {i}: '{line}'");
                        errorCount += 1;
                        continue;
                    }
                    try
                    {
                        var key = words[0];
                        if (key.EndsWith(keyEndComment))
                        {
                            continue;
                        }
                        // Read only those columns that have a locale code.
                        for (var index = 1; index <= localeCount; ++index)
                        {
                            // words are 1 based, empty words are added!
                            var localizedWord = words[index];
                            if (string.IsNullOrEmpty(localizedWord))
                            {
                                localizedWord = "";
                            }
                            else
                            {
                                if (localizedWord.StartsWith('"') && localizedWord.EndsWith('"'))
                                {
                                    localizedWord = localizedWord.Length == 2
                                        ? ""
                                        : localizedWord.Substring(1, localizedWord.Length - 2);
                                }
                                if (localizedWord.Contains('\n'))
                                {
                                    // Add proper CR-LF.
                                    localizedWord = localizedWord.Replace("\n", "\r\n");
                                }
                            }
                            // localeCodes is zero based!
                            var localeCode = localeCodes[index - 1];
                            localeWords[localeCode].Add(key, localizedWord);
                        }
                    }
                    catch (Exception x)
                    {
                        Debug.LogError($"line {i} {x.GetType().Name}: {x.Message}");
                        errorCount += 1;
                    }
                }
                if (errorCount > 0)
                {
                    Debug.LogWarning($"errors {errorCount}");
                    for (var i = 1; i < lines.Length; ++i)
                    {
                        var line = lines[i].Replace('\r', '.').Replace('\n', '.');
                        Debug.Log($"{i,-4} {line}");
                    }
                }
            }
        }

        private static void SaveWords(this LocaleWords asset, Dictionary<string, Dictionary<string, string>> localeWords)
        {
            var localeCodes = localeWords.Keys.ToList();
            var leftMostLocale = localeCodes[0];
            // Keys.
            asset.Keys = new ListItems
            {
                Name = "Keys",
                Items = localeWords[leftMostLocale].Keys.ToList()
            };
            var keyCount = asset.Keys.Items.Count;
            Debug.Log($"keyCount {keyCount}, localeCount {localeCodes.Count}");

            // Words.
            asset.Words = new List<ListItems>();
            foreach (var localeCode in localeCodes)
            {
                var words = localeWords[localeCode];
                var listItems = new ListItems
                {
                    Name = localeCode,
                    Items = words.Values.ToList()
                };
                asset.Words.Add(listItems);
                if (listItems.Items.Count != keyCount)
                {
                    Debug.LogError($"word count {listItems.Items.Count} for locale {listItems.Name} does not match key count {keyCount}");
                }
            }
            Debug.LogWarning($"total keys {asset.Keys.Items.Count}");
        }
    }
}
