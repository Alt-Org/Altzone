namespace UiProto.Scripts.Window
{
    public interface IWindowManager
    {
        /// <summary>
        /// Gets current window.
        /// </summary>
        IWindow CurrentWindow { get; }

        /// <summary>
        /// Shows window by its ID.
        /// </summary>
        /// <param name="windowId">the window ID to show</param>
        /// <param name="levelId">the level ID to show</param>
        /// <returns>the window or null</returns>
        IWindow ShowWindow(WindowId windowId, LevelId levelId);

        /// <summary>
        /// Closes current window and opens previous window from window stack if available.
        /// </summary>
        /// <remarks>
        /// Closes application if possible (on current platform) when window stack is empty.
        /// </remarks>
        void GoBack();
    }
}
