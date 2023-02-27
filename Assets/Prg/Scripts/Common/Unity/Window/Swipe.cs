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

        [SerializeField] private WindowDef[] _naviTargets;

        private void Awake()
        {
            _targetWindowIndex = 0;
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
                if (startPosition.x - 20f > endPosition.x && _targetWindowIndex < 4)_targetWindowIndex++;
                if (startPosition.x + 20f < endPosition.x && _targetWindowIndex > 0)_targetWindowIndex--;

                Debug.Log("toutch");

                var windowManager = WindowManager.Get();
                windowManager.ShowWindow(_naviTargets[_targetWindowIndex]);
            }
        }
    }
}
