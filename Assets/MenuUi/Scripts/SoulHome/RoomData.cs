using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MenuUI.Scripts.SoulHome;
using Debug = Prg.Debug;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.ReferenceSheets;

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
        private SoulHomeFurnitureReference _furnitureRefrence;
        [SerializeField]
        private Transform _floorFurniturePoints;
        [SerializeField]
        private Transform _wallFurniturePoints;

        [SerializeField]
        private SpriteRenderer _roomSprite;
        [SerializeField]
        private SpriteRenderer _wallPaper;
        [SerializeField]
        private Transform _ladder;

        private Room _roomInfo;
        private SoulHomeController _controller;
        private Transform _towerCamera;

        private List<FurnitureSlot> _currentSlotValidity;

        public Room RoomInfo { get => _roomInfo;}
        public int SlotRows { get => _slotRows;}
        public int SlotColumns { get => _slotColumns;}
        public SoulHomeController Controller { get => _controller;}

        void Start()
        {
            //roomInfo = new Room();
            //_controller = GetComponentInParent<SoulHomeController>();
        }
        public void InitializeSoulHomeRoom(Room roomInfo, SoulHomeController controller, Camera towerCamera, bool topRoom)
        {
            _roomInfo = roomInfo;
            _controller = controller;
            _towerCamera = towerCamera.transform;
            _currentSlotValidity = new();
            InitializeRoom(topRoom);
        }

        public void InitializeRaidRoom(Room roomInfo)
        {
            _roomInfo = roomInfo;
            InitializeRoom(true);
        }

        private void InitializeRoom(bool topRoom)
        {
            int row = 0;
            int col = 0;
            _roomSprite.sortingOrder = 1000 * _roomInfo.id;
            _wallPaper.sortingOrder = 1000 * _roomInfo.id + 1;
            GameObject furnitureRowObject = new GameObject();
            float prevBottom = 0;
            for(int i = 0; i < _slotRows; i++)
            {
                GameObject furnitureRow = Instantiate(furnitureRowObject, _floorFurniturePoints);
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
                    furnitureSlot.tag = "FloorFurnitureSlot";
                    col++;
                }
                prevBottom -= _floorDepth / _slotRows + (_floorDepth / _slotRows) * (0.05f * (_slotRows / -2 + 0.5f + i));
                row++;
            }
            row = 0;
            col = 0;
            prevBottom = 0;
            float wallWidth = transform.Find("Room").Find("BackWall").GetComponent<BoxCollider2D>().size.x;
            float wallHeight = transform.Find("Room").Find("BackWall").GetComponent<BoxCollider2D>().size.y;
            int wallSlotRows = (int)Mathf.Floor(wallHeight / 2.5f);
            int wallSlotColumns = (int)Mathf.Floor(wallWidth / 2.5f);

            for (int i = 0; i < wallSlotRows; i++)
            {
                GameObject furnitureRow = Instantiate(furnitureRowObject, _wallFurniturePoints);
                if (_floorAnchorPosition is RectPosition.Center)
                    furnitureRow.transform.localPosition = new Vector3(0, (wallHeight / 2) + -1 * (wallHeight / wallSlotRows * (0.5f + i)), 0);
                else if (_floorAnchorPosition is RectPosition.Top)
                    furnitureRow.transform.localPosition = new Vector3(0, prevBottom + -1 * ((wallHeight / wallSlotRows + (wallHeight / wallSlotRows) * (0.05f * (wallSlotRows / -2 + 0.5f + i))) / 2), 0);
                furnitureRow.name = (1 + i).ToString();
                col = 0;
                for (int j = 0; j < wallSlotColumns; j++)
                {
                    GameObject furnitureSlot = Instantiate(_furnitureSlotPrefab, furnitureRow.transform);
                    furnitureSlot.transform.localPosition = new Vector3((-1 * wallWidth / 2) + wallWidth / wallSlotColumns * (0.5f + j), 0, 0);
                    float slotDepth = wallHeight / wallSlotRows + (wallHeight / wallSlotRows) * (0.05f * (wallSlotRows / -2 + 0.5f + i));
                    float slotWidth = (wallWidth / wallSlotColumns);
                    furnitureSlot.GetComponent<BoxCollider2D>().size = new Vector2(slotWidth, slotDepth);
                    furnitureSlot.name = (1 + j).ToString();
                    furnitureSlot.GetComponent<FurnitureSlot>().InitializeSlot(row, col, _roomInfo.Id, _slotMaxGrowthPercentage, wallSlotRows, slotWidth, slotDepth);
                    furnitureSlot.tag = "WallFurnitureSlot";
                    col++;
                }
                prevBottom -= wallHeight / wallSlotRows + (wallHeight / wallSlotRows) * (0.05f * (wallSlotRows / -2 + 0.5f + i));
                row++;
            }

            Destroy(furnitureRowObject);

            if (!topRoom)
            {
                _ladder.gameObject.SetActive(true);
                foreach(Transform ladderpiece in _ladder)
                {
                    ladderpiece.GetComponent<SpriteRenderer>().sortingOrder = 1000 * _roomInfo.id +20;
                }
                foreach(Transform rowTransform in _wallFurniturePoints)
                {
                    rowTransform.GetChild(1).GetComponent<FurnitureSlot>().Ladder = true;
                    rowTransform.GetChild(2).GetComponent<FurnitureSlot>().Ladder = true;
                }
                _floorFurniturePoints.GetChild(0).GetChild(1).GetComponent<FurnitureSlot>().Ladder = true;
                _floorFurniturePoints.GetChild(1).GetChild(1).GetComponent<FurnitureSlot>().Ladder = true;
            }

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

            if (furniture.Place is FurniturePlacement.Floor or FurniturePlacement.FloorNonblock or FurniturePlacement.Wall)
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
                    if (furniture.Place is FurniturePlacement.Floor or FurniturePlacement.FloorByWall)
                    {
                        if ((_floorFurniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().TempFurniture != null
                            && !_floorFurniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().TempFurniture.Equals(furniture))
                            || _floorFurniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().Ladder) return false;
                    }
                    else if (furniture.Place is FurniturePlacement.FloorNonblock)
                    {
                        if ((_floorFurniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().TempFurnitureNonBlock != null
                            && !_floorFurniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().TempFurnitureNonBlock.Equals(furniture))
                            || _floorFurniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().Ladder) return false;
                    }
                    else if(furniture.Place is FurniturePlacement.Wall)
                    {
                        if ((_wallFurniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().TempFurniture != null
                            && !_wallFurniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().TempFurniture.Equals(furniture))
                            || _floorFurniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().Ladder) return false;
                    }
                    else if (furniture.Place is FurniturePlacement.Ceiling)
                    {
                        /*if (_wallFurniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().TempFurniture != null
                            && !_wallFurniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().TempFurniture.Equals(furniture))*/ return false;
                    }

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
            SetSlotValidity(row, column, furniture.Furniture, check);
            return check;
        }

        private void SetFurniture(int row, int column, Furniture furniture)
        {
            Vector2Int furnitureSize = furniture.GetFurnitureSize();
            if (_controller != null)
            {
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
            }

            if (furnitureSize.x == 0) return;

            int startRow = row - (furnitureSize.y - 1);
            int endColumn = column + (furnitureSize.x - 1);

            for (int i = startRow; i <= row; i++)
            {
                for (int j = column; j <= endColumn; j++)
                {
                    if (furniture.Place is FurniturePlacement.Floor or FurniturePlacement.FloorByWall)
                    {
                        _floorFurniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().Furniture = furniture;
                    }
                    else if (furniture.Place is FurniturePlacement.FloorNonblock)
                    {
                        _floorFurniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().FurnitureNonBlock = furniture;
                    }
                    else if (furniture.Place is FurniturePlacement.Wall)
                    {
                        _wallFurniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().Furniture = furniture;
                    }
                }
            }
            if (furniture.Place is FurniturePlacement.Floor or FurniturePlacement.FloorByWall or FurniturePlacement.FloorNonblock)
            {
                GameObject furnitureObject = Instantiate(_furnitureRefrence.GetSoulHomeFurnitureObject(furniture.Name), _floorFurniturePoints.GetChild(row).GetChild(column));
                furnitureObject.GetComponent<FurnitureHandling>().Furniture = furniture;
                furnitureObject.GetComponent<FurnitureHandling>().Position = new(column, row);
                furnitureObject.GetComponent<FurnitureHandling>().Slot = _floorFurniturePoints.GetChild(row).GetChild(column).GetComponent<FurnitureSlot>();
                furnitureObject.GetComponent<FurnitureHandling>().SetScale(row, _floorFurniturePoints.GetChild(row).GetChild(column).GetComponent<FurnitureSlot>());
                furnitureObject.GetComponent<FurnitureHandling>().ResetFurniturePosition();
            }
            else if (furniture.Place is FurniturePlacement.Wall)
            {
                GameObject furnitureObject = Instantiate(_furnitureRefrence.GetSoulHomeFurnitureObject(furniture.Name), _wallFurniturePoints.GetChild(row).GetChild(column));
                furnitureObject.GetComponent<FurnitureHandling>().Furniture = furniture;
                furnitureObject.GetComponent<FurnitureHandling>().Position = new(column, row);
                furnitureObject.GetComponent<FurnitureHandling>().Slot = _wallFurniturePoints.GetChild(row).GetChild(column).GetComponent<FurnitureSlot>();
                furnitureObject.GetComponent<FurnitureHandling>().SetScale(row, _wallFurniturePoints.GetChild(row).GetChild(column).GetComponent<FurnitureSlot>());
                furnitureObject.GetComponent<FurnitureHandling>().ResetFurniturePosition();
            }
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
                        if (furniture.GetComponent<FurnitureHandling>().Furniture.Place is FurniturePlacement.Floor or FurniturePlacement.FloorByWall)
                        {
                            _floorFurniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().TempFurniture = furniture.GetComponent<FurnitureHandling>().Furniture;
                        }
                        else if (furniture.GetComponent<FurnitureHandling>().Furniture.Place is FurniturePlacement.FloorNonblock)
                        {
                            _floorFurniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().TempFurnitureNonBlock = furniture.GetComponent<FurnitureHandling>().Furniture;
                        }
                        else if (furniture.GetComponent<FurnitureHandling>().Furniture.Place is FurniturePlacement.Wall)
                        {
                            _wallFurniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().TempFurniture = furniture.GetComponent<FurnitureHandling>().Furniture;
                        }
                    }
                }
                if (furniture.GetComponent<FurnitureHandling>().TempSlot != null)
                {

                    int prevRow = furniture.GetComponent<FurnitureHandling>().TempSlot.row;
                    int prevColumn = furniture.GetComponent<FurnitureHandling>().TempSlot.column;
                    if (furniture.GetComponent<FurnitureHandling>().Furniture.Place is FurniturePlacement.Floor or FurniturePlacement.FloorByWall or FurniturePlacement.Wall)
                    {
                        if (furniture.GetComponent<FurnitureHandling>().TempSlot.TempRotated != furniture.GetComponent<FurnitureHandling>().IsRotated)
                        furnitureSize = furniture.GetComponent<FurnitureHandling>().GetFurnitureSizeRotated();
                    }
                    else if (furniture.GetComponent<FurnitureHandling>().Furniture.Place is FurniturePlacement.FloorNonblock)
                    {
                        if (furniture.GetComponent<FurnitureHandling>().TempSlot.TempRotatedNonBlock != furniture.GetComponent<FurnitureHandling>().IsRotated)
                            furnitureSize = furniture.GetComponent<FurnitureHandling>().GetFurnitureSizeRotated();
                    }


                    startRow = prevRow - (furnitureSize.y - 1);
                    endColumn = prevColumn + (furnitureSize.x - 1);
                    Debug.Log("StartRow:" + startRow + ", EndColumn:" + endColumn);
                    for (int i = startRow; i <= prevRow; i++)
                    {
                        for (int j = prevColumn; j <= endColumn; j++)
                        {
                            if (furniture.GetComponent<FurnitureHandling>().Furniture.Place is FurniturePlacement.Floor or FurniturePlacement.FloorByWall)
                            {
                                _floorFurniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().TempFurniture = null;
                            }
                            else if (furniture.GetComponent<FurnitureHandling>().Furniture.Place is FurniturePlacement.FloorNonblock)
                            {
                                _floorFurniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().TempFurnitureNonBlock = null;
                            }
                            else if (furniture.GetComponent<FurnitureHandling>().Furniture.Place is FurniturePlacement.Wall)
                            {
                                _wallFurniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().TempFurniture = null;
                            }
                        }
                    }
                }
                if (furniture.GetComponent<FurnitureHandling>().Furniture.Place is FurniturePlacement.Floor or FurniturePlacement.FloorByWall or FurniturePlacement.FloorNonblock)
                {
                    furniture.GetComponent<FurnitureHandling>().TempSlot = _floorFurniturePoints.GetChild(row).GetChild(column).GetComponent<FurnitureSlot>();
                }
                else if (furniture.GetComponent<FurnitureHandling>().Furniture.Place is FurniturePlacement.Wall)
                {
                    furniture.GetComponent<FurnitureHandling>().TempSlot = _wallFurniturePoints.GetChild(row).GetChild(column).GetComponent<FurnitureSlot>();
                }
            }
            if (furniture.GetComponent<FurnitureHandling>().Furniture.Place is FurniturePlacement.Floor or FurniturePlacement.FloorByWall or FurniturePlacement.FloorNonblock)
            {
                furniture.transform.SetParent(_floorFurniturePoints.GetChild(row).GetChild(column));
                furniture.GetComponent<FurnitureHandling>().ResetFurniturePosition();
                furniture.GetComponent<FurnitureHandling>().SetScale(row, _floorFurniturePoints.GetChild(row).GetChild(column).GetComponent<FurnitureSlot>());
            }
            else if (furniture.GetComponent<FurnitureHandling>().Furniture.Place is FurniturePlacement.Wall)
            {
                furniture.transform.SetParent(_wallFurniturePoints.GetChild(row).GetChild(column));
                furniture.GetComponent<FurnitureHandling>().ResetFurniturePosition();
                furniture.GetComponent<FurnitureHandling>().SetScale(row, _wallFurniturePoints.GetChild(row).GetChild(column).GetComponent<FurnitureSlot>());
            }
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
                        if ((slot.tag == "FloorFurnitureSlot" && furnitureInfo.Furniture.Place is FurniturePlacement.Floor or FurniturePlacement.FloorByWall or FurniturePlacement.FloorNonblock)
                            || slot.tag == "WallFurnitureSlot" && furnitureInfo.Furniture.Place is FurniturePlacement.Wall)
                        {
                            bool check = CheckFurniturePosition(slot.GetComponent<FurnitureSlot>().row, slot.GetComponent<FurnitureSlot>().column, furnitureInfo, backupHit, useBackup);
                            if (check)
                            {
                                MoveFurniture(slot.GetComponent<FurnitureSlot>().row, slot.GetComponent<FurnitureSlot>().column, furniture, hover);
                                return true;
                            }
                        }
                        else
                        {
                            ClearValidity();
                        }
                    }
                    else
                    {
                        ClearValidity();
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

            if (furniture.Furniture.IsRotated != slot.Rotated || furniture.Furniture.IsRotated != slot.RotatedNonBlock)
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
                        if (furniture.Furniture.Place is FurniturePlacement.Floor or FurniturePlacement.FloorByWall)
                        {
                            _floorFurniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().Furniture = null;
                        }
                        else if (furniture.Furniture.Place is FurniturePlacement.FloorNonblock)
                        {
                            _floorFurniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().FurnitureNonBlock = null;
                        }
                        else if(furniture.Furniture.Place is FurniturePlacement.Wall)
                        {
                            _wallFurniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().Furniture = null;
                        }
                    }
                }
            }

        }

        private void SetSlotValidity(int row, int column, Furniture furniture, bool check)
        {
            Vector2Int furnitureSize = furniture.GetFurnitureSize();

            int startRow;
            int endColumn;

            if (furnitureSize.x == 0) return;

            if (_currentSlotValidity.Count > 0) ClearValidity();

            startRow = row - (furnitureSize.y - 1);
            endColumn = column + (furnitureSize.x - 1);

            for (int i = startRow; i <= row; i++)
            {
                if (i < 0 || i >= _slotRows)
                {
                    continue;
                }

                for (int j = column; j <= endColumn; j++)
                {
                    if (j < 0 || j >= _slotColumns)
                    {
                        continue;
                    }

                    if (furniture.Place is FurniturePlacement.Floor or FurniturePlacement.FloorByWall or FurniturePlacement.FloorNonblock)
                    {
                        _floorFurniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().SetValidity(check);
                        _currentSlotValidity.Add(_floorFurniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>());
                    }
                    else if (furniture.Place is FurniturePlacement.Wall)
                    {
                        _wallFurniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().SetValidity(check);
                        _currentSlotValidity.Add(_wallFurniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>());
                    }
                    else if (furniture.Place is FurniturePlacement.Ceiling)
                    {
                        /*if (_wallFurniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().SetValidity(check)*/
                    }
                }
            }
        }

        public void ClearValidity()
        {
            foreach (FurnitureSlot slot in _currentSlotValidity)
            {
                slot.ClearValidity();
            }
            _currentSlotValidity.Clear();
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
                        if (furniture.Furniture.Place is FurniturePlacement.Floor or FurniturePlacement.FloorByWall)
                        {
                            _floorFurniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().Furniture = furnitureObject;
                        }
                        else if (furniture.Furniture.Place is FurniturePlacement.FloorNonblock)
                        {
                            _floorFurniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().FurnitureNonBlock = furnitureObject;
                        }
                        else if (furniture.Furniture.Place is FurniturePlacement.Wall)
                        {
                            _wallFurniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().Furniture = furnitureObject;
                        }
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
            if (furniture.GetComponent<FurnitureHandling>().Furniture.Place is FurniturePlacement.Floor or FurniturePlacement.FloorByWall or FurniturePlacement.FloorNonblock)
            {
                furniture.transform.SetParent(_floorFurniturePoints.GetChild(prevRow).GetChild(prevColumn));
            }
            else if (furniture.GetComponent<FurnitureHandling>().Furniture.Place is FurniturePlacement.Wall)
            {
                furniture.transform.SetParent(_wallFurniturePoints.GetChild(prevRow).GetChild(prevColumn));
            }
            furniture.GetComponent<FurnitureHandling>().SetScale();
        }
    }
}
