using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine;
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
        private int _defaultPenalty = 5;
        [SerializeField]
        private int _backSlotPenalty = 10;
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
        private GridNode[,] _grid;

        // Slots cached when they are created, indexed [row, column] like FurnitureSlot.row and FurnitureSlot.column.
        // Note: the side walls are built column by column, so in the hierarchy their slot is at GetChild(column).GetChild(row).
        private FurnitureSlot[,] _floorSlots;
        private FurnitureSlot[,] _backWallSlots;
        private FurnitureSlot[,] _rightWallSlots;
        private FurnitureSlot[,] _leftWallSlots;
        private FurnitureSlot[,] _ceilingSlots;
        // Local y of each floor row in _floorFurniturePoints
        private float[] _floorRowHeights;

        private List<FurnitureSlot> _currentSlotValidity;
        private List<Vector2Int> _walkableSlots = new();
        private bool _sleeping = false;

        public Room RoomInfo { get => _roomInfo;}
        public int SlotRows { get => _slotRows;}
        public int SlotColumns { get => _slotColumns;}
        public GridNode[,] Grid { get => _grid;}
        public List<Vector2Int> WalkableSlots { get => _walkableSlots;}
        public SoulHomeController Controller { get => _controller;}

        // Floor slot at [row, column]
        public FurnitureSlot GetFloorSlot(int row, int column) => _floorSlots[row, column];

        // The floor row whose height is closest to world position.
        public int GetFloorRowAt(Vector2 worldPosition)
        {
            // Compared in the floor's local space, so this stays right even if the room is moved.
            float y = _floorFurniturePoints.InverseTransformPoint(worldPosition).y;
            int closestRow = 0;
            float minDistance = float.MaxValue;
            for (int row = 0; row < _floorRowHeights.Length; row++)
            {
                float distance = Mathf.Abs(y - _floorRowHeights[row]);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestRow = row;
                }
            }
            return closestRow;
        }

        // The slot a furniture with this placement uses. Floor placements use the floor and ceiling placements the ceiling,
        // wall placements use the wall given by grid. Returns null for a wall placement without a wall grid.
        private FurnitureSlot GetSlot(FurniturePlacement place, FurnitureGrid grid, int row, int column)
        {
            FurnitureSlot[,] slots = place switch
            {
                FurniturePlacement.Floor or FurniturePlacement.FloorByWall or FurniturePlacement.FloorNonblock => _floorSlots,
                FurniturePlacement.Ceiling => _ceilingSlots,
                FurniturePlacement.Wall => grid switch
                {
                    FurnitureGrid.BackWall => _backWallSlots,
                    FurnitureGrid.RightWall => _rightWallSlots,
                    FurnitureGrid.LeftWall => _leftWallSlots,
                    _ => null
                },
                _ => null
            };
            return slots?[row, column];
        }

        /// <summary>
        /// Set by the <see cref="TowerController"/> when the room is far enough from the view.
        /// Tells the characters in the room to sleep or wake up.
        /// </summary>
        public bool Sleeping
        {
            get => _sleeping;
            set
            {
                if (_sleeping == value) return;
                _sleeping = value;
                foreach (Transform child in transform)
                {
                    if (child.TryGetComponent(out SoulHomeAvatarController avatar)) avatar.Sleeping = value;
                }
            }
        }

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
            _floorSlots = new FurnitureSlot[_slotRows, _slotColumns];
            _floorRowHeights = new float[_slotRows];
            for(int i = 0; i < _slotRows; i++)
            {
                GameObject furnitureRow = Instantiate(furnitureRowObject, _floorFurniturePoints);
                if(_floorAnchorPosition is RectPosition.Center)
                    furnitureRow.transform.localPosition = new Vector3(0, (_floorDepth / 2) + -1*(_floorDepth/_slotRows * (0.5f + i)), 0);
                else if(_floorAnchorPosition is RectPosition.Top)
                    furnitureRow.transform.localPosition = new Vector3(0, prevBottom + -1 * ((_floorDepth / _slotRows + (_floorDepth / _slotRows) * (0.05f * (_slotRows / -2 +0.5f+i)))/2), 0);
                // The slots sit at y = 0 inside their row, so the row's height is the slots' height.
                _floorRowHeights[i] = furnitureRow.transform.localPosition.y;
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
                    FurnitureSlot slot = furnitureSlot.GetComponent<FurnitureSlot>();
                    slot.InitializeSlot(this, row, col, _roomInfo.Id, FurnitureGrid.Floor, _slotMaxGrowthPercentage, _slotRows, slotWidth, slotDepth);
                    _floorSlots[row, col] = slot;
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
            _backWallSlots = new FurnitureSlot[wallSlotRows, wallSlotColumns];
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
                    FurnitureSlot slot = furnitureSlot.GetComponent<FurnitureSlot>();
                    slot.InitializeSlot(this, row, col, _roomInfo.Id, FurnitureGrid.BackWall, _slotMaxGrowthPercentage, wallSlotColumns, slotWidth, slotDepth);
                    _backWallSlots[row, col] = slot;
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
            _rightWallSlots = new FurnitureSlot[wallSlotRows, wallSlotColumns];
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
                    FurnitureSlot slot = furnitureSlot.GetComponent<FurnitureSlot>();
                    slot.InitializeSlot(this, row, col, _roomInfo.Id, FurnitureGrid.RightWall, _slotMaxGrowthPercentage, wallSlotColumns, slotWidth, slotDepth);
                    _rightWallSlots[row, col] = slot;
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
            _leftWallSlots = new FurnitureSlot[wallSlotRows, wallSlotColumns];
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
                    FurnitureSlot slot = furnitureSlot.GetComponent<FurnitureSlot>();
                    slot.InitializeSlot(this, row, col, _roomInfo.Id, FurnitureGrid.LeftWall, _slotMaxGrowthPercentage, wallSlotRows, slotWidth, slotDepth);
                    _leftWallSlots[row, col] = slot;
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
            _ceilingSlots = new FurnitureSlot[ceilingSlotRows, ceilingSlotColumns];
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
                    furnitureSlot.name = (1 + j).ToString();
                    FurnitureSlot slot = furnitureSlot.GetComponent<FurnitureSlot>();
                    slot.InitializeSlot(this, row, col, _roomInfo.Id, FurnitureGrid.Floor, _slotMaxGrowthPercentage, ceilingSlotRows, slotWidth, slotDepth);
                    _ceilingSlots[row, col] = slot;
                    furnitureSlot.tag = "CeilingFurnitureSlot";

                    float initialHeightOffset = Mathf.Abs(furnitureSlot.transform.position.y - _wallBackFurniturePoints.position.y);
                    float interactHeight = (_backWallBounds.size.y / ceilingSlotRows);
                    BoxCollider2D slotCollider = furnitureSlot.GetComponent<BoxCollider2D>();
                    slotCollider.offset = new(0, -initialHeightOffset - interactHeight * (i+0.5f));
                    slotCollider.size = new Vector2(slotWidth, interactHeight);
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

                for (int r = 0; r < _backWallSlots.GetLength(0); r++)
                {
                    _backWallSlots[r, 1].Ladder = true;
                    _backWallSlots[r, 2].Ladder = true;
                }
                _floorSlots[0, 1].Ladder = true;
                _floorSlots[0, 2].Ladder = true;
                _floorSlots[1, 1].Ladder = true;
                _floorSlots[1, 2].Ladder = true;
            }

            if (_roomInfo.Furnitures.Count > 0) InitialSetFurniture();

            UpdateGrid();
        }

        public void UpdateGrid()
        {
            int columns = _slotColumns;
            int rows = _slotRows;
            if (_grid == null)
            {
                _grid = new GridNode[columns, rows];

                for (int r = 0; r < rows; r++)
                {
                    for (int c = 0; c < columns; c++)
                    {
                        FurnitureSlot slot = _floorSlots[r, c];

                        //bool walkable = (slot.Furniture == null);
                        //_grid[c, r] = new GridNode(new Vector2Int(c, r), walkable);

                        if (_grid[c, r] == null)
                        {
                            _grid[c, r] = new GridNode(new Vector2Int(c, r), false, false);
                        }
                        GridNode node = _grid[c, r];
                        node.FurnitureSlot = slot;

                        _grid[c, r] = node;
                    }
                }
            }

            _walkableSlots.Clear();

            foreach (GridNode node in _grid)
            {
                node.IsFurniture = (node.FurnitureSlot.Furniture != null);
            }

            int gridWidth = _grid.GetLength(0);
            int gridHeight = _grid.GetLength(1);
            CalculatePenalties();

            // only add slots with no penalty to walkableslots
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    if (!_grid[x, y].IsFurniture && _grid[x, y].penalty == 0)
                    {
                        _walkableSlots.Add(new Vector2Int(x, y));
                    }
                }
            }
            // if there are none include the slots with penalties
            if (_walkableSlots.Count < 1)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    for (int y = 0; y < gridHeight; y++)
                    {
                        if (!_grid[x, y].IsFurniture)
                        {
                            _walkableSlots.Add(new Vector2Int(x, y));
                        }
                    }
                }
            }
        }

        // Add a penalty to tiles to the right or left of furniture, and double penalty if there is furniture to the left and the right,
        // and also add a penalty to the bottom-right corner + 1 node to the left, and bottom-left corner and 1 node from that to the right,
        // so the avatar only goes there if there is no other way (makes clipping with furniture less likely, and avatar only chooses to "slip"
        // through the corner of the furniture if there is absolutely no other way to the end position)
        private void CalculatePenalties()
        {
            foreach (GridNode node in _grid)
            {
                node.penalty = 0;
            }

            int gridWidth = _grid.GetLength(0);
            int gridHeight = _grid.GetLength(1);

            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    GridNode currentNode = _grid[x, y];
                    // skip cells that are not furniture
                    if (!currentNode.IsFurniture) continue;

                    bool isSameFurnitureAbove = (y > 0 &&
                                                 _grid[x, y - 1].IsFurniture &&
                                                 _grid[x, y - 1].FurnitureSlot.Furniture == currentNode.FurnitureSlot.Furniture);


                    if (y == 0)
                    {
                        currentNode.penalty = _backSlotPenalty;
                        currentNode.IsBackSlot = true;
                    }
                    else if (!_grid[x, y - 1].IsFurniture || !isSameFurnitureAbove)
                    {
                        currentNode.penalty = _backSlotPenalty;
                        currentNode.IsBackSlot = true;
                    }
                    else
                    {
                        currentNode.penalty = 800;
                        currentNode.IsBackSlot = false;
                    }


                    // apply penalty to the left and right to the furniture
                    ApplyPenalty(x - 1, y, _defaultPenalty);
                    ApplyPenalty(x + 1, y, _defaultPenalty);


                    // if there is no furniture below
                    if (y < gridHeight - 1 && !_grid[x, y + 1].IsFurniture)
                    {
                        // if there is no furniture to the left (meaning this is the bottom-left corner)
                        if (x > 0 && !_grid[x - 1, y].IsFurniture)
                        {
                            // The bottom-left corner
                            ApplyPenalty(x - 1, y + 1, _defaultPenalty);
                            // The cell above it
                            ApplyPenalty(x - 1, y, _defaultPenalty);
                            // The cell to the right of the bottom-left corner
                            ApplyPenalty(x, y + 1, _defaultPenalty * 2);
                        }
                        // if there is no furniture to the right (meaning this is the bottom-right corner)
                        if (x < gridWidth - 1 && !_grid[x + 1, y].IsFurniture)
                        {
                            // The bottom-right corner
                            ApplyPenalty(x + 1, y + 1, _defaultPenalty);
                            // The cell above it
                            ApplyPenalty(x + 1, y, _defaultPenalty);
                            // The cell to the left of the bottom-right corner
                            ApplyPenalty(x, y + 1, _defaultPenalty * 2);
                        }
                    }
                }
            }
        }

        // applies penalty if inside grid bounds
        public void ApplyPenalty(int x, int y, int amount)
        {
            int gridWidth = _grid.GetLength(0);
            int gridHeight = _grid.GetLength(1);

            if (x >= 0 && x < gridWidth && y >= 0 && y < gridHeight)
            {
                if (!_grid[x, y].IsFurniture)
                {
                    _grid[x, y].penalty += amount;
                }
            }
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
            Vector3Int furnitureSize = furniture.GetFurnitureSize();

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
                    FurnitureSlot slot = GetSlot(furniture.Place, gridtoCheck, i, j);
                    if (slot == null) continue;

                    if (furniture.Place is FurniturePlacement.FloorNonblock)
                    {
                        if ((slot.TempFurnitureNonBlock != null && !slot.TempFurnitureNonBlock.Equals(furniture)) || slot.Ladder) return false;
                    }
                    else if (furniture.Place is FurniturePlacement.Ceiling)
                    {
                        if (slot.TempFurniture != null && !slot.TempFurniture.Equals(furniture)) return false;
                    }
                    else // Floor, FloorByWall and Wall
                    {
                        if ((slot.TempFurniture != null && !slot.TempFurniture.Equals(furniture)) || slot.Ladder) return false;
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
                Vector3Int furnitureSize = furniture.GetFurnitureSizeRotated();
                Vector3Int furnitureSizeCurrent = furniture.GetFurnitureSize();

                Vector2 checkPoint = backupHit + new Vector2((furniture.transform.localScale.x / 2) + ((furniture.transform.localScale.x * furnitureSize.x) / 2) * -1, 0);
                Ray ray2 = new(_towerCamera.position, (Vector3)checkPoint - _towerCamera.position);
                RaycastHit2D[] hitArray;
                hitArray = Physics2D.GetRayIntersectionAll(ray2, 1000);

                int rotatedRow = -1;
                int rotatedColumn = -1;
                //bool slotHit = false;
                foreach (RaycastHit2D hit in hitArray)
                {
                    if (hit.collider.TryGetComponent(out FurnitureSlot hitSlot))
                    {
                        rotatedRow = hitSlot.row;
                        rotatedColumn = hitSlot.column;
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
            Vector3Int furnitureSize = furniture.GetFurnitureSize();
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
                    // Ceiling furniture isn't set up here.
                    if (furniture.Place is FurniturePlacement.Ceiling) continue;
                    FurnitureSlot slot = GetSlot(furniture.Place, grid, i, j);
                    if (slot == null) continue;

                    if (furniture.Place is FurniturePlacement.FloorNonblock) slot.FurnitureNonBlock = furniture;
                    else slot.Furniture = furniture;
                }
            }
            if (furniture.Place is FurniturePlacement.Floor or FurniturePlacement.FloorByWall or FurniturePlacement.FloorNonblock)
            {
                FurnitureSlot slot = _floorSlots[row, column];
                GameObject furnitureObject = Instantiate(_furnitureRefrence.GetSoulHomeFurnitureObject(furniture.Name), slot.transform);
                FurnitureHandling handling = furnitureObject.GetComponent<FurnitureHandling>();
                handling.Furniture = furniture;
                handling.Position = new(column, row);
                handling.Slot = slot;
                handling.SetScale(row, grid, slot);
                handling.ResetFurniturePosition();
            }
            else if (furniture.Place is FurniturePlacement.Wall)
            {
                GameObject furnitureObject = Instantiate(_furnitureRefrence.GetSoulHomeFurnitureObject(furniture.Name), _backWallSlots[row, column].transform);
                FurnitureHandling handling = furnitureObject.GetComponent<FurnitureHandling>();
                handling.Furniture = furniture;
                handling.Position = new(column, row);
                FurnitureSlot slot = GetSlot(FurniturePlacement.Wall, grid, row, column);
                if (slot != null)
                {
                    handling.Slot = slot;
                    // The back wall scales by row, the side walls by column.
                    handling.SetScale(grid is FurnitureGrid.BackWall ? row : column, grid, slot);
                }
                handling.ResetFurniturePosition();
            }
        }
        public void MoveFurniture(int row, int column, FurnitureGrid grid, GameObject furniture, bool hover)
        {
            FurnitureHandling handling = furniture.GetComponent<FurnitureHandling>();
            FurniturePlacement place = handling.Furniture.Place;
            if (!hover) {
                Debug.Log("Set:"+row + ":" + column);
                Vector3Int furnitureSize = handling.GetFurnitureSize();
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
                        FurnitureSlot slot = GetSlot(place, grid, i, j);
                        if (slot == null) continue;
                        if (place is FurniturePlacement.FloorNonblock) slot.TempFurnitureNonBlock = handling.Furniture;
                        else slot.TempFurniture = handling.Furniture;
                    }
                }
                FurnitureSlot tempSlot = handling.TempSlot;
                if (tempSlot != null)
                {

                    int prevRow = tempSlot.row;
                    int prevColumn = tempSlot.column;
                    if (place is FurniturePlacement.Floor or FurniturePlacement.FloorByWall or FurniturePlacement.Wall or FurniturePlacement.Ceiling)
                    {
                        if (tempSlot.TempRotated != handling.IsRotated)
                        furnitureSize = handling.GetFurnitureSizeRotated();
                    }
                    else if (place is FurniturePlacement.FloorNonblock)
                    {
                        if (tempSlot.TempRotatedNonBlock != handling.IsRotated)
                            furnitureSize = handling.GetFurnitureSizeRotated();
                    }


                    startRow = prevRow - (furnitureSize.y - 1);
                    endColumn = prevColumn + (furnitureSize.x - 1);
                    Debug.Log("StartRow:" + startRow + ", EndColumn:" + endColumn);
                    for (int i = startRow; i <= prevRow; i++)
                    {
                        for (int j = prevColumn; j <= endColumn; j++)
                        {
                            // A wall furniture is cleared from the wall it was on before.
                            FurnitureSlot slot = GetSlot(place, tempSlot.furnitureGrid, i, j);
                            if (slot == null) continue;
                            if (place is FurniturePlacement.FloorNonblock) slot.TempFurnitureNonBlock = null;
                            else slot.TempFurniture = null;
                        }
                    }
                }
                FurnitureSlot newTempSlot = GetSlot(place, grid, row, column);
                if (newTempSlot != null) handling.TempSlot = newTempSlot;
            }
            FurnitureSlot targetSlot = GetSlot(place, grid, row, column);
            if (targetSlot == null) return;

            furniture.transform.SetParent(targetSlot.transform);
            // Furniture on the left wall is positioned the other way around.
            handling.ResetFurniturePosition(place is FurniturePlacement.Wall && grid is FurnitureGrid.LeftWall);
            // Side wall furniture scales by column, everything else by row.
            bool sideWall = place is FurniturePlacement.Wall && grid is FurnitureGrid.RightWall or FurnitureGrid.LeftWall;
            handling.SetScale(sideWall ? column : row, grid, targetSlot);
        }

        public bool HandleFurniturePosition(RaycastHit2D[] hitArray, GameObject furniture, bool hover, Vector2 backupHit, bool useBackup)
        {
            if (!furniture.TryGetComponent(out FurnitureHandling furnitureInfo)) return false;

            foreach (RaycastHit2D hit2 in hitArray)
            {
                if (hit2.collider != null)
                {
                    //Debug.Log(hit2 +": Collider found," +hit2.transform.tag);
                    if (hit2.collider.TryGetComponent(out FurnitureSlot hitSlot))
                    {
                        //Debug.Log(hit2 + ": Slot found, Room "+ hit2.collider.transform.parent.parent.parent.GetComponent<RoomData>().RoomInfo.Id+
                        //    ", Slot " + hit2.collider.GetComponent<FurnitureSlot>().row +":"+ hit2.collider.GetComponent<FurnitureSlot>().column);
                        GameObject slot = hit2.collider.gameObject;
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
                            bool check = CheckFurniturePosition(hitSlot.row, hitSlot.column, gridToCheck, furnitureInfo, backupHit, useBackup);
                            if (check)
                            {
                                MoveFurniture(hitSlot.row, hitSlot.column, gridToCheck, furniture, hover);
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
            Vector3Int furnitureSize;

            if (furniture.Furniture.IsRotated)
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
                        FurnitureSlot freedSlot = GetSlot(furniture.Furniture.Place, slot.furnitureGrid, i, j);
                        if (freedSlot == null) continue;
                        if (furniture.Furniture.Place is FurniturePlacement.FloorNonblock) freedSlot.FurnitureNonBlock = null;
                        else freedSlot.Furniture = null;
                    }
                }
            }

        }

        private void SetSlotValidity(int row, int column, FurnitureGrid grid, Furniture furniture, bool check)
        {
            Vector3Int furnitureSize = furniture.GetFurnitureSize();

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
                    // Ceiling slots don't show validity.
                    if (furniture.Place is FurniturePlacement.Ceiling) continue;
                    FurnitureSlot slot = GetSlot(furniture.Place, grid, i, j);
                    if (slot == null) continue;
                    slot.SetValidity(check);
                    _currentSlotValidity.Add(slot);
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

        public FurnitureSlot GetFurnitureSlot(FurnitureGrid grid,int row, int column)
        {
            return grid switch
            {
                FurnitureGrid.Floor => _floorSlots[row, column],
                FurnitureGrid.BackWall => _backWallSlots[row, column],
                FurnitureGrid.RightWall => _rightWallSlots[row, column],
                FurnitureGrid.LeftWall => _leftWallSlots[row, column],
                FurnitureGrid.Ceiling => _ceilingSlots[row, column],
                _ => null,
            };
        }

        public void SetFurnitureSlots(FurnitureHandling furniture)
        {
            FurnitureSlot slot = furniture.Slot;
            Furniture furnitureObject = furniture.Furniture;

            Vector3Int furnitureSize = furniture.GetFurnitureSize();

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
                        FurnitureSlot setSlot = GetSlot(furniture.Furniture.Place, slot.furnitureGrid, i, j);
                        if (setSlot == null) continue;
                        if (furniture.Furniture.Place is FurniturePlacement.FloorNonblock) setSlot.FurnitureNonBlock = furnitureObject;
                        else setSlot.Furniture = furnitureObject;
                    }
                }
            }

        }
        public void ResetPosition(GameObject furniture, bool temp)
        {
            FurnitureHandling handling = furniture.GetComponent<FurnitureHandling>();
            FurnitureSlot previousSlot = temp ? handling.TempSlot : handling.Slot;
            FurnitureSlot parentSlot = GetSlot(handling.Furniture.Place, previousSlot.furnitureGrid, previousSlot.row, previousSlot.column);
            if (parentSlot != null) furniture.transform.SetParent(parentSlot.transform);
            handling.SetScale();
        }
    }
}
