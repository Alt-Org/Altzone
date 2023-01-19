using System.Diagnostics;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model;
using Altzone.Scripts.Service.Audio;
using Altzone.Scripts.Service.LootLocker;
using Prg.Scripts.Common.Unity;
using Prg.Scripts.Common.Unity.Attributes;
using Prg.Scripts.Common.Unity.Localization;
using Prg.Scripts.Common.Unity.Window;
using UnityEngine;

namespace Altzone.Scripts
{
    /// <summary>
    /// Loads all services used by this game.
    /// </summary>
    public class ServiceLoader : MonoBehaviour
    {
        /// <summary>
        /// Development mode API Key suffix (simplified chinese) for <c>LootLocker</c> SDK API.
        /// </summary>
        private const string Prefix1 = "发展"; // Fāzhǎn

        /// <summary>
        /// Production mode API Key suffix (simplified chinese) for <c>LootLocker</c> SDK API.
        /// </summary>
        private const string Prefix2 = "生产"; // Shēngchǎn

        [SerializeField, ReadOnly] private bool _isLootLocker;

        public bool IsLootLocker => _isLootLocker;
        
        private void OnEnable()
        {
            Debug.Log($"{name}");
            Localizer.LoadTranslations(Application.systemLanguage);
            AudioManager.Get();
            // Parts of store can be initialized asynchronously and we start them now.
            Storefront.Get();
            // Development vs production mode needs to be decided during build time!
            const bool isDevelopmentMode = true;
            StartLootLocker(isDevelopmentMode);
            // Start the UI now.
            WindowManager.Get();
        }

        [Conditional("USE_LOOTLOCKER")]
        private void StartLootLocker(bool isDevelopmentMode)
        {
            // We need player name and guid in order to start LootLocker.
            var playerDataCache = GameConfig.Get().PlayerSettings;
            if (string.IsNullOrWhiteSpace(playerDataCache.PlayerName) || string.IsNullOrWhiteSpace(playerDataCache.PlayerGuid))
            {
                Debug.Log("Can not start LootLocker because player name and/or guid is missing");
                return;
            }
            var suffix = isDevelopmentMode ? Prefix1 : Prefix2;
            Debug.Log($"Start LootLocker IsRunning {LootLockerWrapper.IsRunning} suffix {suffix}");
            LootLockerWrapper.Start(isDevelopmentMode,
                () => Resources.Load<StringProperty>($"{nameof(StringProperty)}{suffix}")?.PropertyValue);
            _isLootLocker = true;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Helper class for UNITY Editor operations.
        /// </summary>
        public static class Support
        {
            public static StringProperty GetLootLockerResource() => _GetLootLockerResource();
        }

        private static StringProperty _GetLootLockerResource()
        {
            return Resources.Load<StringProperty>($"{nameof(StringProperty)}{Prefix1}");
        }
#endif
    }
}