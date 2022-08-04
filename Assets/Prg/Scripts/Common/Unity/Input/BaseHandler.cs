using Prg.Scripts.Common.PubSub;
using UnityEngine;
using UnityEngine.Assertions;

namespace Prg.Scripts.Common.Unity.Input
{
    public class BaseHandler : MonoBehaviour, InputManager.IInputHandler
    {
        [Header("Settings")] public bool _isZoom;
        public float _zoomSpeed;
        public bool _isPan;
        public float _panSpeed;

        public BaseHandler Configure(InputManager.ZoomAndPanSettings settings)
        {
            _isZoom = settings._isZoom;
            _zoomSpeed = settings._zoomSpeed;
            _isPan = settings._isPan;
            _panSpeed = settings._panSpeed;
            return this;
        }

        protected void SendMouseDown(Vector2 screenPosition, int clickCount)
        {
            this.Publish(new InputManager.ClickDownEvent(screenPosition, clickCount));
        }

        protected void SendMouseUp(Vector2 screenPosition)
        {
            this.Publish(new InputManager.ClickUpEvent(screenPosition, 0));
        }

        protected void ZoomCamera(float delta)
        {
            Assert.IsTrue(_isZoom, "_isZoom");
            this.Publish(new InputManager.ZoomEvent(delta));
        }

        protected void PanCamera(Vector2 delta)
        {
            Assert.IsTrue(_isPan, "_isPan");
            this.Publish(new InputManager.PanEvent(delta));
        }
    }
}