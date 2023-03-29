using UnityEngine;
using UnityEngine.InputSystem;

namespace Battle.Scripts.Battle.Players
{
    /// <summary>
    /// Receiver for player's input actions.
    /// </summary>
    internal interface IPlayerInputTarget
    {
        void MoveTo(Vector2 targetPosition);
    }

    public class PlayerInputHandler : MonoBehaviour
    {
        [SerializeField] private InputActionReference _clickInputAction;
        [SerializeField] private InputActionReference _moveInputAction;

        private IPlayerInputTarget _inputTarget;
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
            _inputTarget.MoveTo(targetPosition);
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

        internal void SetPlayerDriver(IPlayerInputTarget playerDriver)
        {
            Debug.Log($"{name}");
            _inputTarget = playerDriver;
            SetupInput();
        }
    }
}
