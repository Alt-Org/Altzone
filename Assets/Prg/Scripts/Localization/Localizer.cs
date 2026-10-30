using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Prg.Localization
{
    /// <summary>
    /// Simple <c>i18n</c> and <c>l10n</c> implementation to localize words and phrases.<br />
    /// Contains map of locales that contains map of localized words by localization key.<br />
    /// Localized words are read from game_localizations.tsv file in folder ./etc/localization<br />
    /// Localization data is cached in binary formatted <c>TextAsset</c> file in Assets/Resources folder.
    /// <br />
    /// </summary>
    public static class Localizer
    {
        public static readonly string LocalizationTsvFilename = Path.Combine(".", "etc", "localization", "game_localizations.tsv");

        public static bool IsShowMissingKey;
        public static bool IsShowMissingKeyColor;
        public static Color MissingKeyColor;

        /// <summary>
        /// All known available locales.
        /// </summary>
        private static Dictionary<string, Dictionary<string, string>> _locales;

        /// <summary>
        /// Current locale code.
        /// </summary>
        private static string _currentLocaleCode;

        private static string _defaultLocaleCode;

        /// <summary>
        /// Current locale 'words and phrases' dictionary.
        /// </summary>
        private static Dictionary<string, string> _currentLocale;

#if UNITY_EDITOR
        private static readonly HashSet<string> _missingKeys = new();

        public static bool HasMissingKeys => _missingKeys.Count > 0;
        public static HashSet<string> GetMissingKeys() => _missingKeys;
#endif

        #region Public Localization API

        public static string CurrentLocale => _currentLocaleCode;

        /// <summary>
        /// Localize text using <c>Localized</c> component.
        /// </summary>
        /// <param name="localized">the component to localize</param>
        /// <param name="variableReplacer">optional callback to replace variables in localized result</param>
        public static void Localize(this Localized localized, Func<string, string> variableReplacer = null)
        {
            if (!TryLocalize(localized._key, localized, out var text))
            {
                ReportMissingKey(localized._key, localized);
                if (IsShowMissingKeyColor)
                {
                    localized.SetColor(MissingKeyColor);
                }
                if (IsShowMissingKey)
                {
                    localized.SetText(FormatMissingKey(localized._key));
                    return;
                }
                // Use default text!
                text = localized._defaultText;
            }
            if (variableReplacer != null)
            {
                text = variableReplacer(text);
            }
            localized.SetText(text);
        }

        /// <summary>
        /// Localize given <c>textMeshPro</c> component.
        /// </summary>
        /// <param name="textMeshPro">the component to localize</param>
        /// <param name="key">the localization key</param>
        public static void Localize(this TextMeshProUGUI textMeshPro, string key)
        {
            if (!_currentLocale.TryGetValue(key, out var text))
            {
                ReportMissingKey(key, textMeshPro);
                if (IsShowMissingKeyColor)
                {
                    textMeshPro.color = MissingKeyColor;
                }
                if (IsShowMissingKey)
                {
                    textMeshPro.text = FormatMissingKey(key);
                    return;
                }
                // Use current text!
                text = textMeshPro.text;
            }
            textMeshPro.text = text;
        }

        /// <summary>
        /// Gets localized text if it is found in loacalization data.
        /// </summary>
        /// <param name="key">the localization key</param>
        /// <param name="context">for debug log missing keys</param>
        /// <param name="text">corresponding localized text</param>
        /// <returns>true if localization key was found</returns>
        private static bool TryLocalize(string key, Object context, out string text)
        {
            if (!_currentLocale.TryGetValue(key, out text))
            {
                ReportMissingKey(key, context);
                if (IsShowMissingKey)
                {
                    text = FormatMissingKey(key);
                }
                return false;
            }
            return true;
        }

        public static string Localize(string key, Object context = null)
        {
            if (key == null)
            {
                key = "NULL_KEY";
            }
            else if (key.Length == 0)
            {
                key = "EMPTY_KEY";
            }
            if (!_currentLocale.TryGetValue(key, out var text))
            {
                ReportMissingKey(key, context);
                if (IsShowMissingKey)
                {
                    return FormatMissingKey(key);
                }
                return key;
            }
            return text;
        }

        private static string FormatMissingKey(string key) => $"[{key}]";

        [Conditional("UNITY_EDITOR")]
        private static void ReportMissingKey(string key, Object context)
        {
#if UNITY_EDITOR
            if (context is Localized localized)
            {
                // Save text 'as is' for MissingKeys in localization process (clipboard or .csv).
                var text = localized._defaultText;
                var localizedKey = $"{localized._key}\t{text}\t{localized.GetFullPath()}";
                if (_missingKeys.Add(localizedKey))
                {
                    // Yellow sanitized text extract for log.
                    text = RichText.Yellow(Localized.SanitizeText(localized._defaultText));
                    localizedKey = $"{localized._key}\t{text}\t{localized.GetFullPath()}";
                    MissingKey(localizedKey, localized);
                }
            }
            else
            {
                if (_missingKeys.Add(key))
                {
                    MissingKey(key, context);
                }
            }
#endif
        }

#if UNITY_EDITOR
        private static void MissingKey(string missingKey, Object context)
        {
            Debug.LogWarning($"\"{missingKey}\"", context);
        }
#endif

        #endregion

        #region Data loader

        public static bool HasLanguage(string localeCode) => _locales.ContainsKey(localeCode);

        public static bool SetLanguage(string localeCode)
        {
            if (!_locales.TryGetValue(localeCode, out var localeDictionary))
            {
                return false;
            }
#if UNITY_EDITOR
            _missingKeys.Clear();
#endif
            _currentLocaleCode = localeCode;
            _currentLocale = localeDictionary;
            // Copy missing words from default locale to current.
            if (_currentLocaleCode != _defaultLocaleCode && _locales.TryGetValue(_defaultLocaleCode, out var defaultLocale))
            {
                var missingValueKeys = _currentLocale
                    .Where(x => string.IsNullOrEmpty(x.Value))
                    .Select(x => x.Key).ToList();
                foreach (var key in missingValueKeys)
                {
                    _currentLocale[key] = defaultLocale[key];
                }
                if (missingValueKeys.Count > 0)
                {
                    Debug.LogWarning($"locale {_currentLocaleCode} words  {_currentLocale.Count} missing {missingValueKeys.Count}");
                }
            }
            return true;
        }

        /// <summary>
        /// Loads localization system localization data from given <c>TextAsset</c>.
        /// </summary>
        /// <param name="currentLocaleCode">current locale code to use</param>
        /// <param name="defaultLocaleCode">current locale code to use</param>
        /// <param name="locales">localization data</param>
        public static void LoadLocalizations(string currentLocaleCode, string defaultLocaleCode,
            Dictionary<string, Dictionary<string, string>> locales)
        {
            _defaultLocaleCode = defaultLocaleCode;
            _locales = locales ?? new Dictionary<string, Dictionary<string, string>>();
            var localeCodes = _locales.Keys.ToList();
            if (HasLanguage(currentLocaleCode))
            {
                // OK
            }
            else if (HasLanguage(defaultLocaleCode))
            {
                Debug.LogWarning(
                    $"locale for current {currentLocaleCode} code not found, using default {defaultLocaleCode}");
                currentLocaleCode = defaultLocaleCode;
            }
            else if (localeCodes.Count > 0 && HasLanguage(localeCodes[0]))
            {
                Debug.LogWarning(
                    $"locale for current {currentLocaleCode} or default {defaultLocaleCode} codes not found, using first available {localeCodes[0]}");
                currentLocaleCode = localeCodes[0];
            }
            else
            {
                _currentLocaleCode = string.Empty;
                _currentLocale = new Dictionary<string, string>();
                Debug.LogError($"no locales available");
                return;
            }
            SetLanguage(currentLocaleCode);
        }

        #endregion
    }
}
