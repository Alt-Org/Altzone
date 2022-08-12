using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace MenuUi.Scripts.Credits
{
    /// <summary>
    /// Utility script to manage scrolling text in UNITY Ui component <c>ScrollRect</c>.
    /// </summary>
    public class CreditsTextScroller : MonoBehaviour
    {
        private static readonly Vector3[] WorldCorners = new Vector3[4];

        [Header("Settings"), SerializeField] private float _maxScrollSpeed = 100f;
        [SerializeField] private float _scrollAcceleration = 100f / 60f;
        [SerializeField] private InputActionReference _uiClickButtonRef;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private RectTransform _scrollLimiter;
        [SerializeField] private RectTransform _contentRoot;
        [SerializeField] private RectTransform _scrollableCreditText;
        [SerializeField] private RectTransform _restartPosition;

        [Header("Live Data"), SerializeField] private bool _isMouseHeldDown;
        [SerializeField] private bool _isOverlapping;
        [SerializeField] private float _scrollSpeed;
        [SerializeField] private Rect _scrollLimiterRect;
        [SerializeField] private Rect _scrollableCreditTextRect;

        private void OnEnable()
        {
            Debug.Log($"{name}");
            _uiClickButtonRef.action.performed += OnClickActionPerformed;
        }

        private void OnDisable()
        {
            Debug.Log($"{name}");
            _uiClickButtonRef.action.performed -= OnClickActionPerformed;
        }

        private void OnDestroy()
        {
            OnDisable();
        }

        private void Start()
        {
            GetRect(_scrollLimiter, ref _scrollLimiterRect);
            _scrollSpeed = 0;
        }

        private void Update()
        {
            GetRect(_scrollableCreditText, ref _scrollableCreditTextRect);
            _isOverlapping = _scrollableCreditTextRect.Overlaps(_scrollLimiterRect);
            if (_isOverlapping)
            {
                if (_isMouseHeldDown)
                {
                    // User override, no scrolling!
                    return;
                }
                if (_scrollSpeed < _maxScrollSpeed)
                {
                    _scrollSpeed = Mathf.Min(_scrollSpeed + _scrollAcceleration, _maxScrollSpeed);
                }
                var deltaY = _scrollSpeed * Time.deltaTime;
                var position = _contentRoot.position;
                position.y += deltaY;
                _contentRoot.position = position;
            }
            else
            {
                _scrollSpeed = 0;
                _scrollRect.StopMovement();
                _contentRoot.position = _restartPosition.position;
            }
        }

        private void OnClickActionPerformed(InputAction.CallbackContext ctx)
        {
            var isButtonDown = ctx.ReadValue<float>() != 0;
            if (isButtonDown)
            {
                if (!_isMouseHeldDown)
                {
                    // Start mouse/touch down
                    StopScrolling();
                }
                return;
            }
            // End mouse/touch down
            RestartScrolling();
        }

        private void StopScrolling()
        {
            _isMouseHeldDown = true;
            _scrollSpeed = 0;
        }

        private void RestartScrolling()
        {
            _isMouseHeldDown = false;
        }

        /// <summary>
        /// Gets <c>Rect</c> from <c>RectTransform</c> in world coordinates.
        /// </summary>
        private static void GetRect(RectTransform rectTransform, ref Rect rect)
        {
            rectTransform.GetWorldCorners(WorldCorners);
            rect.x = WorldCorners[0].x;
            rect.y = WorldCorners[0].y;
            rect.width = WorldCorners[2].x - WorldCorners[0].x;
            rect.height = WorldCorners[2].y - WorldCorners[0].y;
        }
    }
}