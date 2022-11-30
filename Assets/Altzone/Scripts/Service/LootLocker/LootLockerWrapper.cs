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
        private const bool IsDevelopmentMode = true;
        private const string DomainKey = "nagpi6si";

#if USE_LOOTLOCKER
        public static bool IsRunning => Manager.IsRunning;

        private static readonly LootLockerManager Manager = new();

        public static void Start(Func<string> getApiKey = null)
        {
            // Read API key.
            var apiKey = getApiKey?.Invoke(); 
#if UNITY_EDITOR
            // https://console.lootlocker.com/settings/api-keys
            apiKey ??= "1dfbd87633b925b496395555f306d754c6a6903e";
#endif
            Manager.Init(GameVersion, apiKey, IsDevelopmentMode, DomainKey);
            var playerDataCache = RuntimeGameConfig.Get().PlayerDataCache;
            Manager.StartSessionAsync(playerDataCache.PlayerGuid, playerDataCache.PlayerName, s => playerDataCache.PlayerName = s);
        }

        public static void Stop()
        {
            Manager.EndSession();
        }
        
        public static string GetPlayerName()
        {
            Assert.IsTrue(Manager.IsRunning, "Manager.IsRunning");
            return Manager.PlayerHandle.PlayerName;
        }

        public static void SetPlayerName(string playerName)
        {
            if (!Manager.IsRunning)
            {
                return;
            }
            if (string.Compare(Manager.PlayerHandle.PlayerName, playerName, StringComparison.Ordinal) == 0)
            {
                return;
            }
            var playerDataCache = RuntimeGameConfig.Get().PlayerDataCache;
            var task = Manager.SetPlayerNameAsync(playerName, s => playerDataCache.PlayerName = s);
        }
#else
        public static bool IsRunning => true;

        public static void Start(Func<string> getApiKey = null)
        {
        }

        public static void Stop()
        {
        }
        
        public static string GetPlayerName()
        {
            var playerDataCache = RuntimeGameConfig.Get().PlayerDataCache;
            return playerDataCache.PlayerName;
        }

        public static void SetPlayerName(string playerName)
        {
        }
#endif
    }
}