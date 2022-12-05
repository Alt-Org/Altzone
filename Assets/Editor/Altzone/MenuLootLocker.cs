using Altzone.Scripts;
using UnityEditor;
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
            var resource = ServiceLoader.Support.GetLootLockerResource();
            Selection.objects = new UnityEngine.Object[] { resource };
            EditorGUIUtility.PingObject(resource);
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