using Altzone.Scripts.Config;
using UnityEngine;

namespace Altzone.Editor.GameDebug
{
    internal static class DebugMenu
    {
        public static void ShowLocalPlayerSettings()
        {
            Debug.Log("*");
            Debug.Log(GameConfig.Get().PlayerSettings);
        }

        public static void SetPhotonRegionToDefault()
        {
            Debug.Log("*");
            GameConfig.Get().PlayerSettings.PhotonRegion = string.Empty;
        }

        public static void SetPhotonRegionToEu()
        {
            Debug.Log("*");
            GameConfig.Get().PlayerSettings.PhotonRegion = "eu";
        }

        public static void SetPhotonRegionToUs()
        {
            Debug.Log("*");
            GameConfig.Get().PlayerSettings.PhotonRegion = "us";
        }

        public static void SetPhotonRegionToAsia()
        {
            Debug.Log("*");
            GameConfig.Get().PlayerSettings.PhotonRegion = "asia";
        }
    }
}
