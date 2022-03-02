using UnityEngine;

namespace UiProto.Scripts.Window
{
    /// <summary>
    /// Marker class for "windows"
    /// </summary>
    public class WindowHandle : MonoBehaviour, IWindow
    {
        [SerializeField] private WindowId _windowId;

        public GameObject root { get; private set; }

        public WindowId windowId
        {
            get => _windowId;
            private set => _windowId = value;
        }

        public void Show()
        {
        }

        public void Hide()
        {
        }

        public void Initialize(GameObject root, WindowId windowId)
        {
            this.root = root;
            this.windowId = windowId;
        }

        public override string ToString()
        {
            var rootName = root != null ? root.name : "";
            return $"{nameof(root)}: {rootName}, {nameof(windowId)}: {windowId}";
        }
    }
}