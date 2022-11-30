using Altzone.Scripts.Service.LootLocker;
using Prg.Scripts.Common.Unity;
using UnityEngine;
#if USE_LOOTLOCKER
using LootLocker;
using LootLocker.Requests;
#endif

namespace Editor.Altzone
{
    internal static class MenuLootLocker
    {
        public static void CreateLootLockerApiKeys()
        {
            Debug.Log("*");
            Debug.Log($"LootLocker ApiKeys are created using {RichText.Yellow("StringProperty")} ScriptableObject assets");
        }

        public static void ShowLootLockerApiKeys()
        {
            Debug.Log("*");
            const string suffix1 = LootLockerWrapper.Prefix1;
            var apiKey1 = Resources.Load<StringProperty>($"{nameof(StringProperty)}{suffix1}").PropertyValue;
            Debug.Log($"apiKey1 {apiKey1}");
            const string suffix2 = LootLockerWrapper.Prefix2;
            var apiKey2 = Resources.Load<StringProperty>($"{nameof(StringProperty)}{suffix2}").PropertyValue;
            Debug.Log($"apiKey2 {apiKey2}");
        }

        public static void CheckSession()
        {
#if USE_LOOTLOCKER
            Debug.Log("*");
            var isInitialized = LootLockerSDKManager.CheckInitialized(skipSessionCheck: true);
            var config = LootLockerConfig.Get();
            var hasSession = isInitialized && !string.IsNullOrEmpty(config.token);
            var playerIdentifier = hasSession ? config.deviceID : string.Empty;
            Debug.Log($"isInitialized {isInitialized} hasSession {hasSession} guid {playerIdentifier}");
#else
            Debug.Log("USE_LOOTLOCKER not defined");
#endif
        }
    }
}