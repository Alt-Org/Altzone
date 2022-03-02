using System;
using System.Collections.Generic;
using Prg.Scripts.Common.Unity.Window;
using UnityEngine;

namespace UiProto.Scripts.Window
{
    [Serializable]
    public class StackEntry
    {
        public string windowName;
        public string levelName;

        public StackEntry(string windowName, string levelName)
        {
            this.windowName = windowName;
            this.levelName = levelName;
        }

        public override string ToString()
        {
            return $"{windowName}{(string.IsNullOrEmpty(levelName)?"":$"({levelName})")}";
        }
    }

    public class WindowStack : MonoBehaviour
    {
        private static WindowStack _Instance;
        private static readonly StackEntry empty = new StackEntry("", "");

        [Header("Settings")] [SerializeField] private List<StackEntry> windowStack;
        [SerializeField] private StackEntry pendingWindow;

        public static WindowStack Instance
        {
            get
            {
                if (_Instance == null)
                {
                    _Instance = new GameObject(nameof(WindowStack)).AddComponent<WindowStack>();
                    DontDestroyOnLoad(_Instance);
                }
                return _Instance;
            }
        }

        public static bool hasWindowStack => _Instance != null;

        public bool hasPendingWindow => !Instance.pendingWindow.Equals(empty);

        public static void dumpWindowStack(string tag = null)
        {
            if (_Instance == null)
            {
                Debug.Log($"WindowStack {tag} NULL");
                return;
            }
            var pending = _Instance.hasPendingWindow ? _Instance.pendingWindow : null;
            Debug.Log($"WindowStack {tag} stack={_Instance.windowCount} pending={pending}");
            var i = -1;
            foreach (var windowId in _Instance.windowStack)
            {
                Debug.Log($"{++i}={windowId}");
            }
        }

        protected void Awake()
        {
            if (_Instance == null)
            {
                // Register us as the singleton!
                _Instance = this;
                var handler = gameObject.AddComponent<EscapeKeyHandler>();
                handler.SetCallback(WindowManager.SafeGoBack);
            }
            windowStack = new List<StackEntry>();
            pendingWindow = empty;
        }

        protected void OnDestroy()
        {
            if (Instance == this)
            {
                _Instance = null;
            }
        }

        public int windowCount => windowStack.Count;

        public void push(string windowName, string levelName)
        {
            windowStack.Add(new StackEntry(windowName, levelName));
            dumpWindowStack("push");
        }

        public StackEntry pop()
        {
            dumpWindowStack("pop");
            var index = windowStack.Count - 1;
            if (index < 0)
            {
                throw new InvalidOperationException("stack is empty");
            }
            var windowId = windowStack[index];
            windowStack.RemoveAt(index);
            dumpWindowStack("done");
            return windowId;
        }

        public void pushPending(string windowName)
        {
            pendingWindow = new StackEntry(windowName, "");
            dumpWindowStack("pushPending");
        }

        public StackEntry popPending()
        {
            dumpWindowStack("popPending");
            var temp = pendingWindow;
            pendingWindow = empty;
            return temp;
        }

        public void clearLevel(string levelName)
        {
            dumpWindowStack($"clearLevel='{levelName}'");
            for (var index = windowStack.Count - 1; index >= 0; --index)
            {
                var windowId = windowStack[index];
                if (windowId.levelName != levelName)
                {
                    break;
                }
                windowStack.RemoveAt(index);
                index -= 1;
            }
            dumpWindowStack("done");
        }

        public void clear()
        {
            dumpWindowStack("clear");
            windowStack.Clear();
            pendingWindow = empty;
        }

        private StackEntry _peek()
        {
            var index = windowStack.Count - 1;
            if (index < 0)
            {
                throw new InvalidOperationException("stack is empty");
            }
            return windowStack[index];
        }

        public override string ToString()
        {
            return $"pending={(hasPendingWindow ? _Instance.pendingWindow : null)} windowStack=#{windowStack.Count}";
        }


        public string ToStringFull()
        {
            return $"{ToString()} {string.Join(", ", windowStack)}";
        }
    }
}