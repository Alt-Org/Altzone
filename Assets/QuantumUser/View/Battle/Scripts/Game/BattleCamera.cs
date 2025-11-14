/// @file BattleCamera.cs
/// <summary>
/// Contains @cref{Battle.View.Game,BattleCamera} class which handles %Battle camera.
/// </summary>
///
/// This script:<br/>
/// Handles %Battle camera functionality.

// Unity usings
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.View.Game
{
    /// <summary>
    /// <span class="brief-h">%Battle camera <a href="https://docs.unity3d.com/ScriptReference/MonoBehaviour.html">Unity MonoBehaviour script@u-exlink</a>.</span><br/>
    /// Handles %Battle camera functionality.
    /// </summary>
    public class BattleCamera : MonoBehaviour
    {
        /// @anchor BattleCamera-SerializeFields
        /// @name SerializeField variables
        /// <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/SerializeField.html">SerializeFields@u-exlink</a> are serialized variables exposed to the Unity editor.
        /// @{

        /// <summary>[SerializeField] Reference to the <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/Camera.html">Camera@u-exlink</a>.</summary>
        /// @ref BattleCamera-SerializeFields
        [SerializeField] private Camera _camera;

        /// <summary>[SerializeField] .</summary>
        /// @ref BattleCamera-SerializeFields
        [SerializeField] private Vector2 _targetSizeMin;

        /// <summary>[SerializeField] .</summary>
        /// @ref BattleCamera-SerializeFields
        [SerializeField] private Vector2 _targetSizeMax;

        /// <summary>[SerializeField] .</summary>
        /// @ref BattleCamera-SerializeFields
        [SerializeField] private float   _targetDistance;

        /// <summary>[SerializeField] .</summary>
        /// @ref BattleCamera-SerializeFields
        [SerializeField] private Color _colorOff;

        /// <summary>[SerializeField] .</summary>
        /// @ref BattleCamera-SerializeFields
        [SerializeField] private Color _colorOutOfBounds;

        /// <summary>[SerializeField] .</summary>
        /// @ref BattleCamera-SerializeFields
        [SerializeField] private Color _colorDebug;

        /// @}

        /// <summary>
        /// Public static getter for #_camera in #s_instance.
        /// </summary>
        public static Camera Camera => s_instance._camera;

        /// <summary>
        /// Public static method to set view variables for positioning the camera.
        /// </summary>
        ///
        /// <param name="scale">The %Battle arena scale.</param>
        /// <param name="offset">The %Battle arena offset.</param>
        /// <param name="rotate">If the camera should rotate or not.</param>
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

        /// <summary>
        /// Public static method to set view variables for having the camera off.
        /// </summary>
        public static void UnsetView()
        {
            s_instance._camera.backgroundColor = s_instance._colorOff;
            s_instance._camera.rect = s_fixedRect;
            s_instance.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
            s_instance._active = false;
        }

        /// <value>.</value>
        private static readonly Rect s_fixedRect = new(0, 0, 1, 1);

        /// <value>Singleton instance of BattleArena.</value>
        private static BattleCamera s_instance;

        /// <value>If the camera is active or not.</value>
        private bool _active;

        /// <value>.</value>
        private float _targetAspect;

        /// <value>.</value>
        private Vector2 _targetExtendsMax;

        /// <value>The %Battle arena scale.</value>
        private float   _scale;

        /// <value>The %Battle arena offset.</value>
        private Vector2 _offset;

        /// <value>If the camera should rotate or not.</value>
        private bool    _rotate;

        /// <value>.</value>
        private float _prevAspect;

        /// <summary>
        /// Private <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/MonoBehaviour.Awake.html">Awake@u-exlink</a> method which .
        /// </summary>
        private void Awake()
        {
            Assert.IsNotNull(_camera, "_camera must be assigned in Editor");

            _targetAspect = _targetSizeMin.x / _targetSizeMin.y;
            _targetExtendsMax = _targetSizeMax * 0.5f;

            s_instance = this;

            UnsetView();
        }

        /// <summary>
        /// Private <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/MonoBehaviour.Update.html">Update@u-exlink</a> method which .
        /// </summary>
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

        /// <summary>
        /// Private <a href="https://docs.unity3d.com/2022.3/Documentation/ScriptReference/MonoBehaviour.OnPreCull.html">OnPreCull@u-exlink</a> method which .
        /// </summary>
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
