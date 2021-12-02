using Altzone.Scripts.Window.ScriptableObjects;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UI;

namespace Altzone.Scripts.Window
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

        private void Awake()
        {
            Assert.IsNotNull(_naviTarget, "_naviTarget != null");
            var button = GetComponent<Button>();
            button.onClick.AddListener(() =>
            {
                Debug.Log($"Click {_naviTarget}");
                var windowManager = WindowManager.Get();
                if (_isCurrentPopOutWindow)
                {
                    windowManager.PopCurrentWindow();
                }
                windowManager.ShowWindow(_naviTarget);
            });
        }
    }
}