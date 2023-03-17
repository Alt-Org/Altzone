using UnityEngine;
using System.Collections;
using Altzone.Scripts;
using MenuUi.Scripts.Window;
using MenuUi.Scripts.Window.ScriptableObjects;

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

        private float _timeOutTime;

        private IEnumerator Start()
        {
            Debug.Log("start");
            
            ServiceLoader serviceLoader = null;
            yield return new WaitUntil(() => (serviceLoader = FindObjectOfType<ServiceLoader>()) != null);
            yield return new WaitUntil(() => serviceLoader.isReady);

            var windowManager = WindowManager.Get();
            Debug.Log($"show {_mainWindow}");
            windowManager.ShowWindow(_mainWindow);
            Debug.Log("exit");
        }
    }
}