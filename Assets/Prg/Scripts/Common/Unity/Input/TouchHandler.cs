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
    /// https://docs.unity3d.com/Packages/com.unity.inputsystem@1.3/api/UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.html
    /// </remarks>
    public class TouchHandler : BaseHandler
    {
        [Header("Debug"),SerializeField]  private int _touchCount;
        [SerializeField] private Vector3 _firstPanPosition;
        [SerializeField] private Vector3 _lastPanPosition;
        [SerializeField] private int _panFingerId;
        [SerializeField] private bool _isFingerDown;
        [SerializeField] private bool _zoomActive;

        private readonly Vector2[] _lastZoomPositions = new Vector2[2];
        private readonly Vector2[] _newPositions = new Vector2[2];

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
                case 1: // Panning
                    // If the touch began, capture its position and its finger ID.
                    // Otherwise, if the finger ID of the touch doesn't match, skip it.
                    _zoomActive = false;
                    var touch = Touch.activeTouches[0];
                    if (touch.phase == TouchPhase.Began)
                    {
                        // Check if finger is over a UI element
                        _isPointerOverGameObject = EventSystem.current.IsPointerOverGameObject(touch.touchId);
                        if (_isPointerOverGameObject)
                        {
                            return;
                        }
                        _isFingerDown = true;
                        _firstPanPosition = touch.screenPosition;
                        _touchCount = 1;
                        SendMouseDown(_firstPanPosition, _touchCount);
                        _panFingerId = touch.touchId;
                    }
                    else if (touch.touchId == _panFingerId && touch.phase == TouchPhase.Moved)
                    {
                        _lastPanPosition = touch.screenPosition;
                        _touchCount += 1;
                        SendMouseDown(_lastPanPosition, _touchCount);
                        if (_isPan)
                        {
                            PanCamera((_firstPanPosition - _lastPanPosition) * _panSpeed);
                        }
                    }
                    break;

                case 2: // Zooming
                    if (_isZoom)
                    {
                        _newPositions[0] = Touch.activeTouches[0].screenPosition;
                        _newPositions[1] = Touch.activeTouches[1].screenPosition;
                        if (!_zoomActive)
                        {
                            _lastZoomPositions[0] = _newPositions[0];
                            _lastZoomPositions[1] = _newPositions[1];
                            _zoomActive = true;
                        }
                        else
                        {
                            // Zoom based on the distance between the new positions compared to the distance between the previous positions.
                            var newDistance = Vector2.Distance(_newPositions[0], _newPositions[1]);
                            var oldDistance = Vector2.Distance(_lastZoomPositions[0], _lastZoomPositions[1]);
                            var offset = newDistance - oldDistance;
                            ZoomCamera(offset * _zoomSpeed);
                            _lastZoomPositions[0] = _newPositions[0];
                            _lastZoomPositions[1] = _newPositions[1];
                        }
                    }
                    break;

                default:
                    if (_zoomActive)
                    {
                        _zoomActive = false;
                    }
                    if (_isFingerDown)
                    {
                        // Report last known touch position
                        _isFingerDown = false;
                        SendMouseUp(_touchCount == 1 ? _firstPanPosition : _lastPanPosition);
                    }
                    break;
            }
        }
    }
}