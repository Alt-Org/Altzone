using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MenuUI.Scripts.SoulHome
{
    public class FurnitureSlot : MonoBehaviour
    {
        public int row;
        public int column;
        private Furniture furniture;

        public Furniture Furniture { get => furniture; set => furniture = value; }

        public void InitializeSlot(int row, int column)
        {
            this.row = row;
            this.column = column;
            //GetComponent<PolygonCollider2D>().points;
        }

    }
}
