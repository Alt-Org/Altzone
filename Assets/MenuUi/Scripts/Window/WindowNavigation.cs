using System.Collections;
using MenuUi.Scripts.Window;
using MenuUi.Scripts.Window.ScriptableObjects;
using UnityEngine;

namespace MenuUi.Scripts.Window
{
    public class WindowNavigation : MonoBehaviour
    {
        private const string Tooltip = "Pop out and hide current window before showing target window";

        [Header("Settings"), SerializeField] protected WindowDef _naviTarget;
        [Tooltip(Tooltip), SerializeField] protected bool _isCurrentPopOutWindow;

        public WindowDef NaviTarget { get => _naviTarget;}

        public IEnumerator Navigate()
        {
            Debug.Log($"naviTarget {_naviTarget} isCurrentPopOutWindow {_isCurrentPopOutWindow}", _naviTarget);
            var windowManager = WindowManager.Get();
            yield return new WaitUntil(() => windowManager.ExecutionLevel == 0);
            if (_isCurrentPopOutWindow)
            {
                windowManager.PopCurrentWindow();
            }
            // Check if navigation target window is already in window stack and we area actually going back to it via button.
            var windowCount = windowManager.WindowCount;
            if (windowCount > 1)
            {
                var targetIndex = windowManager.FindIndex(_naviTarget);
                if (targetIndex == 1)
                {
                    windowManager.GoBack();
                    yield break;
                }
                if (targetIndex > 1)
                {
                    windowManager.Unwind(_naviTarget);
                    windowManager.GoBack();
                    yield break;
                }
            }
            windowManager.ShowWindow(_naviTarget);
        }
    }
}
