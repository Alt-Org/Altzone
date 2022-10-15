using Prg.Scripts.Common.PubSub;
using Prg.Scripts.Common.Unity.Input;
using UnityEngine;

namespace Prg.Scripts.Test
{
    public class InputManagerListenerTest : MonoBehaviour
    {
        [Header("Settings"), SerializeField] private float _longPressDuration = 0.500f;

        [Header("Debug"), SerializeField] private bool _isPointerDown;
        [SerializeField] private int _lastEventId;

        private ThrottledDebugLogger _panLogger;
        private ThrottledDebugLogger _zoomLogger;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnEnable()
        {
            _lastEventId = -1;
            
            _panLogger = new ThrottledDebugLogger(this);
            _zoomLogger = new ThrottledDebugLogger(this);

            this.Subscribe<InputManager.ClickDownEvent>(OnClickDownEvent);
            this.Subscribe<InputManager.ClickUpEvent>(OnClickUpEvent);
            this.Subscribe<InputManager.PanEvent>(OnPanEvent);
            this.Subscribe<InputManager.ZoomEvent>(OnZoomEvent);
            this.Subscribe<ClickListener.ClickObjectEvent>(OnClickObjectEvent);
            this.Subscribe<ClickListenerTimed.ClickObjectTimedEvent>(OnClickObjectTimedEvent);
        }

        private void OnDisable()
        {
            this.Unsubscribe();
        }

        private void OnClickDownEvent(InputManager.ClickDownEvent data)
        {
            if (_isPointerDown)
            {
                return;
            }
            _isPointerDown = true;
        }

        private void OnClickUpEvent(InputManager.ClickUpEvent data)
        {
            _isPointerDown = false;
        }

        private void OnPanEvent(InputManager.PanEvent data)
        {
            ThrottledDebugLogger.Log(_panLogger, $"{data}");
        }

        private void OnZoomEvent(InputManager.ZoomEvent data)
        {
            ThrottledDebugLogger.Log(_zoomLogger, $"{data}");
        }

        private static void OnClickObjectEvent(ClickListener.ClickObjectEvent data)
        {
            Debug.Log($"{data}");
            HandleClick(data.GameObject);
        }

        private void OnClickObjectTimedEvent(ClickListenerTimed.ClickObjectTimedEvent data)
        {
            if (data.Duration > _longPressDuration)
            {
                if (_lastEventId == data.EventId)
                {
                    return;
                }
                _lastEventId = data.EventId;
                Debug.Log($"{data}");
                HandleClick(data.GameObject);
            }
        }

        private static void HandleClick(GameObject targetGameObject)
        {
            // Pick a random, saturated and not-too-dark color
            var targetRenderer = targetGameObject.GetComponent<Renderer>();
            if (targetRenderer != null)
            {
                var curColor = targetRenderer.material.color;
                for (;;)
                {
                    targetRenderer.material.color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);
                    if (targetRenderer.material.color != curColor)
                    {
                        break;
                    }
                }
            }
        }
#endif
    }
}