using System.Collections;
using Altzone.Scripts;
using Prg.Scripts.Common.Unity.Window;
using Prg.Scripts.Common.Unity.Window.ScriptableObjects;
using UnityEngine;

namespace MenuUi.Scripts.Loader
{
    /// <summary>
    /// Loader to check that game can be started with all required services.<br />
    /// Waits until game and services has been loaded and then opens the main window.
    /// </summary>
    public class GameLoader : MonoBehaviour
    {
        private const string Tooltip1 = "First window to show after services has been loaded";
        private const float ServiceTimeoutTime = 3f;

        [SerializeField, Tooltip(Tooltip1)] private WindowDef _mainWindow;

        private IStorefront _storefront;
        private float _timeOutTime;

        private IEnumerator Start()
        {
            Debug.Log("loading");
            _storefront = Storefront.Get();
            _timeOutTime = Time.time + ServiceTimeoutTime;
            yield return new WaitUntil(AllServicesAreRunning);
            var windowManager = WindowManager.Get();
            Debug.Log($"show {_mainWindow}");
            windowManager.ShowWindow(_mainWindow);
        }

        private bool AllServicesAreRunning()
        {
            if (Time.time > _timeOutTime)
            {
                Debug.Log($"timeout {ServiceTimeoutTime:0.0}");
                return true;
            }
            return _storefront.IsInventoryConnected && _storefront.IsGameServerConnected;
        }
    }
}