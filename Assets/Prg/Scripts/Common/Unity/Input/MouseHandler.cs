using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Prg.Scripts.Common.Unity.Input
{
    /// <summary>
    /// Mouse handler implementation.
    /// </summary>
    public class MouseHandler : BaseHandler
    {
        [SerializeField] private int _clickCount;
        [SerializeField] private Vector2 _curPanPosition;
        [SerializeField] private Vector2 _prevPanPosition;
        [SerializeField] private InputActionReference _scrollWheelRef;

        private Vector2 _scrollWheel;
        private float _prevScrollAmount;

        public MouseHandler ScrollWheel(InputActionReference scrollWheelRef)
        {
            Assert.IsNotNull(scrollWheelRef);
            _scrollWheelRef = scrollWheelRef;
            _scrollWheelRef.action.performed += OnScrollWheelActionPerformed;
            _scrollWheelRef.action.Enable();
            return this;
        }

        private static bool Approximately(float a, float b) => Mathf.Abs(b - a) < 0.00001f; // 5 digits is more than enough for mouse precision!

        private void OnEnable()
        {
            if (_scrollWheelRef == null)
            {
                return;
            }
            _scrollWheelRef.action.Enable();
        }

        private void OnDisable()
        {
            if (_scrollWheelRef == null)
            {
                return;
            }
            _scrollWheelRef.action.Disable();
        }

        private void Update()
        {
            _isPointerOverGameObject = EventSystem.current.IsPointerOverGameObject(-1);
            if (_isPointerOverGameObject)
            {
                // Ignore UI elements.
                return;
            }
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                // Start mouse down
                _clickCount = 1;
                _curPanPosition = Mouse.current.position.ReadValue();
                SendMouseDown(_curPanPosition, _clickCount);
                _prevPanPosition = _curPanPosition;
            }
            else if (Mouse.current.leftButton.isPressed)
            {
                // Continue mouse down
                _clickCount += 1;
                _curPanPosition = Mouse.current.position.ReadValue();
                SendMouseDown(_curPanPosition, _clickCount);
                if (_isPan && (!Approximately(_curPanPosition.x, _prevPanPosition.x) || !Approximately(_curPanPosition.y, _prevPanPosition.y)))
                {
                    PanCamera((_curPanPosition - _prevPanPosition) * _panSpeed);
                    _prevPanPosition = _curPanPosition;
                }
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                // End mouse down, report last mouse position
                _clickCount = 0;
                _curPanPosition = Vector2.zero;
                _prevPanPosition = Vector2.zero;
                SendMouseUp(Mouse.current.position.ReadValue());
            }
        }

        private void OnScrollWheelActionPerformed(InputAction.CallbackContext ctx)
        {
            if (!_isZoom)
            {
                return;
            }
            // Zoom can be done even mouse is over UI element!
            _scrollWheel = ctx.ReadValue<Vector2>();
            var scrollAmount = _scrollWheel.y;
            if (scrollAmount == 0f && _prevScrollAmount == 0f)
            {
                return; // Do not report more than one zero scroll at a time
            }
            _prevScrollAmount = scrollAmount;
            ZoomCamera(scrollAmount * _zoomSpeed);
        }
    }
}