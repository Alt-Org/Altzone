using MenuUi.Scripts.Window.ScriptableObjects;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUi.Scripts.Window
{
    /// <summary>
    /// Default navigation button for <c>WindowManager</c>.
    /// </summary>
    /// <remarks>
    /// <c>Button</c> initial <c>interactable</c> state can be set in Editor and later by code.
    /// </remarks>
    [RequireComponent(typeof(Button))]
    public class NaviButton : WindowNavigation
    {
        private void Start()
        {
            Debug.Log($"{name}", gameObject);
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
            button.onClick.AddListener(OnNaviButtonClick);
        }

        protected virtual void OnNaviButtonClick()
        {
            StartCoroutine(Navigate());
        }

        [CustomEditor(typeof(NaviButton))]
        public class NaviButtonEditor : WindowNavigationEditor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
            }
        }
    }
}
