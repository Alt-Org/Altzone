using Prg.Scripts.Common.Unity;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UiProto.Scripts.Window
{
    public class WindowManager : MonoBehaviour, IWindowManager
    {
        public static WindowManager _Instance;

        public static IWindowManager Get()
        {
            return _Instance;
        }

        [Header("Settings"), SerializeField] private WindowInstance initialWindow;
        [SerializeField] private WindowConfig config;

        [Header("Testing")] public bool useHighlightedColor;
        public Color highlightedColor = Color.white;

        [Header("Errors"), SerializeField] private bool showConfigErrors;
        [SerializeField] private Color configErrorColor = Color.white;
        [SerializeField] private bool configErrorInteractable;

        [Header("Live Data"), SerializeField] private GameObject _currentWindow;
        [SerializeField] private List<GameObject> knownWindowsList;
        [SerializeField] private WindowStack stackManager;

        private readonly Dictionary<string, GameObject> knownWindows = new Dictionary<string, GameObject>();

        private void Awake()
        {
            if (_Instance == null)
            {
                // Register us as the singleton!
                _Instance = this;
                stackManager = WindowStack.Instance; // Lazy load
                return;
            }
            Destroy(this);
            throw new UnityException("WindowManager already created");
        }

        private void OnDestroy()
        {
            if (_Instance == this)
            {
                _Instance = null;
            }
        }

        private void Start()
        {
            Debug.LogFormat("Start with config: {0} initial {1}", config.name, initialWindow.window);
            Debug.LogFormat("config: #{0} {1}", config.windows.Count, string.Join("|", config.windows));
            WindowStack.dumpWindowStack();
            if (stackManager.hasPendingWindow)
            {
                showWindow(stackManager.popPending());
                return;
            }
            if (initialWindow.window.windowId == 0)
            {
                // NOP - something is already on the scene and we do not want to load more!
                return;
            }
            showWindow(initialWindow.window, new LevelId());
        }

        private IWindow currentWindow;
        IWindow IWindowManager.CurrentWindow => currentWindow;

        IWindow IWindowManager.ShowWindow(WindowId windowId, LevelId levelId)
        {
            // TODO: make this void when IWindow is refactored to IWindowHandle, we do not need IWindow functionality (as we take care of everything)!?
            showWindow(windowId, levelId);
            return currentWindow;
        }

        public static void SafeGoBack()
        {
            // Sometimes WindowManager is not used but we want to have central point for safe "go back" window navigation functionality
            Debug.Log($"SafeGoBack level={SceneManager.GetActiveScene().name}");
            if (_Instance != null)
            {
                _Instance.GoBack();
                return;
            }
            var windowStack = WindowStack.Instance;
            if (windowStack != null && windowStack.windowCount > 0)
            {
                var windowId = windowStack.pop();
                var levelName = windowId.levelName;
                SceneLoader.LoadScene(levelName);
                return;
            }
            ExitApplication.ExitGracefully();
        }

        public void GoBack()
        {
            Debug.Log($"GoBack {stackManager.ToStringFull()}");
            if (stackManager.windowCount > 0)
            {
                // Remove current window
                stackManager.pop();
            }
            if (stackManager.windowCount > 0)
            {
                // Load previous
                var windowId = stackManager.pop();
                var window = config.windows.FirstOrDefault(x => x.window.windowName == windowId.windowName);
                if (window != null)
                {
                    showWindow(window.window, windowId.levelName);
                    return;
                }
            }
            ExitApplication.ExitGracefully();
        }

        private void showWindow(StackEntry entry)
        {
            Debug.Log($"showWindow {entry}");
            var windows = WindowNames.getWindows();
            var window = windows.FirstOrDefault(x => x.windowName == entry.windowName);
            if (window != null)
            {
                var windowInstance = getWindowInstance(window);
                if (windowInstance.windowTemplate != null)
                {
                    showWindow(new WindowId { windowId = window.windowId, }, new LevelId());
                    return;
                }
                stackManager.push("placeholder", "");
                return; // Can not show - level is started without any window!
            }
            throw new UnityException(White("WindowNames") + ": window name not found for: " + Red(entry.ToString()));
        }

        private void showWindow(WindowId window, string levelName)
        {
            Debug.Log($"showWindow {window} {levelName}");
            var currentSceneName = SceneManager.GetActiveScene().name;
            if (!string.IsNullOrEmpty(levelName) && levelName != currentSceneName)
            {
                var level = SceneLoader.GetLevelId(levelName);
                if (level != null)
                {
                    showWindow(window, level);
                    return;
                }
                throw new UnityException(White("WindowConfig") + ": level not found for: " + Red(levelName));
            }
            showWindow(window);
        }

        private void showWindow(WindowId window, LevelId levelId)
        {
            Debug.Log($"showWindow window={window} levelId={levelId}");
            var windows = WindowNames.getWindows();
            var windowIndex = windows.FindIndex(x => x.windowId == window.windowId);
            if (windowIndex != -1)
            {
                if (levelId.levelId > 0)
                {
                    var level = LevelNames.getLevel(levelId.levelId);
                    if (level == null)
                    {
                        throw new UnityException(White("WindowNames") + ": level not found for: " + Red(levelId.ToString()));
                    }
                    stackManager.pushPending(window.windowName);
                    SceneLoader.LoadScene(level);
                    return;
                }
                showWindow(windows[windowIndex]);
                return;
            }
            throw new UnityException(White("WindowNames") + ": window name not found for: " + Red(window.ToString()));
        }

        private void showWindow(WindowId window)
        {
            var windowInstance = getWindowInstance(window);
            if (windowInstance.windowTemplate == null)
            {
                Debug.Log($"showWindow WindowId={window}");
                throw new UnityException(White("WindowConfig") + ": invalid window template: " + Red(windowInstance.ToString()));
            }
            var prevWindow = _currentWindow;
            _currentWindow = loadWindow(gameObject, windowInstance);
            currentWindow = _currentWindow.GetOrAddComponent<WindowHandle>();
            if (currentWindow.windowId == null)
            {
                currentWindow.Initialize(_currentWindow, window);
            }
            Debug.Log($"showWindow {currentWindow}");
            if (prevWindow != null)
            {
                prevWindow.SetActive(false);
            }
            if (stackManager.windowCount > 0 && isMenuHub(_currentWindow))
            {
                Debug.Log($"clear window stack for: {_currentWindow}");
                stackManager.clear();
            }
            stackManager.push(window.windowName, SceneManager.GetActiveScene().name);
            _currentWindow.SetActive(true);
        }

        private GameObject loadWindow(GameObject parent, WindowInstance window)
        {
            var windowName = window.window.windowName;
            if (knownWindows.TryGetValue(windowName, out var root))
            {
                return root;
            }
            Debug.LogFormat("loadWindow {0}", $"{White(window.window.ToString())} -> {Yellow(window.windowTemplate.name)}");
            if (window.windowTemplate.scene.handle != 0)
            {
                root = window.windowTemplate; // This is already in the scene, just use it.
            }
            else
            {
                root = Instantiate(window.windowTemplate);
                root.name = root.name.Replace("(Clone)", "");
            }
            root.GetComponent<Transform>().SetParent(parent.transform, worldPositionStays: false);
            if (!knownWindows.ContainsKey(windowName))
            {
                knownWindows.Add(windowName, root);
                knownWindowsList.Add(root);
                DebugHelper.checkWindowButtons(root.transform, config);
            }
            return root;
        }

        private static bool isMenuHub(GameObject window)
        {
            var children = window.GetComponentsInChildren<WindowIsMenuHub>(includeInactive: true);
            return children.Any(x => x.isMenuHub);
        }

        private WindowInstance getWindowInstance(WindowId window)
        {
            var windowInstance = config.windows.FirstOrDefault(x => x.window.windowId == window.windowId);
            if (windowInstance == null)
            {
                throw new UnityException(White("WindowConfig") + ": invalid window id: " + Red(window.ToString()));
            }
            return windowInstance;
        }

        private static class DebugHelper
        {
            [Conditional("UNITY_EDITOR")]
            public static void checkWindowButtons(Transform parent, WindowConfig config)
            {
                var button = parent.GetComponent<Button>();
                if (button != null)
                {
                    checkWindowButton(button, config);
                }
                for (int i = 0; i < parent.childCount; ++i)
                {
                    var child = parent.GetChild(i);
                    checkWindowButtons(child, config);
                }
            }

            private static void checkWindowButton(Button button, WindowConfig config)
            {
                var buttonCompanion = button.GetComponent<ButtonCompanion>();
                if (buttonCompanion == null)
                {
                    button.gameObject.AddComponent<ButtonCompanion>();
                }
                var openWindowButton = button.GetComponent<OpenWindowButton>();
                if (openWindowButton == null)
                {
                    if (_Instance.showConfigErrors)
                    {
                        ButtonError($"BUTTON {GetHierarchy(button.gameObject)} has no {Red("OpenWindowButton")} script!", button);
                        setButtonErrorState(button);
                    }
                    return;
                }
                var windowId = openWindowButton.window.windowId;
                if (windowId == 0)
                {
                    if (_Instance.showConfigErrors)
                    {
                        ButtonError($"Button {GetHierarchy(button.gameObject)} requires valid (non zero) {Red("WINDOW")} id", button);
                        setButtonErrorState(button);
                    }
                    return;
                }
                var windowInstance = config.windows.FirstOrDefault(x => x.window.windowId == windowId);
                if (windowInstance == null)
                {
                    if (_Instance.showConfigErrors)
                    {
                        ButtonError(
                            $"Button {GetHierarchy(button.gameObject)} MISSING {Magenta("window config for")} window name {Red(openWindowButton.window.ToString())}",
                            button);
                        setButtonErrorState(button);
                    }
                }
            }

            private static void ButtonError(string message, Button button)
            {
                if (Application.isEditor)
                {
                    Debug.LogError(message, button);
                    return;
                }
                Debug.LogWarning(message);
            }

            private static void setButtonErrorState(Button button)
            {
                button.interactable = _Instance.configErrorInteractable;
                var colors = button.colors;
                if (button.interactable)
                {
                    colors.normalColor = _Instance.configErrorColor;
                }
                else
                {
                    colors.disabledColor = _Instance.configErrorColor;
                }
                button.colors = colors;
            }

            private static string GetHierarchy(GameObject gameObject)
            {
                if (gameObject == null)
                {
                    return "";
                }
                if (gameObject.transform.parent == null)
                {
                    return gameObject.name;
                }
                var path = new StringBuilder(gameObject.name);
                while (gameObject.transform.parent != null)
                {
                    gameObject = gameObject.transform.parent.gameObject;
                    path.Insert(0, '\\').Insert(0, gameObject.name);
                }
                return path.ToString();
            }
        }

        public static string White(string text)
        {
            return $"<color=white>{text}</color>";
        }

        public static string Red(string text)
        {
            return $"<color=red>{text}</color>";
        }

        public static string Magenta(string text)
        {
            return $"<color=magenta>{text}</color>";
        }

        public static string Yellow(string text)
        {
            return $"<color=yellow>{text}</color>";
        }
   }
}