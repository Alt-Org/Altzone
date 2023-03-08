using System.Collections;
using Altzone.Scripts;
using Altzone.Scripts.Model;
using Prg.Scripts.Common.Unity.Window;
using Prg.Scripts.Common.Unity.Window.ScriptableObjects;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MenuUi.Scripts.Loader
{
    /// <summary>
    /// Loader to check that game can be started with all required services.<br />
    /// Waits until game and services has been loaded and then opens the main window.
    /// </summary>
    public class GameLoader : MonoBehaviour
    {
        private const string Tooltip1 = "First window to show after services has been loaded";

        [SerializeField, Tooltip(Tooltip1)] private WindowDef _mainWindow;

        private IStorefront _storefront;
        private IEnumerator Start()
        {
            Debug.Log("loading");
            _storefront = Storefront.Get();
            yield return new WaitUntil(AllServicesAreRunning);
            var windowManager = WindowManager.Get();
            Debug.Log($"show {_mainWindow}");
            windowManager.ShowWindow(_mainWindow);
        }

        private bool AllServicesAreRunning()
        {
            return _storefront.IsInventoryConnected && _storefront.IsGameServerConnected;
        }
    }
}