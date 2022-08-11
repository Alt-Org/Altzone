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
        private const string Tooltip1 = "First window to show after scene has been loaded";
        private const string Tooltip2 = "Find the bottom-most window (Canvas) in the scene (for testing)";

        [SerializeField, Tooltip(Tooltip1)] private WindowDef _window;
        [SerializeField, Tooltip(Tooltip2)] private bool _findWindow;

        private void OnEnable()
        {
            if (_findWindow)
            {
                FindWindow();
            }
            Debug.Log($"{_window}");
            var windowManager = WindowManager.Get();
            windowManager.SetWindowsParent(gameObject);
            Assert.IsNotNull(_window.WindowPrefab, "_window.WindowPrefab != null");
            windowManager.ShowWindow(_window);
        }

        /// <summary>
        /// Create new window for the last <c>Canvas</c> found in current scene.
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
                    if (child.GetComponentInChildren<Canvas>() != null)
                    {
                        Debug.Log($"found {child.GetFullPath()}");
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