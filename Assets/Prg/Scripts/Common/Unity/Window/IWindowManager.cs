using System;
using System.Collections.Generic;
using Prg.Scripts.Common.Unity.Window.ScriptableObjects;
using UnityEngine;

namespace Prg.Scripts.Common.Unity.Window
{
    /// <summary>
    /// Interface for simple <c>WindowManager</c>.
    /// </summary>
    /// <remarks>
    /// Some methods are not reentrant and must be synchronized externally!<br />
    /// At least <c>ShowWindow</c> and <c>Unwind</c> can not be called while somebody (else) is executing them.
    /// </remarks>
    public interface IWindowManager
    {
        /// <summary>
        /// Registers callback handler to listen and optionally intercept going back in window chain.
        /// </summary>
        /// <remarks>Handler is removed after use!</remarks>
        void RegisterGoBackHandlerOnce(Func<WindowManager.GoBackAction> handler);

        /// <summary>
        /// Unregisters callback handler.
        /// </summary>
        void UnRegisterGoBackHandlerOnce(Func<WindowManager.GoBackAction> handler);

        
        /// <summary>
        /// Gets current (top most) window.
        /// </summary>
        WindowDef CurrentWindow { get; }
        
        /// <summary>
        /// Gets current window count including pop-out windows.
        /// </summary>
        int WindowCount { get; }

        /// <summary>
        /// Get current window stack.
        /// </summary>
        List<WindowManager.MyWindow> WindowStack { get; }

        /// <summary>
        /// Finds given windows index in window stack, top most window has index value 0 and not found window -1.
        /// </summary>
        int FindIndex(WindowDef windowDef);

        /// <summary>
        /// Go back in window chain following bread crumbs.
        /// </summary>
        void GoBack();

        /// <summary>
        /// Unwind windows stack until given window is next to topmost window in window chain.
        /// </summary>
        /// <remarks>
        /// If window is not found, it is inserted there.
        /// </remarks>
        void Unwind(WindowDef windowDef);

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