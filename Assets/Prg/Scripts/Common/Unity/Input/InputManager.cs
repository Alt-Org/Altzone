using System;
using UnityEngine;

namespace Prg.Scripts.Common.Unity.Input
{
    public class InputManager : MonoBehaviour
    {
        [Header("Settings"), SerializeField] private InputManagerConfig _config; // For manual Editor config

        [Header("Live Data"),SerializeField] private ZoomAndPan _mouse;
        [SerializeField] private ZoomAndPan _touch;
        [SerializeField] private  BaseHandler _handler;

        // We create clones because objects can be "in" ScriptableObject and we do not want to change original values on disk!
        public ZoomAndPan mouse
        {
            get => _mouse;
            set => _mouse = value.Clone();
        }

        public ZoomAndPan touch
        {
            get => _touch;
            set => _touch = value.Clone();
        }

        public BaseHandler handler => _handler;

        private void Awake()
        {
            if (_config != null)
            {
                _mouse = _config.mouse;
                _touch = _config.touch;
            }
            if (handler == null)
            {
                if (Application.isMobilePlatform)
                {
                    _handler = gameObject.GetOrAddComponent<TouchHandler>()
                        .Configure(touch);
                }
                else
                {
                    _handler = gameObject.GetOrAddComponent<MouseHandler>()
                        .Configure(mouse);
                }
            }
            Debug.Log($"handler {handler}");
        }

        public interface IInputHandler
        {
        }

        [Serializable]
        public class ZoomAndPan
        {
            public float zoomSpeed;
            public float minZoomSpeed;
            public float maxZoomSpeed;
            public float panSpeed;
            public float minPanSpeed;
            public float maxPanSpeed;

            public ZoomAndPan Clone()
            {
                return MemberwiseClone() as ZoomAndPan;
            }
        }

        public class ClickDownEvent
        {
            public readonly Vector3 ScreenPosition;
            public readonly int ClickCount;

            public ClickDownEvent(Vector3 screenPosition, int clickCount)
            {
                ScreenPosition = screenPosition;
                ClickCount = clickCount;
            }
        }

        public class ClickUpEvent : ClickDownEvent
        {
            public ClickUpEvent(Vector3 screenPosition, int clickCount) : base(screenPosition, clickCount)
            {
            }
        }

        public class ZoomEvent // Aka scroll
        {
            public readonly float DeltaZoom;

            public ZoomEvent(float deltaZoom)
            {
                DeltaZoom = deltaZoom;
            }
        }

        public class PanEvent // Aka drag or swipe
        {
            public readonly Vector2 DeltaMove;

            public PanEvent(Vector2 deltaMove)
            {
                DeltaMove = deltaMove;
            }
        }
    }
}