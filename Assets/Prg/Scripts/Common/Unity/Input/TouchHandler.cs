using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace Prg.Scripts.Common.Unity.Input
{
    /// <summary>
    /// Touch handler implementation using <c>EnhancedTouchSupport</c> and polling in <c>Update</c> loop.
    /// </summary>
    /// <remarks>
    /// Pinch to Zoom Detection https://www.youtube.com/watch?v=5LEVj3PLufE <br />
    /// https://docs.unity3d.com/Packages/com.unity.inputsystem@1.3/api/UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.html
    /// </remarks>
    public class TouchHandler : BaseHandler
    {
        [Header("Live Data"), SerializeField] private int _touchCount;
        [SerializeField] private Vector2 _curPanPosition;
        [SerializeField] private Vector2 _prevPanPosition;
        [SerializeField] private int _panFingerId;
        [SerializeField] private bool _isFingerDown;
        [SerializeField] private bool _zoomActive;

        private Vector2 _newPrimaryPosition;
        private Vector2 _newSecondaryPosition;
        private float _newDistance;
        private float _previousDistance;

        private void OnEnable()
        {
            // Enhanced touch support provides automatic finger tracking and touch history recording. It is an API designed for polling!
            EnhancedTouchSupport.Enable();
        }

        private void OnDisable()
        {
            EnhancedTouchSupport.Disable();
        }

        private void Update()
        {
            Assert.IsTrue(EnhancedTouchSupport.enabled, "EnhancedTouchSupport.enabled");
            switch (Touch.activeTouches.Count)
            {
                case 1:
                    // If the touch began, capture its position and its finger ID.
                    // Otherwise, if the finger ID of the touch doesn't match, skip it.
                    _zoomActive = false;
                    var touch = Touch.activeTouches[0];
                    if (touch.phase == TouchPhase.Began)
                    {
                        // Start touch down (click)
                        var isPointerOverGameObject = EventSystem.current.IsPointerOverGameObject(touch.touchId);
                        if (isPointerOverGameObject)
                        {
                            // Ignore touch start if finger is over a UI element.
                            return;
                        }
                        _isFingerDown = true;
                        _curPanPosition = touch.screenPosition;
                        _touchCount = 1;
                        SendMouseDown(_curPanPosition, _touchCount);
                        _prevPanPosition = _curPanPosition;
                        _panFingerId = touch.touchId;
                    }
                    else if (touch.touchId == _panFingerId && touch.phase == TouchPhase.Moved)
                    {
                        // Continue touch down (panning)
                        _curPanPosition = touch.screenPosition;
                        _touchCount += 1;
                        SendMouseDown(_curPanPosition, _touchCount);
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
                    }
                    break;

                case 2:
                    // Two finger zooming
                    if (_isZoom)
                    {
                        _newPrimaryPosition = Touch.activeTouches[0].screenPosition;
                        _newSecondaryPosition = Touch.activeTouches[1].screenPosition;
                        if (!_zoomActive)
                        {
                            _newDistance = Vector2.Distance(_newPrimaryPosition, _newSecondaryPosition);
                            _previousDistance = _newDistance;
                            _zoomActive = true;
                        }
                        else
                        {
                            // Zoom based on the distance between the new positions compared to the distance between the previous positions.
                            _previousDistance = _newDistance;
                            _newDistance = Vector2.Distance(_newPrimaryPosition, _newSecondaryPosition);
                            var deltaDistance = _newDistance - _previousDistance;
                            ZoomCamera(deltaDistance * _zoomSpeed);
                        }
                    }
                    break;

                default:
                    // Exit
                    if (_zoomActive)
                    {
                        _zoomActive = false;
                    }
                    if (_isFingerDown)
                    {
                        // End touch down, report last touch position
                        _isFingerDown = false;
                        SendMouseUp(_prevPanPosition);
                    }
                    break;
            }
        }
    }
}