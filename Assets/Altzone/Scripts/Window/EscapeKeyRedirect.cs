using Altzone.Scripts.Window.ScriptableObjects;
using UnityEngine;

namespace Altzone.Scripts.Window
{
    /// <summary>
    /// Traps Escape key press and replaces current window with this window if Escape key is pressed.
    /// </summary>
    public class EscapeKeyRedirect : MonoBehaviour
    {
        [SerializeField] private WindowDef _redirectWindow;

        private void OnEnable()
        {
            WindowManager.Get().RegisterGoBackHandlerOnce(DoRedirect);
        }

        private WindowManager.GoBackAction DoRedirect()
        {
            var windowManager= WindowManager.Get();
            windowManager.PopCurrentWindow();
            windowManager.ShowWindow(_redirectWindow);
            return WindowManager.GoBackAction.Abort;
        }
    }
}
