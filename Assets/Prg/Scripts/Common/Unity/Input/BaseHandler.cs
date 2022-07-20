using Prg.Scripts.Common.PubSub;
using UnityEngine;
using UnityEngine.Assertions;

namespace Prg.Scripts.Common.Unity.Input
{
    public class BaseHandler : MonoBehaviour, InputManager.IInputHandler
    {
        [Header("Settings")] public bool _isZoom;
        public float _zoomSpeed;
        public float _minZoomSpeed;
        public float _maxZoomSpeed;
        public bool _isPan;
        public float _panSpeed;
        public float _minPanSpeed;
        public float _maxPanSpeed;

        [Header("Debug")] public bool _isPointerOverGameObject;

        private Vector2 _panDelta;

        public BaseHandler Configure(InputManager.ZoomAndPan settings)
        {
            _isZoom = settings._isZoom;
            _zoomSpeed = settings._zoomSpeed;
            _minZoomSpeed = settings._minZoomSpeed;
            _maxZoomSpeed = settings._maxZoomSpeed;
            _isPan = settings._isPan;
            _panSpeed = settings._panSpeed;
            _minPanSpeed = settings._minPanSpeed;
            _maxPanSpeed = settings._maxPanSpeed;
            return this;
        }

        protected void SendMouseDown(Vector3 screenPosition, int clickCount)
        {
            this.Publish(new InputManager.ClickDownEvent(screenPosition, clickCount));
        }

        protected void SendMouseUp(Vector3 screenPosition)
        {
            this.Publish(new InputManager.ClickUpEvent(screenPosition, 0));
        }

        protected void ZoomCamera(float delta)
        {
            Assert.IsTrue(_isZoom, "_isZoom");
            this.Publish(new InputManager.ZoomEvent(delta));
        }

        protected void PanCamera(Vector3 delta)
        {
            Assert.IsTrue(_isPan, "_isPan");
            _panDelta.x = delta.x;
            _panDelta.y = delta.y;
            this.Publish(new InputManager.PanEvent(_panDelta));
        }

        private void PanCamera(Vector2 delta)
        {
            Assert.IsTrue(_isPan, "_isPan");
            _panDelta = delta;
            this.Publish(new InputManager.PanEvent(_panDelta));
        }
    }
}