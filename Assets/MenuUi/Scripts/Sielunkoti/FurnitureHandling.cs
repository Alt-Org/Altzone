using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MenuUI.Scripts.SoulHome
{
    public class FurnitureHandling : MonoBehaviour
    {
        private Furniture furniture;
        private BoxCollider2D _collider;
        private Bounds _bounds;
        [SerializeField]
        private float cullCheckAmount = 0.1f;
        private Vector2 _position = Vector2.zero;
        private FurnitureSlot _slot;

        public Furniture Furniture { get => furniture; set => furniture = value; }
        public Vector2 Position { get => _position; set => _position = value; }
        public FurnitureSlot Slot { get => _slot; set => _slot = value; }

        // Start is called before the first frame update
        void Start()
        {
            _collider = GetComponent<BoxCollider2D>();
            //_collider.size = GetComponent<SpriteRenderer>().;
            _bounds = _collider.bounds;
        }

        // Update is called once per frame
        void Update()
        {

        }

        public bool checkTopCollider(float hitY)
        {
            Vector2 tr = _bounds.max;
            Vector2 bl = _bounds.min;
            float limit = tr.y - Mathf.Abs(tr.y - bl.y) * cullCheckAmount;
            if (hitY >= limit) return true;
            else return false;
        }

        public void ResetFurniturePosition()
        {
            Vector2 position = Vector2.zero;
            //transform.localPosition = Vector2.zero;
            if(furniture.Size is FurnitureSize.OneXTwo)
            {
                    position.x = (transform.localScale.x/2)/2;
            }
            transform.localPosition = position;
        }
    }
}
