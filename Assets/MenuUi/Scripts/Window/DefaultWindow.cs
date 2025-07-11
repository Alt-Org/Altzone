﻿using System.Collections.ObjectModel;
using System.Diagnostics;
using Altzone.Scripts;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.ReferenceSheets;
using MenuUi.Scripts.Window.ScriptableObjects;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace MenuUi.Scripts.Window
{
    /// <summary>
    /// Initial window loader for any level that uses <c>WindowManager</c>.
    /// </summary>
    public class DefaultWindow : MonoBehaviour
    {
        private const string Tooltip1 = "First window to show after scene has been loaded";
        private const string Tooltip2 = "Automatically find the bottom-most active window (Canvas) in the scene (for testing)";

        [SerializeField, Tooltip(Tooltip1)] private WindowDef _window;
        [SerializeField, Tooltip(Tooltip2)] private bool _findWindowForEditor;
        [SerializeField] private StorageFurnitureReference _furnitureReference;

        private void Awake()
        {
            if (_furnitureReference != null)
            { 
                ReadOnlyCollection<GameFurniture> baseFurniture = null;
                Storefront.Get().GetAllGameFurnitureYield(callback => baseFurniture = callback);
                if (baseFurniture == null || baseFurniture.Count < 1)
                    Storefront.Get().SetFurniture(_furnitureReference.GetAllGameFurniture());
            }
        }

        private void OnEnable()
        {
            if (_findWindowForEditor)
            {
                FindWindowForEditor();
            }
            Debug.Log($"{_window}", _window);
            var windowManager = WindowManager.Get();
            windowManager.SetWindowsParent(gameObject);
            Assert.IsNotNull(_window.WindowPrefab, "_window.WindowPrefab != null");
            windowManager.ShowWindow(_window);
        }

        /// <summary>
        /// Create new <c>WindowDef</c> for the last active <c>Canvas</c> found in current scene.
        /// </summary>
        [Conditional("UNITY_EDITOR")]
        private void FindWindowForEditor()
        {
            GameObject FindLastCanvas()
            {
                var currentScene = SceneManager.GetActiveScene();
                var rootGameObjects = currentScene.GetRootGameObjects();
                var index = rootGameObjects.Length;
                while (--index >= 0)
                {
                    var child = rootGameObjects[index];
                    var canvas = child.GetComponentInChildren<Canvas>();
                    if (canvas != null && canvas.isActiveAndEnabled)
                    {
                        Debug.Log($"found {child.GetFullPath()}");
                        return child;
                    }
                }
                return null;
            }

            var canvas = FindLastCanvas();
            if (canvas == null)
            {
                return;
            }
            _window = ScriptableObject.CreateInstance<WindowDef>();
            _window.SetWindowPrefab(canvas);
        }
    }
}
