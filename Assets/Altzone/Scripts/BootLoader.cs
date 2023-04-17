using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;
using Altzone.Scripts.Config.ScriptableObjects;
using Prg.Scripts.Common.Photon;
using Prg.Scripts.Common.Util;
using UnityEngine;

namespace Altzone.Scripts
{
    internal static class BootLoader
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void BeforeSceneLoad()
        {
            // Setup testing first before proceeding to load the game.
            PrepareLocalTesting();
            
            var startupMessage = new StringBuilder()
                .Append(" Game ").Append(Application.productName)
                .Append(" Version ").Append(Application.version)
                .Append(" Photon ").Append(PhotonLobby.GameVersion)
                .Append(" IsSimulator ").Append(AppPlatform.IsSimulator)
                .Append(" Screen ").Append(Screen.currentResolution)
                .ToString();
            Debug.Log(startupMessage);
            PrepareLocalDevice();
            UnitySingleton.CreateStaticSingleton<ServiceBootLoader>();
            PlatformInfo();
        }

        /// <summary>
        /// Device specific local setup before game is started.
        /// </summary>
        private static void PrepareLocalDevice()
        {
            // Nothing special to do here, ServiceLoader & co takes care of things for now.
        }

        /// <summary>
        /// Throws informative exception if is platform is not officially supported. 
        /// </summary>
        /// <remarks>
        /// This list should contain all possible defines for platform specific compilation.
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
            throw new UnityException($"INFO: this {Application.platform} platform is not officially supported");
#endif
        }

        private static void PrepareLocalTesting()
        {
            var localDevConfig = Resources.Load<LocalDevConfig>(nameof(LocalDevConfig));
            SetEditorStatus(localDevConfig);
            SetupLogging(localDevConfig);
            SetupLocalTesting(localDevConfig);
        }

        private static void SetupLogging(LocalDevConfig localDevConfig)
        {
            LoggerConfig loggerConfig = null;
            if (localDevConfig != null && localDevConfig._loggerConfig != null)
            {
                // Local override.
                loggerConfig = localDevConfig._loggerConfig;
            }
            if (loggerConfig == null)
            {
                // Default settings.
                loggerConfig = Resources.Load<LoggerConfig>(nameof(LoggerConfig));
            }
            if (loggerConfig != null)
            {
                LoggerConfig.CreateLoggerFilterConfig(loggerConfig);
            }
        }

        private static void SetupLocalTesting(LocalDevConfig localDevConfig)
        {
            if (localDevConfig == null)
            {
                return;
            }
            if (!string.IsNullOrWhiteSpace(localDevConfig._photonVersionOverride))
            {
                var capturedPhotonVersionOverride = localDevConfig._photonVersionOverride;
                PhotonLobby.GetGameVersion = () => capturedPhotonVersionOverride;
            }
        }
        
        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD"), Conditional("FORCE_LOG")]
        private static void SetEditorStatus(LocalDevConfig localDevConfig)
        {
            // This is just for debugging to get strings (numbers) formatted consistently
            // - everything that goes to UI should go through Localizer using player's locale preferences
            var ci = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
            if (localDevConfig != null && localDevConfig._targetFrameRateOverride != -1)
            {
                Application.targetFrameRate = localDevConfig._targetFrameRateOverride;
            }
            else
            {
                Application.targetFrameRate = -1;
            }
        }
    }
}