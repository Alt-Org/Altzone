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
        public static void ShowLocalPlayerData()
        {
            Debug.Log("*");
            var playerData = RuntimeGameConfig.Get().PlayerDataCache;
            Debug.Log(playerData.ToString());
        }

        public static void CreateDummyPlayerData()
        {
            Debug.Log("*");
            var language = Application.systemLanguage;
            Localizer.LoadTranslations(language);
            var playerData = RuntimeGameConfig.Get().PlayerDataCache;
            playerData.PlayerName = $"Player{1000 * (1 + DateTime.Now.Second % 10) + DateTime.Now.Millisecond:00}";
            playerData.Language = language;
            Localizer.SetLanguage(language);
            playerData.CharacterModelId = Random.Range((int)Defence.Desensitisation, (int)Defence.Confluence + 1);
            playerData.DebugSavePlayer();
            Debug.Log(playerData.ToString());
        }

        public static void SetLanguageToEn()
        {
            Debug.Log("*");
            const SystemLanguage language = SystemLanguage.English;
            Localizer.LoadTranslations(language);
            var playerData = RuntimeGameConfig.Get().PlayerDataCache;
            playerData.Language = language;
            Localizer.SetLanguage(language);
            playerData.DebugSavePlayer();
            Debug.Log(playerData.ToString());
        }

        public static void DeleteLocalPlayerData()
        {
            Debug.Log("*");
            var playerData = RuntimeGameConfig.Get().PlayerDataCache;
            playerData.DebugResetPlayer();
            playerData.DebugSavePlayer();
            Debug.Log(playerData.ToString());
        }
    }
}