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
        }

        public static void SetLanguageToEnglish()
        {
            Debug.Log("*");
            GameConfig.Get().PlayerSettings.SetLanguageToEnglish();
        }
    }
}