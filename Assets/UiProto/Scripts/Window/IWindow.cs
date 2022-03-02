using UnityEngine;

namespace UiProto.Scripts.Window
{
    /// <summary>
    /// Window contract with Window Manager.
    /// </summary>
    /// <remarks>
    /// In order a game object tree (or prefab) to be recognized as a window
    /// it must have a Window (derived) component that implements IWindow interface.
    /// </remarks>
    public interface IWindow
    {
        /// <summary>
        /// Gets window root GameObject.
        /// </summary>
        GameObject root { get; }

        /// <summary>
        /// The window id holder.
        /// </summary>
        WindowId windowId { get; }

        /// <summary>
        /// Shows window.
        /// </summary>
        void Show();

        /// <summary>
        /// Hides windows.
        /// </summary>
        void Hide();

        /// <summary>
        /// Initializes a window for use.
        /// </summary>
        /// <remarks>
        /// Can be called before UNITY life cycle events.
        /// </remarks>
        void Initialize(GameObject root, WindowId windowId);
    }
}
