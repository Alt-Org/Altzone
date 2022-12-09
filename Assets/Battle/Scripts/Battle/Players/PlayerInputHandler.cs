using UnityEngine;
using UnityEngine.InputSystem;

namespace Battle.Scripts.Battle.Players
{
    public interface IPlayerInputHandler
    {
        void SetPlayerDriver(IPlayerDriver playerDriver);
    }

        public class PlayerInputHandler : MonoBehaviour, IPlayerInputHandler
    {
        private Vector2 _inputClick;

        [SerializeField] private InputActionReference _clickInputAction;
        [SerializeField] private InputActionReference _moveInputAction;
        private IPlayerDriver _playerDriver;

        private Camera _camera;

        // We might want to simulate mobile device screen by ignoring click outside out window.
        private bool _isLimitMouseXYOnDesktop;

        private void Awake()
        {
            _isLimitMouseXYOnDesktop = AppPlatform.IsDesktop;
            _camera = Context.GetBattleCamera.Camera;
        }

        public void SetPlayerDriver(IPlayerDriver playerDriver)
        {
            Debug.Log($"{name}");
            _playerDriver = playerDriver;
            SetupInput();
        }

        private void SendMoveTo(Vector2 targetPosition)
        {
            _playerDriver.MoveTo(targetPosition);
        }

        private void SetupInput()
        {
            var clickAction = _clickInputAction.action;
            clickAction.performed += DoPointerClick;
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
