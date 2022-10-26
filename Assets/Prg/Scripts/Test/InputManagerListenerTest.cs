using Prg.Scripts.Common.PubSub;
using Prg.Scripts.Common.Unity.Input;
using UnityEngine;
using UnityEngine.Assertions;

namespace Prg.Scripts.Test
{
    public class InputManagerListenerTest : MonoBehaviour
    {
        [Header("Debug"), SerializeField] private bool _isPointerDown;
        [SerializeField] private GameObject _currentTarget;

        private ThrottledDebugLogger _panLogger;
        private ThrottledDebugLogger _zoomLogger;

        private bool _isMobile;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void OnEnable()
        {
            _panLogger = new ThrottledDebugLogger(this);
            _zoomLogger = new ThrottledDebugLogger(this);

            _isMobile = AppPlatform.IsMobile;

            this.Subscribe<InputManager.ClickDownEvent>(OnClickDownEvent);
            this.Subscribe<InputManager.ClickUpEvent>(OnClickUpEvent);
            this.Subscribe<InputManager.PanEvent>(OnPanEvent);
            this.Subscribe<InputManager.ZoomEvent>(OnZoomEvent);
            this.Subscribe<ClickDownListener.ClickDownObjectEvent>(OnClickObjectEvent);
            this.Subscribe<ClickUpListener.ClickUpObjectEvent>(OnClickUpObjectEvent);
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

        private void OnClickObjectEvent(ClickDownListener.ClickDownObjectEvent data)
        {
            Debug.Log($"{data}");
            HandleClick(data.GameObject, false);
        }

        private void OnClickUpObjectEvent(ClickUpListener.ClickUpObjectEvent data)
        {
            if (data.Phase == ClickUpListener.ClickUpPhase.ClickUpStart)
            {
                Debug.Log($"{data}");
                _currentTarget = data.GameObject;
                return;
            }
            if (data.Phase != ClickUpListener.ClickUpPhase.ClickUpEnd)
            {
                throw new UnityException($"Unknown ClickUpPhase {data.Phase}");
            }
            Assert.IsNotNull(_currentTarget);
            if (!_currentTarget.Equals(data.GameObject))
            {
                Debug.Log($"SKIP {data}");
                return;
            }
            Debug.Log($"{data}");
            HandleClick(data.GameObject, _isMobile);
        }

        private void HandleClick(GameObject targetGameObject, bool vibrate)
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
                        // Clicked - provide haptic feedback
                        if (vibrate)
                        {
#if UNITY_ANDROID
                            Handheld.Vibrate();
#endif
                        }
                        break;
                    }
                }
            }
        }
#endif
    }
}