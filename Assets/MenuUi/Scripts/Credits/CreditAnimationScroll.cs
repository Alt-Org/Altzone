using UnityEngine;
using UnityEngine.InputSystem;

namespace MenuUi.Scripts.Credits
{
    /// <summary>
    /// Manager for animated scrollable content.
    /// </summary>
    /// <remarks>
    /// When scrolling starts animation is stopped and restarted (where it was) when scrolling ends.
    /// </remarks>
    public class CreditAnimationScroll : MonoBehaviour
    {
        [SerializeField] private Transform _scrollableCreditContent;
        [SerializeField] private Animator _animator;

        private bool _isRestartAnimator;
        private Vector3 _savedCreditContentPos;

        private void Start()
        {
            _savedCreditContentPos = _scrollableCreditContent.position;
        }

        private void Update()
        {
            if (Application.isMobilePlatform || AppPlatform.IsSimulator)
            {
                if (Touchscreen.current.press.isPressed)
                {
                    if (Touchscreen.current.press.wasPressedThisFrame)
                    {
                        _savedCreditContentPos = _scrollableCreditContent.position;
                        _isRestartAnimator = true;
                        _animator.enabled = false;
                    }
                    return;
                }
            }
            else
            {
                if (Mouse.current.leftButton.isPressed)
                {
                    if (Mouse.current.leftButton.wasPressedThisFrame)
                    {
                        _savedCreditContentPos = _scrollableCreditContent.position;
                        _isRestartAnimator = true;
                        _animator.enabled = false;
                    }
                    return;
                }
            }
            if (_isRestartAnimator)
            {
                _scrollableCreditContent.position = _savedCreditContentPos;
                _isRestartAnimator = false;
                _animator.enabled = true;
            }
        }
    }
}

