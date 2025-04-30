using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.View.Game
{
    public class BattleCamera : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private Vector2 _targetSizeMin;
        [SerializeField] private Vector2 _targetSizeMax;
        [SerializeField] private float   _targetDistance;
        [SerializeField] private Color _colorOff;
        [SerializeField] private Color _colorOutOfBounds;
        [SerializeField] private Color _colorDebug;

        public static Camera Camera => s_instance._camera;

        public static void SetView(float scale, Vector2 offset, bool rotate)
        {
            s_instance._scale = 1.0f / scale;
            s_instance._offset = new Vector2(
                offset.x - 0.5f,
                offset.y - 0.5f
            );
            s_instance._rotate = rotate;

            s_instance._active = true;
            s_instance._prevAspect = -1f;
        }

        public static void UnsetView()
        {
            s_instance._camera.backgroundColor = s_instance._colorOff;
            s_instance._camera.rect = s_fixedRect;
            s_instance.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
            s_instance._active = false;
        }

        private static readonly Rect s_fixedRect = new(0, 0, 1, 1);

        private static BattleCamera s_instance;

        private bool _active;

        private float _targetAspect;
        private Vector2 _targetExtendsMax;

        private float   _scale;
        private Vector2 _offset;
        private bool    _rotate;

        private float _prevAspect;

        private void Awake()
        {
            Assert.IsNotNull(_camera, "_camera must be assigned in Editor");

            _targetAspect = _targetSizeMin.x / _targetSizeMin.y;
            _targetExtendsMax = _targetSizeMax * 0.5f;

            s_instance = this;

            UnsetView();
        }

        private void Update()
        {
            if (!_active) return;

            float aspect = (float)Screen.width / (float)Screen.height;
            if (aspect == _prevAspect) return;

            Vector2 size;

            if (aspect >= _targetAspect)
            {
                size = new(
                    _targetSizeMin.y * aspect,
                    _targetSizeMin.y
                );
            }
            else
            {
                size = new(
                    _targetSizeMin.x,
                    _targetSizeMin.x / aspect
                );
            }

            size *= _scale;
            Vector2 extends = size * 0.5f;

            Vector2 offsetMax = new(
                size.x - _targetSizeMin.x,
                size.y - _targetSizeMin.y
            );

            Vector2 position = new(
                -_offset.x * offsetMax.x,
                 _offset.y * offsetMax.y
            );

            float outOfBoundsTop    = Mathf.Max(extends.y + position.y - _targetExtendsMax.y, 0f);
            float outOfBoundsBottom = Mathf.Max(extends.y - position.y - _targetExtendsMax.y, 0f);
            float outOfBoundsLeft   = Mathf.Max(extends.x - position.x - _targetExtendsMax.x, 0f);
            float outOfBoundsRight  = Mathf.Max(extends.x + position.x - _targetExtendsMax.x, 0f);

            position.x += (outOfBoundsLeft   - outOfBoundsRight) * 0.5f;
            position.y += (outOfBoundsBottom - outOfBoundsTop  ) * 0.5f;

            if (_rotate) position *= -1f;

            _camera.backgroundColor = _colorDebug;

            _camera.orthographicSize = extends.y - (outOfBoundsBottom + outOfBoundsTop) * 0.5f;

            _camera.rect = new Rect(
               x:      outOfBoundsLeft                                   / size.x,
               y:      outOfBoundsBottom                                 / size.y,
               width:  (size.x - (outOfBoundsLeft   + outOfBoundsRight)) / size.x,
               height: (size.y - (outOfBoundsBottom + outOfBoundsTop  )) / size.y
            );

            transform.SetPositionAndRotation(
                position: new Vector3(
                    position.x,
                    _targetDistance,
                    position.y
                ),
                rotation: Quaternion.Euler(
                    90f,
                    0f,
                    !_rotate ? 0f : 180f
                )
            );

            _prevAspect = aspect;
        }

        private void OnPreCull()
        {
            // https://forum.unity.com/threads/force-camera-aspect-ratio-16-9-in-viewport.385541/
            Rect _tempRect = _camera.rect;
            _camera.rect = s_fixedRect;
            GL.Clear(true, true, _colorOutOfBounds);
            _camera.rect = _tempRect;
        }
    }
}
