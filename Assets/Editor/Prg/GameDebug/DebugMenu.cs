using System;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Editor.Prg.GameDebug
{
    internal static class DebugMenu
    {
        public static void ShowLocalPlayerData()
        {
            Debug.Log("*");
            var playerData = RuntimeGameConfig.GetPlayerDataCacheInEditor();
            Debug.Log(playerData.ToString());
        }

        public static void CreateDummyPlayerData()
        {
            var playerData = RuntimeGameConfig.GetPlayerDataCacheInEditor();
            playerData.BatchSave(() =>
            {
                playerData.PlayerName = $"Player{1000 * (1 + DateTime.Now.Second % 10) + DateTime.Now.Millisecond:00}";
                playerData.Language = Application.systemLanguage;
                playerData.CharacterModelId = Random.Range((int)Defence.Desensitisation, (int)Defence.Confluence + 1);
            });
            Debug.Log(playerData.ToString());
        }

        public static void SetLanguageToEn()
        {
            Debug.Log("*");
            var playerData = RuntimeGameConfig.GetPlayerDataCacheInEditor();
            playerData.Language = SystemLanguage.English;
            Debug.Log(playerData.ToString());
        }

        public static void DeleteLocalPlayerData()
        {
            Debug.Log("*");
            var playerData = RuntimeGameConfig.GetPlayerDataCacheInEditor();
            playerData.DebugResetPlayer();
            Debug.Log(playerData.ToString());
        }

        public static void DeleteLocalAllData()
        {
            Debug.Log("*");
            Debug.Log(RichText.Brown("PlayerPrefs.DeleteAll"));
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }
    }
}