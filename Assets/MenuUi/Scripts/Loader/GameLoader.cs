using MenuUi.Scripts.Window;
using MenuUi.Scripts.Window.ScriptableObjects;
using UnityEngine;

namespace MenuUi.Scripts.Loader
{
    /// <summary>
    /// Loader to check that game can be started with all required services running.<br />
    /// Waits until game and services has been loaded and then opens the main window.
    /// </summary>
    public class GameLoader : MonoBehaviour
    {
        private const string Tooltip1 = "First window to show after services has been loaded";

        [SerializeField, Tooltip(Tooltip1)] private WindowDef _mainWindow;

        private void Start()
        {
            Debug.Log("start");

            var windowManager = WindowManager.Get();
            Debug.Log($"show {_mainWindow}");
            windowManager.ShowWindow(_mainWindow);
            Debug.Log("exit");
        }
    }
}
