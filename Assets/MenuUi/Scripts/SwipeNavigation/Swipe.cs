using MenuUi.Scripts.Window;
using MenuUi.Scripts.Window.ScriptableObjects;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MenuUi.Scripts.SwipeNavigation
{
    public class Swipe : MonoBehaviour
    {
        private PlayerInput _playerInput;
        private Vector2 startPosition;
        private Vector2 endPosition;

        [SerializeField] private WindowDef _prevNaviTarget;
        [SerializeField] private WindowDef _nextNaviTarget;

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
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
                if (startPosition.x - 50f > endPosition.x && _nextNaviTarget != null)
                {
                    var windowManager = WindowManager.Get();
                    windowManager.ShowWindow(_nextNaviTarget);
                }
                if (startPosition.x + 50f < endPosition.x && _prevNaviTarget != null)
                {
                    var windowManager = WindowManager.Get();
                    windowManager.ShowWindow(_prevNaviTarget);
                }

            }
        }
    }
}
