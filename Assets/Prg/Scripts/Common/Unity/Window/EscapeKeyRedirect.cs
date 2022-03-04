using Prg.Scripts.Common.Unity.Window.ScriptableObjects;
using UnityEngine;

namespace Prg.Scripts.Common.Unity.Window
{
    /// <summary>
    /// Traps Escape key press and replaces current window with this window if Escape key is pressed.
    /// </summary>
    public class EscapeKeyRedirect : MonoBehaviour
    {
        [SerializeField] private WindowDef _redirectWindow;
        [SerializeField] private bool _isUiProtoHack;

        private void OnEnable()
        {
            WindowManager.Get().RegisterGoBackHandlerOnce(DoRedirect);
        }

        private void OnDisable()
        {
            WindowManager.Get().UnRegisterGoBackHandlerOnce(DoRedirect);
        }

        private WindowManager.GoBackAction DoRedirect()
        {
            var windowManager = WindowManager.Get();
            if (_isUiProtoHack && windowManager.WindowCount > 1)
            {
                WindowManager.Get().RegisterGoBackHandlerOnce(DoRedirect);
                return WindowManager.GoBackAction.Continue;
            }
            Debug.Log($"DoRedirect start {_redirectWindow} WindowCount {windowManager.WindowCount}");
            windowManager.PopCurrentWindow();
            windowManager.ShowWindow(_redirectWindow);
            Debug.Log($"DoRedirect done {_redirectWindow}");
            return WindowManager.GoBackAction.Abort;
        }
    }
}