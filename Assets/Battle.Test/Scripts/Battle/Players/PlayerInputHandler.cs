using System;
using Altzone.Scripts.Config;
using Battle.Scripts.Battle.Factory;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;

namespace Battle.Test.Scripts.Battle.Players
{
    internal class PlayerInputHandler : MonoBehaviour
    {
        [Header("Settings"), SerializeField] private float _unReachableDistance = 100;

        [Header("Live Data"), SerializeField] private Camera _camera;
        [SerializeField] private InputActionReference _clickInputAction;
        [SerializeField] private InputActionReference _moveInputAction;
        [SerializeField] private Rect _playerArea = Rect.MinMaxRect(-100, -100, 100, 100);

        private IPlayerDriver _playerDriver;
        private Transform _transform;
        private Vector2 _inputClick;
        private Vector3 _inputPosition;

        // We might want to simulate mobile device screen by ignoring click outside out window.
        private bool _isLimitMouseXYOnDesktop;

        private void Awake()
        {
            Assert.IsNull(_camera);
            Assert.IsNull(_clickInputAction);
            Assert.IsNull(_moveInputAction);
            _camera = Context.GetGameCamera.Camera;
            var input = RuntimeGameConfig.Get().Input;
            _clickInputAction = input._clickInputAction;
            _moveInputAction = input._moveInputAction;
            _isLimitMouseXYOnDesktop = !Application.isMobilePlatform;
        }

        public void SetPlayerDriver(IPlayerDriver playerDriver)
        {
            Debug.Log($"{name}");
            _playerDriver = playerDriver;
            _transform = GetComponent<Transform>();
            SetupInput();
        }
        
        private void OnDestroy()
        {
            Debug.Log($"{name}");
            ReleaseInput();
        }

        private void SendMoveTo(Vector2 targetPosition)
        {
            Debug.Log($"{targetPosition}");
            _playerDriver.MoveTo(targetPosition);
        }

        #region UNITY Input System

        private void SetupInput()
        {
            // https://gamedevbeginner.com/input-in-unity-made-easy-complete-guide-to-the-new-system/

            // WASD or GamePad -> performed is called once per key press
            var moveAction = _moveInputAction.action;
            moveAction.performed += DoMove;
            moveAction.canceled += StopMove;

            // Pointer movement when pressed down -> move to given point even pointer is released.
            var clickAction = _clickInputAction.action;
            clickAction.performed += DoClick;
        }

        private void ReleaseInput()
        {
            var moveAction = _moveInputAction.action;
            moveAction.performed -= DoMove;
            moveAction.canceled -= StopMove;

            var clickAction = _clickInputAction.action;
            clickAction.performed -= DoClick;
        }

        private void DoMove(InputAction.CallbackContext ctx)
        {
            // Simulate mouse click by trying to move very far.
            _inputClick = ctx.ReadValue<Vector2>() * _unReachableDistance;
            var inputPosition = _transform.position;
            inputPosition.x += _inputClick.x;
            inputPosition.y += _inputClick.y;
            _inputClick.x = Mathf.Clamp(inputPosition.x, _playerArea.xMin, _playerArea.xMax);
            _inputClick.y = Mathf.Clamp(inputPosition.y, _playerArea.yMin, _playerArea.yMax);
            SendMoveTo(_inputClick);
        }

        private void StopMove(InputAction.CallbackContext ctx)
        {
            // Simulate mouse click by "moving" to our current position.
            _inputClick = _transform.position;
            SendMoveTo(_inputClick);
        }

        private void DoClick(InputAction.CallbackContext ctx)
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
            _inputClick.x = Mathf.Clamp(_inputPosition.x, _playerArea.xMin, _playerArea.xMax);
            _inputClick.y = Mathf.Clamp(_inputPosition.y, _playerArea.yMin, _playerArea.yMax);
            SendMoveTo(_inputClick);
        }

        #endregion
    }
}