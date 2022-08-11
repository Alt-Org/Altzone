using UnityEngine;
using UnityEngine.InputSystem;

namespace MenuUi.Scripts.Credits
{
    public class CreditAnimationScroll : MonoBehaviour
    {
        [SerializeField] private Transform _creditContent;
        [SerializeField] private Animator _animator;
        
        private bool _scrollStop;
        private Vector3 _creditPlacement;

        private void Start()
        {
            _creditPlacement = _creditContent.position;
        }

        private void Update()
        {
            _scrollStop = false;

            if (Application.isMobilePlatform || AppPlatform.IsSimulator)
            {
                if (Touchscreen.current.press.isPressed)
                {
                    _scrollStop = true;
                }
                if (Touchscreen.current.press.wasPressedThisFrame)
                {
                    _creditPlacement = _creditContent.position;
                }
            }
            else
            {
                if (Mouse.current.leftButton.isPressed)
                {
                    _scrollStop = true;
                }
                if (Mouse.current.leftButton.wasPressedThisFrame)
                {
                    _creditPlacement = _creditContent.position;
                }
            }

            StopScrolling();

        }

        private void StopScrolling()
        {

            if (_scrollStop == true)
            {
                _animator.enabled = false;
            }

            if (_scrollStop == false)
            {
                _animator.enabled = true;

                _creditContent.position = _creditPlacement;
            }
        }
    }
}

