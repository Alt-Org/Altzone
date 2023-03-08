using UnityEditor;
using UnityEngine;

namespace Prg.Scripts.Common.Unity
{
    public static class ExitApplication
    {
        public static void ExitGracefully()
        {
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
#if UNITY_EDITOR
                Debug.Log(RichText.Yellow("stop playing"));
                EditorApplication.isPlaying = false;
#endif
                return;
            }
            if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                // NOP - There is no API provided for gracefully terminating an iOS application.
                return;
            }
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                // NOP - no can do in browser
                return;
            }
            // Android, desktop, etc. goes here
            Application.Quit(0);
        }
    }
}