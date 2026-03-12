
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.U2D.Animation;

namespace MenuUI.Scripts.SoulHome
{
    public enum AvatarStatus
    {
        Idle,
        Wander
    }

    public class SoulHomeAvatarController : MonoBehaviour, ISoulHomeObjectClick
    {
        [SerializeField] private float _minIdleTimer = 2f;
        [SerializeField] private float _maxIdleTimer = 4f;
        [SerializeField] private float _speed = 5;
        // how far away the avatar stays from furniture
        [SerializeField] private float _movePadding = 2f;
        [SerializeField]
        private SortingGroup _sortingGroup;
        [SerializeField]
        private Animator _animator;
        [SerializeField]
        private AnimationClip _idleAnimation;
        [SerializeField]
        private AnimationClip _walkAnimation;
        [SerializeField]
        private List<AnimationClip> _interactAnimation;

        private bool _performingAnimation = false;

        private AvatarStatus _status;

        private Transform _points;
        private RoomData _roomData;
        private List<Vector2> _travelPoints = new();

        private AvatarRig _rig;
        private SpriteResolver _lHandResolver;
        private SpriteResolver _rHandResolver;
        private string _lHandLabel;
        private string _rHandLabel;

        private Coroutine _statusCoroutine;
        private GridNode[,] _grid;
        private List<AnimationClip> _verifiedInteractClips = new();
        private Vector2Int _currentGridPosition;
        private List<Vector2Int> _walkableSlots = new();
        private const int DefaultPenalty = 5;
        private int _gridWidth;
        private int _gridHeight;
        private List<Vector2> _rawPath;
        private bool hasInitialized = false;

        private List<Vector2> _smoothPath = new();
        public AvatarStatus Status
        {
            get => _status;
            set
            {
                if (_status == value) return;
                _status = value;
                OnStatusChanged();
            }
        }

        void Start()
        {
            //_animator.keepAnimatorStateOnDisable = true;
            _animator.writeDefaultValuesOnDisable = true;

            if (transform.parent.CompareTag("Room"))
            {
                _points = transform.parent.Find("FurniturePoints").Find("FloorFurniturePoints");
                _roomData = transform.parent.GetComponent<RoomData>();

                SetAnimationClips();

                SetAvatar(_points, _roomData);
                OnStatusChanged();

                _rig = GetComponentInChildren<AvatarRig>();
                hasInitialized = true;
                if (_rig == null)
                {
                    UnityEngine.Debug.LogError("Failed to get AvatarRig");
                    return;
                }

                _lHandResolver = _rig.Resolvers[AvatarPart.L_Hand];
                _rHandResolver = _rig.Resolvers[AvatarPart.R_Hand];
                _lHandLabel = _lHandResolver.GetLabel();
                _rHandLabel = _rHandResolver.GetLabel();
            }
        }
        private void OnEnable()
        {
            if (!hasInitialized) return;

            if (_lHandResolver != null && _rHandResolver != null)
            {
                _lHandLabel = _lHandResolver.GetLabel();
                _rHandLabel = _rHandResolver.GetLabel();
            }

            if (_grid == null)
            {
                UpdateGrid();
            }

            _performingAnimation = false;
            OnStatusChanged();
        }

        private void OnDisable()
        {
            StopAllCoroutines();
        }

        private void SetAnimationClips()
        {
            AnimationClip[] animatorClips = _animator.runtimeAnimatorController.animationClips;

            foreach (AnimationClip clip in _interactAnimation)
            {
                foreach (AnimationClip controllerClip in animatorClips)
                {
                    if (controllerClip == clip)
                    {
                        _verifiedInteractClips.Add(clip);
                        break;
                    }
                }
            }
        }

        private void SelectStatus()
        {
            if (Status == AvatarStatus.Idle) Status = AvatarStatus.Wander;
            else if (Status == AvatarStatus.Wander) Status = AvatarStatus.Idle;
        }

        private void OnStatusChanged()
        {
            if (_statusCoroutine != null)
            {
                StopCoroutine(_statusCoroutine);
            }

            switch (_status)
            {
                case AvatarStatus.Idle:
                    _statusCoroutine = StartCoroutine(HandleIdle());
                    break;
                case AvatarStatus.Wander:
                    HandleWander();
                    break;
                default:
                    SelectStatus();
                    break;
            }
        }

        private IEnumerator HandleIdle()
        {
            float idleTime = Random.Range(_minIdleTimer, _maxIdleTimer);
            float elapsed = 0f;
            _animator.Play(_idleAnimation.name);
            UseDefaultHands(false);
            _performingAnimation = false;

            while (elapsed < idleTime)
            {
                elapsed += Time.deltaTime;

                yield return null;
            }

            SelectStatus();
        }

        private void HandleWander(Vector2Int? targetGridPosition = null)
        {
            _travelPoints.Clear();
            // Need to make grid updating to happen when furniture is changed, no reason to do it this often
            UpdateGrid();
            //Stopwatch stopwatch = Stopwatch.StartNew();
            if (_walkableSlots.Count > 0)
            {
                Vector2Int target = targetGridPosition ?? _walkableSlots[Random.Range(0, _walkableSlots.Count)];
                List<GridNode> nodePath = FindPath(_currentGridPosition, target);
                if (nodePath != null)
                {
                    _rawPath = GetTravelPoints(nodePath);
                    _smoothPath = SmoothPath(_rawPath);
                    _travelPoints = _smoothPath;
                    //_travelPoints = GetTravelPoints(nodePath);
                    _currentGridPosition = target;
                    //stopwatch.Stop();
                    //UnityEngine.Debug.Log($"calculating path took {stopwatch.Elapsed.TotalMilliseconds} milliseconds");
                    _statusCoroutine = StartCoroutine(MoveRoutine());
                }
                else
                {
                    UnityEngine.Debug.LogError("Nodepath was null, Most likely meaning there was no way to the destination without clipping through furniture");
                    SelectStatus();
                }
            }
            else
            {
                SelectStatus();
            }
        }

        private IEnumerator MoveRoutine()
        {
            if (_performingAnimation) yield break;

            while (_travelPoints.Count > 0)
            {
                Vector2 targetPos = _travelPoints[0];

                if (targetPos.x < transform.position.x)
                {
                    transform.rotation = Quaternion.Euler(0, 180, 0);
                }
                else
                {
                    transform.rotation = Quaternion.Euler(Vector3.zero);
                }

                if (!_animator.GetCurrentAnimatorStateInfo(0).IsName(_walkAnimation.name))
                {
                    _animator.Play(_walkAnimation.name);
                    UseDefaultHands(true);
                }

                while (Vector2.Distance(transform.position, targetPos) > 0.05f)
                {
                    transform.position = Vector2.MoveTowards(
                        transform.position,
                        targetPos,
                        _speed * Time.deltaTime);

                    UpdateSortingOrder(WorldToGrid(transform.position).y);

                    yield return null;
                }

                transform.position = targetPos;
                _travelPoints.RemoveAt(0);
            }
            UseDefaultHands(false);
            SelectStatus();
        }

        [ContextMenu("testi")]
        public void UpdateGrid()
        {
            int columns = _roomData.SlotColumns;
            int rows = _roomData.SlotRows;
            _grid = new GridNode[columns, rows];
            _walkableSlots.Clear();

            for (int r = 0; r < rows; r++)
            {
                Transform rowTransform = _points.GetChild(r);

                for (int c = 0; c < columns; c++)
                {
                    FurnitureSlot slot = rowTransform.GetChild(c).GetComponent<FurnitureSlot>();

                    //bool walkable = (slot.Furniture == null);
                    //_grid[c, r] = new GridNode(new Vector2Int(c, r), walkable);
                    GridNode node = new(new Vector2Int(c, r), false);
                    if (slot.Furniture != null)
                    {
                        node.IsFurniture = true;
                        node.Furniture = slot.Furniture;
                    }
                    _grid[c, r] = node;
                }
            }
            _gridWidth = _grid.GetLength(0);
            _gridHeight = _grid.GetLength(1);
            CalculatePenalties();

            // only add slots with no penalty to walkableslots
            // need to make exception if there are none
            for (int x = 0; x < _gridWidth; x++)
            {
                for (int y = 0; y < _gridHeight; y++)
                {
                    if (!_grid[x, y].IsFurniture && _grid[x, y].penalty == 0)
                    {
                        _walkableSlots.Add(new Vector2Int(x, y));
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

            for (int x = 0; x < _gridWidth; x++)
            {
                for (int y = 0; y < _gridHeight; y++)
                {
                    GridNode currentNode = _grid[x, y];
                    // skip cells that are not furniture
                    if (!currentNode.IsFurniture) continue;
                    // Will be true for 2 pieces of the same furniture, which is not ideal
                    bool isSameFurnitureAbove = (y > 0 &&
                                                 _grid[x, y - 1].IsFurniture &&
                                                 _grid[x, y - 1].Furniture == currentNode.Furniture);

                    if (y == 0)
                    {
                        currentNode.penalty = 100;
                    }
                    else if (!_grid[x, y - 1].IsFurniture || !isSameFurnitureAbove)
                    {
                        currentNode.penalty = 150;
                    }
                    else
                    {
                        currentNode.penalty = 800;
                    }

                        // apply penalty to the left and right to the furniture
                        ApplyPenalty(x - 1, y, DefaultPenalty);
                    ApplyPenalty(x + 1, y, DefaultPenalty);


                    // if there is no furniture below
                    if (y < _gridHeight - 1 && !_grid[x, y + 1].IsFurniture)
                    {
                        // if there is no furniture to the left (meaning this is the bottom-left corner)
                        if (x > 0 && !_grid[x - 1, y].IsFurniture)
                        {
                            // The bottom-left corner
                            ApplyPenalty(x - 1, y + 1, DefaultPenalty);
                            // The cell above it
                            ApplyPenalty(x - 1, y, DefaultPenalty);
                            // The cell to the right of the bottom-left corner
                            ApplyPenalty(x , y + 1, DefaultPenalty * 2);
                        }
                        // if there is no furniture to the right (meaning this is the bottom-right corner)
                        if (x < _gridWidth - 1 && !_grid[x + 1, y].IsFurniture)
                        {
                            // The bottom-right corner
                            ApplyPenalty(x + 1, y + 1, DefaultPenalty);
                            // The cell above it
                            ApplyPenalty(x + 1, y, DefaultPenalty);
                            // The cell to the left of the bottom-right corner
                            ApplyPenalty(x, y + 1, DefaultPenalty * 2);
                        }
                    }
                }
            }
        }

        // applies penalty if inside grid bounds
        public void ApplyPenalty(int x, int y, int amount)
        {
            if (x >= 0 && x < _gridWidth && y >= 0 && y < _gridHeight)
            {
                if (!_grid[x, y].IsFurniture)
                {
                    _grid[x, y].penalty += amount;
                }
            }
        }

        private void UpdateSortingOrder(int gridRow)
        {
            _sortingGroup.sortingOrder = 6 + (gridRow * 100);
        }

        #region pathfinding

        private Vector2 GridToWorld(Vector2Int gridPosition)
        {
            Transform rowTransform = _points.GetChild(gridPosition.y);
            Transform slotTransform = rowTransform.GetChild(gridPosition.x);

            return new Vector2(slotTransform.position.x, slotTransform.position.y);
        }

        //finds the closest grid position from the actual avatar position
        private Vector2Int WorldToGrid(Vector2 worldPos)
        {
            Vector2Int closest = Vector2Int.zero;
            float minDistance = float.MaxValue;

            for (int y = 0; y < _roomData.SlotRows; y++)
            {
                for (int x = 0;  x < _roomData.SlotColumns; x++)
                {
                    Vector2 slotPos = GridToWorld(new Vector2Int(x, y));
                    float distance = Vector2.Distance(worldPos, slotPos);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closest = new Vector2Int(x, y);
                    }
                }
            }
            return closest;
        }

        private List<Vector2> GetTravelPoints(List<GridNode> path)
        {
            List<Vector2> worldPath = new();

            foreach (GridNode node in path)
            {
                Vector2 worldPosition = GridToWorld(node.GridPosition);
                worldPath.Add(worldPosition);
            }
            return worldPath;
        }

        private List<GridNode> GetNeighbors(GridNode currentNode)
        {
            List<GridNode> neighbors = new();

            Vector2Int[] directions =
            {
                new Vector2Int(0, 1),  // Up
                new Vector2Int(0, -1), // Down
                new Vector2Int(1, 0),  // Right
                new Vector2Int(-1, 0), // Left

                new Vector2Int(1, 1),  // Top Right
                new Vector2Int(-1, 1), // Top Left
                new Vector2Int(1, -1), // Bottom Right
                new Vector2Int(-1, -1),// Bottom Left
            };

            foreach (Vector2Int direction in directions)
            {
                int checkX = currentNode.GridPosition.x + direction.x;
                int checkY = currentNode.GridPosition.y + direction.y;

                // check if neighbor is inside grid
                if (checkX >= 0 && checkX < _roomData.SlotColumns &&
                    checkY >= 0 && checkY < _roomData.SlotRows)
                {
                    neighbors.Add(_grid[checkX, checkY]);
                }
            }

            return neighbors;
        }

        private float GetDistance(GridNode a, GridNode b)
        {
            int distanceX = Mathf.Abs(a.GridPosition.x - b.GridPosition.x);
            int distanceY = Mathf.Abs(a.GridPosition.y - b.GridPosition.y);

            // Diagonal nodes are 1.41 times farther, otherwise diagonal movement is preferred
            //if (distanceX > distanceY)
            //    return 1.41f * distanceY + 1.0f * (distanceX - distanceY);
            //return 1.41f * distanceX + 1.0f * (distanceY - distanceX);

            return Mathf.Max(distanceX, distanceY);
        }

        private GridNode GetLowestFCostNode(List<GridNode> nodes)
        {
            GridNode best = nodes[0];

            foreach (GridNode node in nodes)
            {
                if (node.FCost < best.FCost ||
                    node.FCost == best.FCost && node.HCost < best.HCost)
                {
                    best = node;
                }
            }
            return best;
        }

        private List<GridNode> RetracePath(GridNode startNode, GridNode endNode)
        {
            List<GridNode> path = new();
            GridNode currentNode = endNode;

            while (currentNode != startNode)
            {
                path.Add(currentNode);
                currentNode = currentNode.Parent;
            }

            path.Reverse();

            return path;
        }

        public List<GridNode> FindPath(Vector2Int startPos, Vector2Int targetPos)
        {
            foreach (GridNode node in _grid)
            {
                node.Reset();
            }

            GridNode startNode = _grid[startPos.x, startPos.y];
            startNode.GCost = 0;
            startNode.HCost = GetDistance(startNode, _grid[targetPos.x, targetPos.y]);

            List<GridNode> openList = new List<GridNode>() { startNode };
            HashSet<GridNode> closedList = new();

            while (openList.Count > 0)
            {
                GridNode currentNode = GetLowestFCostNode(openList);

                openList.Remove(currentNode);
                closedList.Add(currentNode);

                if (currentNode.GridPosition == targetPos)
                {
                    return RetracePath(_grid[startPos.x, startPos.y], _grid[targetPos.x, targetPos.y]);
                }

                foreach(GridNode neighbor in GetNeighbors(currentNode))
                {
                    if (closedList.Contains(neighbor)) continue;

                    float newCostToNeightbor = currentNode.GCost + GetDistance(currentNode, neighbor) + neighbor.penalty;

                    if (newCostToNeightbor < neighbor.GCost)
                    {
                        neighbor.GCost = newCostToNeightbor;
                        neighbor.HCost = GetDistance(neighbor, _grid[targetPos.x, targetPos.y]);
                        neighbor.Parent = currentNode;

                        if (!openList.Contains(neighbor)) openList.Add(neighbor);
                    }
                }
            }
            // no available path
            return null;
        }

        // if there is a clear los between points, removes points in between, so the avatar won't zig-zag
        // currently walks through furniture weirdly if used
        private List<Vector2> SmoothPath(List<Vector2> fullPath)
        {
            if (fullPath.Count < 3) return fullPath;

            List<Vector2> smoothedPath = new();
            Vector2 currentPoint = fullPath[0]; // point where los is checked from
            smoothedPath.Add(currentPoint);

            for (int i = 1; i < fullPath.Count - 1; i++)
            {
                Vector2Int gridPos = WorldToGrid(fullPath[i]);

                // If furniture blocks direct path to future point
                if (IsSmoothPathTooExpensive(currentPoint, fullPath[i + 1]))
                {
                    // stop at last point before hitting furniture
                    currentPoint = fullPath[i];
                    smoothedPath.Add(currentPoint);
                }
            }

            smoothedPath.Add(fullPath[fullPath.Count - 1]); //destination point
            return smoothedPath;
        }

        private bool IsSmoothPathTooExpensive(Vector2 start, Vector2 end)
        {
            Vector2Int gridStart = WorldToGrid(start);
            Vector2Int gridEnd = WorldToGrid(end);

            // check if every cell in a line between start and en is walkable
            foreach (Vector2Int cell in GetCellsOnLine(gridStart, gridEnd))
            {
                if (_grid[cell.x, cell.y].IsFurniture) return true;

                if (_grid[cell.x, cell.y].penalty > 0) return true;
            }

            return false;
        }

        private IEnumerable<Vector2Int> GetCellsOnLine(Vector2Int start, Vector2Int end)
        {
            // Bresenham's line algorithm
            int x = start.x;
            int y = start.y;
            int dx = Mathf.Abs(end.x - start.x); // total horizontal distance
            int dy = Mathf.Abs(end.y - start.y); // total vertical disttance
            int sx = start.x < end.x ? 1 : -1;   // 1 if moving right, otherwise -1
            int sy = start.y < end.y ? 1 : -1;   // 1 if moving up, otherwise -1
            int err = dx - dy;

            while (true)
            {
                yield return new Vector2Int(x, y);
                if (x == end.x && y == end.y) break;
                int e2 = 2 * err;
                if (e2 > -dy) { err -= dy; x += sx; } //horizontal step
                if (e2 < dx) { err += dx; y += sy; } //vertical step
            }
        }

        #endregion

        public void SetAvatar(Transform points, RoomData data)
        {
            int column;
            int row;
            while (true)
            {
                column = Random.Range(0, data.SlotColumns - 1);
                row = Random.Range(0, data.SlotRows - 1);

                if (points.GetChild(row).GetChild(column).GetComponent<FurnitureSlot>().Furniture != null) continue;

                if (column + 1 < data.SlotColumns)
                {
                    if (points.GetChild(row).GetChild(column + 1).GetComponent<FurnitureSlot>().Furniture == null) break;
                }
                if (column - 1 >= 0)
                {
                    if (points.GetChild(row).GetChild(column - 1).GetComponent<FurnitureSlot>().Furniture == null)
                    {
                        column--;
                        break;
                    }
                }
            }
            //transform.SetParent(points.GetChild(row).GetChild(column), false);
            FurnitureSlot slot = points.GetChild(row).GetChild(column).GetComponent<FurnitureSlot>();
            _sortingGroup.sortingOrder = 6 + (slot.row) * 100;

            Vector2 position = slot.transform.position;

            float width = slot.width * 2;
            position.x += (width / 2) - slot.width / 2;
            position.y += -1 * (slot.height / 2);

            transform.position = position;
            _currentGridPosition = new Vector2Int(column, row);
            Status = AvatarStatus.Idle;
            _travelPoints.Clear();
        }


        private IEnumerator InteractAnimation()
        {
            if (_performingAnimation || _verifiedInteractClips.Count == 0)
            {
                yield break;
            }
            StopCoroutine(_statusCoroutine);

            int index = Random.Range(0, _verifiedInteractClips.Count);
            AnimationClip selectedClip = _verifiedInteractClips[index];

            _animator.Play(selectedClip.name);
            _performingAnimation = true;
            UseDefaultHands(true);
            yield return null;

            float duration = selectedClip.length;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            _performingAnimation = false;
            UseDefaultHands(false);

            SelectStatus();
        }


        private void UseDefaultHands(bool useDefaultHands)
        {
            if (_lHandResolver == null || _rHandResolver == null)
            {
                return;
            }

            string category = _lHandResolver.GetCategory();
            if (useDefaultHands)
            {
                _lHandResolver.SetCategoryAndLabel(category, "0000000L");
                _rHandResolver.SetCategoryAndLabel(category, "0000000R");
            }
            else
            {
                _lHandResolver.SetCategoryAndLabel(category, _lHandLabel);
                _rHandResolver.SetCategoryAndLabel(category, _rHandLabel);
            }
        }

        public void HandleClick()
        {
            if (_performingAnimation)
            {
                return;
            }

            if (_statusCoroutine != null)
            {
                StopCoroutine(_statusCoroutine);
            }

            StartCoroutine(InteractAnimation());
        }

        #if UNITY_EDITOR

        private void OnDrawGizmos()
        {
            if (_grid == null) return;

            //  Draw the Grid & Penalties
            for (int x = 0; x < _gridWidth; x++)
            {
                for (int y = 0; y < _gridHeight; y++)
                {
                    Vector2 worldPos = GridToWorld(new Vector2Int(x, y));
                    GridNode node = _grid[x, y];


                    if (node.penalty > 0)
                    {
                        // Yellow tint for penalties - gets darker as penalty increases
                        float alpha = Mathf.Clamp01(node.penalty / 30f);
                        Gizmos.color = new Color(1, 0.9f, 0, alpha * 0.4f);
                        Gizmos.DrawCube(worldPos, new Vector3(0.8f, 0.8f, 0.1f));

                        // Draw the penalty number
                        Handles.Label(worldPos, node.penalty.ToString());
                    }
                }
            }

            // Draw the Current Travel Path
            if (_travelPoints != null && _travelPoints.Count > 0)
            {
                Gizmos.color = Color.cyan;
                Vector2 lastPoint = transform.position; // Start the line from avatar's current position

                foreach (Vector2 point in _travelPoints)
                {
                    // Draw line to the next node
                    Gizmos.DrawLine(lastPoint, point);
                    // Draw a small sphere at the node center
                    Gizmos.DrawSphere(point, 0.1f);
                    lastPoint = point;
                }
            }

            // Draw what smoothpath would be
            //if (_smoothPath != null && _smoothPath.Count > 0)
            //{
            //    Gizmos.color = Color.magenta;
            //    for (int i = 0; i < _smoothPath.Count - 1; i++)
            //    {
            //        Gizmos.DrawLine(_smoothPath[i], _smoothPath[i + 1]);
            //        Gizmos.DrawSphere(_smoothPath[i], 0.1f);
            //    }
            //}

            // Draw rawpath
            if (_rawPath != null && _rawPath.Count > 0)
            {
                Gizmos.color = Color.yellow;
                for (int i = 0; i < _rawPath.Count - 1; i++)
                {
                    Gizmos.DrawLine(_rawPath[i], _rawPath[i + 1]);
                    Gizmos.DrawSphere(_rawPath[i], 0.1f);
                }
            }
        }
        #endif
    }
}
