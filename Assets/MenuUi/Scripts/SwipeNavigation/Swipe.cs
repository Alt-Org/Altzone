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
        private Vector2 _currentPosition;
        private bool _touching;
        [SerializeField] private bool _canSlide;

        [SerializeField] private GameObject _slidingUI;
        
        [SerializeField] private WindowDef _prevNaviTarget;
        [SerializeField] private WindowDef _nextNaviTarget;

        [SerializeField] private float _distanceToSwitch;
        [SerializeField] private float _YdistanceToNotSwitch;
        

        private void Awake()
        {
            _playerInput = GetComponent<PlayerInput>();
        }

        private void slideAnimation()
        {
            if (_slidingUI == null)
            {
                return;
            }
            if(_touching == false)
            {
                _slidingUI.transform.position = Vector3.MoveTowards(_slidingUI.transform.position, new Vector3(0,0,0),2000 * Time.deltaTime);
                return;
            }
            if(_canSlide && _currentPosition.x > startPosition.x + 50 ||
            _canSlide && _currentPosition.x < startPosition.x - 50 )
            {
                _slidingUI.transform.position = Vector3.MoveTowards(_slidingUI.transform.position, new Vector3(_currentPosition.x - startPosition.x, 0, 0), 2000 * Time.deltaTime);
                return;
            }
        }

        public void IsTouching(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                startPosition = _playerInput.actions["TouchPosition"].ReadValue<Vector2>();
                _touching = true;
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
                if(startPosition.y + _YdistanceToNotSwitch !< _currentPosition.y && startPosition.y - _YdistanceToNotSwitch !> _currentPosition.y)
                {
                    
                }
                else
                {
                    _canSlide = true;
                }
                
            }
            slideAnimation();
        }
    }
}
