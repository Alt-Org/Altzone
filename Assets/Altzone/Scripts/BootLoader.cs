using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;
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
            PlatformInfo();
        }

        /// <summary>
        /// Throw error is platform is not known, supported or tested. 
        /// </summary>
        /// <remarks>
        /// https://docs.unity3d.com/Manual/PlatformDependentCompilation.html
        /// </remarks>
        private static void PlatformInfo()
        {
#if UNITY_EDITOR
            return;
#elif UNITY_STANDALONE_WIN
            return;
#elif UNITY_STANDALONE_LINUX
            return;
#elif UNITY_STANDALONE_OSX
            return;
#elif UNITY_ANDROID
            return;
#elif UNITY_IOS
            return;
#elif UNITY_WEBGL
            return;
#else
            // This is 'harmless' but gets logged into analytics system for resolving.
            throw new UnityException($"Platform {Application.platform} is not supported or tested");
#endif
        }

        private static void PrepareLocalTesting()
        {
            var localDevConfig = Resources.Load<LocalDevConfig>(nameof(LocalDevConfig));
            SetEditorStatus(localDevConfig._targetFrameRateOverride);
            SetupLogging(localDevConfig);
        }

        private static void PrepareDevice()
        {
            // Nothing special to do here, ServiceLoader & co takes care of things for now.
        }

        private static void SetupLogging(LocalDevConfig localDevConfig)
        {
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
        private static void SetEditorStatus(int targetFrameRate)
        {
            // This is just for debugging to get strings (numbers) formatted consistently
            // - everything that goes to UI should go through Localizer using player's locale preferences
            var ci = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
            Application.targetFrameRate = targetFrameRate;
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