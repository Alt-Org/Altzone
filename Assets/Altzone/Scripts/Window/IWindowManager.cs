using System;
using Altzone.Scripts.Window.ScriptableObjects;
using UnityEngine;

namespace Altzone.Scripts.Window
{
    /// <summary>
    /// Interface for simple <c>WindowManager</c>.
    /// </summary>
    public interface IWindowManager
    {
        /// <summary>
        /// Registers callback handler to listen and optionally intercept going back in window chain.
        /// </summary>
        /// <remarks>Handler is removed after use!</remarks>
        void RegisterGoBackHandlerOnce(Func<WindowManager.GoBackAction> handler);

        /// <summary>
        /// Go back in window chain following bread crumbs.
        /// </summary>
        void GoBack();

        /// <summary>
        /// Shows given window.
        /// </summary>
        /// <remarks>
        /// It is automatically hidden when next window is shown and can be shown again if user follows bread crumbs.
        /// </remarks>
        void ShowWindow(WindowDef windowDef);

        /// <summary>
        /// Pops current window out from window chain and hides it.
        /// </summary>
        void PopCurrentWindow();

        /// <summary>
        /// Sets parent <c>GameObject</c> for windows so that Editor hierarchy stays clean.
        /// </summary>
        void SetWindowsParent(GameObject windowsParent);
    }
}