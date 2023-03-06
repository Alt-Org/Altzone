using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using Altzone.Scripts.Config;
using Altzone.Scripts.Config.ScriptableObjects;
using Prg.Scripts.Common.Photon;
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
            PrepareLocalTesting();
            var startupMessage = new StringBuilder()
                .Append(" Game ").Append(Application.productName)
                .Append(" Version ").Append(Application.version)
                .Append(" Photon ").Append(PhotonLobby.GameVersion)
                .Append(" IsSimulator ").Append(AppPlatform.IsSimulator)
                .Append(" Photon ").Append(PhotonLobby.GameVersion)
                .Append(" Screen ").Append(Screen.currentResolution)
                .ToString();
            UnityEngine.Debug.Log(startupMessage);
            PrepareDevice();
            UnitySingleton.CreateStaticSingleton<ServiceLoader>();
        }

        private static void PrepareLocalTesting()
        {
            SetEditorStatus();
            SetupLogging();
        }

        private static void PrepareDevice()
        {
            foreach (var filename in new[]
                     {
                         GameFiles.ClanGameRoomModelsFilename,
                         GameFiles.ClanInventoryItemsFilename,
                         //GameFiles.PlayerGameRoomModelsFilename,
                         GameFiles.PlayerCustomCharacterModelsFilename,
                     })
            {
                CopyFile(filename);
            }
            var gameConfig = GameConfig.Get();
            var playerSettings = gameConfig.PlayerSettings;
            if (playerSettings.IsFirstTimePlaying)
            {
                Debug.Log("IsFirstTimePlaying");
            }
        }

        private static void CopyFile(string file)
        {
            var source = Resources.Load<TextAsset>("TestData/" + file);
            var targetPath = Path.Combine(Application.persistentDataPath, file);
            if (!File.Exists(targetPath))
            {
                File.WriteAllBytes(targetPath, source.bytes);
            }
        }

        private static void SetupLogging()
        {
            var localDevConfig = Resources.Load<LocalDevConfig>(nameof(LocalDevConfig));
            var loggerConfig = localDevConfig != null && localDevConfig._loggerConfig
                ? localDevConfig._loggerConfig
                : Resources.Load<LoggerConfig>(nameof(LoggerConfig));
            if (loggerConfig != null)
            {
                LoggerConfig.CreateLoggerConfig(loggerConfig);
            }
            if (localDevConfig != null)
            {
                if (!string.IsNullOrWhiteSpace(localDevConfig._photonVersionOverride))
                {
                    var capturedPhotonVersionOverride = localDevConfig._photonVersionOverride;
                    PhotonLobby.GetGameVersion = () => capturedPhotonVersionOverride;
                }
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