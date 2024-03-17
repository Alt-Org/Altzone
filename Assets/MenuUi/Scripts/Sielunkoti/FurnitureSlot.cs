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
        private Furniture furniture;
        private Furniture tempFurniture;

        public Furniture Furniture { get => furniture;
            set
            {
                furniture = value;
                TempFurniture = value;
            }
        }
        public Furniture TempFurniture { get => tempFurniture; set => tempFurniture = value; }

        public void InitializeSlot(int row, int column, int id)
        {
            this.row = row;
            this.column = column;
            roomId = id;
            //GetComponent<PolygonCollider2D>().points;
        }

    }
}
