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
        private Button _button = null;
        public bool Interactable = true;

        private void Start()
        {
            if (_disabledOnProd && !AppPlatform.IsEditor && !AppPlatform.IsDevelopmentBuild) return;
            Debug.Log($"{name}", gameObject);
            _button = GetComponent<Button>();
            if (_naviTarget == null)
            {
                _button.interactable = false;
                return;
            }
            var windowManager = WindowManager.Get();
            var isCurrentWindow = windowManager.FindIndex(_naviTarget) == 0;
            _button.onClick.AddListener(OnNaviButtonClick);
            if (isCurrentWindow)
            {
                Interactable = false;
                return;
            }
        }

        private void OnEnable()
        {
            UpdateInteractable();
            OverlayPanelCheck.OnUpdateButtonInteractable += UpdateInteractable;
        }

        private void OnDisable()
        {
            OverlayPanelCheck.OnUpdateButtonInteractable -= UpdateInteractable;
        }

        private void UpdateInteractable()
        {
            if (_button)
            {
                var windowManager = WindowManager.Get();
                var isCurrentWindow = windowManager.FindIndex(_naviTarget) == 0;
                if (isCurrentWindow)
                {
                    Interactable = false;
                    return;
                }
                else
                {
                    Interactable = true;
                    return;
                }
            }
        }

        protected virtual void OnNaviButtonClick()
        {
            if (!Interactable) return;
            StartCoroutine(Navigate());
        }
#if UNITY_EDITOR
        [CustomEditor(typeof(NaviButton))]
        public class NaviButtonEditor : WindowNavigationEditor
        {
            public override void OnInspectorGUI()
            {
                base.OnInspectorGUI();
            }
        }
#endif
    }
}
