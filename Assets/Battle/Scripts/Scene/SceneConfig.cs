using Altzone.Scripts.Battle;
using Battle.Scripts.interfaces;
using UnityEngine;

namespace Battle.Scripts.Scene
{
    /// <summary>
    /// General scene setup that was hard to organize in a better way.
    /// </summary>
    public class SceneConfig : MonoBehaviour
    {
        /// <summary>
        /// Startup object for the room.
        /// </summary>
        /// <remarks>
        /// This is used to keep <c>script execution order</c> under our control.
        /// </remarks>
        public GameObject roomStartup;

        /// <summary>
        /// Camera for this scene.
        /// </summary>
        public Camera _camera;

        /// <summary>
        /// Background for the game.
        /// </summary>
        public GameObject gameBackground;

        /// <summary>
        /// Is game camera rotated upside down (180 degrees)?
        /// </summary>
        public bool isCameraRotated { get; private set; }

        /// <summary>
        /// Nice actors can put themselves here - not to pollute top <c>GameObject</c> hierarchy.
        /// </summary>
        public GameObject actorParent;

        /// <summary>
        /// Game arena
        /// </summary>
        public IGameArena gameArena => _gameArena;

        [SerializeField] private GameArena _gameArena;

        /// <summary>
        /// Player start (instantiation) positions on game arena.
        /// </summary>
        public Transform[] playerStartPos = new Transform[4];

        /// <summary>
        /// Anchor positions for ball start if there is only one player.
        /// </summary>
        /// <remarks>
        /// Required for testing!
        /// </remarks>
        public Transform[] ballAnchors = new Transform[2];

        /// <summary>
        /// Ball needs to know where it travels and collides, this is area for upper team activity.
        /// </summary>
        public Collider2D upperTeamCollider;

        /// <summary>
        /// Ball needs to know where it travels and collides, this is area for lower team activity.
        /// </summary>
        public Collider2D lowerTeamCollider;

        /// <summary>
        /// Optional sprite for upper team play area.
        /// </summary>
        public SpriteRenderer upperTeamSprite;

        /// <summary>
        /// Optional sprite for lower team play area.
        /// </summary>
        public SpriteRenderer lowerTeamSprite;

        public static SceneConfig Get()
        {
            if (_Instance == null)
            {
                _Instance = FindObjectOfType<SceneConfig>();
                if (_Instance == null)
                {
                    throw new UnityException("SceneConfig not found");
                }
            }
            return _Instance;
        }

        private static SceneConfig _Instance;

        private void Awake()
        {
            roomStartup.SetActive(true);
        }

        private void OnDestroy()
        {
            _Instance = null;
        }

        public Rect getPlayArea(int playerPos)
        {
            // For convenience player start positions are kept under corresponding play area as child objects.
            // - play area is marked by collider to get its bounds for player area calculation!
            var playerIndex = PhotonBattle.GetPlayerIndex(playerPos);
            var startPos = playerStartPos[playerIndex];
            var playAreaTransform = startPos.parent;
            var center = playAreaTransform.position;
            var bounds = playAreaTransform.GetComponent<Collider2D>().bounds;
            var rect = calculateRectFrom(center, bounds);
            Debug.Log($"getPlayArea {playerPos} startPos {startPos.position} rect {rect}");
            return rect;
        }

        public void rotateBackground(bool upsideDown)
        {
            Debug.Log($"rotateBackground upsideDown {upsideDown}");
            var rotation = upsideDown
                ? Quaternion.Euler(0f, 0f, 180f) // Upside down
                : Quaternion.Euler(0f, 0f, 0f); // Normal orientation
            gameBackground.transform.rotation = rotation;
        }

        public void rotateGameCamera(bool upsideDown)
        {
            // Rotate game camera for upper team
            Debug.Log($"rotateGameCamera upsideDown {upsideDown}");
            var cameraTransform = _camera.transform;
            isCameraRotated = upsideDown;
            var rotation = upsideDown
                ? Quaternion.Euler(0f, 0f, 180f) // Upside down
                : Quaternion.Euler(0f, 0f, 0f); // Normal orientation
            cameraTransform.rotation = rotation;
        }

        private static Rect calculateRectFrom(Vector3 center, Bounds bounds)
        {
            var extents = bounds.extents;
            var size = bounds.size;
            var x = center.x - extents.x;
            var y = center.y - extents.y;
            var width = size.x;
            var height = size.y;
            return new Rect(x, y, width, height);
        }
    }
}