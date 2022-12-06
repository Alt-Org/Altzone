using System;
using System.Threading.Tasks;
using Altzone.Scripts.Config;
#if USE_LOOTLOCKER
using UnityEngine.Assertions;
using Prg.Scripts.Service.LootLocker;
#endif

namespace Altzone.Scripts.Service.LootLocker
{
    /// <summary>
    /// Altzone specific Facade for <c>LootLocker</c> SDK API.
    /// </summary>
    public static class LootLockerWrapper
    {
        /// <summary>
        /// Not sure where this is used but at least <c>LootLocker</c> stores it in its own config during runtime
        /// and it can be fetched from there if required.
        /// </summary>
        private const string GameVersion = "0.1.0.1";

        /// <summary>
        /// Not sure if we really need this but here it is anyway.
        /// </summary>
        /// <remarks>
        /// See https://console.lootlocker.com/settings/api-keys
        /// </remarks>
        private const string DomainKey = "nagpi6si";

        /// <summary>
        /// Do we use (recommended) 'guest login' or 'platform login' (like Android hardcoded now in <c>LootLockerManager</c>).
        /// </summary>
        /// <remarks>
        /// Corresponding setting must be enabled in LootLocker Web Console!<br />
        /// See https://console.lootlocker.com/settings/platforms 
        /// </remarks>
        private const bool IsGuestLogin = true;

#if USE_LOOTLOCKER
        public static bool IsRunning => Manager.IsRunning;

        private static readonly LootLockerManager Manager = new();

        public static void Start(bool isDevelopmentMode, Func<string> getApiKey = null)
        {
            // Read API key.
            var apiKey = getApiKey?.Invoke();
#if UNITY_EDITOR
            // https://console.lootlocker.com/settings/api-keys
            apiKey ??= "1dfbd87633b925b496395555f306d754c6a6903e";
#endif
            Manager.Init(GameVersion, () => apiKey, DomainKey, isDevelopmentMode, IsGuestLogin);
            var playerDataCache = GameConfig.Get().PlayerDataCache;
            Manager.StartSessionAsync(playerDataCache.PlayerGuid, playerDataCache.PlayerName, s => playerDataCache.SetPlayerName(s));
        }

        public static void Stop()
        {
            Manager.EndSession();
        }

        public static Task<string> PingAsync()
        {
            if (!Manager.IsRunning)
            {
                return CreateError("Manager Is NOT Running");
            }
            return Manager.Ping();
        }

        public static string PlayerName
        {
            get
            {
                if (!Manager.IsRunning)
                {
                    return GameConfig.Get().PlayerDataCache.PlayerName;
                }
                return Manager.PlayerHandle.PlayerName;
            }
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
            var playerDataCache = GameConfig.Get().PlayerDataCache;
            var task = Manager.SetPlayerNameAsync(playerName, s => playerDataCache.SetPlayerName(s));
        }
#else
        public static bool IsRunning => false;

        public static void Start(bool isDevelopmentMode, Func<string> getApiKey = null)
        {
        }

        public static void Stop()
        {
        }

        public static Task<string> PingAsync()
        {
            return CreateError("USE_LOOTLOCKER not defined");
        }

        public static string PlayerName
        {
            get
            {
                return GameConfig.Get().PlayerDataCache.PlayerName;
            }
        }

        public static void SetPlayerName(string playerName)
        {
        }
#endif

        private static Task<string> CreateError(string message)
        {
            var taskCompletionSource = new TaskCompletionSource<string>();
            taskCompletionSource.SetResult(message);
            return taskCompletionSource.Task;
        }
    }
}