using System;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model;
using Prg.Scripts.Common.Unity.Localization;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Editor.Prg.GameDebug
{
    internal static class DebugMenu
    {
        public static void ShowLocalPlayerSettings()
        {
            Debug.Log("*");
            var playerData = GameConfig.Get().PlayerSettings;
            Debug.Log(playerData.ToString());
        }

        public static void CreateDummyPlayerSettings()
        {
            Debug.Log("*");
            var language = Application.systemLanguage;
            Localizer.LoadTranslations(language);
            var playerSettings = GameConfig.Get().PlayerSettings;
            playerSettings.Language = language;
            Localizer.SetLanguage(language);
            playerSettings.DebugSavePlayerSettings();
            Debug.Log(playerSettings.ToString());
        }

        public static void SetLanguageToEn()
        {
            Debug.Log("*");
            const SystemLanguage language = SystemLanguage.English;
            Localizer.LoadTranslations(language);
            var playerData = GameConfig.Get().PlayerSettings;
            playerData.Language = language;
            Localizer.SetLanguage(language);
            playerData.DebugSavePlayerSettings();
            Debug.Log(playerData.ToString());
        }

        public static void ResetPlayerSettings()
        {
            Debug.Log("*");
            var playerData = GameConfig.Get().PlayerSettings;
            playerData.DebugResetPlayerSettings();
            playerData.DebugSavePlayerSettings();
            Debug.Log(playerData.ToString());
        }
    }
}