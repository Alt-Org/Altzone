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
                Debug.Log("Idle");
                StartCoroutine("IdleTimer");
            }
            else if(_status == AvatarStatus.Wander)
            {
                Debug.Log("Wander");
                transform.SetParent(_points.GetChild(_newPosition.y).GetChild(_newPosition.x), false);

                transform.Find("Sprite").GetComponent<SpriteRenderer>().sortingOrder = 3 + transform.parent.GetComponent<FurnitureSlot>().row * 2;
                Vector2 position = Vector2.zero;

                float width = transform.parent.GetComponent<FurnitureSlot>().width * 2;
                position.x = (width / 2) - transform.parent.GetComponent<FurnitureSlot>().width / 2;
                position.y = -1 * (transform.parent.GetComponent<FurnitureSlot>().height / 2);
                transform.localPosition = position;

                _status = AvatarStatus.Idle;
            }
        }

        private IEnumerator IdleTimer()
        {
            _idleTimerStarted = true;
            float idleTimer = 0;
            bool firstFrame = true;
            while (true)
            {
                if (firstFrame) firstFrame = false;
                else idleTimer += Time.deltaTime;


                if(idleTimer >= _minIdleTimer)
                {
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
            transform.SetParent(points.GetChild(row).GetChild(column), false);
            transform.Find("Sprite").GetComponent<SpriteRenderer>().sortingOrder = 3 + transform.parent.GetComponent<FurnitureSlot>().row * 2;

            Vector2 position = Vector2.zero;

            float width = transform.parent.GetComponent<FurnitureSlot>().width*2;
            position.x = (width / 2) - transform.parent.GetComponent<FurnitureSlot>().width / 2;
            position.y = -1 * (transform.parent.GetComponent<FurnitureSlot>().height / 2);
            transform.localPosition = position;

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
    }
}
