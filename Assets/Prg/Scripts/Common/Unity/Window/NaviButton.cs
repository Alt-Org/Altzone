using Prg.Scripts.Common.Unity.Window.ScriptableObjects;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Prg.Scripts.Common.Unity.Window
{
    /// <summary>
    /// Default navigation button for <c>WindowManager</c>.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class NaviButton : MonoBehaviour
    {
        private const string Tooltip = "Pop out and hide current window before showing target window";

        [Header("Settings"), SerializeField] private WindowDef _naviTarget;
        [Tooltip(Tooltip), SerializeField] private bool _isCurrentPopOutWindow;

        private void Start()
        {
            var button = GetComponent<Button>();
            if (_naviTarget == null)
            {
                button.interactable = false;
                return;
            }
            var windowManager = WindowManager.Get();
            var isCurrentWindow = windowManager.FindIndex(_naviTarget) == 0;
            if (isCurrentWindow)
            {
                button.interactable = false;
                return;
            }
            button.interactable = true;
            button.onClick.AddListener(() =>
            {
                Debug.Log($"Click {_naviTarget}");
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
                        return;
                    }
                }
                windowManager.ShowWindow(_naviTarget);
            });
        }
    }
}