using Altzone.Scripts.Config;
#if USE_LOOTLOCKER
using UnityEngine.Assertions;
using Prg.Scripts.Service.LootLocker;
#endif

namespace Altzone.Scripts.Service.LootLocker
{
    public static class LootLockerWrapper
    {
        private const bool OnDevelopmentMode = true;
        private const string DomainKey = "nagpi6si";

#if USE_LOOTLOCKER
        private static readonly LootLockerManager Manager = new ();

        public static void Init(string apiKey)
        {
            if (Manager.IsRunning)
            {
                return;
            }
            Manager.Init("", OnDevelopmentMode, DomainKey);
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
            var playerDataCache = RuntimeGameConfig.Get().PlayerDataCache;
            var task = Manager.SetPlayerName(playerName, s => playerDataCache.PlayerName = s);
        }
#else
        public static void Init(string apiKey)
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