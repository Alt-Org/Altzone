using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;

namespace MenuUI.Scripts.SoulHome
{
    public class FurnitureSlot : MonoBehaviour
    {
        public int row;
        public int column;
        public FurnitureGrid furnitureGrid;
        public int roomId;
        public float maxDepthScale;
        public float maxRow;
        public float height;
        public float width;
        private Furniture furniture;
        private Furniture _furnitureNonBlock;
        private bool _rotated;
        private bool _rotatedNonBlock;
        private Furniture tempFurniture;
        private Furniture _tempFurnitureNonBlock;
        private bool _tempRotated;
        private bool _tempRotatedNonBlock;
        private bool _ladder = false;

        [SerializeField] private SpriteRenderer _slotValidityIndicator;

        public Furniture Furniture { get => furniture;
            set
            {
                furniture = value;
                if (value == null) _rotated = false;
                else _rotated = value.IsRotated;
                TempFurniture = value;
            }
        }
        public Furniture TempFurniture { get => tempFurniture;
            set
            {
                tempFurniture = value;
                if(value == null)_tempRotated = false;
                else _tempRotated = value.IsRotated;
            }
        }
        public Furniture FurnitureNonBlock { get => _furnitureNonBlock;
            set
            {
                _furnitureNonBlock = value;
                if (value == null) _rotatedNonBlock = false;
                else _rotatedNonBlock = value.IsRotated;
                TempFurnitureNonBlock = value;
            }
        }
        public Furniture TempFurnitureNonBlock { get => _tempFurnitureNonBlock;
            set
            {
                _tempFurnitureNonBlock = value;
                if (value == null) _tempRotatedNonBlock = false;
                else _tempRotatedNonBlock = value.IsRotated;
            }
        }
        public bool Rotated { get => _rotated;}
        public bool TempRotated { get => _tempRotated;}
        public bool RotatedNonBlock { get => _rotatedNonBlock; }
        public bool TempRotatedNonBlock { get => _tempRotatedNonBlock;}
        public bool Ladder { get => _ladder; set => _ladder = value; }

        public void InitializeSlot(int row, int column, int id, FurnitureGrid furnitureGrid, float scale, float maxRow, float width, float height)
        {
            this.row = row;
            this.column = column;
            this.furnitureGrid = furnitureGrid;
            roomId = id;
            maxDepthScale = scale;
            this.maxRow = maxRow;
            this.width = width;
            this.height = height;
            //GetComponent<PolygonCollider2D>().points;
            _slotValidityIndicator.transform.localScale = new Vector2(width, height);
            _slotValidityIndicator.sortingOrder = id*1000 + 901;
        }

        public void SetValidity(bool validity)
        {
            if (validity)
            {
                _slotValidityIndicator.color = new Color(0,1,0, 0.3f);
            }
            else
            {
                _slotValidityIndicator.color = new Color(1, 0, 0, 0.3f);
            }
            _slotValidityIndicator.gameObject.SetActive(true);
        }
        public void ClearValidity()
        {
            _slotValidityIndicator.gameObject.SetActive(false);
        }
    }
}
