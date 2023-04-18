using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Battle.Scripts.Battle.Players
{
    public class PlayerInputHandler : MonoBehaviour
    {
        [Header("Prefab Settings"), SerializeField] private InputActionReference _clickInputAction;
        [SerializeField] private InputActionReference _moveInputAction;

        [Header("Debug Settings")] public GameObject _hostForInput;
        
        private Action<Vector2> _onMoveTo;
        public Action<Vector2> OnMoveTo
        {
            set
            {
                _onMoveTo = value;
                SetupInput();
            }
        }

        private Camera _camera;
        private Vector2 _inputClick;

        // We might want to simulate mobile device screen by ignoring click outside out window.
        private bool _isLimitMouseXYOnDesktop;

        private void Awake()
        {
            _isLimitMouseXYOnDesktop = AppPlatform.IsDesktop;
            _camera = Context.GetBattleCamera;
        }

        private void OnDestroy()
        {
            Debug.Log($"{name}");
            ReleaseInput();
        }

        private void SendMoveTo(Vector2 targetPosition)
        {
            _onMoveTo(targetPosition);
        }

        private void SetupInput()
        {
            var clickAction = _clickInputAction.action;
            clickAction.performed += DoPointerClick;
        }

        private void ReleaseInput()
        {
            var clickAction = _clickInputAction.action;
            clickAction.performed -= DoPointerClick;
            _onMoveTo = null;
        }

        private void DoPointerClick(InputAction.CallbackContext ctx)
        {
            _inputClick = ctx.ReadValue<Vector2>();
#if UNITY_STANDALONE
            if (_isLimitMouseXYOnDesktop)
            {
                if (_inputClick.x < 0 || _inputClick.y < 0 ||
                    _inputClick.x > Screen.width || _inputClick.y > Screen.height)
                {
                    return;
                }
            }
#endif
            _inputClick = _camera.ScreenToWorldPoint(_inputClick);
            SendMoveTo(_inputClick);
        }
    }
}
