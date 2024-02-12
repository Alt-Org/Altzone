using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

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

        private Bounds cameraBounds;
        private float cameraMinX;
        private float cameraMinY;
        private float cameraMaxX;
        private float cameraMaxY;

        private Vector2 prevp;
        // Start is called before the first frame update
        void Start()
        {
            Camera = GetComponent<Camera>();
            cameraBounds = backgroundSprite.bounds;
            cameraMinX = cameraBounds.min.x;
            cameraMinY = cameraBounds.min.y;
            cameraMaxX = cameraBounds.max.x;
            cameraMaxY = cameraBounds.max.y;
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
            }
        }

        public void FindRayPoint(Vector2 relPoint)
        {
            Ray ray = Camera.ViewportPointToRay(relPoint);
            RaycastHit2D[] hit;
            Debug.Log("Camera2: " + ray);
            hit = Physics2D.GetRayIntersectionAll(ray, 1000);
            bool hitRoom = false;
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
                        }
                        else if (selectedRoom != roomObject)
                        {
                            ZoomOut();
                        }
                        hitRoom = true;
                    }
                }

            }
            if (!hitRoom && selectedRoom != null) ZoomOut();
        }

        public void ZoomIn(GameObject room)
        {
            _soulHomeController.SetRoomName(selectedRoom);
            prevWideCameraPos = Camera.transform.position;
            prevWideCameraFoV = Camera.fieldOfView;
            Camera.transform.position = new(room.transform.position.x, room.transform.position.y+10, -30);
            Camera.fieldOfView = 60;
        }

        public void ZoomOut()
        {
            if (selectedRoom != null)
            {
                selectedRoom = null;
                _soulHomeController.SetRoomName(selectedRoom);
                Camera.transform.position = prevWideCameraPos;
                Camera.fieldOfView = prevWideCameraFoV;
            }
        }
    }
}
