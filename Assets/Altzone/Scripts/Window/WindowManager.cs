using System;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.Window.ScriptableObjects;
using Prg.Scripts.Common.Unity;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

namespace Altzone.Scripts.Window
{
    /// <summary>
    /// Simple <c>WindowManager</c> with managed window bread crumbs list.
    /// </summary>
    public class WindowManager : MonoBehaviour, IWindowManager
    {
        public enum GoBackAction
        {
            Continue,
            Abort
        }

        [Serializable]
        private class MyWindow
        {
            public WindowDef _windowDef;
            public GameObject _window;

            public bool IsValid => _window != null;

            public void Invalidate()
            {
                _window = null;
            }

            public MyWindow(WindowDef windowDef, GameObject window)
            {
                _windowDef = windowDef;
                _window = window;
            }
        }

        public static IWindowManager Get() => FindObjectOfType<WindowManager>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void BeforeSceneLoad()
        {
            var windowManager = Get();
            if (windowManager == null)
            {
                UnityExtensions.CreateGameObjectAndComponent<WindowManager>(nameof(WindowManager), true);
            }
        }

        [SerializeField] private List<MyWindow> _currentWindows;
        [SerializeField] private List<MyWindow> _knownWindows;

        private GameObject _windowsParent;
        private WindowDef _pendingWindow;
        private Func<GoBackAction> _goBackOnceHandler;

        private void Awake()
        {
            Debug.Log("Awake");
            _currentWindows = new List<MyWindow>();
            _knownWindows = new List<MyWindow>();
            SceneManager.sceneLoaded += SceneLoaded;
            SceneManager.sceneUnloaded += SceneUnloaded;
            var handler = gameObject.AddComponent<EscapeKeyHandler>();
            handler.SetCallback(EscapeKeyPressed);
            ResetState();
        }

        private void ResetState()
        {
            _currentWindows.Clear();
            _knownWindows.Clear();
            _pendingWindow = null;
            _goBackOnceHandler = null;
        }

#if UNITY_EDITOR
        private void OnApplicationQuit()
        {
            ResetState();
        }
#endif
        private void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"sceneLoaded {scene.name} ({scene.buildIndex}) pending {_pendingWindow}");
            if (_pendingWindow != null)
            {
                ((IWindowManager)this).ShowWindow(_pendingWindow);
                _pendingWindow = null;
            }
        }

        private void SceneUnloaded(Scene scene)
        {
            Debug.Log($"sceneUnloaded {scene.name} ({scene.buildIndex}) prefabCount {_knownWindows.Count} pending {_pendingWindow}");
            _knownWindows.Clear();
            _windowsParent = null;
            _goBackOnceHandler = null;
        }

        void IWindowManager.SetWindowsParent(GameObject windowsParent)
        {
            _windowsParent = windowsParent;
        }

        private void EscapeKeyPressed()
        {
            ((IWindowManager)this).GoBack();
        }

        void IWindowManager.RegisterGoBackHandlerOnce(Func<GoBackAction> handler)
        {
            _goBackOnceHandler += handler;
        }

        void IWindowManager.GoBack()
        {
            Debug.Log($"GoBack {_currentWindows.Count} handler {_goBackOnceHandler?.GetInvocationList().Length ?? -1}");
            if (_goBackOnceHandler != null)
            {
                var goBackResult = InvokeCallbacks(_goBackOnceHandler);
                _goBackOnceHandler = null;
                if (goBackResult == GoBackAction.Abort)
                {
                    Debug.Log($"GoBack interrupted by handler");
                    return;
                }
            }
            if (_currentWindows.Count == 1)
            {
                ExitApplication.ExitGracefully();
                return;
            }
            PopAndHide();
            if (_currentWindows.Count == 0)
            {
                Debug.Log($"NOTE! GoBack has no windows to show");
                return;
            }
            var currentWindow = _currentWindows[0];
            if (currentWindow.IsValid)
            {
                Show(currentWindow);
                return;
            }
            // Re-create the window
            _currentWindows.RemoveAt(0);
            ((IWindowManager)this).ShowWindow(currentWindow._windowDef);
        }

        void IWindowManager.ShowWindow(WindowDef windowDef)
        {
            Debug.Log($"LoadWindow {windowDef} count {_currentWindows.Count}");
            Assert.IsNotNull(windowDef, "windowDef != null");
            if (windowDef.NeedsSceneLoad)
            {
                _pendingWindow = windowDef;
                InvalidateWindows(_currentWindows);
                SceneLoader.LoadScene(windowDef);
                return;
            }
            if (_pendingWindow != null && !_pendingWindow.Equals(windowDef))
            {
                Debug.Log($"LoadWindow IGNORE {windowDef} PENDING {_pendingWindow}");
                return;
            }
            if (IsVisible(windowDef))
            {
                Debug.Log($"LoadWindow ALREADY IsVisible {windowDef}");
                return;
            }
            var currentWindow = _knownWindows.FirstOrDefault(x => windowDef.Equals(x._windowDef));
            if (currentWindow == null)
            {
                currentWindow = CreateWindow(windowDef);
            }
            if (_currentWindows.Count > 0)
            {
                var previousWindow = _currentWindows[0];
                Assert.IsFalse(currentWindow._windowDef.Equals(previousWindow._windowDef));
                if (previousWindow._windowDef.IsPopOutWindow)
                {
                    PopAndHide();
                }
                else
                {
                    Hide(previousWindow);
                }
            }
            _currentWindows.Insert(0, currentWindow);
            Show(currentWindow);
        }

        void IWindowManager.PopCurrentWindow()
        {
            Debug.Log($"PopCurrentWindow count {_currentWindows.Count}");
            PopAndHide();
        }

        private MyWindow CreateWindow(WindowDef windowDef)
        {
            var windowName = windowDef.name;
            Debug.Log($"CreateWindow [{windowName}] {windowDef} count {_currentWindows.Count}");
            var prefab = windowDef.WindowPrefab;
            var isSceneObject = prefab.scene.handle != 0;
            if (!isSceneObject)
            {
                prefab = Instantiate(prefab);
                if (_windowsParent != null)
                {
                    prefab.transform.SetParent(_windowsParent.transform);
                }
                prefab.name = prefab.name.Replace("(Clone)", "");
            }
            var currentWindow = new MyWindow(windowDef, prefab);
            _knownWindows.Add(currentWindow);
            return currentWindow;
        }

        private void PopAndHide()
        {
            Assert.IsTrue(_currentWindows.Count > 0, "_currentWindows.Count > 0");
            var firstWindow = _currentWindows[0];
            _currentWindows.RemoveAt(0);
            Hide(firstWindow);
        }

        private static void Show(MyWindow window)
        {
            Debug.Log($"Show {window._windowDef}");
            window._window.SetActive(true);
        }

        private static void Hide(MyWindow window)
        {
            Debug.Log($"Hide {window._windowDef}");
            if (window.IsValid)
            {
                window._window.SetActive(false);
            }
        }

        private bool IsVisible(WindowDef windowDef)
        {
            if (_currentWindows.Count == 0)
            {
                return false;
            }
            var firstWindow = _currentWindows[0];
            var isVisible = windowDef.Equals(firstWindow._windowDef) && firstWindow.IsValid;
            Debug.Log($"IsVisible new {windowDef} first {firstWindow} : {isVisible}");
            return isVisible;
        }

        private static void InvalidateWindows(List<MyWindow> windowList)
        {
            foreach (var window in windowList)
            {
                window.Invalidate();
            }
        }

        private static GoBackAction InvokeCallbacks(Func<GoBackAction> func)
        {
            var goBackResult = GoBackAction.Continue;
            var invocationList = func.GetInvocationList();
            foreach (var handler in invocationList)
            {
                var invokeResult = handler.DynamicInvoke();
                Debug.Log($"invokeResult {handler} = {invokeResult}");
                if (invokeResult is GoBackAction result && result == GoBackAction.Abort)
                {
                    goBackResult = GoBackAction.Abort;
                }
            }
            Debug.Log($"InvokeCallbacks : {goBackResult}");
            return goBackResult;
        }
    }
}