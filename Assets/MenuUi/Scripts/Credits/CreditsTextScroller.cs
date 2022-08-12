using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MenuUi.Scripts.Credits
{
    /// <summary>
    /// Utility script to manage scrolling text in UNITY Ui component <c>ScrollRect</c>.
    /// </summary>
    public class CreditsTextScroller : MonoBehaviour
    {
        private static readonly Vector3[] WorldCorners = new Vector3[4];

        [Header("Settings"), SerializeField] private float _scrollSpeed = 100f;
        [SerializeField] private float _scrollAcceleration = 100f / 60f;
        [SerializeField] private InputActionReference _uiClickButtonRef;
        [SerializeField] private RectTransform _scrollLimiter;
        [SerializeField] private RectTransform _contentRoot;
        [SerializeField] private RectTransform _scrollableCreditText;
        [SerializeField] private RectTransform _restartPosition;

        [Header("Live Data"), SerializeField] private bool _isMouseHeldDown;
        [SerializeField] private bool _isOverlapping;
        [SerializeField] private Rect _scrollLimiterRect;
        [SerializeField] private Rect _scrollableCreditTextRect;

        private float _maxScrollSpeed;
        private Coroutine _scrollerCoroutine;

        private void OnEnable()
        {
            Debug.Log($"{name}");
            _maxScrollSpeed = _scrollSpeed;
            _uiClickButtonRef.action.performed += OnClickActionPerformed;

            _scrollerCoroutine = StartCoroutine(ScrollerCoroutine());
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
            if (_scrollerCoroutine != null)
            {
                StopCoroutine(_scrollerCoroutine);
            }
        }

        private void RestartScrolling()
        {
            _isMouseHeldDown = false;
            _scrollerCoroutine = StartCoroutine(ScrollerCoroutine());
        }

        private IEnumerator ScrollerCoroutine()
        {
            yield return null;
            GetRect(_scrollLimiter, ref _scrollLimiterRect);
            _scrollSpeed = 0;
            while (enabled)
            {
                GetRect(_scrollableCreditText, ref _scrollableCreditTextRect);
                _isOverlapping = _scrollableCreditTextRect.Overlaps(_scrollLimiterRect);
                if (_isOverlapping)
                {
                    var deltaY = _scrollSpeed * Time.deltaTime;
                    if (_scrollSpeed < _maxScrollSpeed)
                    {
                        _scrollSpeed = Mathf.Min(_scrollSpeed + _scrollAcceleration, _maxScrollSpeed);
                    }
                    var position = _contentRoot.position;
                    position.y += deltaY;
                    _contentRoot.position = position;
                }
                else
                {
                    _scrollSpeed = 0;
                    _contentRoot.position = _restartPosition.position;
                }
                yield return null;
            }
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