using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MenuUI.Scripts.SoulHome
{
    public class FurnitureSlot : MonoBehaviour
    {
        public int row;
        public int column;
        public int roomId;
        public float maxDepthScale;
        public float maxRow;
        public float height;
        public float width;
        private Furniture furniture;
        private bool _rotated;
        private Furniture tempFurniture;
        private bool _tempRotated;

        public Furniture Furniture { get => furniture;
            set
            {
                furniture = value;
                if (value == null) _tempRotated = false;
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
        public bool Rotated { get => _rotated;}
        public bool TempRotated { get => _tempRotated;}

        public void InitializeSlot(int row, int column, int id, float scale, float maxRow, float width, float height)
        {
            this.row = row;
            this.column = column;
            roomId = id;
            maxDepthScale = scale;
            this.maxRow = maxRow;
            this.width = width;
            this.height = height;
            //GetComponent<PolygonCollider2D>().points;
        }

    }
}
