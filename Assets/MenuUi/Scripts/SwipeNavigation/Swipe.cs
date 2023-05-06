using System.Collections.Generic;
using MenuUi.Scripts.Window;
using MenuUi.Scripts.Window.ScriptableObjects;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MenuUi.Scripts.SwipeNavigation
{
    public class Swipe : MonoBehaviour
    {
        private const float SlideTouchSensitivity = 5f;
        private const float SlideMoveSpeedFactor = 2000f;

        private PlayerInput _playerInput;
        private Vector2 _startPosition;
        private Vector2 _endPosition;
        private Vector2 _currentPosition;
        private List<Vector2> _defaultPos;
        private bool _isTouching;
        private bool _canCheck;
        private bool _canSlide;

        [SerializeField] private GameObject[] _slidingUI;

        [SerializeField] private WindowDef _prevNaviTarget;
        [SerializeField] private WindowDef _nextNaviTarget;

        [SerializeField] private float _distanceToSwitch;

        private void Awake()
        {
            _defaultPos = new List<Vector2>();
            _playerInput = GetComponent<PlayerInput>();
            foreach (var sliding in _slidingUI)
            {
                _defaultPos.Add(sliding.transform.position);
            }
        }

        public void IsTouching(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _startPosition = _playerInput.actions["TouchPosition"].ReadValue<Vector2>();
                _isTouching = true;
                _canCheck = true;
                _canSlide = false;
                return;
            }
            if (context.canceled)
            {
                _endPosition = _playerInput.actions["TouchPosition"].ReadValue<Vector2>();
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
                _currentPosition = _playerInput.actions["TouchPosition"].ReadValue<Vector2>();
                UpdateSwipeState();
            }
            if (_canSlide && _slidingUI.Length > 0)
            {
                foreach (var slidingPos in _defaultPos)
                {
                    foreach (var slidingObj in _slidingUI)
                    {
                        slidingObj.transform.position = Vector3.MoveTowards(
                            slidingObj.transform.position,
                            new Vector3(_currentPosition.x - _startPosition.x + slidingPos.x, slidingPos.y, 0),
                            SlideMoveSpeedFactor * Time.deltaTime);
                    }
                }
            }
            else
            {
                foreach (var sliding in _defaultPos)
                {
                    foreach (var slidingObj in _slidingUI)
                    {
                        slidingObj.transform.position = Vector3.MoveTowards(slidingObj.transform.position,
                            new Vector3(sliding.x, sliding.y, 0),
                            SlideMoveSpeedFactor * Time.deltaTime);
                    }
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
