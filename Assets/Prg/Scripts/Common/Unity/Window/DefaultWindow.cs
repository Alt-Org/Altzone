using System.Diagnostics;
using Prg.Scripts.Common.Unity.Window.ScriptableObjects;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

namespace Prg.Scripts.Common.Unity.Window
{
    /// <summary>
    /// Initial window loader for any level that uses <c>WindowManager</c>.
    /// </summary>
    public class DefaultWindow : MonoBehaviour
    {
        [SerializeField] private WindowDef _window;
        [SerializeField] private bool _findWindow;

        private void OnEnable()
        {
            if (_findWindow)
            {
                FindWindow();
            }
            Debug.Log($"OnEnable {_window}");
            var windowManager = WindowManager.Get();
            windowManager.SetWindowsParent(gameObject);
            Assert.IsNotNull(_window.WindowPrefab, "_window.WindowPrefab != null");
            windowManager.ShowWindow(_window);
        }

        /// <summary>
        /// Create new window for first <c>Canvas</c> in current scene.
        /// </summary>
        private void FindWindow()
        {
            GameObject FindLastCanvas()
            {
                var currentScene = SceneManager.GetActiveScene();
                var rootGameObjects = currentScene.GetRootGameObjects();
                var index = rootGameObjects.Length;
                while (--index >= 0)
                {
                    var child = rootGameObjects[index];
                    if (child.GetComponent<Canvas>() != null)
                    {
                        return child;
                    }
                }
                return null;
            }

            _window = ScriptableObject.CreateInstance<WindowDef>();
            _window.SetWindowPrefab(FindLastCanvas());
        }
    }
}