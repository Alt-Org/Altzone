using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MenuUI.Scripts.SoulHome;
using Debug = Prg.Debug;

namespace MenuUI.Scripts.SoulHome
{
    public enum RectPosition
    {
        Center,
        Top,
        Bottom
    }
    public class RoomData : MonoBehaviour
    {
        [SerializeField]
        private float _floorWidth = 50;
        [SerializeField]
        private float _floorDepth = 6;
        [SerializeField]
        private float _slotRows = 3;
        [SerializeField]
        private float _slotColumns = 8;
        [SerializeField]
        private float _slotMaxGrowthPercentage = 20;
        [SerializeField]
        private RectPosition _floorAnchorPosition = RectPosition.Top;
        [SerializeField]
        private GameObject _furnitureSlotPrefab;

        private Room _roomInfo;

        public Room RoomInfo { get => _roomInfo; set => _roomInfo = value; }
        public float SlotRows { get => _slotRows;}
        public float SlotColumns { get => _slotColumns;}

        void Start()
        {
            //roomInfo = new Room();
        }
        public void InitializeRoom()
        {
            Transform points = transform.Find("FurniturePoints");

            int row = 0;
            int col = 0;
            GameObject furnitureRowObject = new GameObject();
            for(int i = 0; i < _slotRows; i++)
            {
                GameObject furnitureRow = Instantiate(furnitureRowObject, points.transform);
                if(_floorAnchorPosition is RectPosition.Center)
                    furnitureRow.transform.localPosition = new Vector3(0, (_floorDepth / 2) + -1*(_floorDepth/_slotRows * (0.5f + i)), 0);
                else if(_floorAnchorPosition is RectPosition.Top)
                    furnitureRow.transform.localPosition = new Vector3(0, -1 * (_floorDepth / _slotRows * (0.5f + i)), 0);
                furnitureRow.name = (1+i).ToString();
                col = 0;
                for (int j = 0; j < _slotColumns; j++)
                {
                    GameObject furnitureSlot = Instantiate(_furnitureSlotPrefab, furnitureRow.transform);
                    furnitureSlot.transform.localPosition = new Vector3((-1 * (_floorWidth * (1+ (_slotMaxGrowthPercentage * (i / (_slotRows - 1)) / 100))) / 2 ) + _floorWidth * (1 + (_slotMaxGrowthPercentage * (i / (_slotRows - 1)) / 100)) / _slotColumns  * (0.5f + j), 0, 0);
                    float slotDepth = _floorDepth / _slotRows;
                    float slotWidth = (_floorWidth / _slotColumns) * (1 + _slotMaxGrowthPercentage * (i / (_slotRows - 1))/100);
                    furnitureSlot.GetComponent<BoxCollider2D>().size = new Vector2(slotWidth, slotDepth);
                    furnitureSlot.name = (1 + j).ToString();
                    furnitureSlot.GetComponent<FurnitureSlot>().InitializeSlot(row, col, _roomInfo.Id, _slotMaxGrowthPercentage, _slotRows, slotWidth, slotDepth);
                    col++;
                }
                row++;
            }
            Destroy(furnitureRowObject);

            if (_roomInfo.Furnitures.Count > 0) InitialSetFurniture();
        }

        private void InitialSetFurniture()
        {
            foreach (Furniture furniture in _roomInfo.Furnitures)
            {
                int furnitureRow = furniture.Position.y;
                int furnitureColumn = furniture.Position.x;
                bool check = CheckFurniturePosition(furnitureRow, furnitureColumn, furniture);
                if (check) SetFurniture(furnitureRow, furnitureColumn, furniture);
            }
        }
        private bool CheckFurniturePosition(int row, int column, Furniture furniture)
        {
            Transform points = transform.Find("FurniturePoints");

            Vector2Int furnitureSize = furniture.GetFurnitureSize();

            int startRow;
            int endColumn;

            if (furnitureSize.x == 0) return false;

            startRow = row - (furnitureSize.y - 1);
            endColumn = column + (furnitureSize.x - 1);

            if(startRow < 0 || endColumn >= _slotColumns) return false;

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
        private void SetFurniture(int row, int column, Furniture furniture)
        {
            Transform points = transform.Find("FurniturePoints");
            Vector2Int furnitureSize = furniture.GetFurnitureSize();

            if (furnitureSize.x == 0) return;

            int startRow = row - (furnitureSize.y - 1);
            int endColumn = column + (furnitureSize.x - 1);

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
            furnitureObject.GetComponent<FurnitureHandling>().SetScale(row, points.GetChild(row).GetChild(column).GetComponent<FurnitureSlot>());
            furnitureObject.GetComponent<FurnitureHandling>().ResetFurniturePosition();
        }
        public void MoveFurniture(int row, int column, GameObject furniture, bool hover)
        {
            Transform points = transform.Find("FurniturePoints");
            if (!hover) {
                Vector2Int furnitureSize = furniture.GetComponent<FurnitureHandling>().GetFurnitureSize();

                int startRow;
                int endColumn;

                if (furnitureSize.x == 0) return;

                startRow = row - (furnitureSize.y - 1);
                endColumn = column + (furnitureSize.x - 1);

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

                    startRow = prevRow - (furnitureSize.y - 1);
                    endColumn = prevColumn + (furnitureSize.x - 1);

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
            furniture.GetComponent<FurnitureHandling>().SetScale(row, points.GetChild(row).GetChild(column).GetComponent<FurnitureSlot>());
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
                        bool check = CheckFurniturePosition(slot.GetComponent<FurnitureSlot>().row, slot.GetComponent<FurnitureSlot>().column, furnitureInfo);
                        if (check)
                        {
                            MoveFurniture(slot.GetComponent<FurnitureSlot>().row, slot.GetComponent<FurnitureSlot>().column, furniture, hover);
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
        public void FreeFurnitureSlots(FurnitureHandling furniture, FurnitureSlot slot)
        {
            Transform points = transform.Find("FurniturePoints");
            Vector2Int furnitureSize = furniture.GetFurnitureSize();

            if (furnitureSize.x == 0) return;

            if (slot != null)
            {
                int prevRow = slot.row;
                int prevColumn = slot.column;

                int startRow;
                int endColumn;

                if (furnitureSize.x == 0 || furniture != null) return;

                startRow = prevRow - (furnitureSize.y - 1);
                endColumn = prevColumn + (furnitureSize.x - 1);

                for (int i = startRow; i <= prevRow; i++)
                {
                    for (int j = prevColumn; j <= endColumn; j++)
                    {
                        points.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().Furniture = null;
                    }
                }
            }

        }

        public void SetFurnitureSlots(FurnitureHandling furniture)
        {
            FurnitureSlot slot = furniture.Slot;
            Furniture furnitureObject = furniture.Furniture;


            Transform points = transform.Find("FurniturePoints");
            Vector2Int furnitureSize = furniture.GetFurnitureSize();

            if (furnitureSize.x == 0) return;

            if (slot != null)
            {
                int prevRow = slot.row;
                int prevColumn = slot.column;

                int startRow;
                int endColumn;

                startRow = prevRow - (furnitureSize.y - 1);
                endColumn = prevColumn + (furnitureSize.x - 1);

                for (int i = startRow; i <= prevRow; i++)
                {
                    for (int j = prevColumn; j <= endColumn; j++)
                    {
                        points.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().Furniture = furnitureObject;
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


        private Vector2Int CheckFurnitureSize(FurnitureSize size)
        {
            if (size == FurnitureSize.OneXOne)
            {
                return new Vector2Int(1,1);
            }
            else if (size == FurnitureSize.OneXTwo)
            {
                return new Vector2Int(2, 1);
            }
            else if (size == FurnitureSize.OneXFour)
            {
                return new Vector2Int(4, 1);
            }
            else
            {
                Debug.LogError("Error: Invalid furniture size");
                return new Vector2Int(0, 0);
            }
        }
    }
}
