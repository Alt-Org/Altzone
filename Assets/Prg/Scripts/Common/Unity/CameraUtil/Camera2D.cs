/* Camera2D v1.0
 * 
 * By Jason Hein
 * https://assetstore.unity.com/packages/tools/camera/gameeye2d-76875
 */

using UnityEngine;

namespace Prg.Scripts.Common.Unity.CameraUtil
{
    /// <summary>
    /// When added to an orthographic camera, provides helper functions and collects information for making camera behaviors.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class Camera2D : MonoBehaviour
    {
        #region Variables

        private bool _hasCamera;
        private Camera _camera;
        private Transform _transform;

        //The camera will never go beyond these limits as long as you use position2D and zoom to set the camera's values.
        //The action rect will never go beyond these limits.
        [SerializeField] private Rect _myCameraLimits = new(-500, -500, 1000, 1000);
        [SerializeField] private Renderer _myCameraLimitsSource;

        #endregion

        #region Properties

        /// <summary>
        /// Gets the camera attached to the Camera2D.
        /// </summary>
        public Camera GameCamera
        {
            get
            {
                if (_hasCamera)
                {
                    return _camera;
                }
                _camera = GetComponent<Camera>();
                _transform = GetComponent<Transform>();
                _hasCamera = _camera != null;
                return _camera;
            }
        }

        /// <summary>
        /// Gets or sets the position of the camera as a vector2. This position is clamped to not put the camera's view outside of the camera limits.
        /// </summary>
        public Vector2 Position2D
        {
            get
            {
                var position = _transform.position;
                return new Vector2(position.x, position.y);
            }
            set
            {
                var viewSize = new Vector2(Zoom * GameCamera.aspect, Zoom);
                _transform.position = new Vector3(
                    Mathf.Clamp(value.x, CameraLimits.xMin + viewSize.x, CameraLimits.xMax - viewSize.x),
                    Mathf.Clamp(value.y, CameraLimits.yMin + viewSize.y, CameraLimits.yMax - viewSize.y),
                    _transform.position.z);
            }
        }

        /// <summary>
        /// Gets or sets the local position of the camera as a vector2. This position is clamped to not put the camera's view outside of the camera limits.
        /// </summary>
        public Vector2 LocalPosition2D
        {
            get
            {
                var localPosition = _transform.localPosition;
                return new Vector2(localPosition.x, localPosition.y);
            }
            set
            {
                if (_transform.parent == null)
                {
                    Position2D = value;
                }
                else
                {
                    var parentPosition = _transform.parent.position;
                    Position2D = value + new Vector2(parentPosition.x, parentPosition.y);
                }
            }
        }

        /// <summary>
        /// Gets or sets the orthographic size of the camera. The greater the value, the more zoomed out the camera.
        /// If the new zoom is larger than can be contained within the camera limits, the zoom is clamped to be containable within the camera limits.
        /// If the new zoom would look outside of the camera limits, the camera's position is clamped to not look out of the camera limits.
        /// </summary>
        public float Zoom
        {
            get { return GameCamera.orthographicSize; }
            set
            {
                GameCamera.orthographicSize = Mathf.Min(value, MaximumZoom(CameraLimits.size));
                Position2D = Position2D;
            }
        }

        /// <summary>
        /// Gets or sets the rect that the camera's view and the action rect are clamped to.
        /// </summary>
        public Rect CameraLimits
        {
            get => _myCameraLimits;
            set
            {
                _myCameraLimits = value;
                Zoom = Zoom;
            }
        }

        #endregion

        #region UNITY

        private void Awake()
        {
            _camera = GetComponent<Camera>();
            _transform = GetComponent<Transform>();
            _hasCamera = _camera != null;
            if (_myCameraLimitsSource != null)
            {
                var bounds = _myCameraLimitsSource.bounds;
                CameraLimits = new Rect(
                    bounds.min.x,
                    bounds.min.y,
                    bounds.extents.x * 2f,
                    bounds.extents.y * 2f);
            }
        }

        #endregion

        #region Public Functions

        /// <summary>
        /// Clamps the given position to the be within the camera's limits.
        /// </summary>
        public Vector2 ClampToCameraLimits(Vector2 position)
        {
            return new Vector2(Mathf.Clamp(position.x, CameraLimits.xMin, CameraLimits.xMax),
                Mathf.Clamp(position.y, CameraLimits.yMin, CameraLimits.yMax));
        }

        /// <summary>
        /// Clamps the given position to the be within the camera's limits.
        /// </summary>
        public Vector3 ClampToCameraLimits(Vector3 position)
        {
            return new Vector3(Mathf.Clamp(position.x, CameraLimits.xMin, CameraLimits.xMax),
                Mathf.Clamp(position.y, CameraLimits.yMin, CameraLimits.yMax), position.z);
        }

        /// <summary>
        /// Clamps a given rectangle to be within a camera limits and returns the center position.
        /// </summary>
        public Vector2 ClampToCameraLimits(Rect rect)
        {
            //Get the camera view rect and half the size of the camera
            var halfSize = new Vector2(Mathf.Min(rect.width, CameraLimits.width),
                Mathf.Min(rect.height, CameraLimits.height)) / 2f;

            //Clamp the center of the rect as if the camera view was smaller by an amount equal to half the size of the rect
            return new Vector2(
                Mathf.Clamp(rect.center.x, CameraLimits.xMin + halfSize.x, CameraLimits.xMax - halfSize.x),
                Mathf.Clamp(rect.center.y, CameraLimits.yMin + halfSize.y, CameraLimits.yMax - halfSize.y));
        }

        /// <summary>
        /// Clamps a given bounds to be within a camera limits and returns the center position.
        /// </summary>
        public Vector3 ClampToCameraLimits(Bounds bounds)
        {
            //Clamp the center of the bounds as if the camera view was smaller by an amount equal to the extents of the bounds
            return new Vector3(
                Mathf.Clamp(bounds.center.x, CameraLimits.xMin + bounds.extents.x,
                    CameraLimits.xMax - bounds.extents.x),
                Mathf.Clamp(bounds.center.y, CameraLimits.yMin + bounds.extents.y,
                    CameraLimits.yMax - bounds.extents.y),
                bounds.center.z);
        }

        /// <summary>
        /// Clamps a given position to be within the camera's view.
        /// </summary>
        public Vector2 ClampToCameraView(Vector2 position)
        {
            var cameraView = GetViewRectAsWorldSpace(GameCamera);
            return new Vector2(Mathf.Clamp(position.x, cameraView.xMin, cameraView.xMax),
                Mathf.Clamp(position.y, cameraView.yMin, cameraView.yMax));
        }

        /// <summary>
        /// Clamps a given position to be within the camera's view.
        /// </summary>
        public Vector3 ClampToCameraView(Vector3 position)
        {
            var cameraView = GetViewRectAsWorldSpace(GameCamera);

            return new Vector3(Mathf.Clamp(position.x, cameraView.xMin, cameraView.xMax),
                Mathf.Clamp(position.y, cameraView.yMin, cameraView.yMax), position.z);
        }

        /// <summary>
        /// Clamps a given rectangle to be within the camera's view and returns the center position.
        /// </summary>
        public Vector2 ClampToCameraView(Rect rect)
        {
            //Get the camera view rect and half the size of the camera
            var cameraView = GetViewRectAsWorldSpace(GameCamera);
            var halfSize = new Vector2(Mathf.Min(rect.width, cameraView.width),
                Mathf.Min(rect.height, cameraView.height)) / 2f;

            //Clamp the center of the rect as if the camera view was smaller by an amount equal to half the size of the rect
            return new Vector2(Mathf.Clamp(rect.center.x, cameraView.xMin + halfSize.x, cameraView.xMax - halfSize.x),
                Mathf.Clamp(rect.center.y, cameraView.yMin + halfSize.y, cameraView.yMax - halfSize.y));
        }

        /// <summary>
        /// Clamps a given bounds to be within the camera's view and returns the center position.
        /// </summary>
        public Vector3 ClampToCameraView(Bounds bounds)
        {
            var cameraView = GetViewRectAsWorldSpace(GameCamera);

            //Clamp the center of the bounds as if the camera view was smaller by an amount equal to the extents of the bounds
            return new Vector3(
                Mathf.Clamp(bounds.center.x, cameraView.xMin + bounds.extents.x, cameraView.xMax - bounds.extents.x),
                Mathf.Clamp(bounds.center.y, cameraView.yMin + bounds.extents.y, cameraView.yMax - bounds.extents.y),
                bounds.center.z);
        }

        /// <summary>
        /// Calculates the maximum orthographic size of the camera that would keep the view inside a rectangle of the given size.
        /// </summary>
        public float MaximumZoom(Vector2 size)
        {
            size /= 2f;
            return Mathf.Min(size.x / GameCamera.aspect, size.y);
        }

        /// <summary>
        /// Calculates the maximum orthographic size of the camera that would keep a rect of the given size inside the camera's view.
        /// </summary>
        public float WorldToZoom(Vector2 size)
        {
            size /= 2f;
            return Mathf.Max(size.x / GameCamera.aspect, size.y);
        }

        #endregion

        #region Static Functions

        /// <summary>
        /// Clamps a given position to be within a camera's view and returns the new position.
        /// </summary>
        public static Vector2 ClampToCameraView(Camera camera, Vector2 position)
        {
            var cameraView = GetViewRectAsWorldSpace(camera);
            return new Vector2(Mathf.Clamp(position.x, cameraView.xMin, cameraView.xMax),
                Mathf.Clamp(position.y, cameraView.yMin, cameraView.yMax));
        }

        /// <summary>
        /// Clamps a given position to be within a camera's view and returns the new position.
        /// </summary>
        public static Vector3 ClampToCameraView(Camera camera, Vector3 position)
        {
            var cameraView = GetViewRectAsWorldSpace(camera);

            return new Vector3(Mathf.Clamp(position.x, cameraView.xMin, cameraView.xMax),
                Mathf.Clamp(position.y, cameraView.yMin, cameraView.yMax), position.z);
        }

        /// <summary>
        /// Clamps a given rectangle to be within a camera's view and returns the center position.
        /// </summary>
        public static Vector2 ClampToCameraView(Camera camera, Rect rect)
        {
            //Get the camera view rect and half the size of the camera
            var cameraView = GetViewRectAsWorldSpace(camera);
            var halfSize = new Vector2(Mathf.Min(rect.width, cameraView.width),
                Mathf.Min(rect.height, cameraView.height)) / 2f;

            //Clamp the center of the rect as if the camera view was smaller by an amount equal to half the size of the rect
            return new Vector2(Mathf.Clamp(rect.center.x, cameraView.xMin + halfSize.x, cameraView.xMax - halfSize.x),
                Mathf.Clamp(rect.center.y, cameraView.yMin + halfSize.y, cameraView.yMax - halfSize.y));
        }

        /// <summary>
        /// Clamps the given bounds to be within a camera's view and returns the center position.
        /// </summary>
        public static Vector3 ClampToCameraView(Camera camera, Bounds bounds)
        {
            var cameraView = GetViewRectAsWorldSpace(camera);

            //Clamp the center of the bounds as if the camera view was smaller by an amount equal to the extents of the bounds
            return new Vector3(
                Mathf.Clamp(bounds.center.x, cameraView.xMin + bounds.extents.x, cameraView.xMax - bounds.extents.x),
                Mathf.Clamp(bounds.center.y, cameraView.yMin + bounds.extents.y, cameraView.yMax - bounds.extents.y),
                bounds.center.z);
        }

        /// <summary>
        /// Calculates the view of the given camera as a rectangle in worldspace coordinates.
        /// </summary>
        public static Rect GetViewRectAsWorldSpace(Camera camera)
        {
            if (!camera.orthographic)
            {
#if UNITY_EDITOR
                Debug.Log(MessageRequireOrthographic);
#endif
                return new Rect(0f, 0f, 1f, 1f);
            }

            var position = camera.transform.position;
            var orthographicSize = camera.orthographicSize;
            var aspect = camera.aspect;
            return new Rect(
                position.x - orthographicSize * aspect,
                position.y - orthographicSize,
                orthographicSize * 2f * aspect,
                orthographicSize * 2f);
        }

        /// <summary>
        /// Calculates the maximum orthographic size of a view of the given aspect ratio that would keep the view inside a rectangle of the given size.
        /// </summary>
        public static float MaximumZoom(Vector2 size, float aspectRatio)
        {
            size /= 2f;
            return Mathf.Min(size.x / aspectRatio, size.y);
        }

        /// <summary>
        /// Calculates the maximum orthographic size that would keep a rect of the given size inside the view of the given aspect ratio.
        /// </summary>
        public static float WorldToZoom(Vector2 size, float aspectRatio)
        {
            size /= 2f;
            return Mathf.Max(size.x / aspectRatio, size.y);
        }

        #endregion

        #region UNITY Editor

#if UNITY_EDITOR

        //Track previous position and zoom while the game is not playing, to check if they have changed and if the camera should update them to be within the camera limits while in the scene window
        private Vector3 _lastPos = Vector3.zero;
        private float _lastZoom = 5f;

        //Whether to draw debug data or not
        [SerializeField] private bool _isDrawDebugGizmos;

        //The radius of the center sphere when drawing gizmo rectangles
        private const float DrawOriginSize = 0.1f;

        //The minimum size the camera limits can be
        private const float MinimumCameraLimitsSize = 1f;

        //Warning messages
        private const string MessageRequireOrthographic = "The camera used for Camera2D must be set to orthographic.";

        //Every frame in the editor that the game is not playing the camera is clamped to be within the camera's limits
        private void Update()
        {
            if (!Application.isPlaying)
            {
                //If the camera moves or zooms in the unity editor, the camera checks if it has been set outside the camera limits
                if ((_lastPos != _transform.position || _lastZoom != GameCamera.orthographicSize))
                {
                    //Clamp the camera's view to be within the camera limits while in the editor
                    Zoom = GameCamera.orthographicSize;

                    //Save the last position and zoom
                    _lastPos = _transform.position;
                    _lastZoom = Zoom;
                }
            }
        }

        //When you reset through the context menu or add the component for the first time, change the camera to orthographic
        private void Reset()
        {
            GameCamera.orthographic = true;
        }

        //When you change something in the inspector, or when you start or stop the game in the editor
        private void OnValidate()
        {
            //Make sure the camera is set to orhographic in the editor
            if (GameCamera.orthographic == false)
            {
                GameCamera.orthographic = true;
                Debug.Log(MessageRequireOrthographic);
            }

            //The camera limits cannot have the minimum greater than the maximum, or a minimum that would cause a camera error
            _myCameraLimits.width = Mathf.Max(MinimumCameraLimitsSize, _myCameraLimits.width);
            _myCameraLimits.height = Mathf.Max(MinimumCameraLimitsSize, _myCameraLimits.height);
        }

        //Sets the camera's local XY position to 0.
        [ContextMenu("Set origin To (0, 0)")] private void SetLocalPositionToZero()
        {
            if (_transform.parent != null)
            {
                Position2D = _transform.parent.position;
            }
            else
            {
                Position2D = Vector2.zero;
            }
        }

        //Logs the maximum zoom possible
        [ContextMenu("Log Maximum Zoom")] private void LogMaximumZoom()
        {
            Debug.LogFormat("MaximumZoom {0}", MaximumZoom(CameraLimits.size, GameCamera.aspect));
        }

        //Logs the view rect in world space
        [ContextMenu("Log View Rect As World Space")] private void LogViewRectAsWorldSpace()
        {
            Debug.LogFormat("LogViewRectAsWorldSpace {0}", GetViewRectAsWorldSpace(GameCamera));
        }

        //In the scene window draw the camera limits and action rect
        private void OnDrawGizmos()
        {
            if (_isDrawDebugGizmos)
            {
                DrawGizmoRect(CameraLimits, Color.red);
            }
        }

        //Draws the given rect using gizmos
        private void DrawGizmoRect(Rect rect, Color color)
        {
            //Set the gizmo color
            Gizmos.color = color;

            if (_hasCamera)
            {
                _transform = GetComponent<Transform>();
            }
            //Get the rect's corner positions
            var z = _transform.position.z;
            var topLeft = new Vector3(rect.xMin, rect.yMax, z);
            var botLeft = new Vector3(rect.xMin, rect.yMin, z);
            var topRight = new Vector3(rect.xMax, rect.yMax, z);
            var botRight = new Vector3(rect.xMax, rect.yMin, z);

            //Draw the rect
            Gizmos.DrawLine(topLeft, botLeft);
            Gizmos.DrawLine(topRight, botRight);
            Gizmos.DrawLine(topLeft, topRight);
            Gizmos.DrawLine(botLeft, botRight);

            Gizmos.DrawWireSphere(new Vector3(rect.center.x, rect.center.y, z), DrawOriginSize);
        }
#endif

        #endregion
    }
}