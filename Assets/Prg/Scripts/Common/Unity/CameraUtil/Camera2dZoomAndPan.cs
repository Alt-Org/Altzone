using Prg.Scripts.Common.PubSub;
using Prg.Scripts.Common.Unity.Input;
using UnityEngine;

namespace Prg.Scripts.Common.Unity.CameraUtil
{
    public interface ICamera2dZoomAndPan
    {
        void MoveCamera(Vector2 deltaPosition);
        void MoveCameraTo(Vector2 position);
        void Zoom(float deltaZoom);
        void SetZoomTo(float zoomSize);
    }
    
    [RequireComponent(typeof(Camera2D))]
    public class Camera2dZoomAndPan : MonoBehaviour, ICamera2dZoomAndPan
    {
        [Header("Zoom")] public bool _isZoomEnabled = true;
        [SerializeField] private float _minZoom = 5f;
        [SerializeField] private float _maxZoom = 18f;

        [Header("Pan")] public bool _isPanEnabled = true;

        [Header("Live Data")] public float _currentZoom;

        private Camera2D _camera2D;

        protected void Awake()
        {
            _camera2D = GetComponent<Camera2D>();
            if (_minZoom < 1f)
            {
                _minZoom = 1f;
            }
            if (_maxZoom < _minZoom)
            {
                _maxZoom = _minZoom;
            }
            _currentZoom = _camera2D.Zoom;
        }

        private void OnEnable()
        {
            this.Subscribe<InputManager.PanEvent>((data) => MoveCamera(data.DeltaMove));
            this.Subscribe<InputManager.ZoomEvent>((data) => Zoom(data.DeltaZoom));
        }

        private void OnDisable()
        {
            this.Unsubscribe();
        }

        public void MoveCamera(Vector2 deltaPosition)
        {
            if (!_isPanEnabled) return;
            _camera2D.Position2D += deltaPosition;
        }

        public void MoveCameraTo(Vector2 position)
        {
            if (!_isPanEnabled) return;
            _camera2D.Position2D = position;
        }

        public void Zoom(float deltaZoom)
        {
            if (!_isZoomEnabled) return;
            _currentZoom = Mathf.Clamp(_currentZoom + deltaZoom, _minZoom, _maxZoom);
            _camera2D.Zoom = _currentZoom;
        }

        public void SetZoomTo(float zoomSize)
        {
            if (!_isZoomEnabled) return;
            _currentZoom = Mathf.Clamp(zoomSize, _minZoom, _maxZoom);
            _camera2D.Zoom = _currentZoom;
        }
    }
}