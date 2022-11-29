using System;
using Altzone.Scripts.Config;
#if USE_LOOTLOCKER
using UnityEngine.Assertions;
using Prg.Scripts.Service.LootLocker;
#endif

namespace Altzone.Scripts.Service.LootLocker
{
    public static class LootLockerWrapper
    {
        private const string GameVersion = "0.1.0.0";
        private const bool OnDevelopmentMode = true;
        private const string DomainKey = "nagpi6si";

#if USE_LOOTLOCKER
        public static bool IsRunning => Manager.IsRunning;

        private static readonly LootLockerManager Manager = new();

        public static void Init(Func<string> getApiKey = null)
        {
            if (Manager.IsRunning)
            {
                return;
            }
            // Read API key.
            var apiKey = getApiKey?.Invoke(); 
#if UNITY_EDITOR
            // https://console.lootlocker.com/settings/api-keys
            apiKey ??= "1dfbd87633b925b496395555f306d754c6a6903e";
#endif
            Manager.Init(GameVersion, apiKey, OnDevelopmentMode, DomainKey);
            var playerDataCache = RuntimeGameConfig.Get().PlayerDataCache;
            Manager.Run(playerDataCache.PlayerGuid, playerDataCache.PlayerName, s => playerDataCache.PlayerName = s);
        }

        public static string GetPlayerName()
        {
            Assert.IsTrue(Manager.IsRunning, "Manager.IsRunning");
            return Manager.PlayerHandle.PlayerName;
        }

        public static void SetPlayerName(string playerName)
        {
            Assert.IsTrue(Manager.IsRunning, "Manager.IsRunning");
            var playerDataCache = RuntimeGameConfig.Get().PlayerDataCache;
            var task = Manager.SetPlayerName(playerName, s => playerDataCache.PlayerName = s);
        }
#else
        public static bool IsRunning => true;

        public static void Init(Func<string> getApiKey = null)
        {
        }

        public static string GetPlayerName()
        {
            var playerDataCache = RuntimeGameConfig.Get().PlayerDataCache;
            return playerDataCache.PlayerName;
        }

        public static void SetPlayerName(string playerName)
        {
            var playerDataCache = RuntimeGameConfig.Get().PlayerDataCache;
            playerDataCache.PlayerName = playerName;
        }
#endif
    }
}