using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MenuUI.Scripts.SoulHome
{
    public class FurnitureHandling : MonoBehaviour
    {
        private Furniture furniture;
        [SerializeField]
        private GameObject _trayFurnitureObject;
        private BoxCollider2D _collider;
        private Bounds _bounds;
        [SerializeField]
        private float cullCheckAmount = 0.1f;
        private Vector2 _position = Vector2.zero;
        private FurnitureSlot _slot;
        private FurnitureSlot _tempSlot;

        public Furniture Furniture { get => furniture; set => furniture = value; }
        public Vector2 Position { get => _position; set => _position = value; }
        public FurnitureSlot Slot { get => _slot;
            set
            {
                _slot = value;
                TempSlot = value;
            }
        }
        public FurnitureSlot TempSlot { get => _tempSlot; set => _tempSlot = value; }
        public GameObject TrayFurnitureObject { get => _trayFurnitureObject;}

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

        public void SaveSlot()
        {
            _slot = _tempSlot;
        }

        public void ResetSlot()
        {
            _tempSlot = _slot;
        }

        public void SetScale()
        {
            if (GetComponent<SpriteRenderer>().sortingOrder == 4) transform.localScale /= 1.1f;
            else if (GetComponent<SpriteRenderer>().sortingOrder == 5) transform.localScale /= 1.2f;

            if (_tempSlot.row == 0)
            {
                GetComponent<SpriteRenderer>().sortingOrder = 3;
                transform.localScale *= 1.0f;
            }
            else if (_tempSlot.row == 1)
            {
                GetComponent<SpriteRenderer>().sortingOrder = 4;
                transform.localScale *= 1.1f;
            }
            else if (_tempSlot.row == 2)
            {
                GetComponent<SpriteRenderer>().sortingOrder = 5;
                transform.localScale *= 1.2f;
            }
        }

        public void SetScale(int row)
        {
            if (GetComponent<SpriteRenderer>().sortingOrder == 4) transform.localScale /= 1.1f;
            else if (GetComponent<SpriteRenderer>().sortingOrder == 5) transform.localScale /= 1.2f;

            if (row == 0)
            {
                GetComponent<SpriteRenderer>().sortingOrder = 3;
                transform.localScale *= 1.0f;
            }
            else if (row == 1)
            {
                GetComponent<SpriteRenderer>().sortingOrder = 4;
                transform.localScale *= 1.1f;
            }
            else if (row == 2)
            {
                GetComponent<SpriteRenderer>().sortingOrder = 5;
                transform.localScale *= 1.2f;
            }
        }

        public void SetTransparency(float alpha)
        {
            alpha = Mathf.Clamp(alpha, 0.0f, 1.0f);
            Color color = GetComponent<SpriteRenderer>().color;
            color.a = alpha;
            GetComponent<SpriteRenderer>().color = color;
        }

    }
}
