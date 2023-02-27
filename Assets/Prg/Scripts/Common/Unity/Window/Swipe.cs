using Prg.Scripts.Common.Unity.Window.ScriptableObjects;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Prg.Scripts.Common.Unity.Window
{
    public class Swipe : MonoBehaviour
    {
        [SerializeField] private PlayerInput _playerInput;
        private Vector2 startPosition;
        private Vector2 endPosition;

        private int _targetWindowIndex;

        private const string Tooltip = "Pop out and hide current window before showing target window";

        [SerializeField] private WindowDef[] _naviTargets;
        [Tooltip(Tooltip), SerializeField] private bool _isCurrentPopOutWindow;

        private void Awake()
        {
            _targetWindowIndex = 0;
            _playerInput = GetComponent<PlayerInput>();
            var windowManager = WindowManager.Get();
            var isCurrentWindow = windowManager.FindIndex(_naviTargets[0]) == 0;
        }

        public void IsTouching(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                startPosition = _playerInput.actions["TouchPosition"].ReadValue<Vector2>();
            }
            if (context.canceled)
            {

                endPosition = _playerInput.actions["TouchPosition"].ReadValue<Vector2>();
                if (startPosition.x > endPosition.x)_targetWindowIndex++;
                if (startPosition.x < endPosition.x)_targetWindowIndex--;

                Debug.Log("toutch");

                var windowManager = WindowManager.Get();
                if (_isCurrentPopOutWindow)
                {
                    windowManager.PopCurrentWindow();
                }

                if (_isCurrentPopOutWindow)
                {
                    windowManager.PopCurrentWindow();
                }

                var windowCount = windowManager.WindowCount;

                if (windowCount > 1)
                {
                    var targetIndex = windowManager.FindIndex(_naviTargets[_targetWindowIndex]);
                    if (targetIndex == 1)
                    {
                        windowManager.GoBack();
                        return;
                    }
                    if (targetIndex > 1)
                    {
                        windowManager.Unwind(_naviTargets[_targetWindowIndex]);
                        windowManager.GoBack();
                        return;
                    }
                }

                windowManager.ShowWindow(_naviTargets[_targetWindowIndex]);
            }
        }
    }
}
