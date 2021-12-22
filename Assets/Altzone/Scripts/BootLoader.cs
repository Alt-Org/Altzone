using System.Diagnostics;
using System.Globalization;
using System.Threading;
using Altzone.Scripts.Config.ScriptableObjects;
using Prg.Scripts.Common.Photon;
using Prg.Scripts.Common.Unity;
using Prg.Scripts.Common.Util;
using UnityEngine;

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
        }

        [Conditional("UNITY_EDITOR")]
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
            UnityEngine.Debug.Log($"Photon {RichText.Brown(PhotonLobby.GameVersion)}");
        }
    }
}