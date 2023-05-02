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

        public static void ResetPlayerSettings()
        {
            Debug.Log("*");
            GameConfig.Get().PlayerSettings.ResetPlayerSettings();
            Debug.Log(GameConfig.Get().PlayerSettings);
        }

        public static void SetPhotonRegionToDefault()
        {
            Debug.Log("*");
            GameConfig.Get().PlayerSettings.PhotonRegion = string.Empty;
            Debug.Log(GameConfig.Get().PlayerSettings);
        }

        public static void SetPhotonRegionToEu()
        {
            Debug.Log("*");
            GameConfig.Get().PlayerSettings.PhotonRegion = "eu";
            Debug.Log(GameConfig.Get().PlayerSettings);
        }

        public static void SetPhotonRegionToUs()
        {
            Debug.Log("*");
            GameConfig.Get().PlayerSettings.PhotonRegion = "us";
            Debug.Log(GameConfig.Get().PlayerSettings);
        }

        public static void SetPhotonRegionToAsia()
        {
            Debug.Log("*");
            GameConfig.Get().PlayerSettings.PhotonRegion = "asia";
            Debug.Log(GameConfig.Get().PlayerSettings);
        }

        public static void SetLanguageToEnglish()
        {
            Debug.Log("*");
            GameConfig.Get().PlayerSettings.SetLanguageToEnglish();
            Debug.Log(GameConfig.Get().PlayerSettings);
        }
    }
}