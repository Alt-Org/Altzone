using System.Collections.Generic;
using MenuUi.Scripts.Window;
using MenuUi.Scripts.Window.ScriptableObjects;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MenuUi.Scripts.SwipeNavigation
{
    /// <summary>
    /// Simple swipe navigation controller to switch between windows horizontally using 'left' or 'right' swipe gestures.
    /// </summary>
    public class Swipe : MonoBehaviour
    {
        private const float SlideTouchSensitivity = 5f;
        private const float SlideMoveSpeedFactor = 2000f;

        private PlayerInput _playerInput;
        private InputAction _touchPositionInputAction;
        private Vector2 _startPosition;
        private Vector2 _endPosition;
        private Vector2 _currentPosition;
        private List<Vector2> _defaultPos;
        // This is only to reduce garbage collectors work avoiding Vector3/Vector2 conversions in Update()
        private Vector3 _tempMoveTargetPos = Vector3.zero;
        private bool _isTouching;
        private bool _canCheck;
        private bool _canSlide;

        [Header("UI components that should slide"), SerializeField] private GameObject[] _slidingUI;

        [Header("Swipe window chain"), SerializeField] private WindowDef _prevNaviTarget;
        [SerializeField] private WindowDef _nextNaviTarget;

        [Header("Swipe sensitivity"), SerializeField] private float _distanceToSwitch;

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
            _touchPositionInputAction = _playerInput.actions["TouchPosition"];
            if (_slidingUI.Length == 0)
            {
                enabled = false;
                _touchPositionInputAction.Disable();
                return;
            }
            _defaultPos = new List<Vector2>();
            foreach (var sliding in _slidingUI)
            {
                _defaultPos.Add(sliding.transform.position);
            }
        }

        /// <summary>
        /// <c>PlayerInput</c> component is configured in UNITY Editor to send this UNITY Input System Event to us.
        /// </summary>
        public void IsTouching(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _startPosition = _touchPositionInputAction.ReadValue<Vector2>();
                _isTouching = true;
                _canCheck = true;
                _canSlide = false;
                return;
            }
            if (context.canceled)
            {
                _endPosition = _touchPositionInputAction.ReadValue<Vector2>();
                if (_startPosition.x - _distanceToSwitch > _endPosition.x && _nextNaviTarget != null && _canSlide)
                {
                    var windowManager = WindowManager.Get();
                    windowManager.ShowWindow(_nextNaviTarget);
                }
                if (_startPosition.x + _distanceToSwitch < _endPosition.x && _prevNaviTarget != null && _canSlide)
                {
                    var windowManager = WindowManager.Get();
                    windowManager.ShowWindow(_prevNaviTarget);
                }
                _isTouching = false;
                _canSlide = false;
            }
        }

        private void Update()
        {
            if (_isTouching)
            {
                _currentPosition = _touchPositionInputAction.ReadValue<Vector2>();
                UpdateSwipeState();
            }
            if (_canSlide)
            {
                foreach (var slidingPos in _defaultPos)
                {
                    foreach (var slidingObj in _slidingUI)
                    {
                        _tempMoveTargetPos.x = _currentPosition.x - _startPosition.x + slidingPos.x;
                        _tempMoveTargetPos.y = slidingPos.y;
                        slidingObj.transform.position = Vector3.MoveTowards(
                            slidingObj.transform.position,
                            _tempMoveTargetPos,
                            SlideMoveSpeedFactor * Time.deltaTime);
                    }
                }
                return;
            }
            foreach (var sliding in _defaultPos)
            {
                foreach (var slidingObj in _slidingUI)
                {
                    _tempMoveTargetPos.x = sliding.x;
                    _tempMoveTargetPos.y = sliding.y;
                    slidingObj.transform.position = Vector3.MoveTowards(slidingObj.transform.position,
                        _tempMoveTargetPos,
                        SlideMoveSpeedFactor * Time.deltaTime);
                }
            }
        }

        private void UpdateSwipeState()
        {
            if (!_canCheck)
            {
                return;
            }
            if (_startPosition.y - SlideTouchSensitivity > _currentPosition.y || _startPosition.y + SlideTouchSensitivity < _currentPosition.y)
            {
                _canCheck = false;
                return;
            }
            if (_currentPosition.x > _startPosition.x + SlideTouchSensitivity || _currentPosition.x < _startPosition.x - SlideTouchSensitivity)
            {
                _canSlide = true;
                _canCheck = false;
            }
        }
    }
}
