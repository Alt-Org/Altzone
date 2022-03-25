using System.Diagnostics;
using System.Globalization;
using System.Threading;
using Altzone.Scripts.Config.ScriptableObjects;
using Prg.Scripts.Common.Photon;
using Prg.Scripts.Common.Unity;
using Prg.Scripts.Common.Util;
using UnityEngine;
#if USE_UNITY_ADS
using UnityEngine.Advertisements;
#endif

namespace Altzone.Scripts
{
    public class BootLoader : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void BeforeSceneLoad()
        {
            var localDevConfig = ResourceLoader.Get().LoadAsset<LocalDevConfig>(nameof(LocalDevConfig));
            LocalDevConfig.Instance = Instantiate(localDevConfig);

            var folderConfig = ResourceLoader.Get().LoadAsset<FolderConfig>(nameof(FolderConfig));
            var resourceLoader = ResourceLoader.Get(folderConfig.primaryConfigFolder, localDevConfig.developmentConfigFolder);

            var loggerConfig = resourceLoader.LoadAsset<LoggerConfig>(nameof(LoggerConfig));
            LoggerConfig.CreateLoggerConfig(loggerConfig);

            SetEditorStatus();
            GeoLocation.Load(data =>
            {
                UnityEngine.Debug.Log($"Photon {PhotonLobby.GameVersion} GeoLocation {data}");
                SetConsentMetaData(data);
            });
        }

        [Conditional("USE_UNITY_ADS")]
        private static void SetConsentMetaData(GeoLocation.LocationData data)
        {
            // Google Play Families compliance:
            // - mixed indicates that the app is directed at mixed audiences (including children).
            var privacyValue = PlayerPrefs.GetString("consent.families.privacy.mode", "mixed");
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

        [Conditional("UNITY_EDITOR"), Conditional("FORCE_LOG")]
        private static void SetEditorStatus()
        {
            // This is just for debugging to get strings (numbers) formatted consistently
            // - everything that goes to UI should go through Localizer using player's locale preferences
            var ci = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;

            if (!string.IsNullOrWhiteSpace(LocalDevConfig.Instance.photonVersionPrefix))
            {
                PhotonLobby.GetGameVersion = () => $"{LocalDevConfig.Instance.photonVersionPrefix}{Application.version}";
            }
        }
    }
#if USE_UNITY_ADS
#else
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