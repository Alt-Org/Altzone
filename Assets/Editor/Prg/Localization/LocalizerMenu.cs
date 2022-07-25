using System;
using System.Collections.Generic;
using Prg.Scripts.Common.Unity.Localization;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Editor.Prg.Localization
{
    /// <summary>
    /// Localization process in Editor menu commands.
    /// </summary>
    internal static class LocalizerMenu
    {
        private const string GoogleDriveUrl =
            @"https://docs.google.com/spreadsheets/d/1ZwjasMpaXABXZ5N6_ivuSMeXv-764TDmqUSG5ufJmvA/edit#gid=1638234547";

        public static void LoadTranslations()
        {
            Debug.Log("*");
            Localizer.LoadTranslations(Application.systemLanguage);
        }

        public static void SaveTranslations()
        {
            Debug.Log("*");
            Localizer.LocalizerHelper.SaveTranslations();
        }

        public static void ShowTranslations()
        {
            Debug.Log("*");
            Localizer.LocalizerHelper.ShowTranslations();
        }

        public static void CheckUsedTranslationsInAssets()
        {
            Debug.Log("*");
            if (Selection.assetGUIDs.Length == 0)
            {
                Debug.Log("Select one or more assets in Project window");
                return;
            }
            var selectedGuids = Selection.assetGUIDs;
            foreach (var guid in selectedGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var gameObject = AssetDatabase.LoadAssetAtPath(path, typeof(GameObject)) as GameObject;
                if (gameObject == null)
                {
                    Debug.Log($"Unable to load asset: {path}");
                    continue;
                }
                DoSmartTextAndTextAssetCheck(gameObject);
            }
            Localizer.LocalizerHelper.SaveIfDirty();
        }

        public static void OpenInGoogleDrive()
        {
            Debug.Log("*");
            Application.OpenURL(GoogleDriveUrl);
        }

        private static void DoSmartTextAndTextAssetCheck(GameObject gameObject)
        {
            var result = new List<SmartTextContext>();
            var components = gameObject.GetComponentsInChildren<Text>(true);
            foreach (var component in components)
            {
                CheckGameObject(component.gameObject, ref result);
            }
            Debug.LogWarning($"Checked components {result.Count} in {gameObject.GetFullPath()}", gameObject);
            result.Sort((a, b) => String.Compare(a.SortKey, b.SortKey, StringComparison.Ordinal));
            foreach (var smartTextContext in result)
            {
                smartTextContext.ShowMissing();
            }
        }

        private static void CheckGameObject(GameObject child, ref List<SmartTextContext> context)
        {
            var smartText = child.GetComponent<SmartText>();
            if (smartText == null)
            {
                context.Add(new SmartTextContext(child, "9 Text is not localized: "));
                return;
            }
            string[] reasonTexts = { "OK", "NO_KEY", "NO_TEXT", "ALT_TEXT" };
            // Emulate SmartText Localize() method
            var localizationKey = smartText.LocalizationKey;
            var localizedText = Localizer.Localize(localizationKey);
            var result = Localizer.LocalizerHelper.TrackWords(smartText, localizationKey, localizedText);
            context.Add(new SmartTextContext(child, $"{result} {reasonTexts[result]} SmartText: {localizationKey} <- {localizedText}"));
        }

        private class SmartTextContext
        {
            private readonly string _message;
            private readonly string _childPath;

            public readonly string SortKey;

            public SmartTextContext(GameObject child, string message)
            {
                _childPath = child.GetFullPath();
                _message = message;
                SortKey = message + _childPath;
            }

            public void ShowMissing()
            {
                Debug.Log($"{_message} in {_childPath}");
            }
        }
    }
}