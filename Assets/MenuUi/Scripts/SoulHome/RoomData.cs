using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MenuUI.Scripts.SoulHome;
using Debug = Prg.Debug;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.ReferenceSheets;
using UnityEngine.Rendering;

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
        [Header("Furniture Slot Data")]
        [SerializeField]
        private float _floorWidth = 50;
        [SerializeField]
        private float _floorDepth = 6;
        [SerializeField]
        private int _slotRows = 3;
        [SerializeField]
        private int _slotColumns = 8;
        private int _slotHeight = 6;
        [SerializeField]
        private float _slotMaxGrowthPercentage = 20;
        [SerializeField]
        private RectPosition _floorAnchorPosition = RectPosition.Top;
        [SerializeField]
        private GameObject _furnitureSlotPrefab;
        [SerializeField]
        private SoulHomeFurnitureReference _furnitureRefrence;
        [SerializeField]
        private SortingGroup _sortingGroup;

        [Header("Furniture Slot Points")]
        [SerializeField]
        private Transform _floorFurniturePoints;
        [SerializeField]
        private Transform _wallBackFurniturePoints;
        [SerializeField]
        private Transform _wallRightFurniturePoints;
        [SerializeField]
        private Transform _wallLeftFurniturePoints;
        [SerializeField]
        private Transform _ceilingFurniturePoints;

        [Header("Furniture Slot Bounds")]
        [SerializeField]
        private BoxCollider2D _floorBounds;
        [SerializeField]
        private BoxCollider2D _floorMaxBounds;
        [SerializeField]
        private BoxCollider2D _backWallBounds;
        [SerializeField]
        private BoxCollider2D _rightSideWallBounds;
        [SerializeField]
        private BoxCollider2D _rightSideWallMaxBounds;
        [SerializeField]
        private BoxCollider2D _leftSideWallBounds;
        [SerializeField]
        private BoxCollider2D _leftSideWallMaxBounds;
        [SerializeField]
        private BoxCollider2D _ceilingBounds;
        [SerializeField]
        private BoxCollider2D _ceilingMaxBounds;

        [Header("Room Sprites")]
        [SerializeField]
        private SpriteRenderer _roomSprite;
        [SerializeField]
        private SpriteRenderer _wallPaper;

        [Header("Static Objects")]
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
            _sortingGroup.sortingOrder = _roomInfo.id;
            _roomSprite.sortingOrder = 0;
            _wallPaper.sortingOrder = 1;
            //Floor Slot generation
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
                    furnitureSlot.GetComponent<FurnitureSlot>().InitializeSlot(row, col, _roomInfo.Id, FurnitureGrid.Floor, _slotMaxGrowthPercentage, _slotRows, slotWidth, slotDepth);
                    furnitureSlot.tag = "FloorFurnitureSlot";
                    col++;
                }
                prevBottom -= _floorDepth / _slotRows + (_floorDepth / _slotRows) * (0.05f * (_slotRows / -2 + 0.5f + i));
                row++;
            }
            //Back Wall Slot generation
            row = 0;
            col = 0;
            prevBottom = 0;
            float wallWidth = _backWallBounds.size.x;
            float wallHeight = _backWallBounds.size.y;
            int wallSlotRows = (int)Mathf.Floor(wallHeight / 2.5f);
            _slotHeight = wallSlotRows;
            int wallSlotColumns = _slotColumns;
            _wallBackFurniturePoints.transform.position = new(_backWallBounds.transform.position.x,
                                                               _backWallBounds.transform.position.y + _backWallBounds.size.y / 2);
            for (int i = 0; i < wallSlotRows; i++)
            {
                GameObject furnitureRow = Instantiate(furnitureRowObject, _wallBackFurniturePoints);
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
                    furnitureSlot.GetComponent<FurnitureSlot>().InitializeSlot(row, col, _roomInfo.Id, FurnitureGrid.BackWall, _slotMaxGrowthPercentage, wallSlotColumns, slotWidth, slotDepth);
                    furnitureSlot.tag = "WallFurnitureSlot";
                    col++;
                }
                prevBottom -= wallHeight / wallSlotRows + (wallHeight / wallSlotRows) * (0.05f * (wallSlotRows / -2 + 0.5f + i));
                row++;
            }
            //Right Wall Slot generation
            row = 0;
            col = 0;
            wallWidth = _rightSideWallBounds.size.x;
            wallHeight = _rightSideWallBounds.size.y;
            wallSlotRows = _slotHeight;
            wallSlotColumns = _slotRows;
            _wallRightFurniturePoints.transform.position = new(_rightSideWallBounds.transform.position.x - _rightSideWallBounds.size.x / 2,
                                                               _rightSideWallBounds.transform.position.y + _rightSideWallBounds.size.y/2);
            for (int i = 0; i < wallSlotColumns; i++)
            {
                GameObject furnitureColumn = Instantiate(furnitureRowObject, _wallRightFurniturePoints);
                furnitureColumn.transform.localPosition = new Vector3((wallWidth/wallSlotColumns)* (0.5f + i),0, 0);
                furnitureColumn.name = (1 + i).ToString();
                row = 0;
                float columnHeight = wallHeight + (_rightSideWallMaxBounds.size.y-(wallHeight))/ (wallSlotColumns)*(i+0.5f);
                for (int j = 0; j < wallSlotRows; j++)
                {
                    GameObject furnitureSlot = Instantiate(_furnitureSlotPrefab, furnitureColumn.transform);
                    furnitureSlot.transform.localPosition = new Vector3(0, columnHeight/ wallSlotRows * -1 * (j+0.5f), 0);
                    float slotDepth = columnHeight / wallSlotRows;
                    float slotWidth = (wallWidth / wallSlotColumns);
                    furnitureSlot.GetComponent<BoxCollider2D>().size = new Vector2(slotWidth, slotDepth);
                    furnitureSlot.name = (1 + j).ToString();
                    furnitureSlot.GetComponent<FurnitureSlot>().InitializeSlot(row, col, _roomInfo.Id, FurnitureGrid.RightWall, _slotMaxGrowthPercentage, wallSlotColumns, slotWidth, slotDepth);
                    furnitureSlot.tag = "RightWallFurnitureSlot";
                    row++;
                }
                col++;
            }
            //Left Wall Slot generation
            row = 0;
            col = 0;
            wallWidth = _leftSideWallBounds.size.x;
            wallHeight = _leftSideWallBounds.size.y;
            wallSlotRows = (int)Mathf.Floor(wallHeight / 2.5f);
            wallSlotColumns = _slotRows;
            _wallLeftFurniturePoints.transform.position = new(_leftSideWallBounds.transform.position.x + _leftSideWallBounds.size.x / 2,
                                                               _leftSideWallBounds.transform.position.y + _leftSideWallBounds.size.y / 2);
            for (int i = 0; i < wallSlotColumns; i++)
            {
                GameObject furnitureColumn = Instantiate(furnitureRowObject, _wallLeftFurniturePoints);
                furnitureColumn.transform.localPosition = new Vector3((wallWidth / wallSlotColumns) * -1 *(0.5f + i), 0, 0);
                furnitureColumn.name = (1 + i).ToString();
                row = 0;
                float columnHeight = wallHeight + (_leftSideWallMaxBounds.size.y - (wallHeight)) / (wallSlotColumns) * (i + 0.5f);
                for (int j = 0; j < wallSlotRows; j++)
                {
                    GameObject furnitureSlot = Instantiate(_furnitureSlotPrefab, furnitureColumn.transform);
                    furnitureSlot.transform.localPosition = new Vector3(0, columnHeight / wallSlotRows * -1 * (j + 0.5f), 0);
                    float slotDepth = columnHeight / wallSlotRows;
                    float slotWidth = (wallWidth / wallSlotColumns);
                    furnitureSlot.GetComponent<BoxCollider2D>().size = new Vector2(slotWidth, slotDepth);
                    furnitureSlot.name = (1 + j).ToString();
                    furnitureSlot.GetComponent<FurnitureSlot>().InitializeSlot(row, col, _roomInfo.Id, FurnitureGrid.LeftWall, _slotMaxGrowthPercentage, wallSlotRows, slotWidth, slotDepth);
                    furnitureSlot.tag = "LeftWallFurnitureSlot";
                    row++;
                }
                col++;
            }
            //Ceiling Slot generation
            col = 0;
            row = 0;
            float ceilingWidth = _ceilingBounds.size.x;
            float ceilingHeight = _ceilingBounds.size.y;
            int ceilingSlotRows = _slotRows;
            int ceilingSlotColumns = _slotColumns;

            prevBottom = 0;
            for (int i = 0; i < _slotRows; i++)
            {
                GameObject furnitureRow = Instantiate(furnitureRowObject, _ceilingFurniturePoints);
                if (_floorAnchorPosition is RectPosition.Center)
                    furnitureRow.transform.localPosition = new Vector3(0, (ceilingHeight / 2) + -1 * (ceilingHeight / ceilingSlotRows * (0.5f + i)), 0);
                else if (_floorAnchorPosition is RectPosition.Top)
                    furnitureRow.transform.localPosition = new Vector3(0, prevBottom + -1 * ((ceilingHeight / ceilingSlotRows + (ceilingHeight / ceilingSlotRows) * (0.05f * (ceilingSlotRows / -2 + 0.5f + i))) / 2), 0);
                furnitureRow.name = (1 + i).ToString();
                col = 0;
                for (int j = 0; j < ceilingSlotColumns; j++)
                {
                    GameObject furnitureSlot = Instantiate(_furnitureSlotPrefab, furnitureRow.transform);
                    furnitureSlot.transform.localPosition = new Vector3((-1 * (ceilingWidth * (1 + (_slotMaxGrowthPercentage * (((float)i) / (((float)ceilingSlotRows) - 1)) / 100))) / 2) + _floorWidth * (1 + (_slotMaxGrowthPercentage * (((float)i) / (((float)ceilingSlotRows) - 1)) / 100)) / ceilingSlotColumns * (0.5f + j), 0, 0);
                    float slotDepth = ceilingHeight / ceilingSlotRows + (ceilingHeight / ceilingSlotRows) * (0.05f * (ceilingSlotRows / -2 + 0.5f + i));
                    float slotWidth = (ceilingWidth / ceilingSlotColumns) * (1 + _slotMaxGrowthPercentage * (((float)i) / (((float)ceilingSlotRows) - 1)) / 100);
                    furnitureSlot.GetComponent<BoxCollider2D>().size = new Vector2(slotWidth, slotDepth);
                    furnitureSlot.name = (1 + j).ToString();
                    furnitureSlot.GetComponent<FurnitureSlot>().InitializeSlot(row, col, _roomInfo.Id, FurnitureGrid.Floor, _slotMaxGrowthPercentage, ceilingSlotRows, slotWidth, slotDepth);
                    furnitureSlot.tag = "CeilingFurnitureSlot";

                    float initialHeightOffset = Mathf.Abs(furnitureSlot.transform.position.y - _wallBackFurniturePoints.position.y);
                    float interactHeight = (_backWallBounds.size.y / ceilingSlotRows);
                    furnitureSlot.GetComponent<BoxCollider2D>().offset = new(0, -initialHeightOffset - interactHeight * (i+0.5f));
                    furnitureSlot.GetComponent<BoxCollider2D>().size = new Vector2(slotWidth, interactHeight);
                    col++;
                }
                prevBottom -= ceilingHeight / ceilingSlotRows + (ceilingHeight / ceilingSlotRows) * (0.05f * (ceilingSlotRows / -2 + 0.5f + i));
                row++;
            }


            Destroy(furnitureRowObject);
            //Check if ladder should be enabled
            if (!topRoom)
            {
                _ladder.gameObject.SetActive(true);
                _ladder.GetComponent<SortingGroup>().sortingOrder = 20;
                
                foreach(Transform rowTransform in _wallBackFurniturePoints)
                {
                    rowTransform.GetChild(1).GetComponent<FurnitureSlot>().Ladder = true;
                    rowTransform.GetChild(2).GetComponent<FurnitureSlot>().Ladder = true;
                }
                _floorFurniturePoints.GetChild(0).GetChild(1).GetComponent<FurnitureSlot>().Ladder = true;
                _floorFurniturePoints.GetChild(0).GetChild(2).GetComponent<FurnitureSlot>().Ladder = true;
                _floorFurniturePoints.GetChild(1).GetChild(1).GetComponent<FurnitureSlot>().Ladder = true;
                _floorFurniturePoints.GetChild(1).GetChild(2).GetComponent<FurnitureSlot>().Ladder = true;
            }

            if (_roomInfo.Furnitures.Count > 0) InitialSetFurniture();
        }

        private void InitialSetFurniture()
        {
            foreach (Furniture furniture in _roomInfo.Furnitures)
            {
                int furnitureRow = furniture.Position.y;
                int furnitureColumn = furniture.Position.x;
                bool check = CheckFurniturePosition(furnitureRow, furnitureColumn, furniture.FurnitureGrid, furniture);
                if (check) SetFurniture(furnitureRow, furnitureColumn, furniture.FurnitureGrid, furniture);
            }
        }
        private bool CheckFurniturePosition(int row, int column, FurnitureGrid gridtoCheck, Furniture furniture)
        {
            Vector2Int furnitureSize = furniture.GetFurnitureSize();

            int startRow;
            int endColumn;

            if (furnitureSize.x == 0) return false;

            startRow = row - (furnitureSize.y - 1);
            endColumn = column + (furnitureSize.x - 1);

            if (furniture.Place is FurniturePlacement.Floor or FurniturePlacement.FloorNonblock or FurniturePlacement.Wall or FurniturePlacement.Ceiling)
            {
                if (gridtoCheck is FurnitureGrid.Floor or FurnitureGrid.Ceiling)
                {
                    if (startRow < 0 || endColumn >= _slotColumns) return false;
                }
                else if (gridtoCheck is FurnitureGrid.BackWall)
                {
                    if (startRow < 0 || endColumn >= _slotColumns) return false;
                }
                else if (gridtoCheck is FurnitureGrid.RightWall or FurnitureGrid.LeftWall)
                {
                    if (startRow < 0 || endColumn >= _slotRows) return false;
                }
                else return false;
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
                        if (gridtoCheck is FurnitureGrid.BackWall)
                        {
                            if ((_wallBackFurniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().TempFurniture != null
                                && !_wallBackFurniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().TempFurniture.Equals(furniture))
                                || _wallBackFurniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().Ladder) return false;
                        }
                        if (gridtoCheck is FurnitureGrid.RightWall)
                        {
                            if ((_wallRightFurniturePoints.GetChild(j).GetChild(i).GetComponent<FurnitureSlot>().TempFurniture != null
                                && !_wallRightFurniturePoints.GetChild(j).GetChild(i).GetComponent<FurnitureSlot>().TempFurniture.Equals(furniture))
                                || _wallRightFurniturePoints.GetChild(j).GetChild(i).GetComponent<FurnitureSlot>().Ladder) return false;
                        }
                        if (gridtoCheck is FurnitureGrid.LeftWall)
                        {
                            if ((_wallLeftFurniturePoints.GetChild(j).GetChild(i).GetComponent<FurnitureSlot>().TempFurniture != null
                                && !_wallLeftFurniturePoints.GetChild(j).GetChild(i).GetComponent<FurnitureSlot>().TempFurniture.Equals(furniture))
                                || _wallLeftFurniturePoints.GetChild(j).GetChild(i).GetComponent<FurnitureSlot>().Ladder) return false;
                        }
                    }
                    else if (furniture.Place is FurniturePlacement.Ceiling)
                    {
                        if (_ceilingFurniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().TempFurniture != null
                            && !_ceilingFurniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().TempFurniture.Equals(furniture)) return false;
                    }

                }
            }
            return true;
        }

        private bool CheckFurniturePosition(int row, int column, FurnitureGrid gridtoCheck, FurnitureHandling furniture, Vector2 backupHit, bool useBackup)
        {
            bool? check = null;
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
                        if (furniture.FurnitureSpriteLeft == null && (!furniture.SpriteCanBeFlipped || furniture.FurnitureSpriteRight == null)) check = false;
                        else
                        {
                            furniture.RotateFurniture(FurnitureHandling.Direction.Left);
                            row = rotatedRow;
                            column = rotatedColumn;
                        }
                    }
                }
                else if (endColumn == _slotColumns - 1)
                {
                    if (rotatedRow == 0) { }
                    else if (furniture.TempSpriteDirection is not FurnitureHandling.Direction.Right)
                    {
                        if (furniture.FurnitureSpriteRight == null && (!furniture.SpriteCanBeFlipped || furniture.FurnitureSpriteLeft == null)) check = false;
                        else
                        {
                            furniture.RotateFurniture(FurnitureHandling.Direction.Right);
                            row = rotatedRow;
                            column = rotatedColumn;
                        }
                    }
                }
            }

            if(gridtoCheck is FurnitureGrid.BackWall)
            {
                if (furniture.FurnitureSpriteFront == null && (!furniture.SpriteCanBeFlipped || furniture.FurnitureSpriteBack == null)) check = false;
                furniture.RotateFurniture(FurnitureHandling.Direction.Front);
            }
            else if (gridtoCheck is FurnitureGrid.RightWall)
            {
                if (furniture.FurnitureSpriteRight == null && (!furniture.SpriteCanBeFlipped || furniture.FurnitureSpriteLeft == null)) check = false;
                furniture.RotateFurniture(FurnitureHandling.Direction.Right);
            }
            else if (gridtoCheck is FurnitureGrid.LeftWall)
            {
                if (furniture.FurnitureSpriteLeft == null && (!furniture.SpriteCanBeFlipped || furniture.FurnitureSpriteRight == null)) check = false;
                furniture.RotateFurniture(FurnitureHandling.Direction.Left);
            }


            if(check == null) check = CheckFurniturePosition(row, column, gridtoCheck, furniture.Furniture);
            SetSlotValidity(row, column, gridtoCheck, furniture.Furniture, (bool)check);
            return (bool)check;
        }

        private void SetFurniture(int row, int column, FurnitureGrid grid, Furniture furniture)
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
                        if(grid is FurnitureGrid.BackWall)
                            _wallBackFurniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().Furniture = furniture;
                        else if(grid is FurnitureGrid.RightWall)
                            _wallRightFurniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().Furniture = furniture;
                        else if (grid is FurnitureGrid.LeftWall)
                            _wallLeftFurniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().Furniture = furniture;
                    }
                }
            }
            if (furniture.Place is FurniturePlacement.Floor or FurniturePlacement.FloorByWall or FurniturePlacement.FloorNonblock)
            {
                GameObject furnitureObject = Instantiate(_furnitureRefrence.GetSoulHomeFurnitureObject(furniture.Name), _floorFurniturePoints.GetChild(row).GetChild(column));
                furnitureObject.GetComponent<FurnitureHandling>().Furniture = furniture;
                furnitureObject.GetComponent<FurnitureHandling>().Position = new(column, row);
                furnitureObject.GetComponent<FurnitureHandling>().Slot = _floorFurniturePoints.GetChild(row).GetChild(column).GetComponent<FurnitureSlot>();
                furnitureObject.GetComponent<FurnitureHandling>().SetScale(row, grid, _floorFurniturePoints.GetChild(row).GetChild(column).GetComponent<FurnitureSlot>());
                furnitureObject.GetComponent<FurnitureHandling>().ResetFurniturePosition();
            }
            else if (furniture.Place is FurniturePlacement.Wall)
            {
                GameObject furnitureObject = Instantiate(_furnitureRefrence.GetSoulHomeFurnitureObject(furniture.Name), _wallBackFurniturePoints.GetChild(row).GetChild(column));
                furnitureObject.GetComponent<FurnitureHandling>().Furniture = furniture;
                furnitureObject.GetComponent<FurnitureHandling>().Position = new(column, row);
                if (grid is FurnitureGrid.BackWall)
                {
                    furnitureObject.GetComponent<FurnitureHandling>().Slot = _wallBackFurniturePoints.GetChild(row).GetChild(column).GetComponent<FurnitureSlot>();
                    furnitureObject.GetComponent<FurnitureHandling>().SetScale(row, grid, _wallBackFurniturePoints.GetChild(row).GetChild(column).GetComponent<FurnitureSlot>());
                }
                if (grid is FurnitureGrid.RightWall)
                {
                    furnitureObject.GetComponent<FurnitureHandling>().Slot = _wallRightFurniturePoints.GetChild(row).GetChild(column).GetComponent<FurnitureSlot>();
                    furnitureObject.GetComponent<FurnitureHandling>().SetScale(column, grid, _wallRightFurniturePoints.GetChild(row).GetChild(column).GetComponent<FurnitureSlot>());
                }
                if (grid is FurnitureGrid.LeftWall)
                {
                    furnitureObject.GetComponent<FurnitureHandling>().Slot = _wallLeftFurniturePoints.GetChild(row).GetChild(column).GetComponent<FurnitureSlot>();
                    furnitureObject.GetComponent<FurnitureHandling>().SetScale(column, grid, _wallLeftFurniturePoints.GetChild(row).GetChild(column).GetComponent<FurnitureSlot>());
                }
                furnitureObject.GetComponent<FurnitureHandling>().ResetFurniturePosition();
            }
        }
        public void MoveFurniture(int row, int column, FurnitureGrid grid, GameObject furniture, bool hover)
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
                            if(grid is FurnitureGrid.BackWall)
                                _wallBackFurniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().TempFurniture = furniture.GetComponent<FurnitureHandling>().Furniture;
                            else if (grid is FurnitureGrid.RightWall)
                                _wallRightFurniturePoints.GetChild(j).GetChild(i).GetComponent<FurnitureSlot>().TempFurniture = furniture.GetComponent<FurnitureHandling>().Furniture;
                            else if (grid is FurnitureGrid.LeftWall)
                                _wallLeftFurniturePoints.GetChild(j).GetChild(i).GetComponent<FurnitureSlot>().TempFurniture = furniture.GetComponent<FurnitureHandling>().Furniture;
                        }
                        else if (furniture.GetComponent<FurnitureHandling>().Furniture.Place is FurniturePlacement.Ceiling)
                        {
                            _ceilingFurniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().TempFurniture = furniture.GetComponent<FurnitureHandling>().Furniture;
                        }
                    }
                }
                if (furniture.GetComponent<FurnitureHandling>().TempSlot != null)
                {

                    int prevRow = furniture.GetComponent<FurnitureHandling>().TempSlot.row;
                    int prevColumn = furniture.GetComponent<FurnitureHandling>().TempSlot.column;
                    if (furniture.GetComponent<FurnitureHandling>().Furniture.Place is FurniturePlacement.Floor or FurniturePlacement.FloorByWall or FurniturePlacement.Wall or FurniturePlacement.Ceiling)
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
                                FurnitureGrid furnituregrid = furniture.GetComponent<FurnitureHandling>().TempSlot.furnitureGrid;
                                if (furnituregrid is FurnitureGrid.BackWall)
                                    _wallBackFurniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().TempFurniture = null;
                                if (furnituregrid is FurnitureGrid.RightWall)
                                    _wallRightFurniturePoints.GetChild(j).GetChild(i).GetComponent<FurnitureSlot>().TempFurniture = null;
                                if (furnituregrid is FurnitureGrid.LeftWall)
                                    _wallLeftFurniturePoints.GetChild(j).GetChild(i).GetComponent<FurnitureSlot>().TempFurniture = null;
                            }
                            else if (furniture.GetComponent<FurnitureHandling>().Furniture.Place is FurniturePlacement.Ceiling)
                            {
                                _ceilingFurniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().TempFurniture = null;
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
                    if (grid is FurnitureGrid.BackWall)
                        furniture.GetComponent<FurnitureHandling>().TempSlot = _wallBackFurniturePoints.GetChild(row).GetChild(column).GetComponent<FurnitureSlot>();
                    if (grid is FurnitureGrid.RightWall)
                        furniture.GetComponent<FurnitureHandling>().TempSlot = _wallRightFurniturePoints.GetChild(column).GetChild(row).GetComponent<FurnitureSlot>();
                    if (grid is FurnitureGrid.LeftWall)
                        furniture.GetComponent<FurnitureHandling>().TempSlot = _wallLeftFurniturePoints.GetChild(column).GetChild(row).GetComponent<FurnitureSlot>();
                }
                else if (furniture.GetComponent<FurnitureHandling>().Furniture.Place is FurniturePlacement.Ceiling)
                {
                    furniture.GetComponent<FurnitureHandling>().TempSlot = _ceilingFurniturePoints.GetChild(row).GetChild(column).GetComponent<FurnitureSlot>();
                }
            }
            if (furniture.GetComponent<FurnitureHandling>().Furniture.Place is FurniturePlacement.Floor or FurniturePlacement.FloorByWall or FurniturePlacement.FloorNonblock)
            {
                furniture.transform.SetParent(_floorFurniturePoints.GetChild(row).GetChild(column));
                furniture.GetComponent<FurnitureHandling>().ResetFurniturePosition();
                furniture.GetComponent<FurnitureHandling>().SetScale(row, grid, _floorFurniturePoints.GetChild(row).GetChild(column).GetComponent<FurnitureSlot>());
            }
            else if (furniture.GetComponent<FurnitureHandling>().Furniture.Place is FurniturePlacement.Wall)
            {
                if (grid is FurnitureGrid.BackWall)
                {
                    furniture.transform.SetParent(_wallBackFurniturePoints.GetChild(row).GetChild(column));
                    furniture.GetComponent<FurnitureHandling>().ResetFurniturePosition();
                    furniture.GetComponent<FurnitureHandling>().SetScale(row, grid, _wallBackFurniturePoints.GetChild(row).GetChild(column).GetComponent<FurnitureSlot>());
                }
                else if (grid is FurnitureGrid.RightWall)
                {
                    furniture.transform.SetParent(_wallRightFurniturePoints.GetChild(column).GetChild(row));
                    furniture.GetComponent<FurnitureHandling>().ResetFurniturePosition();
                    furniture.GetComponent<FurnitureHandling>().SetScale(column, grid, _wallRightFurniturePoints.GetChild(column).GetChild(row).GetComponent<FurnitureSlot>());
                }
                else if (grid is FurnitureGrid.LeftWall)
                {
                    furniture.transform.SetParent(_wallLeftFurniturePoints.GetChild(column).GetChild(row));
                    furniture.GetComponent<FurnitureHandling>().ResetFurniturePosition(true);
                    furniture.GetComponent<FurnitureHandling>().SetScale(column, grid, _wallLeftFurniturePoints.GetChild(column).GetChild(row).GetComponent<FurnitureSlot>());
                }
            }
            else if (furniture.GetComponent<FurnitureHandling>().Furniture.Place is FurniturePlacement.Ceiling)
            {
                furniture.transform.SetParent(_ceilingFurniturePoints.GetChild(row).GetChild(column));
                furniture.GetComponent<FurnitureHandling>().ResetFurniturePosition();
                furniture.GetComponent<FurnitureHandling>().SetScale(row, grid, _ceilingFurniturePoints.GetChild(row).GetChild(column).GetComponent<FurnitureSlot>());
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
                        FurnitureGrid gridToCheck = FurnitureGrid.None;
                        if (slot.CompareTag("FloorFurnitureSlot") && furnitureInfo.Furniture.Place is FurniturePlacement.Floor or FurniturePlacement.FloorByWall or FurniturePlacement.FloorNonblock)
                            {
                                gridToCheck = FurnitureGrid.Floor;
                            }
                        else if (slot.CompareTag("WallFurnitureSlot") && furnitureInfo.Furniture.Place is FurniturePlacement.Wall)
                            {
                            gridToCheck = FurnitureGrid.BackWall;
                        }
                        else if (slot.CompareTag("RightWallFurnitureSlot") && furnitureInfo.Furniture.Place is FurniturePlacement.Wall)
                        {
                            gridToCheck = FurnitureGrid.RightWall;
                        }
                        else if (slot.CompareTag("LeftWallFurnitureSlot") && furnitureInfo.Furniture.Place is FurniturePlacement.Wall)
                        {
                            gridToCheck = FurnitureGrid.LeftWall;
                        }
                        else if (slot.CompareTag("CeilingFurnitureSlot") && furnitureInfo.Furniture.Place is FurniturePlacement.Ceiling)
                        {
                            gridToCheck = FurnitureGrid.Ceiling;
                        }
                        if (gridToCheck is not FurnitureGrid.None)
                        {
                            bool check = CheckFurniturePosition(slot.GetComponent<FurnitureSlot>().row, slot.GetComponent<FurnitureSlot>().column, gridToCheck, furnitureInfo, backupHit, useBackup);
                            if (check)
                            {
                                MoveFurniture(slot.GetComponent<FurnitureSlot>().row, slot.GetComponent<FurnitureSlot>().column, gridToCheck, furniture, hover);
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
                            if(slot.furnitureGrid is FurnitureGrid.BackWall)
                                _wallBackFurniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().Furniture = null;
                            if (slot.furnitureGrid is FurnitureGrid.RightWall)
                                _wallRightFurniturePoints.GetChild(j).GetChild(i).GetComponent<FurnitureSlot>().Furniture = null;
                            if (slot.furnitureGrid is FurnitureGrid.LeftWall)
                                _wallLeftFurniturePoints.GetChild(j).GetChild(i).GetComponent<FurnitureSlot>().Furniture = null;
                        }
                        else if (furniture.Furniture.Place is FurniturePlacement.Ceiling)
                        {
                            _ceilingFurniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().Furniture = null;
                        }
                    }
                }
            }

        }

        private void SetSlotValidity(int row, int column, FurnitureGrid grid, Furniture furniture, bool check)
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
                if (grid is FurnitureGrid.Floor or FurnitureGrid.Ceiling)
                {
                    if (i < 0 || i >= _slotRows)
                    {
                        continue;
                    }
                }
                else if (grid is FurnitureGrid.BackWall or FurnitureGrid.RightWall or FurnitureGrid.LeftWall)
                {
                    if (i < 0 || i >= _slotHeight)
                    {
                        continue;
                    }
                }

                for (int j = column; j <= endColumn; j++)
                {
                    if (grid is FurnitureGrid.Floor or FurnitureGrid.BackWall or FurnitureGrid.Ceiling)
                    {
                        if (j < 0 || j >= _slotColumns)
                        {
                            continue;
                        }
                    }
                    else if (grid is FurnitureGrid.RightWall or FurnitureGrid.LeftWall)
                    {
                        if (j < 0 || j >= _slotRows)
                        {
                            continue;
                        }
                    }
                    if (furniture.Place is FurniturePlacement.Floor or FurniturePlacement.FloorByWall or FurniturePlacement.FloorNonblock)
                    {
                        _floorFurniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().SetValidity(check);
                        _currentSlotValidity.Add(_floorFurniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>());
                    }
                    else if (furniture.Place is FurniturePlacement.Wall)
                    {
                        if (grid is FurnitureGrid.BackWall)
                        {
                            _wallBackFurniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().SetValidity(check);
                            _currentSlotValidity.Add(_wallBackFurniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>());
                        }
                        if (grid is FurnitureGrid.RightWall)
                        {
                            _wallRightFurniturePoints.GetChild(j).GetChild(i).GetComponent<FurnitureSlot>().SetValidity(check);
                            _currentSlotValidity.Add(_wallRightFurniturePoints.GetChild(j).GetChild(i).GetComponent<FurnitureSlot>());
                        }
                        if (grid is FurnitureGrid.LeftWall)
                        {
                            _wallLeftFurniturePoints.GetChild(j).GetChild(i).GetComponent<FurnitureSlot>().SetValidity(check);
                            _currentSlotValidity.Add(_wallLeftFurniturePoints.GetChild(j).GetChild(i).GetComponent<FurnitureSlot>());
                        }
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
                            if(slot.furnitureGrid is FurnitureGrid.BackWall)
                                _wallBackFurniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().Furniture = furnitureObject;
                            else if (slot.furnitureGrid is FurnitureGrid.RightWall)
                                _wallRightFurniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().Furniture = furnitureObject;
                            else if (slot.furnitureGrid is FurnitureGrid.LeftWall)
                                _wallLeftFurniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().Furniture = furnitureObject;
                        }
                        if (furniture.Furniture.Place is FurniturePlacement.Ceiling)
                        {
                            _ceilingFurniturePoints.GetChild(i).GetChild(j).GetComponent<FurnitureSlot>().Furniture = furnitureObject;
                        }
                    }
                }
            }

        }
        public void ResetPosition(GameObject furniture, bool temp)
        {
            int prevRow;
            int prevColumn;
            FurnitureGrid furnitureGrid = FurnitureGrid.None;
            if (temp)
            {
                prevRow = furniture.GetComponent<FurnitureHandling>().TempSlot.row;
                prevColumn = furniture.GetComponent<FurnitureHandling>().TempSlot.column;
                furnitureGrid = furniture.GetComponent<FurnitureHandling>().TempSlot.furnitureGrid;
            }
            else
            {
                prevRow = furniture.GetComponent<FurnitureHandling>().Slot.row;
                prevColumn = furniture.GetComponent<FurnitureHandling>().Slot.column;
                furnitureGrid = furniture.GetComponent<FurnitureHandling>().Slot.furnitureGrid;
            }
            if (furniture.GetComponent<FurnitureHandling>().Furniture.Place is FurniturePlacement.Floor or FurniturePlacement.FloorByWall or FurniturePlacement.FloorNonblock)
            {
                furniture.transform.SetParent(_floorFurniturePoints.GetChild(prevRow).GetChild(prevColumn));
            }
            else if (furniture.GetComponent<FurnitureHandling>().Furniture.Place is FurniturePlacement.Wall)
            {
                if (furnitureGrid is FurnitureGrid.BackWall)
                    furniture.transform.SetParent(_wallBackFurniturePoints.GetChild(prevRow).GetChild(prevColumn));
                else if (furnitureGrid is FurnitureGrid.RightWall)
                    furniture.transform.SetParent(_wallRightFurniturePoints.GetChild(prevRow).GetChild(prevColumn));
                else if (furnitureGrid is FurnitureGrid.LeftWall)
                    furniture.transform.SetParent(_wallLeftFurniturePoints.GetChild(prevRow).GetChild(prevColumn));
            }
            else if (furniture.GetComponent<FurnitureHandling>().Furniture.Place is FurniturePlacement.Ceiling)
            {
                furniture.transform.SetParent(_ceilingFurniturePoints.GetChild(prevRow).GetChild(prevColumn));
            }
            furniture.GetComponent<FurnitureHandling>().SetScale();
        }
    }
}
