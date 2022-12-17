using System;
using System.Collections.Generic;
using System.Linq;
using Prg.Scripts.Common.Unity.Window.ScriptableObjects;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

namespace Prg.Scripts.Common.Unity.Window
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
        public class MyWindow
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

            public override string ToString()
            {
                return $"{(_windowDef != null ? _windowDef.name : "noname")}/{(_window != null ? _window.name : "noname")}";
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void SubsystemRegistration()
        {
            // Manual reset if UNITY Domain Reloading is disabled.
            _windowManager = null;
        }

        public static IWindowManager Get()
        {
            if (_windowManager == null)
            {
                _windowManager = UnitySingleton.CreateStaticSingleton<WindowManager>();
            }
            return _windowManager;
        }

        private static IWindowManager _windowManager;

        [SerializeField] private List<MyWindow> _currentWindows;
        [SerializeField] private List<MyWindow> _knownWindows;

        private GameObject _windowsParent;
        private WindowDef _pendingWindow;
        private List<Func<GoBackAction>> _goBackOnceHandler;
        private int _executionLevel;

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
            // Replace actual WindowManager with dummy so that our clients do not have to check for Application Quit themselves. 
            _windowManager = new NoOpWindowManager();
            ResetState();
        }
#endif
        private void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (_pendingWindow != null)
            {
                Debug.Log($"sceneLoaded {scene.name} ({scene.buildIndex}) pending {_pendingWindow}");
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
            if (_goBackOnceHandler == null)
            {
                _goBackOnceHandler = new List<Func<GoBackAction>>();
            }
            if (!_goBackOnceHandler.Contains(handler))
            {
                _goBackOnceHandler.Add(handler);
            }
        }

        void IWindowManager.UnRegisterGoBackHandlerOnce(Func<GoBackAction> handler)
        {
            _goBackOnceHandler?.Remove(handler);
        }

        int IWindowManager.WindowCount => _currentWindows.Count;

        List<MyWindow> IWindowManager.WindowStack => _currentWindows;

        int IWindowManager.FindIndex(WindowDef windowDef)
        {
            return _currentWindows.FindIndex(x => x._windowDef == windowDef);
        }

        void IWindowManager.GoBack()
        {
            Debug.Log($"GoBack count {_currentWindows.Count} handler {_goBackOnceHandler?.Count ?? -1}");
            if (_goBackOnceHandler != null)
            {
                // Callbacks can register again!
                var tempHandler = _goBackOnceHandler;
                _goBackOnceHandler = null;
                var goBackResult = InvokeCallbacks(tempHandler);
                if (goBackResult == GoBackAction.Abort)
                {
                    Debug.Log($"GoBack interrupted by handler");
                    return;
                }
            }
            if (_currentWindows.Count <= 1)
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

        void IWindowManager.Unwind(WindowDef unwindWindowDef)
        {
            void DoUnwind()
            {
                while (_currentWindows.Count > 1)
                {
                    var stackWindow = _currentWindows[1];
                    if (stackWindow._windowDef.Equals(unwindWindowDef))
                    {
                        break;
                    }
                    Debug.Log($"Unwind RemoveAt {stackWindow} count {_currentWindows.Count}");
                    _currentWindows.RemoveAt(1);
                }
                // Add if required - note that window prefab will not be instantiated now!
                var insertionIndex = 0;
                if (_currentWindows.Count == 1)
                {
                    var stackWindow = _currentWindows[0];
                    insertionIndex = stackWindow._windowDef.Equals(unwindWindowDef) ? -1 : 1;
                }
                else if (_currentWindows.Count > 1)
                {
                    var stackWindow = _currentWindows[1];
                    insertionIndex = stackWindow._windowDef.Equals(unwindWindowDef) ? -1 : 1;
                }
                if (insertionIndex >= 0)
                {
                    var currentWindow = new MyWindow(unwindWindowDef, null);
                    Debug.Log($"Unwind Insert {currentWindow} count {_currentWindows.Count} index {insertionIndex}");
                    _currentWindows.Insert(insertionIndex, currentWindow);
                }
            }

            if (unwindWindowDef != null)
            {
                SafeExecution("Unwind", DoUnwind);
            }
            else
            {
                _currentWindows.Clear();
            }
        }

        void IWindowManager.ShowWindow(WindowDef windowDef)
        {
            void DoShowWindow()
            {
                Assert.IsNotNull(windowDef, "windowDef != null");
                if (windowDef.NeedsSceneLoad)
                {
                    _pendingWindow = windowDef;
                    InvalidateWindows(_currentWindows);
                    SceneLoader.LoadScene(windowDef);
                    Debug.Log($"LoadWindow {windowDef} exit");
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
                var currentWindow =
                    _knownWindows.FirstOrDefault(x => windowDef.Equals(x._windowDef))
                    ?? CreateWindow(windowDef);
                if (_currentWindows.Count > 0)
                {
                    var previousWindow = _currentWindows[0];
                    // It seems that currentWindow can be previousWindow due to some misconfiguration or missing configuration
                    if (currentWindow._windowDef.Equals(previousWindow._windowDef))
                    {
                        // We must accept this fact - for now - and can not do anything about it (but remove it).
                        Debug.Log(
                            $"ShowWindow {windowDef} is already in window stack ({_currentWindows.Count}) - when it possibly should not be");
                        PopAndHide();
                    }
                    else if (previousWindow._windowDef.IsPopOutWindow)
                    {
                        PopAndHide();
                    }
                    else
                    {
                        Hide(previousWindow);
                    }
                }
                if (!currentWindow.IsValid)
                {
                    var windowName = windowDef.name;
                    Debug.Log($"CreateWindowPrefab [{windowName}] {windowDef}");
                    currentWindow._window = CreateWindowPrefab(currentWindow._windowDef);
                }
                _currentWindows.Insert(0, currentWindow);
                Show(currentWindow);
            }

            SafeExecution("DoShowWindow", DoShowWindow);
        }

        void IWindowManager.PopCurrentWindow()
        {
            if (_currentWindows.Count == 0)
            {
                return;
            }
            SafeExecution("PopCurrentWindow", PopAndHide);
        }

        private void SafeExecution(string actionName, Action action)
        {
            Assert.IsTrue(_executionLevel == 0, "_executionLevel == 0");
            _executionLevel += 1;
            Debug.Log($"SafeExecution {actionName} start count {_currentWindows.Count}");
            try
            {
                action();
            }
            catch (Exception)
            {
                _executionLevel = 0;
                throw;
            }
            Debug.Log($"SafeExecution {actionName} exit count {_currentWindows.Count}");
            _executionLevel -= 1;
            Assert.IsTrue(_executionLevel == 0, "_executionLevel == 0");
        }

        private MyWindow CreateWindow(WindowDef windowDef)
        {
            var windowName = windowDef.name;
            Debug.Log($"CreateWindow [{windowName}] {windowDef} count {_currentWindows.Count}");
            var prefab = CreateWindowPrefab(windowDef);
            var currentWindow = new MyWindow(windowDef, prefab);
            _knownWindows.Add(currentWindow);
            return currentWindow;
        }

        private GameObject CreateWindowPrefab(WindowDef windowDef)
        {
            var prefab = windowDef.WindowPrefab;
            var isSceneObject = prefab.scene.handle != 0;
            if (!isSceneObject)
            {
                prefab = _windowsParent == null
                    ? Instantiate(prefab)
                    : Instantiate(prefab, _windowsParent.transform);
                prefab.name = prefab.name.Replace("(Clone)", string.Empty);
            }
            return prefab;
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
            Debug.Log($"Show {window._windowDef}", window._window);
            window._window.SetActive(true);
        }

        private static void Hide(MyWindow window)
        {
            Debug.Log($"Hide {window._windowDef}", window._window);
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

        private static GoBackAction InvokeCallbacks(List<Func<GoBackAction>> callbackList)
        {
            var goBackResult = GoBackAction.Continue;
            foreach (var func in callbackList)
            {
                var result = func();
                Debug.Log($"invokeResult {func} = {result}");
                if (result == GoBackAction.Abort)
                {
                    goBackResult = GoBackAction.Abort;
                }
            }
            Debug.Log($"InvokeCallbacks : {goBackResult}");
            return goBackResult;
        }

        /// <summary>
        /// No-op implementation when actual implementation is not available.
        /// </summary>
        /// <remarks>
        /// This can happen during app exit.
        /// </remarks>
        private class NoOpWindowManager : IWindowManager
        {
            public void RegisterGoBackHandlerOnce(Func<GoBackAction> handler)
            {
                // NOP
            }

            public void UnRegisterGoBackHandlerOnce(Func<GoBackAction> handler)
            {
                // NOP
            }

            public int WindowCount => 0;

            public List<MyWindow> WindowStack => new();

            public int FindIndex(WindowDef windowDef) => -1;

            public void GoBack()
            {
                // NOP
            }

            public void Unwind(WindowDef windowDef)
            {
                // NOP
            }

            public void ShowWindow(WindowDef windowDef)
            {
                // NOP
            }

            public void PopCurrentWindow()
            {
                // NOP
            }

            public void SetWindowsParent(GameObject windowsParent)
            {
                // NOP
            }
        }
    }
}