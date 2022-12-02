using System.Diagnostics;
using System.Globalization;
using System.Threading;
using Altzone.Scripts.Config;
using Altzone.Scripts.Config.ScriptableObjects;
using Altzone.Scripts.Service.LootLocker;
using Prg.Scripts.Common.Photon;
using Prg.Scripts.Common.Unity;
using Prg.Scripts.Common.Util;
using UnityEngine;
#if USE_UNITY_ADS
using UnityEngine.Advertisements;
#endif

namespace Altzone.Scripts
{
    internal static class BootLoader
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void BeforeSceneLoad()
        {
            var localDevConfig = Resources.Load<LocalDevConfig>(nameof(LocalDevConfig));
            var loggerConfig = localDevConfig != null && localDevConfig._loggerConfig
                ? localDevConfig._loggerConfig
                : Resources.Load<LoggerConfig>(nameof(LoggerConfig));
            if (loggerConfig != null)
            {
                LoggerConfig.CreateLoggerConfig(loggerConfig);
            }
            SetEditorStatus();
            if (localDevConfig != null)
            {
                if (!string.IsNullOrWhiteSpace(localDevConfig._photonVersionOverride))
                {
                    var capturedPhotonVersionOverride = localDevConfig._photonVersionOverride;
                    PhotonLobby.GetGameVersion = () => capturedPhotonVersionOverride;
                }
            }
            GeoLocation.Load(data =>
            {
                UnityEngine.Debug.Log($"Photon {PhotonLobby.GameVersion} GeoLocation {data} IsSimulator {AppPlatform.IsSimulator}");
                SetConsentMetaData(data);
                UnitySingleton.CreateStaticSingleton<ServiceLoader>();
            });
        }

        [Conditional("USE_UNITY_ADS")]
        private static void SetConsentMetaData(GeoLocation.LocationData data)
        {
            // Google Play Families compliance:
            // - mixed indicates that the app is directed at mixed audiences (including children).
            var privacyValue = PlayerPrefs.GetString(PlayerPrefKeys.ConsentFamiliesPrivacyMode, "mixed");
            var metaData = new MetaData("privacy");
            metaData.Set("mode", privacyValue);
            Advertisement.SetMetaData(metaData);

            // Complying with GDPR:
            // - false indicates that the user opts out of targeted advertising.
            var gdprConsentValue = data.GdprConsent == GeoLocation.GdprConsent.Yes ? "true" : "false";
            metaData = new MetaData("gdpr");
            metaData.Set("consent", gdprConsentValue);
            Advertisement.SetMetaData(metaData);

            // COPPA compliance and contextual ads:
            // - true indicates that the user may not receive personalized ads.
            var nonBehavioralValue = data.GdprConsent == GeoLocation.GdprConsent.Yes ? "false" : "true";
            metaData = new MetaData("user");
            metaData.Set("nonbehavioral", nonBehavioralValue);
            Advertisement.SetMetaData(metaData);
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("FORCE_LOG")]
        private static void SetEditorStatus()
        {
            // This is just for debugging to get strings (numbers) formatted consistently
            // - everything that goes to UI should go through Localizer using player's locale preferences
            var ci = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
        }
    }
#if USE_UNITY_ADS
#else
    // Keep compiler happy with these dummy classes for UnityEngine.Advertisements package when UNITY ADS are not in use.
    public class MetaData
    {
        public MetaData(string category)
        {
        }

        public void Set(string key, object value)
        {
        }
    }

    public static class Advertisement
    {
        public static void SetMetaData(MetaData _)
        {
        }
    }
#endif
}