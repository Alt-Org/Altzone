using Prg.Scripts.Common.PubSub;
using Prg.Scripts.Common.Unity.Input;
using UnityEngine;

namespace Prg.Scripts.Test
{
    public class InputManagerListenerTest : MonoBehaviour
    {
        public bool _isPointerDown;

        private ThrottledDebugLogger _panLogger;
        private ThrottledDebugLogger _zoomLogger;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        protected void Awake()
        {
        }

        private void OnEnable()
        {
            _panLogger = new ThrottledDebugLogger(this);
            _zoomLogger = new ThrottledDebugLogger(this);

            this.Subscribe<InputManager.ClickDownEvent>(OnClickDownEvent);
            this.Subscribe<InputManager.ClickUpEvent>(OnClickUpEvent);
            this.Subscribe<InputManager.PanEvent>(OnPanEvent);
            this.Subscribe<InputManager.ZoomEvent>(OnZoomEvent);
            this.Subscribe<ClickListener.ClickObjectEvent>(OnClickObjectEvent);
        }

        private void OnDisable()
        {
            this.Unsubscribe();
            _panLogger.IsStopped = true;
            _zoomLogger.IsStopped = true;
        }

        private void OnClickDownEvent(InputManager.ClickDownEvent data)
        {
            if (_isPointerDown)
            {
                return;
            }
            _isPointerDown = true;
            Debug.Log($"{data}");
        }

        private void OnClickUpEvent(InputManager.ClickUpEvent data)
        {
            _isPointerDown = false;
            Debug.Log($"{data}");
        }

        private void OnPanEvent(InputManager.PanEvent data)
        {
            ThrottledDebugLogger.Log(_panLogger, $"OnPanEvent {data}");
        }

        private void OnZoomEvent(InputManager.ZoomEvent data)
        {
            ThrottledDebugLogger.Log(_zoomLogger, $"OnZoomEvent {data}");
        }

        private void OnClickObjectEvent(ClickListener.ClickObjectEvent data)
        {
            Debug.Log($"{data}");
            // Pick a random, saturated and not-too-dark color
            var target = data.GameObject.GetComponent<Renderer>();
            if (target != null)
            {
                target.material.color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
            }
        }
#endif
    }
}