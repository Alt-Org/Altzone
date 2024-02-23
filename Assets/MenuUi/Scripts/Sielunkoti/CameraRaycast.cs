using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace MenuUI.Scripts.SoulHome
{
    public class CameraRaycast : MonoBehaviour
    {
        private Camera Camera;
        private Vector3 prevWideCameraPos = new(0,0);
        private float prevWideCameraFoV;
        private GameObject selectedRoom = null;
        [SerializeField]
        private float scrollSpeed = 2f;
        [SerializeField]
        private SpriteRenderer backgroundSprite;
        [SerializeField]
        private bool isometric = false;
        [SerializeField]
        private SoulHomeController _soulHomeController;
        [SerializeField]
        private RawImage _displayScreen;

        private Bounds cameraBounds;
        private float cameraMinX;
        private float cameraMinY;
        private float cameraMaxX;
        private float cameraMaxY;

        private Vector2 prevp;

        private float outDelay = 0;
        private float inDelay = 0;
        // Start is called before the first frame update
        void Start()
        {
            Camera = GetComponent<Camera>();
            cameraBounds = backgroundSprite.bounds;
            cameraMinX = cameraBounds.min.x;
            cameraMinY = cameraBounds.min.y;
            cameraMaxX = cameraBounds.max.x;
            cameraMaxY = cameraBounds.max.y;
            Debug.Log(_displayScreen.GetComponent<RectTransform>().rect.x /*.sizeDelta.x*/ +" : "+ _displayScreen.GetComponent<RectTransform>().rect.y /*.sizeDelta.y*/);
            //Camera.aspect = _displayScreen.GetComponent<RectTransform>().sizeDelta.x / _displayScreen.GetComponent<RectTransform>().sizeDelta.y;
            Camera.aspect = _displayScreen.GetComponent<RectTransform>().rect.x / _displayScreen.GetComponent<RectTransform>().rect.y;

        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetMouseButton(0) || Input.touchCount == 1)
            {
                if (selectedRoom == null)
                {
                    Vector3 bl = Camera.ViewportToWorldPoint(new Vector3(0, 0, Mathf.Abs(Camera.transform.position.z)));
                    Vector3 tr = Camera.ViewportToWorldPoint(new Vector3(1, 1, Mathf.Abs(Camera.transform.position.z)));
                    float currentY = transform.position.y;
                    float offsetY = Mathf.Abs(currentY-bl.y);
                    float targetY;
                    if (Input.touchCount == 1)
                    {
                        Touch touch = Input.GetTouch(0);
                        if (touch.phase == TouchPhase.Began) prevp = touch.position;
                        Vector2 lp = touch.position;
                        targetY = currentY + (prevp.y - lp.y) / scrollSpeed;
                        Debug.Log("Touch: Y: "+(prevp.y - lp.y));
                        prevp = touch.position;
                        if (touch.phase == TouchPhase.Ended) prevp = Vector2.zero;
                    }
                    else
                    {
                        float moveAmountY = Input.GetAxis("Mouse Y");
                        targetY = currentY - moveAmountY * scrollSpeed;
                    }

                    float y = Mathf.Clamp(targetY, cameraMinY+offsetY, cameraMaxY - offsetY);

                    float currentX = transform.position.x;
                    float offsetX = Mathf.Abs(currentX - bl.x);
                    float moveAmountX = Input.GetAxis("Mouse X");
                    float targetX = currentX - moveAmountX * scrollSpeed;

                    float x = Mathf.Clamp(targetX, cameraMinX + offsetX, cameraMaxX - offsetX);
                    transform.position = new(x, y, transform.position.z);
                }
                else
                {
                    Bounds roomCameraBounds = selectedRoom.GetComponent<BoxCollider2D>().bounds;
                    float roomCameraMinX = roomCameraBounds.min.x;
                    float roomCameraMinY = roomCameraBounds.min.y;
                    float roomCameraMaxX = roomCameraBounds.max.x;
                    float roomCameraMaxY = roomCameraBounds.max.y;
                    Vector3 bl = Camera.ViewportToWorldPoint(new Vector3(0, 0, Mathf.Abs(Camera.transform.position.z)));
                    Vector3 tr = Camera.ViewportToWorldPoint(new Vector3(1, 1, Mathf.Abs(Camera.transform.position.z)));
                    float currentX = transform.position.x;
                    float offsetX = Mathf.Abs(currentX - bl.x);
                    float targetX;
                    if (Input.touchCount == 1)
                    {
                        Touch touch = Input.GetTouch(0);
                        if (touch.phase == TouchPhase.Began) prevp = touch.position;
                        Vector2 lp = touch.position;
                        targetX = currentX + (prevp.x - lp.x) / scrollSpeed;
                        Debug.Log("Touch: X: " + (prevp.x - lp.x));
                        prevp = touch.position;
                        if (touch.phase == TouchPhase.Ended) prevp = Vector2.zero;
                    }
                    else
                    {
                        float moveAmountY = Input.GetAxis("Mouse X");
                        targetX = currentX - moveAmountY * scrollSpeed;
                    }

                    float x = Mathf.Clamp(targetX, roomCameraMinX + offsetX, roomCameraMaxX - offsetX);
                    transform.position = new(x, transform.position.y, transform.position.z);
                }
            }
        }

        public bool FindRayPoint(Vector2 relPoint)
        {
            Ray ray = Camera.ViewportPointToRay(relPoint);
            RaycastHit2D[] hit;
            Debug.Log("Camera2: " + ray);
            hit = Physics2D.GetRayIntersectionAll(ray, 1000);
            bool hitRoom = false;
            bool enterRoom = false;
            foreach (RaycastHit2D hit2 in hit)
            {
                if (hit2.collider != null)
                {
                    if (hit2.collider.gameObject.tag == "ScrollRectCanvas") continue;

                    if (hit2.collider.gameObject.tag == "Room")
                    {
                        Debug.Log("Camera2: " + hit2.collider.gameObject.name);
                        Vector3 hitPoint = hit2.transform.InverseTransformPoint(hit2.point);
                        Debug.Log("Camera2: " + hitPoint);
                        GameObject roomObject;
                        if (isometric)
                            roomObject = hit2.collider.gameObject.transform.parent.parent.gameObject;
                        else
                            roomObject = hit2.collider.gameObject;
                        if (selectedRoom == null)
                        {
                            selectedRoom = roomObject;
                            ZoomIn(roomObject);
                            //enterRoom = true;
                        }
                        else if (selectedRoom != roomObject)
                        {
                            ZoomOut();
                        }
                        hitRoom = true;
                    }
                }

            }
            if (!hitRoom && selectedRoom != null)
            {
                ZoomOut();
            }
            return enterRoom;
        }

        public void ZoomIn(GameObject room)
        {
            if (inDelay + 1f < Time.time)
            {
                _soulHomeController.SetRoomName(selectedRoom);
                prevWideCameraPos = Camera.transform.position;
                prevWideCameraFoV = Camera.fieldOfView;
                Camera.transform.position = new(room.transform.position.x, room.transform.position.y + 7.5f, -25);
                Camera.fieldOfView = 60;
                outDelay = Time.time;

                //_displayScreen.GetComponent<RectTransform>().anchorMin = new(_displayScreen.GetComponent<RectTransform>().anchorMin.x, 0.4f);
                //_displayScreen.GetComponent<RectTransform>().anchorMax = new(_displayScreen.GetComponent<RectTransform>().anchorMax.x, 0.6f);
                //Camera.aspect = _displayScreen.GetComponent<RectTransform>().rect.x / _displayScreen.GetComponent<RectTransform>().rect.y;
            }
        }

        public void ZoomOut()
        {
            if (selectedRoom != null && outDelay + 1f < Time.time)
            {
                selectedRoom = null;
                _soulHomeController.SetRoomName(selectedRoom);
                Camera.transform.position = prevWideCameraPos;
                Camera.fieldOfView = prevWideCameraFoV;
                inDelay = Time.time;

                //_displayScreen.GetComponent<RectTransform>().anchorMin = new(_displayScreen.GetComponent<RectTransform>().anchorMin.x, 0.2f);
                //_displayScreen.GetComponent<RectTransform>().anchorMax = new(_displayScreen.GetComponent<RectTransform>().anchorMax.x, 0.8f);
                //Camera.aspect = _displayScreen.GetComponent<RectTransform>().rect.x / _displayScreen.GetComponent<RectTransform>().rect.y;
            }
        }
    }
}
