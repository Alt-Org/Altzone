using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Prg.Scripts.Common.Unity.Input
{
    public class InputManager : MonoBehaviour
    {
        [Header("Settings"), SerializeField] private InputManagerConfig _config; // For manual Editor config
        [SerializeField] private InputActionReference _scrollWheelActionRef;

        [Header("Live Data"),SerializeField] private ZoomAndPan _mouse;
        [SerializeField] private ZoomAndPan _touch;
        [SerializeField] private  BaseHandler _handler;

        // We create clones because objects can be "in" ScriptableObject and we do not want to change original values on disk!
        public ZoomAndPan Mouse
        {
            get => _mouse;
            set => _mouse = value.Clone();
        }

        public ZoomAndPan Touch
        {
            get => _touch;
            set => _touch = value.Clone();
        }

        public BaseHandler Handler => _handler;

        private void Awake()
        {
            if (_config != null)
            {
                _mouse = _config._mouse;
                _touch = _config._touch;
            }
            if (Handler == null)
            {
                if (Application.isMobilePlatform)
                {
                    _handler = gameObject.GetOrAddComponent<TouchHandler>()
                        .Configure(Touch);
                }
                else
                {
                    _handler = gameObject.GetOrAddComponent<MouseHandler>()
                        .ScrollWheel(_scrollWheelActionRef)
                        .Configure(Mouse);
                }
            }
            Debug.Log($"handler {Handler}");
        }

        public interface IInputHandler
        {
        }

        [Serializable]
        public class ZoomAndPan
        {
            public bool _isZoom;
            public float _zoomSpeed;
            public float _minZoomSpeed;
            public float _maxZoomSpeed;
            public bool _isPan;
            public float _panSpeed;
            public float _minPanSpeed;
            public float _maxPanSpeed;

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