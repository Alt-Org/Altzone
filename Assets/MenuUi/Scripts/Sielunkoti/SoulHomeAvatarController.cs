using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MenuUI.Scripts.SoulHome
{
    public enum AvatarStatus
    {
        Idle,
        Wander
    }

    public class SoulHomeAvatarController : MonoBehaviour
    {
        [SerializeField] private float _minIdleTimer = 2f;
        [SerializeField] private float _maxIdleTimer = 4f;
        [SerializeField] private float _speed = 5;

        private AvatarStatus _status;
        private bool _idleTimerStarted = false;
        private Vector2Int _newPosition;

        private Transform _points;
        private RoomData _roomData;

        // Start is called before the first frame update
        void Start()
        {
            if (transform.parent.CompareTag("Room"))
            {
                _points = transform.parent.Find("FurniturePoints");
                _roomData = transform.parent.GetComponent<RoomData>();
                SetAvatar(_points, _roomData);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if(_status == AvatarStatus.Idle && !_idleTimerStarted)
            {
                Debug.Log("Character Idle");
                StartCoroutine("IdleTimer");
            }
            else if(_status == AvatarStatus.Wander)
            {
                Debug.Log("Character Wander");

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
            while (true)
            {
                if (firstFrame) firstFrame = false;
                else
                {
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
            transform.Find("Sprite").GetComponent<SpriteRenderer>().sortingOrder = 3 + slot.row * 2;

            Vector2 position = slot.transform.position;

            float width = slot.width*2;
            position.x += (width / 2) - slot.width / 2;
            position.y += -1 * (slot.height / 2);

            transform.position = position;

            _status = AvatarStatus.Idle;
        }

        private void SelectNewPosition()
        {
            int column;
            int row;
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
            _status = AvatarStatus.Wander;
        }
        private void MoveAvatar()
        {
            Vector2 direction = GetDirection();
            Vector2 position = transform.position;
            position += direction * _speed * Time.deltaTime;
            transform.position = position;
        }

        private Vector2 GetDirection()
        {
            FurnitureSlot slot = _points.GetChild(_newPosition.y).GetChild(_newPosition.x).GetComponent<FurnitureSlot>();
            Vector2 destination = slot.transform.position;
            float width = slot.width * 2;
            destination.x += (width / 2) - slot.width / 2;
            destination.y += -1 * (slot.height / 2);
            Vector2 direction = (destination - (Vector2)transform.position).normalized;
            return direction;
        }

        private void CheckPositionDepth()
        {
            //Vector2 checkPoint;
            //Vector2Int size = _selectedFurniture.GetComponent<FurnitureHandling>().GetFurnitureSize();

            Vector2 hitPoint = transform.position + new Vector3(0, -0.001f);

            //checkPoint = hitPoint + new Vector2((transform.localScale.x / 2) * -1 + transform.localScale.x / (2 * size.x), 0);
            Vector3 origin = new(hitPoint.x, hitPoint.y, 1);
            Ray ray = new(origin, (Vector3)hitPoint - origin);
            RaycastHit2D[] hitArray;
            hitArray = Physics2D.GetRayIntersectionAll(ray, 10);
            foreach (RaycastHit2D hit2 in hitArray)
            {
                if (hit2.collider.gameObject.GetComponent<FurnitureSlot>() != null)
                {
                    transform.Find("Sprite").GetComponent<SpriteRenderer>().sortingOrder = 3 + (hit2.collider.gameObject.GetComponent<FurnitureSlot>().row - 1) * 2;
                    return;
                }
            }
            hitPoint = transform.position + new Vector3(0, 0.001f);
            origin = new(hitPoint.x, hitPoint.y, 1);
            ray = new(origin, (Vector3)hitPoint - origin);
            hitArray = Physics2D.GetRayIntersectionAll(ray, 10);
            foreach (RaycastHit2D hit in hitArray)
            {
                if (hit.collider.gameObject.GetComponent<FurnitureSlot>() != null)
                {
                    transform.Find("Sprite").GetComponent<SpriteRenderer>().sortingOrder = 3 + hit.collider.gameObject.GetComponent<FurnitureSlot>().row * 2;
                }
            }
        }
    }
}
