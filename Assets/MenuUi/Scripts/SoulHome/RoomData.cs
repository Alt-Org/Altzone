using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MenuUI.Scripts.SoulHome;
using Debug = Prg.Debug;
using Altzone.Scripts.Model.Poco.Game;
using MenuUi.Scripts.Storage;

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
        private int _slotRows = 3;
        [SerializeField]
        private int _slotColumns = 8;
        [SerializeField]
        private float _slotMaxGrowthPercentage = 20;
        [SerializeField]
        private RectPosition _floorAnchorPosition = RectPosition.Top;
        [SerializeField]
        private GameObject _furnitureSlotPrefab;
        [SerializeField]
        private StorageFurnitureReference _furnitureRefrence;
        [SerializeField]
        private Transform _furniturePoints;

        private Room _roomInfo;
        private SoulHomeController _controller;
        private Transform _towerCamera;

        public Room RoomInfo { get => _roomInfo;}
        public int SlotRows { get => _slotRows;}
        public int SlotColumns { get => _slotColumns;}
        public SoulHomeController Controller { get => _controller;}

        void Start()
        {
            //roomInfo = new Room();
            //_controller = GetComponentInParent<SoulHomeController>();
        }
        public void InitializeRoom(Room roomInfo, SoulHomeController controller,Camera towerCamera)
        {
            _roomInfo = roomInfo;
            _controller = controller;
            _towerCamera = towerCamera.transform;
            int row = 0;
            int col = 0;
            GameObject furnitureRowObject = new GameObject();
            float prevBottom = 0;
            for(int i = 0; i < _slotRows; i++)
            {
                GameObject furnitureRow = Instantiate(furnitureRowObject, _furniturePoints);
                if(_floorAnchorPosition is RectPosition.Center)
                    furnitureRow.transform.localPosition = new Vector3(0, (_floorDepth / 2) + -1*(_floorDepth/_slotRows * (0.5f + i)), 0);
                else if(_floorAnchorPosition is RectPosition.Top)
                    furnitureRow.transform.localPosition = new Vector3(0, prevBottom + -1 * ((_floorDepth / _slotRows + (_floorDepth / _slotRows) * (0.05f * (_slotRows / -2 +0.5f+i)))/2), 0);
                furnitureRow.name = (1+i).ToString();
                col = 0;
                for (int j = 0; j < _slotColumns; j++)
                {
                    GameObject furnitureSlot = Instantiate(_furnitureSlotPrefab, furnitureRow.transform);
                    furnitureSlot.transform.localPosition = new Vector3((-1 * (_floorWidth * (1+ (_slotMaxGrowthPercentage * (((float)i) / (((float)_slotRows) - 1)) / 100))) / 2 ) + _floorWidth * (1 + (_slotMaxGrowthPercentage * (((float)i) / (((float)_slotRows) - 1)) / 100)) / _slotColumns  * (0.5f + j), 0, 0);
                    float slotDepth = _floorDepth / _slotRows + (_floorDepth / _slotRows) * (0.05f * (_slotRows / -2 + 0.5f + i));
                    float slotWidth = (_floorWidth / _slotColumns) * (1 + _slotMaxGrowthPercentage * (((float)i) / (((float)_slotRows) - 1))/100);
                    furnitureSlot.GetComponent<BoxCollider2D>().size = new Vector2(slotWidth, slotDepth);
                    furnitureSlot.name = (1 + j).ToString();
                    furnitureSlot.GetComponent<FurnitureSlot>().InitializeSlot(row, col, _roomInfo.Id, _slotMaxGrowthPercentage, _slotRows, slotWidth, slotDepth);
                    col++;
                }
                prevBottom -= _floorDepth / _slotRows + (_floorDepth / _slotRows) * (0.05f * (_slotRows / -2 + 0.5f + i));
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
            Vector2Int furnitureSize = furniture.GetFurnitureSize();

            int startRow;
            int endColumn;

            if (furnitureSize.x == 0) return false;

            startRow = row - (furnitureSize.y - 1);
            endColumn = column + (furnitureSize.x - 1);

            if (furniture.Place is FurniturePlacement.Floor)
            {
                if (startRow < 0 || endColumn >= _slotColumns) return false;
            }
            else if (furniture.Place is FurniturePlacement.FloorByWall)
            {
                if (!furniture.IsRotated)
                {

                    if (startRow == 0)
                    {
                        if (column < 0 || endColumn >= _slotColumns) return false;
                    }
                    else return false;
                }
                else
                {

                    if (column == 0 || endColumn == _slotColumns -1)
                    {
                        if (startRow < 0 || row >= _slotRows) return false;
                    }
                    else return false;
                }
            }

            for (int i = startRow; i <= row; i++)
            {
                for (int j = column; j <= endColumn; j++)
                {
                    if (_furniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().TempFurniture != null
                        && !_furniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().TempFurniture.Equals(furniture)) return false;
                }
            }
            return true;
        }

        private bool CheckFurniturePosition(int row, int column, FurnitureHandling furniture, Vector2 backupHit, bool useBackup)
        {
            if (furniture.Furniture.Place is FurniturePlacement.FloorByWall)
            {
                Vector2Int furnitureSize = furniture.GetFurnitureSizeRotated();
                Vector2Int furnitureSizeCurrent = furniture.GetFurnitureSize();

                Vector2 checkPoint = backupHit + new Vector2((furniture.transform.localScale.x / 2) + ((furniture.transform.localScale.x * furnitureSize.x) / 2) * -1, 0);
                Ray ray2 = new(_towerCamera.position, (Vector3)checkPoint - _towerCamera.position);
                RaycastHit2D[] hitArray;
                hitArray = Physics2D.GetRayIntersectionAll(ray2, 1000);

                int rotatedRow = -1;
                int rotatedColumn = -1;
                //bool slotHit = false;
                foreach (RaycastHit2D hit in hitArray)
                {
                    if (hit.collider.gameObject.GetComponent<FurnitureSlot>() != null)
                    {
                        rotatedRow = hit.collider.gameObject.GetComponent<FurnitureSlot>().row;
                        rotatedColumn = hit.collider.gameObject.GetComponent<FurnitureSlot>().column;
                        //slotHit = true;
                        break;
                    }
                }

                int startRow = rotatedRow - (furnitureSize.y - 1);
                int endColumn = rotatedColumn + (furnitureSize.x - 1);

                if (startRow == 0)
                {
                    if (rotatedColumn == 0 || endColumn == _slotColumns - 1) { }
                    else if (furniture.TempSpriteDirection is not FurnitureHandling.Direction.Front)
                    {
                        furniture.RotateFurniture(FurnitureHandling.Direction.Front);
                        row = rotatedRow;
                        column = rotatedColumn;
                    }
                }
                else if (rotatedColumn == 0)
                {
                    if (rotatedRow == 0) { }
                    else if (furniture.TempSpriteDirection is not FurnitureHandling.Direction.Left)
                    {
                        furniture.RotateFurniture(FurnitureHandling.Direction.Left);
                        row = rotatedRow;
                        column = rotatedColumn;
                    }
                }
                else if (endColumn == _slotColumns - 1)
                {
                    if (rotatedRow == 0) { }
                    else if (furniture.TempSpriteDirection is not FurnitureHandling.Direction.Right)
                    {
                        furniture.RotateFurniture(FurnitureHandling.Direction.Right);
                        row = rotatedRow;
                        column = rotatedColumn;
                    }
                }
            }

            bool check = CheckFurniturePosition(row, column, furniture.Furniture);

            return check;
        }

        private void SetFurniture(int row, int column, Furniture furniture)
        {
            Vector2Int furnitureSize = furniture.GetFurnitureSize();
            FurnitureList list = _controller.FurnitureList;
            foreach (FurnitureListObject listObject in list.List)
            {
                if (listObject.Name.Equals(furniture.Name))
                {
                    foreach (Furniture furnitureInList in listObject.List)
                    {
                        if (furnitureInList.Id == furniture.Id)
                        {
                            furniture = furnitureInList;
                        }
                    }
                }
            }


                if (furnitureSize.x == 0) return;

            int startRow = row - (furnitureSize.y - 1);
            int endColumn = column + (furnitureSize.x - 1);

            for (int i = startRow; i <= row; i++)
            {
                for (int j = column; j <= endColumn; j++)
                {
                    _furniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().Furniture = furniture;
                }
            }

            GameObject furnitureObject = Instantiate(_furnitureRefrence.GetSoulHomeFurnitureObject(furniture.Name), _furniturePoints.GetChild(row).GetChild(column));
            furnitureObject.GetComponent<FurnitureHandling>().Furniture = furniture;
            furnitureObject.GetComponent<FurnitureHandling>().Position = new(column, row);
            furnitureObject.GetComponent<FurnitureHandling>().Slot = _furniturePoints.GetChild(row).GetChild(column).GetComponent<FurnitureSlot>();
            furnitureObject.GetComponent<FurnitureHandling>().SetScale(row, _furniturePoints.GetChild(row).GetChild(column).GetComponent<FurnitureSlot>());
            furnitureObject.GetComponent<FurnitureHandling>().ResetFurniturePosition();
        }
        public void MoveFurniture(int row, int column, GameObject furniture, bool hover)
        {
            if (!hover) {
                Debug.Log("Set:"+row + ":" + column);
                Vector2Int furnitureSize = furniture.GetComponent<FurnitureHandling>().GetFurnitureSize();
                Debug.Log("Set:" + furnitureSize.x + ":" + furnitureSize.y);
                int startRow;
                int endColumn;

                if (furnitureSize.x == 0) return;

                startRow = row - (furnitureSize.y - 1);
                endColumn = column + (furnitureSize.x - 1);

                for (int i = startRow; i <= row; i++)
                {
                    for (int j = column; j <= endColumn; j++)
                    {
                        _furniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().TempFurniture = furniture.GetComponent<FurnitureHandling>().Furniture;
                    }
                }
                if (furniture.GetComponent<FurnitureHandling>().TempSlot != null)
                {
                    int prevRow = furniture.GetComponent<FurnitureHandling>().TempSlot.row;
                    int prevColumn = furniture.GetComponent<FurnitureHandling>().TempSlot.column;

                    if(furniture.GetComponent<FurnitureHandling>().TempSlot.TempRotated != furniture.GetComponent<FurnitureHandling>().IsRotated)
                    furnitureSize = furniture.GetComponent<FurnitureHandling>().GetFurnitureSizeRotated();


                    startRow = prevRow - (furnitureSize.y - 1);
                    endColumn = prevColumn + (furnitureSize.x - 1);
                    Debug.Log("StartColumn:" + startRow + ", EndColumn:" + endColumn);
                    for (int i = startRow; i <= prevRow; i++)
                    {
                        for (int j = prevColumn; j <= endColumn; j++)
                        {
                            _furniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().TempFurniture = null;
                        }
                    }
                }
                furniture.GetComponent<FurnitureHandling>().TempSlot = _furniturePoints.GetChild(row).GetChild(column).GetComponent<FurnitureSlot>();
            }
            furniture.transform.SetParent(_furniturePoints.GetChild(row).GetChild(column));
            furniture.GetComponent<FurnitureHandling>().ResetFurniturePosition();
            furniture.GetComponent<FurnitureHandling>().SetScale(row, _furniturePoints.GetChild(row).GetChild(column).GetComponent<FurnitureSlot>());
        }

        public bool HandleFurniturePosition(RaycastHit2D[] hitArray, GameObject furniture, bool hover, Vector2 backupHit, bool useBackup)
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
                        FurnitureHandling furnitureInfo = furniture.GetComponent<FurnitureHandling>();
                        bool check = CheckFurniturePosition(slot.GetComponent<FurnitureSlot>().row, slot.GetComponent<FurnitureSlot>().column, furnitureInfo, backupHit, useBackup);
                        if (check)
                        {
                            MoveFurniture(slot.GetComponent<FurnitureSlot>().row, slot.GetComponent<FurnitureSlot>().column, furniture, false);
                            return true;
                        }
                    }
                }
            }
            if (!hover)
            {
                /*if (furniture.GetComponent<FurnitureHandling>().TempSlot != null)
                ResetPosition(furniture, true);
                else if (furniture.GetComponent<FurnitureHandling>().Slot == null)
                {
                    Destroy(furniture);
                    return false;
                }*/
            }
            return false;
        }
        public void FreeFurnitureSlots(FurnitureHandling furniture, FurnitureSlot slot)
        {
            Debug.Log("Free:"+slot.row+":"+slot.column);
            Vector2Int furnitureSize;

            if (furniture.Furniture.IsRotated != slot.Rotated)
                furnitureSize = furniture.GetFurnitureSizeRotated();
            else
                furnitureSize = furniture.GetFurnitureSize();

            if (furnitureSize.x == 0) return;

            if (slot != null)
            {
                int prevRow = slot.row;
                int prevColumn = slot.column;

                int startRow;
                int endColumn;
                if (furnitureSize.x == 0 || furniture == null) return;
                startRow = prevRow - (furnitureSize.y - 1);
                endColumn = prevColumn + (furnitureSize.x - 1);

                for (int i = startRow; i <= prevRow; i++)
                {
                    for (int j = prevColumn; j <= endColumn; j++)
                    {
                        _furniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().Furniture = null;
                    }
                }
            }

        }

        public void SetFurnitureSlots(FurnitureHandling furniture)
        {
            FurnitureSlot slot = furniture.Slot;
            Furniture furnitureObject = furniture.Furniture;

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
                        _furniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().Furniture = furnitureObject;
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
            furniture.transform.SetParent(_furniturePoints.GetChild(prevRow).GetChild(prevColumn));
            furniture.GetComponent<FurnitureHandling>().SetScale();
        }
    }
}
