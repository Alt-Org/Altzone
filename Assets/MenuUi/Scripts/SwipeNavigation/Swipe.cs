using MenuUi.Scripts.Window;
using MenuUi.Scripts.Window.ScriptableObjects;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

namespace MenuUi.Scripts.SwipeNavigation
{
    public class Swipe : MonoBehaviour
    {
        private PlayerInput _playerInput;
        private Vector2 startPosition;
        private Vector2 endPosition;
        private Vector2 _currentPosition;
        private List<Vector2> _defaultPos;
        private bool _touching;
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
            foreach (GameObject sliding in _slidingUI)
            {
                _defaultPos.Add(new Vector2(sliding.transform.position.x, sliding.transform.position.y));
            }
        }

        public void IsTouching(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                startPosition = _playerInput.actions["TouchPosition"].ReadValue<Vector2>();
                _touching = true;
                _canCheck = true;
                _canSlide = false;
            }
            if (context.canceled)
            {
                
                endPosition = _playerInput.actions["TouchPosition"].ReadValue<Vector2>();
                if (startPosition.x - _distanceToSwitch > endPosition.x && _nextNaviTarget != null && _canSlide)
                {
                    var windowManager = WindowManager.Get();
                    windowManager.ShowWindow(_nextNaviTarget);
                }
                if (startPosition.x + _distanceToSwitch < endPosition.x && _prevNaviTarget != null && _canSlide)
                {
                    var windowManager = WindowManager.Get();
                    windowManager.ShowWindow(_prevNaviTarget);
                }
                _touching = false;
                _canSlide = false;
            }
        }

        private void Update()
        {
            if (_touching == true)
            {
                _currentPosition = _playerInput.actions["TouchPosition"].ReadValue<Vector2>();
                swipeCheck();
            }
            if (_canSlide == true && _slidingUI[0] != null)
            {
                foreach (Vector2 slidingPos in _defaultPos)
                {
                    foreach (GameObject slidingObj in _slidingUI)
                    {
                        slidingObj.transform.position = Vector3.MoveTowards(slidingObj.transform.position, new Vector3(_currentPosition.x - startPosition.x + slidingPos.x, slidingPos.y, 0), 2000 * Time.deltaTime);
                    }
                }
            }
            else
            {
                foreach (Vector2 sliding in _defaultPos)
                {
                    foreach (GameObject slidingObj in _slidingUI)
                    {
                        slidingObj.transform.position = Vector3.MoveTowards(slidingObj.transform.position, new Vector3(sliding.x, sliding.y, 0), 2000 * Time.deltaTime);
                    }
                }
            }
            
        }

        private void swipeCheck()
        {
            if(_canCheck == false)
            {
                return;
            }
            if(startPosition.y - 5 > _currentPosition.y || startPosition.y + 5 < _currentPosition.y)
            {
                _canCheck = false;
                return;
            }
            if(_currentPosition.x > startPosition.x + 5 || _currentPosition.x < startPosition.x - 5)
            {
                _canSlide = true;
                _canCheck = false;
                return;
            }
        }
    }
}
