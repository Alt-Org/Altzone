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
        [SerializeField] private InputActionReference _uiClickButtonRef;
        [SerializeField] private Transform _scrollableCreditContent;
        [SerializeField] private Animator _animator;

        private bool _isMouseHeldDown;
        private Vector3 _savedCreditContentPos;

        private void Start()
        {
            _savedCreditContentPos = _scrollableCreditContent.position;
        }

        private void OnEnable()
        {
            _uiClickButtonRef.action.performed += OnActionPerformed;
        }

        private void OnDisable()
        {
            _uiClickButtonRef.action.performed -= OnActionPerformed;
        }

        private void OnDestroy()
        {
            OnDisable();
        }

        private void OnActionPerformed(InputAction.CallbackContext ctx)
        {
            var isButtonDown = ctx.ReadValue<float>() != 0;
            Debug.Log($"{ctx.control} {(isButtonDown ? "down" : "up")}");
            if (isButtonDown)
            {
                if (!_isMouseHeldDown)
                {
                    // Start mouse/touch down
                    _isMouseHeldDown = true;
                    _savedCreditContentPos = _scrollableCreditContent.position;
                    _animator.enabled = false;
                }
                return;
            }
            // End mouse/touch down
            _isMouseHeldDown = false;
            _scrollableCreditContent.position = _savedCreditContentPos;
            _animator.enabled = true;
        }
    }
}

