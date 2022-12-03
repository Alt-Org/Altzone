using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Prg.Scripts.Common.Unity.Localization
{
    #region Language implementation

    /// <summary>
    /// Dictionary for single localized language.
    /// </summary>
    internal class Language
    {
        private readonly Dictionary<string, string> _words;
        private readonly Dictionary<string, string> _altWords;

        public SystemLanguage LanguageName { get; }

        public string Locale { get; }

        internal Dictionary<string, string> Words => _words;
        internal Dictionary<string, string> AltWords => _altWords;

#if UNITY_EDITOR && false
        public string Word(string key)
        {
            string result = string.Empty;
            string source = string.Empty;
            if (_words.TryGetValue(key, out var value))
            {
                result = value;
                source = "ok";
            }
            else if (_altWords.TryGetValue(key, out var altValue))
            {
                result = altValue;
                source = "alt";
            }
            else
            {
              result = $"[{key}]";
              source = "NOT FOUND";
            }
            Debug.Log($"Word {Locale} {key} <- {result} {source}");
            return result;
        }
#else
        public string Word(string key) =>
            _words.TryGetValue(key, out var value) ? value
            : _altWords.TryGetValue(key, out var altValue) ? altValue
            : $"[{key}]";
#endif
        public Language(SystemLanguage language, string localeName = null,
            Dictionary<string, string> words = null,
            Dictionary<string, string> altWords = null)
        {
            LanguageName = language;
            Locale = localeName ?? "xx";
            _words = words ?? new Dictionary<string, string>();
            _altWords = altWords ?? new Dictionary<string, string>();
        }

        public override string ToString()
        {
            return $"{LanguageName} [{Locale}] Words: {_words?.Count}/{_altWords?.Count}";
        }

        #region Localization process in Editor

#if UNITY_EDITOR
        private const int OkKey = 0;
        private const int NoKey = 1;
        private const int NoText = 2;
        private const int AltText = 3;

        private readonly string[] _reasonTexts = { "OK", "NO_KEY", "NO_TEXT", "ALT_TEXT" };

        private Dictionary<string, Tuple<string, int>> _debugWords;
        private HashSet<string> _usedWords;

        internal int TrackWords(SmartText component, string key, string word)
        {
            var hasWord = _words.ContainsKey(key);
            if (hasWord)
            {
                if (_usedWords == null)
                {
                    _usedWords = new HashSet<string>();
                }
                _usedWords.Add(key);
                return OkKey;
            }
            var isNoKey = string.IsNullOrWhiteSpace(key);
            var isMissing = string.IsNullOrEmpty(word) || (word.StartsWith("[") && word.EndsWith("]"));
            var reasonIndex = isNoKey ? NoKey : isMissing ? NoText : AltText;
            var reason = _reasonTexts[reasonIndex];
            var text = component.GetComponent<Text>().text;
            var componentName = component.ComponentName;
            if (isNoKey)
            {
                key = componentName;
            }
            if (isMissing)
            {
                word = text;
            }
            if (_debugWords == null)
            {
                _debugWords = new Dictionary<string, Tuple<string, int>>();
            }
            if (!_debugWords.TryGetValue(key, out var tuple))
            {
                Debug.Log($"{Locale} {reason} {componentName} key={key} word={word} text={text}");
                _debugWords.Add(key, new Tuple<string, int>(word, reasonIndex));
                return reasonIndex;
            }
            if (tuple.Item1 != word)
            {
                // Duplicate key with different text!
                key = $"{component.GetFullPath()}_{_debugWords.Count}";
                Debug.Log($"{Locale} {reason} {componentName} key={key} word={word} text={text}");
                _debugWords.Add(key, new Tuple<string, int>(word, reasonIndex));
            }
            return reasonIndex;
        }

        /// <summary>
        /// Save current words and new words found during app execution.
        /// </summary>
        internal void SaveIfDirty()
        {
            if (_debugWords == null)
            {
                return;
            }
            var builder = new StringBuilder();
            {
                // Add current words "as is".
                foreach (var entry in _words)
                {
                    var used = _usedWords.Contains(entry.Key) ? "\tIN_USE" : "";
                    builder.Append(entry.Key).Append('\t')
                        .Append(entry.Value)
                        .Append(used).AppendLine();
                }
                // Sort "new" words by category.
                foreach (var item2 in new[] { NoKey, NoText, AltText })
                {
                    foreach (var entry in _debugWords)
                    {
                        if (entry.Value.Item2 != item2)
                        {
                            continue;
                        }
                        builder.Append(entry.Key).Append('\t')
                            .Append(entry.Value.Item1).Append('\t')
                            .Append(_reasonTexts[entry.Value.Item2]).AppendLine();
                    }
                }
            }
            var text = builder.ToString();
            var path = Path.Combine(Application.dataPath, $"_dirty_words_{Locale}_tsv.txt");
            if (AppPlatform.IsWindows)
            {
                path = AppPlatform.ConvertToWindowsPath(path);
            }
            Debug.Log($"Save {_debugWords.Count} NEW 'dirty' words to {path}");
            File.WriteAllText(path, text);
        }
#else
        internal void SaveIfDirty(){}
#endif

        #endregion
    }

    /// <summary>
    /// Container for all installed languages.
    /// </summary>
    internal class Languages
    {
        private readonly List<Language> _languages = new();

        internal ReadOnlyCollection<Language> GetLanguages => _languages.AsReadOnly();

        internal void Add(Language language)
        {
            Assert.IsTrue(_languages.FindIndex(x => x.Locale.Equals(language.Locale, StringComparison.Ordinal)) == -1,
                "_languages.FindIndex(x => x.Locale == language.Locale) == -1");
            _languages.Add(language);
        }

        internal bool TryGetLanguage(SystemLanguage systemLanguage, out Language language)
        {
            var index = _languages.FindIndex(x => x.LanguageName == systemLanguage);
            if (index == -1)
            {
                language = null;
                return false;
            }
            language = _languages[index];
            return true;
        }

        internal bool HasLanguage(SystemLanguage systemLanguage)
        {
            return _languages.FindIndex(x => x.LanguageName == systemLanguage) != -1;
        }

        internal Language GetLanguage(SystemLanguage systemLanguage)
        {
            var index = _languages.FindIndex(x => x.LanguageName == systemLanguage);
            if (index == -1)
            {
                if (_languages.Count > 0)
                {
                    return _languages[0];
                }
                return new Language(systemLanguage);
            }
            return _languages[index];
        }
    }

    #endregion

    #region Localizer implementation

    /// <summary>
    /// Simple <c>I18N</c> implementation to localize words and phrases.
    /// </summary>
    public static class Localizer
    {
        private static Languages _languages;
        private static Language _curLanguage;
        private static SystemLanguage _curSystemLanguage = SystemLanguage.Unknown;

        public static SystemLanguage Language => _curSystemLanguage;

        public static string Localize(string key) => _curLanguage.Word(key);

        public static bool HasLanguage(SystemLanguage language)
        {
            return _languages?.HasLanguage(language) ?? false;
        }

        public static void SetLanguage(SystemLanguage language)
        {
            Debug.Log($"SetLanguage {_curLanguage} : {_curSystemLanguage} <- {language}");
            if (!_languages.TryGetLanguage(language, out _curLanguage))
            {
                _curLanguage = _languages.GetLanguages[0];
            }
            _curSystemLanguage = _curLanguage.LanguageName;
        }

        public static void LoadTranslations(SystemLanguage language)
        {
            var config = Resources.Load<LocalizationConfig>(nameof(LocalizationConfig));
            if (config == null)
            {
                Debug.LogWarning($"{nameof(LocalizationConfig)} is missing");
            }
            var languagesBinFile = config != null ? config.LanguagesBinFile : null;
            if (languagesBinFile == null)
            {
                _languages = new Languages();
                _languages.Add(new Language(language));
            }
            else
            {
                _languages = BinAsset.Load(config.LanguagesBinFile, false);
            }
            Assert.IsTrue(_languages.GetLanguages.Count > 0, "_languages.GetLanguages.Count > 0");
            SetLanguage(language);
            LocalizerHelper.SetEditorStatus();
        }

        /// <summary>
        /// Helper to provide access to selected internal localization data for Editor utilities to use.
        /// </summary>
        /// <remarks>
        /// This is mostly used for "Localization process in Editor"
        /// </remarks>
        public static class LocalizerHelper
        {
            private static List<string> _cachedKeys;
            private static string _localeForKeys;

            [Conditional("UNITY_EDITOR")]
            internal static void SetEditorStatus()
            {
#if UNITY_EDITOR
                void PlayModeStateChangeCallback(PlayModeStateChange change)
                {
                    if (change == PlayModeStateChange.ExitingPlayMode)
                    {
                        SaveIfDirty();
                    }
                }

                // Trying to keep at most one callback alive at a time.
                EditorApplication.playModeStateChanged -= PlayModeStateChangeCallback;
                EditorApplication.playModeStateChanged += PlayModeStateChangeCallback;
#endif
            }

            public static int TrackWords(SmartText component, string key, string word)
            {
#if UNITY_EDITOR
                return _curLanguage.TrackWords(component, key, word);
#else
                return 0;
#endif
            }

            [Conditional("UNITY_EDITOR")]
            public static void SaveIfDirty()
            {
                if (_languages != null)
                {
                    foreach (var language in _languages.GetLanguages)
                    {
                        language.SaveIfDirty();
                    }
                }
            }

            /// <summary>
            /// Save translations from .tsv file to internal binary format.
            /// </summary>
            [Conditional("UNITY_EDITOR")]
            public static void SaveTranslations()
            {
                var config = Resources.Load<LocalizationConfig>(nameof(LocalizationConfig));
                Assert.IsNotNull(config, "config != null");
                var languages = TsvLoader.LoadTranslations(config.TranslationsTsvFile);
                BinAsset.Save(languages, config.LanguagesBinFile);
                ResetKeys();
            }

            /// <summary>
            /// Show current translations info if they are loaded.
            /// </summary>
            [Conditional("UNITY_EDITOR")]
            public static void ShowTranslations()
            {
                if (_languages == null)
                {
                    Debug.Log("No languages loaded");
                    return;
                }
                Debug.Log($"Current language is {(_curLanguage != null ? _curLanguage.LanguageName.ToString() : "NOT SELECTED")}");
                foreach (var language in _languages.GetLanguages)
                {
                    Debug.Log(
                        $"Language {language.Locale} {language.LanguageName} words {language.Words.Count} alt words {language.AltWords.Count}");
                }
            }

            /// <summary>
            /// Gets sorted list of all localization keys found in current language.
            /// </summary>
            /// <remarks>
            /// We cache this because UI might ask it frequently.
            /// </remarks>
            public static List<string> GetTranslationKeys()
            {
                if (_curLanguage != null)
                {
                    if (_cachedKeys == null || !_curLanguage.Locale.Equals(_localeForKeys, StringComparison.Ordinal))
                    {
                        var set = new HashSet<string>();
                        // AltWords should be "more complete" than Words.
                        set.UnionWith(_curLanguage.AltWords.Keys);
                        set.UnionWith(_curLanguage.Words.Keys);
                        _cachedKeys = set.ToList();
                        _cachedKeys.Sort();
                        _localeForKeys = _curLanguage.Locale;
                    }
                }
                return _cachedKeys;
            }

            private static void ResetKeys()
            {
                _cachedKeys = null;
                _localeForKeys = null;
            }
        }
    }

    #endregion

    #region Language file management

    /// <summary>
    /// Content loader for localized words and phrases in Tab Separated Values (.tsv) format.
    /// </summary>
    /// <remarks>
    /// File column and row format is documented elsewhere!
    /// </remarks>
    internal static class TsvLoader
    {
        private const string DefaultLocale = "en";

        // https://en.wikipedia.org/wiki/List_of_ISO_639-1_codes
        private static readonly string[] SupportedLocales =
        {
            "key",
            "en",
            "fi",
            "sv"
        };

        private static readonly SystemLanguage[] SupportedLanguages =
        {
            SystemLanguage.Unknown,
            SystemLanguage.English,
            SystemLanguage.Finnish,
            SystemLanguage.Swedish
        };

        internal static SystemLanguage GetLanguageFor(string locale)
        {
            var index = Array.FindIndex(SupportedLocales, x => x.Equals(locale, StringComparison.Ordinal));
            Assert.IsTrue(index >= 0);
            return SupportedLanguages[index];
        }

        internal static Languages LoadTranslations(TextAsset textAsset)
        {
            //--Debug.Log($"Translations tsv {textAsset.name} text len {textAsset.text.Length}");
            var lines = textAsset.text;
            var languages = new Languages();
            var maxIndex = SupportedLocales.Length;
            var dictionaries = new Dictionary<string, string>[maxIndex];
            using (var reader = new StringReader(lines))
            {
                var line = reader.ReadLine();
                Assert.IsNotNull(line, "line != null");
                //--Debug.Log($"FIRST LINE: {line.Replace('\t', ' ')}");
                // key en fi sv es ru it de fr zh-CN
                var tokens = line.Split('\t');
                Assert.IsTrue(tokens.Length >= maxIndex, "tokens.Length >= maxIndex");
                for (var i = 1; i < maxIndex; ++i)
                {
                    // Translation file columns must match our understanding of locale columns!
                    var requiredLocale = SupportedLocales[i];
                    var currentLocale = tokens[i];
                    Assert.IsTrue(currentLocale.Equals(requiredLocale, StringComparison.Ordinal), "currentLocale == requiredLocale");
                    if (i == 1)
                    {
                        // Default locale must be in first locale column.
                        Assert.IsTrue(currentLocale.Equals(DefaultLocale, StringComparison.Ordinal), "currentLocale == _defaultLocale");
                    }
                    dictionaries[i] = new Dictionary<string, string>();
                }
                for (;;)
                {
                    line = reader.ReadLine();
                    if (line == null)
                    {
                        break;
                    }
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        continue;
                    }
                    ParseLine(line, maxIndex, ref dictionaries);
                }
            }
            //--Debug.Log($"lineCount {lineCount} in {stopwatch.ElapsedMilliseconds} ms");
            Dictionary<string, string> altDictionary = null;
            for (var i = 1; i < maxIndex; ++i)
            {
                var lang = SupportedLanguages[i];
                var locale = SupportedLocales[i];
                var dictionary = dictionaries[i];
                if (i == 1)
                {
                    altDictionary = dictionary;
                }
                var language = new Language(lang, locale, dictionary, altDictionary);
                languages.Add(language);
                //--Debug.Log($"dictionary for {locale} {lang} has {dictionary.Count} words");
            }
            return languages;
        }

        private static void ParseLine(string line, int maxIndex, ref Dictionary<string, string>[] dictionaries)
        {
            var tokens = line.Split('\t');
            Assert.IsTrue(tokens.Length >= maxIndex, "tokens.Length >= maxIndex");
            var key = tokens[0];
            var defaultValue = tokens[1];
            if (string.IsNullOrEmpty(defaultValue))
            {
                Debug.LogWarning($"SKIP EMPTY column 1 in line {line}");
                return;
            }
            for (var i = 1; i < maxIndex; ++i)
            {
                var colValue = tokens[i];
                if (string.IsNullOrEmpty(colValue))
                {
                    continue;
                }
                var dictionary = dictionaries[i];
                if (dictionary.ContainsKey(key))
                {
                    Debug.LogWarning($"SKIP DUPLICATE key '{key}' in line {line}");
                    continue;
                }
                dictionary.Add(key, colValue);
            }
        }
    }

    /// <summary>
    /// Binary formatted <c>TextAsset</c> storage for localized words and phrases.
    /// </summary>
    /// <remarks>
    /// File format is proprietary and contained in this file!
    /// </remarks>
    internal static class BinAsset
    {
        private const string AssetRoot = "Assets";

        private const byte FileMark = 0xAA;
        private const byte LocaleStart = 0xBB;
        private const byte LocaleEnd = 0xCC;

        private const int FileVersion = 100;

        [Conditional("UNITY_EDITOR")]
        internal static void Save(Languages languages, TextAsset binAsset)
        {
#if UNITY_EDITOR
            string GetAssetPath(string name)
            {
                var assetFilter = $"{name} t:TextAsset";
                var foundAssets = AssetDatabase.FindAssets(assetFilter, new[] { AssetRoot });
                Assert.IsTrue(foundAssets.Length == 1, "foundAssets.Length == 1");
                return AssetDatabase.GUIDToAssetPath(foundAssets[0]);
            }

            var path = GetAssetPath(binAsset.name);
            //--Debug.Log($"Save Languages bin {binAsset.name} path {path}");
            int byteCount;
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var languageList = languages.GetLanguages;
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    writer.Write(FileMark);
                    writer.Write(FileVersion);
                    writer.Write(languageList.Count);
                    foreach (var language in languageList)
                    {
                        var locale = language.Locale;
                        var words = language.Words;
                        writer.Write(LocaleStart);
                        writer.Write(locale);
                        writer.Write(words.Count);
                        foreach (var entry in words)
                        {
                            writer.Write(entry.Key);
                            writer.Write(entry.Value);
                        }
                        writer.Write(LocaleEnd);
                    }
                }
                var bytes = stream.ToArray();
                byteCount = bytes.Length;
                File.WriteAllBytes(path, bytes);
            }
            stopwatch.Stop();
            AssetDatabase.Refresh();
            Debug.Log($"Save Languages bin {binAsset.name} bytes len {byteCount} in {stopwatch.ElapsedMilliseconds} ms");
            foreach (var language in languageList)
            {
                DumpLanguage(language);
            }
#endif
        }

        internal static Languages Load(TextAsset binAsset, bool isLogging)
        {
            var bytes = binAsset.bytes;
            if (isLogging) Debug.Log($"Load Languages bin {binAsset.name} bytes len {bytes.Length}");
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            var languages = new Languages();
            using (var stream = new MemoryStream(bytes))
            {
                using (var reader = new BinaryReader(stream))
                {
                    var fileMark = reader.ReadByte();
                    Assert.IsTrue(fileMark == FileMark, "fileMark == FileMark");
                    var fileVersion = reader.ReadInt32();
                    Assert.IsTrue(fileVersion == FileVersion, "fileVersion == FileVersion");
                    var localeCount = reader.ReadInt32();
                    Assert.IsTrue(localeCount > 0, "localeCount > 0");
                    Dictionary<string, string> altDictionary = null;
                    for (var i = 0; i < localeCount; ++i)
                    {
                        var localeStart = reader.ReadByte();
                        Assert.IsTrue(localeStart == LocaleStart, "localeStart == LocaleStart");
                        var locale = reader.ReadString();
                        Assert.IsFalse(string.IsNullOrWhiteSpace(locale), "string.IsNullOrWhiteSpace(locale)");
                        var lang = TsvLoader.GetLanguageFor(locale);
                        var wordCount = reader.ReadInt32();
                        Assert.IsTrue(wordCount >= 0, "wordCount >= 0");
                        var words = new Dictionary<string, string>();
                        for (var counter = 0; counter < wordCount; ++counter)
                        {
                            var key = reader.ReadString();
                            var value = reader.ReadString();
                            words.Add(key, value);
                        }
                        var localeEnd = reader.ReadByte();
                        Assert.IsTrue(localeEnd == LocaleEnd, "localeEnd == LocaleEnd");
                        if (i == 0)
                        {
                            altDictionary = words;
                        }
                        var language = new Language(lang, locale, words, altDictionary);
                        languages.Add(language);
                    }
                }
            }
            stopwatch.Stop();
            if (isLogging)
            {
                Debug.Log($"Load Languages bin {binAsset.name} bytes len {bytes.Length} in {stopwatch.ElapsedMilliseconds} ms");
                foreach (var language in languages.GetLanguages)
                {
                    DumpLanguage(language);
                }
            }
            return languages;
        }

        [Conditional("UNITY_EDITOR")]
        private static void DumpLanguage(Language language)
        {
            Debug.Log($"Language {language.Locale} {language.LanguageName} words {language.Words.Count} alt words {language.AltWords.Count}");
        }
    }

    #endregion
}