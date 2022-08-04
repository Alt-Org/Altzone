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
        [Header("Live Data"), SerializeField] private bool _isPointerOverGameObject;
        [SerializeField] private bool _isIgnoringPointer;
        [SerializeField] private bool _isPanning;
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
            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                if (_isPointerOverGameObject)
                {
                    // Ignore mouse down if it is over an UI element.
                    if (!_isIgnoringPointer)
                    {
                        _isIgnoringPointer = true;
                    }
                    return;
                }
                // Start mouse down (click)
                _clickCount = 1;
                _curPanPosition = Mouse.current.position.ReadValue();
                SendMouseDown(_curPanPosition, _clickCount);
                _prevPanPosition = _curPanPosition;
                return;
            }
            if (Mouse.current.leftButton.isPressed)
            {
                if (!_isPanning)
                {
                    // After panning has been started we never stop it!
                    if (_isPointerOverGameObject)
                    {
                        // Ignore mouse down if it is over an UI element or mouse drag (panning) started from there.
                        return;
                    }
                    if (_isIgnoringPointer)
                    {
                        // We went outside of an UI element - start panning now
                        _isIgnoringPointer = false;
                    }
                    _isPanning = true;
                }
                // Continue or start mouse down (panning)
                _clickCount += 1;
                _curPanPosition = Mouse.current.position.ReadValue();
                SendMouseDown(_curPanPosition, _clickCount);
                if (_isPan)
                {
                    const float minDelta = 0.00001f;
                    var delta = _curPanPosition - _prevPanPosition;
                    if (Mathf.Abs(delta.x) > minDelta || Mathf.Abs(delta.y) > minDelta)
                    {
                        PanCamera(delta * _panSpeed);
                        _prevPanPosition = _curPanPosition;
                    }
                }
                return;
            }
            if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                if (_isIgnoringPointer)
                {
                    _isIgnoringPointer = false;
                    return;
                }
                // End mouse down, report last mouse position
                _clickCount = 0;
                _isPanning = false;
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