using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Debug = Prg.Debug;

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
            float width;
            if (furniture.Size is FurnitureSize.OneXOne)
            {
                width = _tempSlot.width;
            }
            else if (furniture.Size is FurnitureSize.OneXTwo)
            {
                width = _tempSlot.width * 2;
            }
            else if (furniture.Size is FurnitureSize.OneXFour)
            {
                width = _tempSlot.width * 4;
            }
            else
            {
                Debug.LogError("Invalid furniture size.");
                return;
            }
            position.x = (transform.localScale.x / 2) - _tempSlot.width / 2;
            position.y = -1 * (_tempSlot.height/2);
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
            transform.localScale /= 1.0f + (_tempSlot.maxDepthScale / 100f) * ((GetComponent<SpriteRenderer>().sortingOrder < 3 ? 3 : GetComponent<SpriteRenderer>().sortingOrder - 3) / (_tempSlot.maxRow - 1f));

            GetComponent<SpriteRenderer>().sortingOrder = 3 + _tempSlot.row;
            transform.localScale *= 1.0f + (_tempSlot.maxDepthScale/100f) * (((float)_tempSlot.row)/(_tempSlot.maxRow-1f));
            Debug.Log("Scale 1: " + ((_tempSlot.maxDepthScale / 100f) * (((float)_tempSlot.row) / (_tempSlot.maxRow - 1f))));
        }

        public void SetScale(int row, FurnitureSlot slot)
        {
            transform.localScale /= 1.0f + (slot.maxDepthScale / 100f) * ((GetComponent<SpriteRenderer>().sortingOrder < 3 ? 3 : GetComponent<SpriteRenderer>().sortingOrder - 3) / (slot.maxRow - 1f));

            GetComponent<SpriteRenderer>().sortingOrder = 3 + row;
            transform.localScale *= (1.0f + (slot.maxDepthScale / 100f) * (((float)row) / (slot.maxRow - 1f)));
            Debug.Log("Scale 2: " +(slot.maxDepthScale / 100f) * (((float)row) / (slot.maxRow - 1f)));
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
