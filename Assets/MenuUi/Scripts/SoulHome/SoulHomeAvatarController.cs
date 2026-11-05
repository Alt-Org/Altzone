

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.U2D.Animation;

namespace MenuUI.Scripts.SoulHome
{
    public class SoulHomeAvatarController : MonoBehaviour, ISoulHomeObjectClick
    {
        [Tooltip("Time between decisions is random between these, so the decisions of all characters are spread out.")]
        [SerializeField] private float _minDecisionTime = 5f;
        [SerializeField] private float _maxDecisionTime = 10f;
        [Header("Decision Weights")]
        [SerializeField] private float _stayWeight = 30f;
        [SerializeField] private float _walkWeight = 50f;
        [SerializeField] private float _changeRoomWeight = 20f;
        [Space]
        [SerializeField] private float _speed = 5;
        [SerializeField]
        private SortingGroup _sortingGroup;
        [SerializeField]
        private Animator _animator;
        [SerializeField]
        private AnimationClip _idleAnimation;
        [SerializeField]
        private AnimationClip _walkAnimation;
        [SerializeField]
        private AnimationClip _climbAnimation;
        [SerializeField]
        private List<AvatarAnimation> _interactAnimation;

        private List<AvatarAnimation> _validatedInteractAnimation;

        private bool _performingAnimation = false;

        private CharacterClassType _class = CharacterClassType.None;

        private static readonly Vector2Int LadderGridPosition = new(2, 1);

        private Transform _points;
        private RoomData _roomData;
        private List<Vector2> _travelPoints = new();

        private AvatarRig _rig;
        private SpriteResolver _lHandResolver;
        private SpriteResolver _rHandResolver;
        private string _lHandLabel;
        private string _rHandLabel;

        private bool _isMoving = false;
        private RoomData _pendingRoom;
        private Vector2Int _pendingTarget;
        private GridNode[,] _grid => _roomData.Grid;
        private Vector2Int _currentGridPosition;
        private int _gridWidth => _roomData.Grid.GetLength(0);
        private int _gridHeight => _roomData.Grid.GetLength(1);
        private List<Vector2> _rawPath;
        private bool hasInitialized = false;

        private List<Vector2> _smoothPath = new();

        public float NextDecisionTime { get; private set; }
        private bool IsBusy => _performingAnimation || _isMoving;

        void Start()
        {
            //_animator.keepAnimatorStateOnDisable = true;
            _animator.writeDefaultValuesOnDisable = true;

            if (!hasInitialized) return;

            _rig = GetComponentInChildren<AvatarRig>();
            if (_rig == null)
            {
                UnityEngine.Debug.LogError("Failed to get AvatarRig");
                return;
            }

            _lHandResolver = _rig.Resolvers[AvatarPart.L_Hand];
            _rHandResolver = _rig.Resolvers[AvatarPart.R_Hand];
            _lHandLabel = _lHandResolver.GetLabel();
            _rHandLabel = _rHandResolver.GetLabel();
            _validatedInteractAnimation = ValidateAnimations(_interactAnimation);
            StartIdle();
        }
        private void OnEnable()
        {
            if (!hasInitialized) return;

            if (_grid == null)
            {
                _roomData.UpdateGrid();
            }

            _performingAnimation = false;
            _pendingRoom = null;

            transform.position = GridToWorld(_currentGridPosition);
            UpdateSortingOrder(_currentGridPosition.y);
            StartIdle();
        }

        public void InitializeAvatar(PlayerData data)
        {
            _class = (CharacterClassType)BaseCharacter.GetClass(data.SelectedCharacterId);

            // Done here instead of Start, because Start doesn't run while the room is sleeping and the decisions still need the room.
            if (!transform.parent.CompareTag("Room")) return;
            SetRoom(transform.parent.GetComponent<RoomData>());
            SetAvatar(_points, _roomData);
            hasInitialized = true;
            // The first decisions are spread over the whole interval so the characters don't all decide at once.
            NextDecisionTime = Time.time + Random.Range(0f, _maxDecisionTime);
        }

        private void OnDisable()
        {
            StopAllCoroutines();
            _isMoving = false;
            _performingAnimation = false;
        }

        public void Decide()
        {
            NextDecisionTime = Time.time + Random.Range(_minDecisionTime, _maxDecisionTime);
            if (!hasInitialized) return;

            // Finish the room change that was cut off mid-climb before anything else
            if (_pendingRoom != null && !isActiveAndEnabled)
            {
                RoomData pendingRoom = _pendingRoom;
                _pendingRoom = null;
                MoveOffScreen(pendingRoom, _pendingTarget);
                return;
            }
            if (IsBusy) return;

            float roll = Random.Range(0f, _stayWeight + _walkWeight + _changeRoomWeight);
            if (roll < _stayWeight) return;

            RoomData targetRoom = roll < _stayWeight + _walkWeight ? _roomData : GetRoomToMoveTo();
            if (targetRoom == null) targetRoom = _roomData; //No room above or below, walk in this room instead.

            List<Vector2Int> slots = targetRoom.WalkableSlots;
            if (slots.Count == 0 && targetRoom == _roomData) return;
            Vector2Int target = slots.Count > 0 ? slots[Random.Range(0, slots.Count)] : LadderGridPosition;

            if (isActiveAndEnabled) StartMove(targetRoom, target);
            else MoveOffScreen(targetRoom, target);
        }

        private void StartMove(RoomData targetRoom, Vector2Int target)
        {
            bool changeRoom = targetRoom != _roomData;
            // A room change walks to the ladder first and continues to the target after climbing.
            if (!SetPath(changeRoom ? LadderGridPosition : target)) return;

            _isMoving = true;
            StartCoroutine(MoveRoutine(changeRoom ? targetRoom : null, target));
        }

        // Disabled characters don't walk, they jump straight to where the move would end.
        private void MoveOffScreen(RoomData room, Vector2Int target)
        {
            SetRoom(room);
            _currentGridPosition = target;
            transform.position = GridToWorld(target);
            if (gameObject.activeSelf == room.Sleeping) gameObject.SetActive(!room.Sleeping);
        }

        private void SetRoom(RoomData room)
        {
            if (transform.parent != room.transform) transform.SetParent(room.transform, true);
            _roomData = room;
            _points = room.transform.Find("FurniturePoints").Find("FloorFurniturePoints");
        }

        // Finds a path inside the current room and sets it as the travel points. Returns false if there is no path.
        private bool SetPath(Vector2Int target)
        {
            //Stopwatch stopwatch = Stopwatch.StartNew();
            List<GridNode> nodePath = FindPath(_currentGridPosition, target);
            if (nodePath == null) return false;

            _rawPath = GetTravelPoints(nodePath);
            _smoothPath = SmoothPath(_rawPath);
            _travelPoints = _smoothPath;
            //_travelPoints = GetTravelPoints(nodePath);
            _currentGridPosition = target;
            //stopwatch.Stop();
            //UnityEngine.Debug.Log($"calculating path took {stopwatch.Elapsed.TotalMilliseconds} milliseconds");
            return true;
        }

        // The path to the first target is already set. targetRoom is null when staying in the same room.
        private IEnumerator MoveRoutine(RoomData targetRoom, Vector2Int target)
        {
            yield return FollowPath();

            if (targetRoom != null)
            {
                yield return ClimbLadder(targetRoom, target);
                // Climbed into a room that's sleeping off-screen, sleep with it at the target. OnEnable continues from there when it wakes up.
                if (_roomData.Sleeping)
                {
                    _currentGridPosition = target;
                    _isMoving = false;
                    gameObject.SetActive(false);
                    yield break;
                }
                if (SetPath(target)) yield return FollowPath();
            }

            _isMoving = false;
            StartIdle();
        }

        private IEnumerator FollowPath()
        {
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

                    // offset is needed because when walking close to gridnode edge the closest grid node changes every step
                    float offset = _grid[0, 0].FurnitureSlot.height * 0.4f;
                    UpdateSortingOrder(WorldToGrid(new(transform.position.x, transform.position.y - offset)).y);

                    yield return null;
                }

                transform.position = targetPos;
                _travelPoints.RemoveAt(0);
            }
            UseDefaultHands(false);
        }

        private IEnumerator ClimbLadder(RoomData targetRoom, Vector2Int target)
        {
            _performingAnimation = true;
            // If the room goes to sleep mid-climb, the next decision finishes the room change.
            _pendingRoom = targetRoom;
            _pendingTarget = target;

            Transform targetRoomPoints = targetRoom.transform.Find("FurniturePoints").Find("FloorFurniturePoints");

            Vector3 targetWorldPosition = targetRoomPoints.GetChild(LadderGridPosition.y).GetChild(LadderGridPosition.x).position;

            bool movingDown = targetWorldPosition.y < transform.position.y;

            if (movingDown) _sortingGroup.sortingOrder = -1;

            _animator.Play(_climbAnimation.name);
            UseDefaultHands(true);
            yield return null;

            while (Vector2.Distance(transform.position, targetWorldPosition) > 0.05f)
            {
                transform.position = Vector2.MoveTowards(transform.position, targetWorldPosition, _speed * Time.deltaTime);

                yield return null;
            }

            _pendingRoom = null;
            SetRoom(targetRoom);

            transform.position = targetWorldPosition;
            _currentGridPosition = LadderGridPosition;
            UpdateSortingOrder(_currentGridPosition.y);
            _performingAnimation = false;
            UseDefaultHands(false);
        }

        // Returns a random room directly above or below, or null if there are none.
        private RoomData GetRoomToMoveTo()
        {
            Transform roomPosition = _roomData.transform.parent;
            Transform roomPositions = roomPosition.parent;
            int index = roomPosition.GetSiblingIndex();

            RoomData upperRoom = GetRoomAt(roomPositions, index - 1);
            RoomData lowerRoom = GetRoomAt(roomPositions, index + 1);

            if (upperRoom != null && lowerRoom != null) return Random.value > 0.5f ? upperRoom : lowerRoom;
            return upperRoom != null ? upperRoom : lowerRoom;
        }

        // Room positions above the clan's room count are left empty.
        private RoomData GetRoomAt(Transform roomPositions, int index)
        {
            if (index < 0 || index >= roomPositions.childCount) return null;
            Transform roomPosition = roomPositions.GetChild(index);
            return roomPosition.childCount > 0 ? roomPosition.GetChild(0).GetComponent<RoomData>() : null;
        }

        private void StartIdle()
        {
            _animator.Play(_idleAnimation.name);
            UseDefaultHands(false);
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

        private List<Vector2> SmoothPath(List<Vector2> fullPath)
        {
            if (fullPath.Count < 1) return fullPath;

            List<Vector2> smoothedPath = new();
            Vector2 currentPoint = transform.position; // point where los is checked from
            smoothedPath.Add(currentPoint);

            for (int i = 0; i < fullPath.Count - 1; i++)
            {


                // If furniture blocks direct path to future point
                if (IsSmoothPathTooExpensive(currentPoint, fullPath[i + 1]))
                {
                    Vector2 blockedPoint = fullPath[i + 1];
                    // Check backwards from the blocked point
                    for (int j = i; j >= 0; j--)
                    {
                        if (IsSmoothPathTooExpensive(blockedPoint, fullPath[j]))
                        {
                            currentPoint = fullPath[j + 1];
                            break;
                        }
                        currentPoint = fullPath[j];
                    }

                    Vector2Int gridPos = WorldToGrid(currentPoint);
                    GridNode node = _grid[gridPos.x, gridPos.y];

                    if (node.IsBackSlot)
                    {
                        currentPoint.y += node.FurnitureSlot.height * 0.5f;
                    }
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
            _travelPoints.Clear();
        }


        private IEnumerator InteractAnimation()
        {
            if (_performingAnimation || _validatedInteractAnimation.Count == 0)
            {
                yield break;
            }

            int index = Random.Range(0, _validatedInteractAnimation.Count);
            AnimationClip selectedClip = _validatedInteractAnimation[index].Clip;

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
            StartIdle();
        }
        private List<AvatarAnimation> ValidateAnimations(List<AvatarAnimation> animationToValidate)
        {
            List<AvatarAnimation> validatedAnimation = new();
            if (_class is CharacterClassType.None) _class = CharacterClassType.Trickster;
            foreach (AvatarAnimation animation in animationToValidate)
            {
                if (animation != null && animation.Clip != null)
                {
                    List<AnimationClip> clips = _animator.runtimeAnimatorController.animationClips.ToList();
                    if (!clips.Contains(animation.Clip)) continue;
                    if (animation.ValidClass == _class || animation.ValidClass is CharacterClassType.None) validatedAnimation.Add(animation);
                }
            }
            return validatedAnimation;
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
            if (_performingAnimation || _validatedInteractAnimation == null || _validatedInteractAnimation.Count == 0)
            {
                return;
            }

            if (_isMoving)
            {
                // Stops where it is, so the next path starts from here instead of the old target.
                _isMoving = false;
                _currentGridPosition = WorldToGrid(transform.position);
            }
            StopAllCoroutines();

            StartCoroutine(InteractAnimation());
        }

        #if UNITY_EDITOR

        private static GUIStyle _debugLabelStyle;

        /// <summary>
        /// Draws the character's state in the Scene view. Called by <see cref="SoulHomeLoad"/> instead of OnDrawGizmos,
        /// because OnDrawGizmos isn't called for the disabled characters in sleeping rooms.
        /// </summary>
        public void DrawDebugGizmos()
        {
            if (!hasInitialized || _roomData == null) return;

            string state;
            Color color;
            if (!gameObject.activeInHierarchy) { state = _roomData.Sleeping ? "Sleeping" : "Disabled"; color = Color.gray; }
            else if (_pendingRoom != null) { state = "Climbing"; color = Color.magenta; }
            else if (_performingAnimation) { state = "Animating"; color = Color.yellow; }
            else if (_isMoving) { state = "Walking"; color = Color.cyan; }
            else { state = "Idle"; color = Color.green; }

            Vector3 position = transform.position;
            Vector3 destination = GridToWorld(_currentGridPosition);

            Gizmos.color = color;
            Gizmos.DrawWireSphere(position, 0.5f);

            // Where the character is heading. For a disabled character this is where it shows up when its room wakes up.
            Gizmos.DrawWireCube(destination, new Vector3(0.6f, 0.6f, 0.1f));
            if (Vector3.Distance(position, destination) > 0.05f) Gizmos.DrawLine(position, destination);

            // Room change cut off mid-climb, finished by the next decision.
            if (_pendingRoom != null && !gameObject.activeInHierarchy)
            {
                Gizmos.color = Color.magenta;
                Transform pendingPoints = _pendingRoom.transform.Find("FurniturePoints").Find("FloorFurniturePoints");
                Gizmos.DrawLine(position, pendingPoints.GetChild(_pendingTarget.y).GetChild(_pendingTarget.x).position);
            }

            // Draw the Current Travel Path
            if (gameObject.activeInHierarchy && _travelPoints != null && _travelPoints.Count > 0)
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

            _debugLabelStyle ??= new GUIStyle(UnityEditor.EditorStyles.boldLabel);
            _debugLabelStyle.normal.textColor = color;
            float nextDecision = Mathf.Max(0f, NextDecisionTime - Time.time);
            UnityEditor.Handles.Label(position + Vector3.up,
                $"{state}\nNext: {nextDecision:0.0}s",
                _debugLabelStyle);
        }

        // The grid and penalties of the character's room, only for the selected character so the rooms don't fill up with numbers.
        private void OnDrawGizmosSelected()
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
                        if (node.IsBackSlot) Gizmos.color = new Color(1, 0, 0, alpha * 0.4f);
                        Gizmos.DrawCube(worldPos, new Vector3(0.8f, 0.8f, 0.1f));

                        // Draw the penalty number
                        UnityEditor.Handles.Label(worldPos, node.penalty.ToString());
                    }
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
