using System;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Editor.Prg.GameDebug
{
    public static class DebugMenu
    {
        private const string MenuRoot = "Window/ALT-Zone/Game Debug/";

        [MenuItem(MenuRoot +"Show Player Data", false, 1)]
        private static void ShowLocalPlayerData()
        {
            Debug.Log("*");
            var playerData = RuntimeGameConfig.GetPlayerDataCacheInEditor();
            Debug.Log(playerData.ToString());
        }

        [MenuItem(MenuRoot +"Create Dummy Player Data", false, 2)]
        private static void CreateDummyPlayerData()
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

        [MenuItem(MenuRoot +"Set Player Language to EN", false, 3)]
        private static void SetLanguageToEn()
        {
            Debug.Log("*");
            var playerData = RuntimeGameConfig.GetPlayerDataCacheInEditor();
            playerData.Language = SystemLanguage.English;
            Debug.Log(playerData.ToString());
        }

        [MenuItem(MenuRoot +"Delete Player Data", false, 4)]
        private static void DeleteLocalPlayerData()
        {
            Debug.Log("*");
            var playerData = RuntimeGameConfig.GetPlayerDataCacheInEditor();
            playerData.DebugResetPlayer();
            Debug.Log(playerData.ToString());
        }

        [MenuItem(MenuRoot +"Danger Zone/Delete All Local Data", false, 1)]
        private static void DeleteLocalAllData()
        {
            Debug.Log("*");
            Debug.Log(RichText.Brown("PlayerPrefs.DeleteAll"));
            PlayerPrefs.DeleteAll();
            PlayerPrefs.Save();
        }
    }
}