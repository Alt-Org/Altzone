using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MenuUI.Scripts.SoulHome;

namespace MenuUI.Scripts.SoulHome
{
    public class RoomData : MonoBehaviour
    {
        [SerializeField]
        private float _roomWidth = 50;
        [SerializeField]
        private float _slotRows = 3;
        [SerializeField]
        private float _slotColumns = 8;

        private Room _roomInfo;

        public Room RoomInfo { get => _roomInfo; set => _roomInfo = value; }

        void Start()
        {
            //roomInfo = new Room();
        }
        public void InitializeRoom()
        {
            Transform points = transform.Find("FurniturePoints");
            int row = 0;
            int col = 0;
            /*for(int i = 0; i < _slotRows; i++)
            {
                Instantiate(new GameObject(),points.transform);
                for (int j = 0; j < _slotColumns; j++)
                {

                }
            }*/

            foreach (Transform pointRow in points)
            {
                foreach(Transform point in pointRow)
                {
                    point.GetComponent<FurnitureSlot>().InitializeSlot(row, col);
                    col++;
                }
                row++;
            }
            if(_roomInfo.Furnitures.Count > 0) InitialSetFurniture();
        }

        private void InitialSetFurniture()
        {
            foreach (Furniture furniture in _roomInfo.Furnitures)
            {
                int furnitureRow = furniture.Position.y;
                int furnitureColumn = furniture.Position.x;
                bool check = CheckFurniturePosition(furnitureRow, furnitureColumn, furniture.Size);
                if(check) SetFurniture(furnitureRow, furnitureColumn, furniture.Size, furniture);
            }
        }
        private bool CheckFurniturePosition(int row, int column, FurnitureSize size)
        {
            int furnitureRowSize;
            int furnitureColumnSize;
            Transform points = transform.Find("FurniturePoints");
            if (size == FurnitureSize.OneXOne)
            {
                furnitureRowSize = 1;
                furnitureColumnSize = 1;
            }
            else if (size == FurnitureSize.OneXTwo)
            {
                furnitureRowSize = 2;
                furnitureColumnSize = 1;
            }
            else
            {
                Debug.Log("Error: Invalid furniture size");
                return false;
            }
            int startRow = row - (furnitureColumnSize - 1);
            int endColumn = column + (furnitureRowSize - 1);

            for (int i = startRow; i <= row ; i++)
            {
                for (int j = column; j <= endColumn; j++)
                {
                    if (points.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().Furniture != null) return false;
                }
            }
            return true;
        }
        private void SetFurniture(int row, int column, FurnitureSize size, Furniture furniture)
        {
            int furnitureRowSize;
            int furnitureColumnSize;
            Transform points = transform.Find("FurniturePoints");
            if (size == FurnitureSize.OneXOne)
            {
                furnitureRowSize = 1;
                furnitureColumnSize = 1;
            }
            else if (size == FurnitureSize.OneXTwo)
            {
                furnitureRowSize = 2;
                furnitureColumnSize = 1;
            }
            else
            {
                Debug.Log("Error: Invalid furniture size");
                return;
            }

            int startRow = row - (furnitureColumnSize - 1);
            int endColumn = column + (furnitureRowSize - 1);

            for (int i = startRow; i <= row; i++)
            {
                for (int j = column; j <= endColumn; j++)
                {
                    points.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().Furniture = furniture;
                }
            }

            Instantiate(points.GetComponent<FurnitureReference>().GetFurniture(furniture.Name), points.GetChild(row).GetChild(column));
            GameObject furnitureObject = points.GetChild(row).GetChild(column).GetChild(0).gameObject;
            if (row == 0)
            {
                furnitureObject.GetComponent<SpriteRenderer>().sortingOrder = 3;
                furnitureObject.transform.localScale *= 1.0f;
            }
            else if (row == 1)
            {
                furnitureObject.GetComponent<SpriteRenderer>().sortingOrder = 4;
                furnitureObject.transform.localScale *= 1.1f;
            }
            else if (row == 2)
            {
                furnitureObject.GetComponent<SpriteRenderer>().sortingOrder = 5;
                furnitureObject.transform.localScale *= 1.2f;
            }
        }
    }
}
