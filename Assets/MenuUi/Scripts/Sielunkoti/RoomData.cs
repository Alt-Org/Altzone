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
                col = 0;
                foreach (Transform point in pointRow)
                {
                    point.GetComponent<FurnitureSlot>().InitializeSlot(row, col, _roomInfo.Id);
                    col++;
                }
                row++;
            }
            if (_roomInfo.Furnitures.Count > 0) InitialSetFurniture();
        }

        private void InitialSetFurniture()
        {
            foreach (Furniture furniture in _roomInfo.Furnitures)
            {
                int furnitureRow = furniture.Position.y;
                int furnitureColumn = furniture.Position.x;
                bool check = CheckFurniturePosition(furnitureRow, furnitureColumn, furniture.Size, furniture);
                if (check) SetFurniture(furnitureRow, furnitureColumn, furniture.Size, furniture);
            }
        }
        private bool CheckFurniturePosition(int row, int column, FurnitureSize size, Furniture furniture)
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

            for (int i = startRow; i <= row; i++)
            {
                for (int j = column; j <= endColumn; j++)
                {
                    if (points.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().TempFurniture != null
                        && points.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().TempFurniture != furniture) return false;
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
            furnitureObject.GetComponent<FurnitureHandling>().Furniture = furniture;
            furnitureObject.GetComponent<FurnitureHandling>().Position = new(column, row);
            furnitureObject.GetComponent<FurnitureHandling>().Slot = points.GetChild(row).GetChild(column).GetComponent<FurnitureSlot>();
            furnitureObject.GetComponent<FurnitureHandling>().SetScale(row);
        }
        public void MoveFurniture(int row, int column, FurnitureSize size, GameObject furniture, bool hover)
        {
            int furnitureRowSize;
            int furnitureColumnSize;
            Transform points = transform.Find("FurniturePoints");
            if (!hover) {
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
                        points.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().TempFurniture = furniture.GetComponent<FurnitureHandling>().Furniture;
                    }
                }
                if (furniture.GetComponent<FurnitureHandling>().TempSlot != null)
                {
                    int prevRow = furniture.GetComponent<FurnitureHandling>().TempSlot.row;
                    int prevColumn = furniture.GetComponent<FurnitureHandling>().TempSlot.column;

                    startRow = prevRow - (furnitureColumnSize - 1);
                    endColumn = prevColumn + (furnitureRowSize - 1);

                    for (int i = startRow; i <= prevRow; i++)
                    {
                        for (int j = prevColumn; j <= endColumn; j++)
                        {
                            points.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().TempFurniture = null;
                        }
                    }
                }
                furniture.GetComponent<FurnitureHandling>().TempSlot = points.GetChild(row).GetChild(column).GetComponent<FurnitureSlot>();
            }
            furniture.transform.SetParent(points.GetChild(row).GetChild(column));
            furniture.GetComponent<FurnitureHandling>().ResetFurniturePosition();
            furniture.GetComponent<FurnitureHandling>().SetScale(row);
        }

        public bool HandleFurniturePosition(RaycastHit2D[] hitArray, GameObject furniture, bool hover)
        {
            if (furniture.GetComponent<FurnitureHandling>() == null) return false;

            foreach (RaycastHit2D hit2 in hitArray)
            {
                if (hit2.collider != null)
                {
                    //Debug.Log(hit2 +": Collider found," +hit2.transform.tag);
                    if (hit2.collider.gameObject.GetComponent<FurnitureSlot>() != null)
                    {
                        //Debug.Log(hit2 + ": Slot found, Room "+ hit2.collider.transform.parent.parent.parent.GetComponent<RoomData>().RoomInfo.Id+
                        //    ", Slot " + hit2.collider.GetComponent<FurnitureSlot>().row +":"+ hit2.collider.GetComponent<FurnitureSlot>().column);
                        GameObject slot = hit2.collider.gameObject;
                        Furniture furnitureInfo = furniture.GetComponent<FurnitureHandling>().Furniture;
                        bool check = CheckFurniturePosition(slot.GetComponent<FurnitureSlot>().row, slot.GetComponent<FurnitureSlot>().column, furnitureInfo.Size, furnitureInfo);
                        if (check)
                        {
                            MoveFurniture(slot.GetComponent<FurnitureSlot>().row, slot.GetComponent<FurnitureSlot>().column, furnitureInfo.Size, furniture, hover);
                            return true;
                        }
                    }
                }
            }
            if (!hover)
            {
                if (furniture.GetComponent<FurnitureHandling>().Slot == null)
                {
                    Destroy(furniture);
                    return false;
                }
                /*else if (furniture.GetComponent<FurnitureHandling>().TempSlot != null)
                ResetPosition(furniture, true);*/
            }
            return false;
        }
        public void FreeFurnitureSlots(FurnitureSize size ,FurnitureSlot slot)
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

            if (slot != null)
            {
                int prevRow = slot.row;
                int prevColumn = slot.column;

                int startRow = prevRow - (furnitureColumnSize - 1);
                int endColumn = prevColumn + (furnitureRowSize - 1);

                for (int i = startRow; i <= prevRow; i++)
                {
                    for (int j = prevColumn; j <= endColumn; j++)
                    {
                        points.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().Furniture = null;
                    }
                }
            }

        }

        public void SetFurnitureSlots(FurnitureSize size, FurnitureSlot slot, Furniture furniture)
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

            if (slot != null)
            {
                int prevRow = slot.row;
                int prevColumn = slot.column;

                int startRow = prevRow - (furnitureColumnSize - 1);
                int endColumn = prevColumn + (furnitureRowSize - 1);

                for (int i = startRow; i <= prevRow; i++)
                {
                    for (int j = prevColumn; j <= endColumn; j++)
                    {
                        points.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().Furniture = furniture;
                    }
                }
            }

        }
        public void ResetPosition(GameObject furniture, bool temp)
        {
            int prevRow;
            int prevColumn;
            if (temp)
            {
                prevRow = furniture.GetComponent<FurnitureHandling>().TempSlot.row;
                prevColumn = furniture.GetComponent<FurnitureHandling>().TempSlot.column;
            }
            else
            {
                prevRow = furniture.GetComponent<FurnitureHandling>().Slot.row;
                prevColumn = furniture.GetComponent<FurnitureHandling>().Slot.column;
            }
            furniture.transform.SetParent(transform.Find("FurniturePoints").GetChild(prevRow).GetChild(prevColumn));
            furniture.GetComponent<FurnitureHandling>().SetScale();
        }
    }
}
