using System.Diagnostics;
using Altzone.Scripts.Window.ScriptableObjects;
using UnityEngine;
using UnityEngine.Assertions;

namespace Altzone.Scripts.Window
{
    /// <summary>
    /// Initial window loader for any level that uses <c>WindowManager</c>.
    /// </summary>
    public class DefaultWindow : MonoBehaviour
    {
        [SerializeField] private WindowDef _window;
        [SerializeField] private GameObject _sceneTestWindow;

        private void OnEnable()
        {
            SetEditorStatus();

            Debug.Log($"OnEnable {_window}");
            var windowManager = WindowManager.Get();
            windowManager.SetWindowsParent(gameObject);
            Assert.IsNotNull(_window.WindowPrefab, "_window.WindowPrefab != null");
            windowManager.ShowWindow(_window);
        }

        [Conditional("UNITY_EDITOR")]
        private void SetEditorStatus()
        {
            if (_window.WindowPrefab == null)
            {
                _window.SetWindowPrefab(_sceneTestWindow);
            }
        }
    }
}