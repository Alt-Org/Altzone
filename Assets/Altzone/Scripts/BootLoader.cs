using System.Diagnostics;
using System.Globalization;
using System.IO;
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
            CopyFile("CustomCharacterModels.json");
            CopyFile("InventoryItems.json");
            CopyFile("RaidGameRoomModels.json");
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
            UnityEngine.Debug.Log($"{Application.productName} Photon {PhotonLobby.GameVersion} IsSimulator {AppPlatform.IsSimulator}");
            UnitySingleton.CreateStaticSingleton<ServiceLoader>();
        }

        private static void CopyFile(string file)
        {
            var source = Resources.Load<TextAsset>("TestData/" +file);
            var targetPath = Path.Combine(Application.persistentDataPath, file);
            if (!File.Exists(targetPath))
            {
                File.WriteAllBytes(targetPath, source.bytes);
            }
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
