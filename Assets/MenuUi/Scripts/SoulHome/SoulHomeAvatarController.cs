using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

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
        [SerializeField]
        private SortingGroup _sortingGroup;
        [SerializeField]
        private Animator _animator;
        [SerializeField]
        private AnimationClip _idleAnimation;
        [SerializeField]
        private AnimationClip _walkAnimation;
        [SerializeField]
        private AnimationClip _waveAnimation;

        private bool _performingAnimation = false;

        private AvatarStatus _status;
        private bool _idleTimerStarted = false;
        private Vector2Int _newPosition;
        private Vector2 _currentCalculatedPosition;

        private Transform _points;
        private RoomData _roomData;
        private List<Vector2> _travelPoints = new();

        // Start is called before the first frame update
        void Start()
        {
            if (transform.parent.CompareTag("Room"))
            {
                _points = transform.parent.Find("FurniturePoints").Find("FloorFurniturePoints");
                _roomData = transform.parent.GetComponent<RoomData>();
                SetAvatar(_points, _roomData);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if(_status == AvatarStatus.Idle && !_idleTimerStarted)
            {
                //Debug.Log("Character Idle");
                StartCoroutine("IdleTimer");
            }
            else if(_status == AvatarStatus.Wander)
            {
                if (_performingAnimation) return;
                //Debug.Log("Character Wander");
                if (!_animator.GetCurrentAnimatorStateInfo(0).IsName(_walkAnimation.name)) _animator.Play(_walkAnimation.name);
                MoveAvatar();
                //transform.SetParent(_points.GetChild(_newPosition.y).GetChild(_newPosition.x), false);

                CheckPositionDepth();

                if(GetDirection() == Vector2.zero) _status = AvatarStatus.Idle;
            }
        }

        private IEnumerator IdleTimer()
        {
            _idleTimerStarted = true;
            float idleTimer = 0;
            bool firstFrame = true;
            float checkTimer = 0;
            if (!_animator.GetCurrentAnimatorStateInfo(0).IsName(_idleAnimation.name)) _animator.Play(_idleAnimation.name);
            while (true)
            {
                if (firstFrame) firstFrame = false;
                else
                {
                        yield return new WaitUntil(() => !_performingAnimation);
                        idleTimer += Time.deltaTime;
                        checkTimer += Time.deltaTime;
                }


                if(idleTimer >= _minIdleTimer && checkTimer >= 0.5f)
                {
                    checkTimer = 0;
                    int randomValue = Random.Range(0, 2);
                    if (randomValue == 0)
                    {
                        SelectNewPosition();
                        //CalculatePath();
                        break;
                    }
                }
                if(idleTimer >= _maxIdleTimer)
                {
                    SelectNewPosition();
                    break;
                }
                yield return null;
            }
            _idleTimerStarted = false;
        }


        public void SetAvatar(Transform points, RoomData data)
        {
            int column;
            int row;
            while (true)
            {
                column = Random.Range(0, data.SlotColumns - 1);
                row = Random.Range(0, data.SlotRows - 1);

                if (points.GetChild(row).GetChild(column).GetComponent<FurnitureSlot>().Furniture != null) continue;

                if(column + 1 < data.SlotColumns)
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

            float width = slot.width*2;
            position.x += (width / 2) - slot.width / 2;
            position.y += -1 * (slot.height / 2);

            transform.position = position;

            _status = AvatarStatus.Idle;
            _travelPoints.Clear();
        }

        private void SelectNewPosition()
        {
            _travelPoints.Clear();
            int column;
            int row;
            while (true) {
            while (true)
            {
                column = Random.Range(0, _roomData.SlotColumns - 1);
                row = Random.Range(0, _roomData.SlotRows - 1);

                if (_points.GetChild(row).GetChild(column).GetComponent<FurnitureSlot>().Furniture != null) continue;

                if (column + 1 < _roomData.SlotColumns)
                {
                    if (_points.GetChild(row).GetChild(column + 1).GetComponent<FurnitureSlot>().Furniture == null) break;
                }
                if (column - 1 >= 0)
                {
                    if (_points.GetChild(row).GetChild(column - 1).GetComponent<FurnitureSlot>().Furniture == null)
                    {
                        column--;
                        break;
                    }
                }
            }
            _newPosition = new(column,row);
            _currentCalculatedPosition = transform.position;
            bool check = CalculatePath();
            if (!check) continue;
            else break;
            }
            _travelPoints.Add(GetEndPosition());
            _status = AvatarStatus.Wander;
            //Debug.Log(Time.time+" : "+_travelPoints.ToString());
        }

        private bool CalculatePath(Furniture prevFurniture = null)
        {
            Vector2 path = GetDirection(_currentCalculatedPosition);
            //if (Mathf.Abs(path.normalized.y) == 0) return true;
            Vector2 endPosition = GetEndPosition();
            //Debug.Log("Calculate Path: Origin: "+ (_currentCalculatedPosition + path.normalized * 0.01f )+ ", Direction: "+ path.normalized + ", Magnitude: "+ (path.magnitude - (0.01f * path.normalized).magnitude)+ ", EndPoint: " + (endPosition));
            RaycastHit2D[] hits;
            if (path.y == 0) hits = Physics2D.RaycastAll(_currentCalculatedPosition + new Vector2(0, 0.01f) + path.normalized * 0.01f, path.normalized, path.magnitude - (0.01f * path.normalized).magnitude);
            else hits = Physics2D.RaycastAll(_currentCalculatedPosition+ path.normalized * 0.01f, path.normalized, path.magnitude - (0.01f*path.normalized).magnitude);
            foreach(RaycastHit2D hit in hits)
            {
                FurnitureSlot slot = hit.collider.GetComponent<FurnitureSlot>();
                if (slot != null)
                {
                    if ((Mathf.Approximately(endPosition.x, hit.point.x) && !Mathf.Approximately(path.y, 0)) || (Mathf.Approximately(endPosition.y, hit.point.y) && !Mathf.Approximately(path.x, 0))) continue;
                    //Debug.Log("Collider Hit:"+ Time.time+" Row: "+ slot.row+" Column: "+ slot.column+" Furniture: "+slot.Furniture?.Name);
                    if(slot.Furniture != null && !slot.Furniture.Equals(prevFurniture))
                    {
                        prevFurniture = hit.collider.GetComponent<FurnitureSlot>().Furniture;
                        //Debug.Log(Time.time + " Furniture: " +hit.collider.GetComponent<FurnitureSlot>().Furniture);

                        if(Mathf.Approximately(path.y, 0))
                        {
                            FurnitureSlot underSlot = _points.GetChild(slot.row + 1).GetChild(slot.column).GetComponent<FurnitureSlot>();
                            if (!slot.Furniture.Equals(underSlot.Furniture)) continue;
                        }

                        Vector2 position = CheckCollision(hit, path.normalized);
                        if (position.Equals(Vector2.negativeInfinity)) return false;
                        _currentCalculatedPosition = position;
                        _currentCalculatedPosition = CheckDirection(path, hit);
                        return CalculatePath(prevFurniture);
                    }
                }
            }
            return true;
        }

        private void MoveAvatar()
        {
            Vector2 position = transform.position;
            Vector2 direction = GetDirection(position,_travelPoints[0]);
            if(direction.x < 0) transform.rotation = Quaternion.Euler(new(0, 180, 0));
            else transform.rotation = Quaternion.Euler(Vector3.zero);
            if (Vector2.Distance(position, position + direction.normalized * _speed * Time.deltaTime) > Vector2.Distance(position, _travelPoints[0])) position = _travelPoints[0];
            else position += direction.normalized * _speed * Time.deltaTime;
            transform.position = position;
            if(GetDirection(position, _travelPoints[0]).magnitude < 0.01f) _travelPoints.RemoveAt(0);
            if(_travelPoints.Count == 0) _status = AvatarStatus.Idle;
        }

        private Vector2 GetDirection()
        {
            FurnitureSlot slot = _points.GetChild(_newPosition.y).GetChild(_newPosition.x).GetComponent<FurnitureSlot>();
            Vector2 destination = slot.transform.position;
            float width = slot.width * 2;
            destination.x += (width / 2) - slot.width / 2;
            destination.y += -1 * (slot.height / 2);
            Vector2 direction = (destination - (Vector2)transform.position);
            return direction;
        }

        private Vector2 GetDirection(Vector2 startPosition)
        {
            Vector2 endPosition = GetEndPosition();
            //Debug.Log("StartPosition: " + startPosition + "EndPosition: " + endPosition);
            Vector2 direction = (endPosition - startPosition);
            return direction;
        }

        private Vector2 GetDirection(Vector2 startPosition, Vector2 endPosition)
        {
            if(endPosition.Equals(Vector2.negativeInfinity)) endPosition = GetEndPosition();
            Vector2 direction = (endPosition - startPosition);
            return direction;
        }

        private Vector2 GetEndPosition()
        {
            FurnitureSlot slot = _points.GetChild(_newPosition.y).GetChild(_newPosition.x).GetComponent<FurnitureSlot>();
            Vector2 destination = slot.transform.position;
            float width = slot.width * 2;
            destination.x += (width / 2) - slot.width / 2;
            destination.y += -1 * (slot.height / 2);
            return destination;
        }

        private Vector2 GetPath()
        {
            Vector2 currentCalculatedPosition = transform.position;
            Vector2 path = GetDirection();
            path = GetDirection(currentCalculatedPosition+ path.normalized*0.01f);
            return path;
        }

        private Vector2 CheckCollision(RaycastHit2D hit, Vector2 direction)
        {
            Vector2 normal = hit.normal;
            FurnitureSlot slot = hit.collider.GetComponent<FurnitureSlot>();
            int row = slot.row;
            int column = slot.column;
            if (normal == Vector2.down || normal == Vector2.up)
            {
                if (direction.x >= 0)
                {
                    int i = 0;
                    while (true)
                    {
                        if (column + i < transform.parent.GetComponent<RoomData>().SlotColumns - 1)
                        {
                            if (_points.GetChild(row).GetChild(column + i + 1).GetComponent<FurnitureSlot>().Furniture == slot.Furniture)
                            {
                                i++;
                                continue;
                            }
                            else
                            {
                                return FindSlotCorner(row, column + i, normal, direction, hit, false);
                            }
                        }
                        else
                        {
                            i = 0;
                            break;
                        }
                    }
                    while (true)
                    {
                        if (column + i > 0)
                        {
                            if (_points.GetChild(row).GetChild(column + i -1).GetComponent<FurnitureSlot>().Furniture == slot.Furniture)
                            {
                                i--;
                                continue;
                            }
                            else
                            {
                                return FindSlotCorner(row, column + i, normal, direction, hit, true);
                            }
                        }
                        else
                        {
                            i = 0;
                            break;
                        }
                    }
                }
                else
                {
                    int i = 0;
                    while (true)
                    {
                        if (column + i > 0)
                        {
                            if (_points.GetChild(row).GetChild(column + i -1).GetComponent<FurnitureSlot>().Furniture == slot.Furniture)
                            {
                                i--;
                                continue;
                            }
                            else
                            {
                                return FindSlotCorner(row, column + i, normal, direction, hit, false);
                            }
                        }
                        else
                        {
                            i = 0;
                            break;
                        }
                    }
                    while (true)
                    {
                        if (column + i < transform.parent.GetComponent<RoomData>().SlotColumns - 1)
                        {
                            if (_points.GetChild(row).GetChild(column + i + 1).GetComponent<FurnitureSlot>().Furniture == slot.Furniture)
                            {
                                i++;
                                continue;
                            }
                            else
                            {
                                return FindSlotCorner(row, column + i, normal, direction, hit, true);
                            }
                        }
                        else
                        {
                            i = 0;
                            break;
                        }
                    }
                }
            }
            else if (normal == Vector2.left || normal == Vector2.right)
            {
                if (direction.y <= 0)
                {
                    int i = 0;
                    while (true)
                    {
                        if (row + i < transform.parent.GetComponent<RoomData>().SlotRows - 1)
                        {
                            if (_points.GetChild(row + i + 1).GetChild(column).GetComponent<FurnitureSlot>().Furniture == slot.Furniture)
                            {
                                i++;
                                continue;
                            }
                            else
                            {
                                return FindSlotCorner(row + i, column, normal, direction, hit, false);
                            }
                        }
                        else
                        {
                            i = 0;
                            break;
                        }
                    }
                    while (true)
                    {
                        if (row + i > 0)
                        {
                            if (_points.GetChild(row + i -1).GetChild(column).GetComponent<FurnitureSlot>().Furniture == slot.Furniture)
                            {
                                i--;
                                continue;
                            }
                            else
                            {
                                return FindSlotCorner(row + i, column, normal, direction, hit, true);
                            }
                        }
                        else
                        {
                            i = 0;
                            break;
                        }
                    }
                }
                else
                {
                    int i = 0;
                    while (true)
                    {
                        if (row + i > 0)
                        {
                            if (_points.GetChild(row + i - 1).GetChild(column).GetComponent<FurnitureSlot>().Furniture == slot.Furniture)
                            {
                                i--;
                                continue;
                            }
                            else
                            {
                                return FindSlotCorner(row + i, column, normal, direction, hit, false);
                            }
                        }
                        else
                        {
                            i = 0;
                            break;
                        }
                    }
                    while (true)
                    {
                        if (row + i < transform.parent.GetComponent<RoomData>().SlotRows - 1)
                        {
                            if (_points.GetChild(row + i + 1).GetChild(column).GetComponent<FurnitureSlot>().Furniture == slot.Furniture)
                            {
                                i++;
                                continue;
                            }
                            else
                            {
                                return FindSlotCorner(row+i, column, normal, direction, hit, true);
                            }
                        }
                        else
                        {
                            i = 0;
                            break;
                        }
                    }
                }
            }
            return Vector2.negativeInfinity;
        }

        private Vector2 FindSlotCorner(int row, int column, Vector2 normal, Vector2 direction, RaycastHit2D hit, bool reverse)
        {
            FurnitureSlot newSlot = _points.GetChild(row).GetChild(column).GetComponent<FurnitureSlot>();
            Vector2 destination = newSlot.transform.position;
            float width = newSlot.width;
            if (normal == Vector2.left || ((normal == Vector2.down|| normal == Vector2.up) && ((direction.x < 0 && !reverse)|| (direction.x >= 0 && reverse))))
                destination.x -= width / 2;
            else if (normal == Vector2.right || ((normal == Vector2.down || normal == Vector2.up) && ((direction.x < 0 && reverse) || (direction.x >= 0 && !reverse))))
                destination.x += width / 2;
            if (normal == Vector2.down || ((normal == Vector2.right || normal == Vector2.left) && ((direction.y <= 0 && !reverse) || (direction.y > 0 && reverse))))
                destination.y -= (newSlot.height / 2);
            else if (normal == Vector2.up || ((normal == Vector2.right || normal == Vector2.left) && ((direction.y <= 0 && reverse) || (direction.y > 0 && !reverse))))
                destination.y += (newSlot.height / 2);

            CheckNewPosition(destination, hit);
            return destination;
        }

        private void CheckNewPosition(Vector2 destination, RaycastHit2D hit)
        {
            bool hitFound = false;
            Vector2 path = GetDirection(_currentCalculatedPosition, destination);
            RaycastHit2D[] hits = Physics2D.RaycastAll(_currentCalculatedPosition + path.normalized * 0.01f, path.normalized, path.magnitude - (0.01f * path.normalized).magnitude);
            foreach (RaycastHit2D hit2 in hits)
            {
                if (hit2.collider.GetComponent<FurnitureSlot>() != null)
                {
                    if ((Mathf.Approximately(hit2.point.x, hit.point.x) && !Mathf.Approximately(path.y, 0)) || (Mathf.Approximately(hit2.point.y, hit.point.y) && !Mathf.Approximately(path.x, 0))) continue;
                    if (hit2.collider.GetComponent<FurnitureSlot>().Furniture != null
                        && !hit2.collider.GetComponent<FurnitureSlot>().Furniture.Equals(hit.collider.GetComponent<FurnitureSlot>().Furniture))
                    {

                        hitFound = true;
                        break;
                    }
                }
            }
            if (!hitFound)
            {
                _travelPoints.Add(destination);
            }
            else
            {
                _travelPoints.Add(hit.point);
                _travelPoints.Add(destination);
            }
        }

        private Vector2 CheckDirection(Vector2 prevDirection, RaycastHit2D hit)
        {
            Vector2 newDirection = GetDirection(_currentCalculatedPosition);
            if(hit.normal == Vector2.down || hit.normal == Vector2.up)
            {
                if (newDirection.x * prevDirection.x < 0)
                {
                    Vector2Int size = hit.collider.GetComponent<FurnitureSlot>().Furniture.GetFurnitureSize();
                    FurnitureSlot newSlot;
                    int slotOffset = (int)((_currentCalculatedPosition.x - hit.point.x) / hit.collider.GetComponent<FurnitureSlot>().width);
                    if (hit.normal == Vector2.down)
                    newSlot = _points.GetChild(hit.collider.GetComponent<FurnitureSlot>().row - (size.y-1)).GetChild(hit.collider.GetComponent<FurnitureSlot>().column + slotOffset).GetComponent<FurnitureSlot>();
                    else newSlot = _points.GetChild(hit.collider.GetComponent<FurnitureSlot>().row + (size.y - 1)).GetChild(hit.collider.GetComponent<FurnitureSlot>().column + slotOffset).GetComponent<FurnitureSlot>();
                    Vector2 destination = newSlot.transform.position;
                    if (hit.point.x-_currentCalculatedPosition.x >= 0)
                        destination.x -= newSlot.width / 2;
                    else if (hit.point.x - _currentCalculatedPosition.x < 0)
                        destination.x += newSlot.width / 2;
                    if (hit.normal == Vector2.down )
                        destination.y -= (newSlot.height / 2);
                    else if (hit.normal == Vector2.up )
                        destination.y += (newSlot.height / 2);

                    _travelPoints.Add(destination);
                    return destination;
                }
                else return _currentCalculatedPosition;
            }
            else if (hit.normal == Vector2.left || hit.normal == Vector2.right)
            {
                if (newDirection.y * prevDirection.y < 0)
                {
                    Vector2Int size = hit.collider.GetComponent<FurnitureSlot>().Furniture.GetFurnitureSize();
                    FurnitureSlot newSlot;
                    int slotOffset = (int)((hit.point.y - _currentCalculatedPosition.y) / hit.collider.GetComponent<FurnitureSlot>().height);
                    if (hit.normal == Vector2.left)
                        newSlot = _points.GetChild(hit.collider.GetComponent<FurnitureSlot>().row + slotOffset).GetChild(hit.collider.GetComponent<FurnitureSlot>().column + (size.x - 1)).GetComponent<FurnitureSlot>();
                    else newSlot = _points.GetChild(hit.collider.GetComponent<FurnitureSlot>().row + slotOffset).GetChild(hit.collider.GetComponent<FurnitureSlot>().column - (size.x - 1)).GetComponent<FurnitureSlot>();
                    Vector2 destination = newSlot.transform.position;
                    if (hit.normal == Vector2.left)
                        destination.x -= newSlot.width / 2;
                    else if (hit.normal == Vector2.right)
                        destination.x += newSlot.width / 2;
                    if (hit.point.y - _currentCalculatedPosition.y >= 0)
                        destination.y -= (newSlot.height / 2);
                    else if (hit.point.y - _currentCalculatedPosition.y < 0)
                        destination.y += (newSlot.height / 2);

                    _travelPoints.Add(destination);
                    return destination;
                }
                else return _currentCalculatedPosition;
            }
            else return _currentCalculatedPosition;
        }

        private void CheckPositionDepth()
        {
            Vector2 hitPoint = transform.position + new Vector3(0, -0.01f);

            Vector3 origin = new(hitPoint.x, hitPoint.y, 1);
            Ray ray = new(origin, (Vector3)hitPoint - origin);
            RaycastHit2D[] hitArray;
            hitArray = Physics2D.GetRayIntersectionAll(ray, 10);
            foreach (RaycastHit2D hit2 in hitArray)
            {
                if (hit2.collider.gameObject.GetComponent<FurnitureSlot>() != null)
                {
                    _sortingGroup.sortingOrder = 6 + (hit2.collider.gameObject.GetComponent<FurnitureSlot>().row) * 100;
                    return;
                }
            }
            hitPoint = transform.position + new Vector3(0, 0.01f);
            origin = new(hitPoint.x, hitPoint.y, 1);
            ray = new(origin, (Vector3)hitPoint - origin);
            hitArray = Physics2D.GetRayIntersectionAll(ray, 10);
            foreach (RaycastHit2D hit in hitArray)
            {
                if (hit.collider.gameObject.GetComponent<FurnitureSlot>() != null)
                {
                    _sortingGroup.sortingOrder = 6 + (hit.collider.gameObject.GetComponent<FurnitureSlot>().row+1) * 100;
                }
            }
        }

        private IEnumerator WaveAnimation()
        {
            if (!_performingAnimation)
            {
                _animator.Play(_waveAnimation.name);
                _performingAnimation = true;
                yield return new WaitUntil(() => !_animator.GetCurrentAnimatorStateInfo(0).IsName(_waveAnimation.name) && !_animator.IsInTransition(0));
                _performingAnimation = false;
            }
        }

        public void HandleClick()
        {
            StartCoroutine(WaveAnimation());
        }
    }
}
