using Battle.Scripts.interfaces;
using Prg.Scripts.Common.Unity;
using UnityEngine;

namespace Battle.Scripts.Scene
{
    /// <summary>
    /// Creates reversed box collider around given template <c>Sprite</c> that provides the area to be "boxed" by colliders.
    /// </summary>
    /// <remarks>
    /// Wall collider parent, "wall" thickness, tag and layer are configurable.
    /// </remarks>
    public class GameArena : MonoBehaviour, IGameArena
    {
        [Header("Settings"), SerializeField] private SpriteRenderer templateSprite;
        [SerializeField] private Transform colliderParent;
        [SerializeField] private float wallThickness;
        [SerializeField] private PhysicsMaterial2D wallMaterial;
        [SerializeField, TagSelector] private string wallTopTag;
        [SerializeField, LayerSelector] private int wallTopLayer;
        [SerializeField, TagSelector] private string wallBottomTag;
        [SerializeField, LayerSelector] private int wallBottomLayer;
        [SerializeField, TagSelector] private string wallLeftTag;
        [SerializeField, LayerSelector] private int wallLeftLayer;
        [SerializeField, TagSelector] private string wallRightTag;
        [SerializeField, LayerSelector] private int wallRightLayer;

        [Header("Live Data"), SerializeField] private BoxCollider2D wallTop;
        [SerializeField] private BoxCollider2D wallBottom;
        [SerializeField] private BoxCollider2D wallLeft;
        [SerializeField] private BoxCollider2D wallRight;
        [SerializeField] private Rect _outerArea;

        Rect IGameArena.outerArea => _outerArea;

        void IGameArena.makeWalls()
        {
            wallTop = createWall("wallTop", colliderParent, wallTopTag, wallTopLayer, wallMaterial);
            wallBottom = createWall("wallBottom", colliderParent, wallBottomTag, wallBottomLayer, wallMaterial);
            wallLeft = createWall("wallLeft", colliderParent, wallLeftTag, wallLeftLayer, wallMaterial);
            wallRight = createWall("wallRight", colliderParent, wallRightTag, wallRightLayer, wallMaterial);

            if (wallThickness == 0)
            {
                throw new UnityException("wall thickness can not be zero");
            }
            var size = templateSprite.size;
            var width = size.x / 2f;
            var height = size.y / 2f;
            var wallAdjustment = wallThickness / 2f;

            wallTop.offset = new Vector2(0f, height + wallAdjustment);
            wallTop.size = new Vector2(size.x, wallThickness);

            wallBottom.offset = new Vector2(0f, -height - wallAdjustment);
            wallBottom.size = new Vector2(size.x, wallThickness);

            wallLeft.offset = new Vector2(-width - wallAdjustment, 0f);
            wallLeft.size = new Vector2(wallThickness, size.y);

            wallRight.offset = new Vector2(width + wallAdjustment, 0f);
            wallRight.size = new Vector2(wallThickness, size.y);

            var originalArea = calculateRectFrom(templateSprite.transform.position, templateSprite.bounds);
            _outerArea = originalArea.Inflate(wallThickness, wallThickness, wallThickness, wallThickness);
        }

        private static BoxCollider2D createWall(string name, Transform parent, string tag, int layer, PhysicsMaterial2D wallMaterial)
        {
            var wall = new GameObject(name) { isStatic = true };
            wall.transform.SetParent(parent);
            if (!string.IsNullOrEmpty(tag))
            {
                wall.tag = tag;
            }
            wall.layer = layer;
            wall.isStatic = true;
            var collider = wall.AddComponent<BoxCollider2D>();
            collider.sharedMaterial = wallMaterial;
            return collider;
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