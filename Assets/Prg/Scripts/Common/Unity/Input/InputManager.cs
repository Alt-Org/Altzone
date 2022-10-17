using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Prg.Scripts.Common.Unity.Input
{
    /// <summary>
    /// Input manager to read mouse and touch inputs and convert them to higher level events.<br />
    /// These events are click down, click up, zoom and pan.
    /// </summary>
    public class InputManager : MonoBehaviour
    {
        [Header("Settings"), SerializeField] private InputManagerConfig _config; // For manual Editor config
        [SerializeField] private InputActionReference _scrollWheelActionRef;

        [Header("Live Data"), SerializeField] private ZoomAndPanSettings _mouse;
        [SerializeField] private ZoomAndPanSettings _touch;
        [SerializeField] private BaseHandler _handler;

        // We create clones because objects can be "in" ScriptableObject and we do not want to change original values on disk!
        public ZoomAndPanSettings Mouse
        {
            get => _mouse;
            set => _mouse = value.Clone();
        }

        public ZoomAndPanSettings Touch
        {
            get => _touch;
            set => _touch = value.Clone();
        }

        public BaseHandler Handler => _handler;

        private void Awake()
        {
            if (_config != null)
            {
                Mouse = _config._mouse;
                Touch = _config._touch;
            }
            if (Handler == null)
            {
                if (AppPlatform.IsMobile || AppPlatform.IsSimulator)
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

        /// <summary>
        /// Marker interface for input handlers.
        /// </summary>
        public interface IInputHandler
        {
        }

        /// <summary>
        /// Setting for Zoom and Pan.
        /// </summary>
        /// <remarks>
        /// Mouse and touch needs to have separate settings because devices behave differently.
        /// </remarks>
        [Serializable]
        public class ZoomAndPanSettings
        {
            public bool _isZoom;
            public float _zoomSpeed;
            public bool _isPan;
            public float _panSpeed;

            public ZoomAndPanSettings Clone()
            {
                return MemberwiseClone() as ZoomAndPanSettings;
            }
        }

        public class ClickDownEvent
        {
            public readonly Vector2 ScreenPosition;
            public readonly int ClickCount;

            public ClickDownEvent(Vector2 screenPosition, int clickCount)
            {
                ScreenPosition = screenPosition;
                ClickCount = clickCount;
            }

            public override string ToString()
            {
                return $"{nameof(ScreenPosition)}: {ScreenPosition}, {nameof(ClickCount)}: {ClickCount}";
            }
        }

        public class ClickUpEvent
        {
            public readonly Vector2 ScreenPosition;
            public readonly int ClickCount;

            public ClickUpEvent(Vector2 screenPosition, int clickCount)
            {
                ScreenPosition = screenPosition;
                ClickCount = clickCount;
            }

            public override string ToString()
            {
                return $"{nameof(ScreenPosition)}: {ScreenPosition}, {nameof(ClickCount)}: {ClickCount}";
            }
        }

        /// <summary>
        /// Zoom aka scroll.
        /// </summary>
        public class ZoomEvent
        {
            /// <summary>
            /// Relative "zoom" positive/negative value compared to previous event.
            /// </summary>
            public readonly float DeltaZoom;

            public ZoomEvent(float deltaZoom)
            {
                DeltaZoom = deltaZoom;
            }

            public override string ToString()
            {
                return $"{nameof(DeltaZoom)}: {DeltaZoom}";
            }
        }

        /// <summary>
        /// Pan aka drag or swipe.
        /// </summary>
        public class PanEvent
        {
            /// <summary>
            /// Relative "pan" direction compared to previous event.
            /// </summary>
            public readonly Vector2 DeltaMove;

            public PanEvent(Vector2 deltaMove)
            {
                DeltaMove = deltaMove;
            }

            public override string ToString()
            {
                return $"{nameof(DeltaMove)}: {DeltaMove}";
            }
        }
    }
}