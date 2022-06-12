using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Battle.Test.Scripts.Test
{
    internal class OnGuiWindowHelper : MonoBehaviour
    {
        public Key _controlKey = Key.F4;
        public string _windowTitle = nameof(OnGuiWindowHelper);
        public string _buttonCaption = nameof(_buttonCaption);
        public Action OnKeyPressed;

        public bool IsVisible => enabled;

        public void Show() => enabled = true;

        public void Hide() => enabled = false;

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        private int _windowId;
        private Rect _windowRect;

        private void OnEnable()
        {
            _windowId = (int)DateTime.Now.Ticks;
            var margin = Screen.width / 10;
            _windowRect = new Rect(margin, 0, Screen.width - 2 * margin, Screen.height / 10f * 1.5f);
        }

        private void Update()
        {
            if (Keyboard.current[_controlKey].wasPressedThisFrame)
            {
                enabled = false;
                OnKeyPressed?.Invoke();
            }
        }

        private void OnGUI()
        {
            _windowRect = GUILayout.Window(_windowId, _windowRect, DebugWindow, _windowTitle);
        }

        private void DebugWindow(int windowId)
        {
            GUILayout.Label(" ");
            if (GUILayout.Button(_buttonCaption))
            {
                enabled = false;
                OnKeyPressed?.Invoke();
            }
        }
#endif
    }
}