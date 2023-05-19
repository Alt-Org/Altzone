using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Threading;
using Altzone.Scripts.Config.ScriptableObjects;
using Prg.Scripts.Common.Photon;
using Prg.Scripts.Common.Util;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace Altzone.Scripts
{
    internal static class BootLoader
    {
        /// <summary>
        /// Starts the game when (before) first scene is loaded.
        /// </summary>
        /// <remarks>
        /// Test and production functionality is not quite there yet, this needs more works to separate test things from production.
        /// </remarks>
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
        }

        private static void PrepareLocalTesting()
        {
            SetEditorStatus();
            var localDevConfig = Resources.Load<LocalDevConfig>(nameof(LocalDevConfig));
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
                LoggerConfig.CreateLoggerFilterConfig(loggerConfig, localDevConfig != null ? localDevConfig.SetLoggedDebugTypes : null);
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
#if UNITY_EDITOR
            if (localDevConfig != null && localDevConfig._targetFrameRateOverride != -1)
            {
                Application.targetFrameRate = localDevConfig._targetFrameRateOverride;
            }
            else
            {
                Application.targetFrameRate = -1;
            }
#endif
        }

        [Conditional("UNITY_EDITOR")]
        private static void SetEditorStatus()
        {
            // This is just for debugging to get strings (numbers) formatted consistently
            // - everything that goes to UI should go through Localizer using player's locale preferences
            var ci = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
        }
    }
}
