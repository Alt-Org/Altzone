using Battle1.Scripts.Battle.Game;
using UnityEngine;

namespace Battle1.Scripts.Battle.Players
{
    internal class PlayerPositionIndicator : MonoBehaviour
    {
        // Serialized Fields
        [SerializeField] private GameObject _glowSprite;
        [SerializeField] private Sprite _glow;

        // Important objects
        private GridManager _gridManager;

        // Components
        private SpriteRenderer _sprite;

        // Glow sprite position variables
        private GridPos _gridPosition;
        private Vector3 _worldPosition;

        private void Start()
        {
            // Get components
            _sprite = GetComponentInChildren<SpriteRenderer>();

            // Get important objects
            _gridManager = Context.GetGridManager;

            _sprite.enabled = false;
        }

        private void FixedUpdate()
        {
            // Check for touch input
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    // Convert touch position to world space
                    Vector3 touchPosition = Camera.main.ScreenToWorldPoint(touch.position);
                    touchPosition.z = 0;

                    // Update grid and world position based on the touch position
                    UpdateGridAndWorldPosition(touchPosition);

                    // Position the glow sprite at the updated world position
                    SetGlowSpritePosition(_worldPosition);
                }
            }
            // Check for mouse click
            else if (Input.GetMouseButtonDown(0))
            {
                // Convert mouse position to world space
                Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mousePosition.z = 0;

                // Update grid and world position based on the mouse position
                UpdateGridAndWorldPosition(mousePosition);

                // Position the glow sprite at the updated world position
                SetGlowSpritePosition(_worldPosition);
            }
        }

        public void SetGlowSpritePosition(Vector3 position)
        {
            _sprite.sprite = _glow;

            // Move the glow sprite to the new position
            _sprite.transform.position = position;
            _sprite.enabled = true;
        }

        private void UpdateGridAndWorldPosition(Vector3 position)
        {
            _gridPosition = _gridManager.WorldPointToGridPosition(position);
            _worldPosition = _gridManager.GridPositionToWorldPoint(_gridPosition);
        }
    }
}
