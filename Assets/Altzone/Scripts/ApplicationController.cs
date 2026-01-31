using System.Globalization;
using System.Text;
using System.Threading;

using UnityEngine;
using Debug = UnityEngine.Debug;

using Altzone.Scripts.Config.ScriptableObjects;
using Altzone.Scripts.AzDebug;
using Altzone.Scripts.GA;
using System.Linq;
using Prg.Scripts.Common;

namespace Altzone.Scripts
{
    public class ApplicationController : MonoBehaviour
    {
        public static readonly int VersionNumber = 209;

        /// <summary>
        /// Starts the game when (before) first scene is loaded.
        /// </summary>
        /// <remarks>
        /// Test and production functionality is not quite there yet, this needs more works to separate test things from production.
        /// </remarks>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnApplicationStart()
        {
            UnitySingleton.CreateStaticSingleton<ApplicationController>();

            // Setup loggin and testing first before proceeding to load the game.
            LoggingSetup();
            Debug.Log("[ApplicationController] OnApplicationStart");
#if UNITY_EDITOR
            PrepareLocalTesting();
#endif

            string startupMessage = new StringBuilder()
                .Append(" Game ").Append(Application.productName)
                .Append(" Version ").Append(Application.version)
                //.Append(" Photon ").Append(PhotonLobby.GameVersion)
                .Append(" IsSimulator ").Append(AppPlatform.IsSimulator)
                .Append(" Screen ").Append(Screen.currentResolution)
                .ToString();
            Debug.Log(startupMessage);

            AltzoneBattleLink.Init();

            ClickStateHandler.Init();

            InstantiateGlobalGameObjects();
        }

        private void OnApplicationPause(bool pause)
        {
            Debug.LogFormat("[ApplicationController] OnApplicationPause ({0})", pause);
        }

        private void OnApplicationQuit()
        {
            Debug.Log("[ApplicationController] OnApplicationQuit");

            LoggingEnd();
        }

        private static void InstantiateGlobalGameObjects()
        {
            //Instantiating object from AltZoneResources/Prefabs/GlobalObjects folder. This folder should only contain global prefabs that are instantiated on startup.
            foreach(Object prefab in Resources.LoadAll("Prefabs/GlobalPrefabs").ToList())
            {
                Instantiate(prefab);
            }
        }

        private static void LoggingSetup()
        {
            DebugLogFileHandler.Init(DebugLogFileHandler.ContextID.MenuUI);
            DebugLogFileHandler.FileOpen();
        }

        private static void LoggingEnd()
        {
            DebugLogFileHandler.Exit();
        }

#if UNITY_EDITOR
        private static void PrepareLocalTesting()
        {
            LocalTestingSetEditorStatus();
            LocalTestingSetup();
        }

        private static void LocalTestingSetup()
        {
            LocalDevConfig localDevConfig = Resources.Load<LocalDevConfig>(nameof(LocalDevConfig));
            if (localDevConfig == null) return;

            Application.targetFrameRate = localDevConfig._targetFrameRateOverride;
        }

        private static void LocalTestingSetEditorStatus()
        {
            // This is just for debugging to get strings (numbers) formatted consistently
            // - everything that goes to UI should go through Localizer using player's locale preferences
            CultureInfo ci = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentCulture = ci;
            Thread.CurrentThread.CurrentUICulture = ci;
        }
#endif
    }
}
