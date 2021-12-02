using Prg.Scripts.Common.PubSub;
using UnityEngine;

namespace Prg.Scripts.Common.Unity.Input
{
    public class BaseHandler : MonoBehaviour, InputManager.IInputHandler
    {
        [Header("Settings")] public float zoomSpeed;
        public float minZoomSpeed;
        public float maxZoomSpeed;
        public float panSpeed;
        public float minPanSpeed;
        public float maxPanSpeed;

        [Header("Debug")] public bool IsPointerOverGameObject;

        private Vector2 panDelta;

        public BaseHandler Configure(InputManager.ZoomAndPan settings)
        {
            zoomSpeed = settings.zoomSpeed;
            minZoomSpeed = settings.minZoomSpeed;
            maxZoomSpeed = settings.maxZoomSpeed;
            panSpeed = settings.panSpeed;
            minPanSpeed = settings.minPanSpeed;
            maxPanSpeed = settings.maxPanSpeed;
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
            this.Publish(new InputManager.ZoomEvent(delta));
        }

        protected void PanCamera(Vector3 delta)
        {
            panDelta.x = delta.x;
            panDelta.y = delta.y;
            PanCamera(panDelta);
        }

        private void PanCamera(Vector2 delta)
        {
            this.Publish(new InputManager.PanEvent(delta));
        }
    }
}