using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Prg;
using Prg.Localization;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;
using Debug = Prg.Debug;
using FileUtil = Prg.Util.FileUtil;

namespace PrgEditor.Localization
{
    /// <summary>
    /// A helper editor script for finding components used in <c>i18n</c> and <c>l10n</c>.<br />
    /// </summary>
    /// <remarks>
    /// Checks only <b>prefabs</b> to keep process focused.
    /// </remarks>
    public class CheckLocalization
    {
        private class Counters
        {
            public int Localized;
            public int TextMeshProUGUI;
            public int TextMeshPro;
            public int Button;
            public int Toggle;
            public int NoLocalize;
            public int Debug;
            public int Test;
            public int Deprecated;

            public override string ToString()
            {
                return
                    $"{nameof(Localized)}: {Localized}, {nameof(TextMeshProUGUI)}: {TextMeshProUGUI}, {nameof(TextMeshPro)}: {TextMeshPro}" +
                    $", {nameof(Button)}: {Button}, {nameof(Toggle)}: {Toggle}, {nameof(NoLocalize)}: {NoLocalize}" +
                    $", {nameof(Debug)}: {Debug}, {nameof(Test)}: {Test}, {nameof(Deprecated)}: {Deprecated}";
            }
        }

        /// <summary>
        /// Unique key, value pair.
        /// </summary>
        private class Word
        {
            private static readonly Regex CountDotsRegex = new(@"\.",
                RegexOptions.Multiline | RegexOptions.CultureInvariant | RegexOptions.Compiled);

            // Keep tags in alphabetical order as they are sorted by key,tag.
            public const string TagKey = ".KEY";
            public const string TagText = "text";

            public readonly string Key;
            public readonly int TokenCount;
            public readonly string Value;
            public readonly string Tag;

            public Word(string key, string value, string tag)
            {
                Key = key;
                TokenCount = 1 + CountDotsRegex.Matches(Key).Count;
                Value = EscapeText(value);
                Tag = tag;
            }

            public bool KeyMatchWithText(Word textWord)
            {
                Assert.AreEqual(TagKey, Tag);
                Assert.AreEqual(TagText, textWord.Tag);
                if (Value != textWord.Value)
                {
                    return false;
                }
                // Keys match or text Key is longer than this Key (must be button or toggle on level higher).
                var isMatch = Key == textWord.Key;
                if (isMatch)
                {
                    return true;
                }
                var isStart = textWord.Key.StartsWith($"{Key}.");
                return isStart;
            }

            private static string EscapeText(string text)
            {
                Assert.IsFalse(text.Contains('\t'), $"do not add TAB in text: {text}");
                var needsEscape = text.Contains('\n') || text.Contains('\r');
                return needsEscape
                    ? $"\"{text}\""
                    : text;
            }
        }

        public static readonly string TempFilename =
            Path.Combine(".", "etc", "localization", "_local_localizations.tsv");

        private const string Context = "I18n";

        private static HashSet<string> _keys;
        private static HashSet<Word> _words;

        public static void CheckLocalizationInAllPrefabs()
        {
            var allPrefabs = AssetDatabase.GetAllAssetPaths()
                .Where(path => path.StartsWith("Assets/") && path.EndsWith(".prefab"))
                .OrderBy(x => x)
                .ToList();
            var gameObjects = allPrefabs.Select(a =>
                    AssetDatabase.LoadAssetAtPath(a, typeof(GameObject)) as GameObject).Where(a => a != null)
                .ToList();
            var report = CreateReport(gameObjects, isLogVerbose: false);
            WriteReport(report, isCsvReport: true);
        }

        public static void CheckLocalizationInPrefabs(string[] selectedGuids, bool isCsvReport)
        {
            var gameObjects = new List<GameObject>();
            foreach (var guid in selectedGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith(".prefab"))
                {
                    continue;
                }
                var asset = AssetDatabase.LoadMainAssetAtPath(path) as GameObject;
                if (asset == null)
                {
                    continue;
                }
                gameObjects.Add(asset);
            }
            var report = CreateReport(gameObjects);
            WriteReport(report, isCsvReport);
        }

        public static void CheckLocalizationInGameObject(GameObject go, bool isCsvReport)
        {
            var gameObjects = new List<GameObject>
            {
                go
            };
            var report = CreateReport(gameObjects);
            WriteReport(report, isCsvReport);
        }

        public static void CheckLocalizationInGameObjects(GameObject[] array, bool isCsvReport)
        {
            var report = CreateReport(array.ToList());
            WriteReport(report, isCsvReport);
        }

        public static void WriteReport(string report, bool isCsvReport)
        {
            if (!isCsvReport)
            {
                EditorGUIUtility.systemCopyBuffer = report;
                return;
            }
            var filename = TempFilename;
            try
            {
                File.WriteAllText(filename, report, FileUtil.Encoding);
            }
            catch (Exception)
            {
                Debug.Log($"Unable to write file: {RichText.Yellow(filename)}");
                Debug.Log(RichText.White("Report copied to Clipboard"));
                EditorGUIUtility.systemCopyBuffer = report;
                throw;
            }
        }

        private static string CreateReport(List<GameObject> gameObjects, bool isLogVerbose = true)
        {
            _keys = new HashSet<string>();
            _words = new HashSet<Word>();
            var counters = new Counters();
            var totalCount = FindReferences(gameObjects, counters);
            totalCount -= (counters.NoLocalize + counters.Debug);
            Debug.Log($"{counters}");
            Debug.Log($"localizeCount {totalCount}, noLocalize {counters.NoLocalize}");
            if (counters.Debug > 0 || counters.Test > 0)
            {
                Debug.Log($"debug {counters.Debug}, test {counters.Test}");
            }
            return WriteReport(isLogVerbose);
        }

        private static string WriteReport(bool isLogVerbose)
        {
            if (isLogVerbose)
            {
                Debug.Log($"*");
            }
            // Sort word on key, tag order.
            var words = new List<Word>(_words)
                .OrderBy(x => x.Key)
                .ThenBy(x => x.Tag);
            var builder = new StringBuilder("key\tphrase\tTYPE\ttokens");
            var minKeys = int.MaxValue;
            var maxKeys = 0;
            var textLines = 0;
            var keyLines = 0;
            var matchedLines = 0;
            var prevKeyWord = new Word("\r", "\n", Word.TagKey);
            foreach (var word in words)
            {
                if (word.TokenCount < minKeys)
                {
                    minKeys = word.TokenCount;
                }

                if (word.TokenCount > maxKeys)
                {
                    maxKeys = word.TokenCount;
                }
                if (word.Tag == Word.TagKey)
                {
                    prevKeyWord = word;
                    keyLines += 1;
                }
                else
                {
                    textLines += 1;
                    if (prevKeyWord.KeyMatchWithText(word))
                    {
                        // Key + text pair, skip text
                        matchedLines += 1;
                        continue;
                    }
                }
                if (isLogVerbose)
                {
                    Debug.Log($"{word.Key.Length:00} {word.TokenCount} {word.Tag} {word.Key} | {WordToLog(word.Value)}");
                }
                // This is easier to read but copying localization data (key, value) needs to select two rightmost columns now.
                builder.AppendLine()
                    .Append($"{word.Key.Length:00}\t{word.TokenCount}\t{word.Tag}\t{word.Key}\t{word.Value}");
            }
            Debug.Log($"key parts between {minKeys} and {maxKeys}");
            Debug.Log(
                $"keyLines {keyLines}, textLines {textLines}, matchedLines {matchedLines}, missing {textLines - keyLines}");
            return builder.ToString();
        }

        private static void AddLocalizationEntry(Localized localized)
        {
            var key = localized._key;
            var value = localized._defaultText;
            var equalityValue = $"{key}.{value}";
            if (!_keys.Add(equalityValue))
            {
                return;
            }
            _words.Add(new Word(key, value, Word.TagKey));
        }

        private static void AddLocalizationEntry(TMP_Text text)
        {
            var key = Localized.KeyFrom(text);
            var value = text.text;
            var equalityValue = $"{key}.{value}";
            if (!_keys.Add(equalityValue))
            {
                return;
            }
            _words.Add(new Word(key, value, Word.TagText));
        }

        private static int FindReferences(List<GameObject> gameObjects, Counters counters)
        {
            if (gameObjects.Count > 0)
            {
                Debug.Log($"entries #{gameObjects.Count}");
            }
            var foundCount = 0;
            foreach (var gameObject in gameObjects)
            {
                foundCount += FindReferences(gameObject, counters);
            }
            return foundCount;
        }

        private static int FindReferences(GameObject root, Counters counters)
        {
            var foundCount = 0;
            var allComponents = root.GetComponentsInChildren<Component>(includeInactive: true);
            foreach (var component in allComponents)
            {
                // Missing components will be null, we can't find their type, etc.
                if (!component)
                {
                    continue;
                }
                var wasFound = false;
                var gameObject = component.gameObject;
                if (component is Localized localized)
                {
                    if (gameObject.CompareTag(Localized.NoLocalizeTag))
                    {
                        counters.NoLocalize += 1;
                        continue;
                    }
                    LogIsFound(Context, gameObject, $"[KEY] '{localized._key}'");
                    counters.Localized += 1;
                    wasFound = true;
                    AddLocalizationEntry(localized);
                }
                else if (component is TMP_Text text)
                {
                    if (gameObject.CompareTag(Localized.NoLocalizeTag))
                    {
                        counters.NoLocalize += 1;
                        continue;
                    }
                    var isTextMeshProUGUI = component.GetType() == typeof(TextMeshProUGUI);
                    var role = isTextMeshProUGUI ? "Canvas" : "World";
                    LogIsFound(Context, gameObject, $"[TEXT] {role} '{WordToLog(text.text)}'");
                    if (isTextMeshProUGUI)
                    {
                        counters.TextMeshProUGUI += 1;
                    }
                    else
                    {
                        counters.TextMeshPro += 1;
                    }
                    wasFound = true;
                    AddLocalizationEntry(text);
                }
                else if (component is Button)
                {
                    if (gameObject.CompareTag(Localized.NoLocalizeTag))
                    {
                        counters.NoLocalize += 1;
                        continue;
                    }
                    LogIsFound(Context, gameObject, $"Button");
                    counters.Button += 1;
                    wasFound = true;
                }
                else if (component is Toggle)
                {
                    if (gameObject.CompareTag(Localized.NoLocalizeTag))
                    {
                        counters.NoLocalize += 1;
                        continue;
                    }
                    LogIsFound(Context, gameObject, $"Toggle");
                    counters.Toggle += 1;
                    wasFound = true;
                }
                else if (component is Text oldText)
                {
                    LogIsFound(Context, gameObject, $"{RichText.Red("Text")}: '{WordToLog(oldText.text)}'");
                    counters.Deprecated += 1;
                    wasFound = true;
                }
                if (wasFound)
                {
                    foundCount += 1;
                    var path = gameObject.GetFullPath().ToLower();
                    if (path.Contains("debug"))
                    {
                        counters.Debug += 1;
                    }
                    else if (path.Contains("test"))
                    {
                        counters.Test += 1;
                    }
                }
            }
            return foundCount;
        }

        private static string WordToLog(string word) => word.Replace('\r', '_').Replace('\n', '_');

        private static void LogIsFound(string context, GameObject gameObject, string message)
        {
            Debug.Log($"{RichText.Green("FOUND")}: [{context}] {gameObject.GetFullPath()}: " +
                      $"{message}", gameObject);
        }
    }
}
