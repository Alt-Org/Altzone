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
        private GameObject selectedRoom = null;
        [SerializeField]
        private SpriteRenderer backgroundSprite;

        private Bounds cameraBounds;
        private float cameraMinX;
        private float cameraMinY;
        private float cameraMaxX;
        private float cameraMaxY;
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
            if (Input.GetMouseButton(0))
            {
                if (selectedRoom == null)
                {
                    Vector3 bl = Camera.ViewportToWorldPoint(new Vector3(0, 0, Mathf.Abs(Camera.transform.position.z)));
                    Vector3 tr = Camera.ViewportToWorldPoint(new Vector3(1, 1, Mathf.Abs(Camera.transform.position.z)));
                    float currentY = transform.position.y;
                    float offsetY = Mathf.Abs(currentY-bl.y);
                    float moveAmountY = Input.GetAxis("Mouse Y");
                    float targetY = currentY - moveAmountY;

                    float y = Mathf.Clamp(targetY, cameraMinY+offsetY, cameraMaxY - offsetY);

                    float currentX = transform.position.x;
                    float offsetX = Mathf.Abs(currentX - bl.x);
                    float moveAmountX = Input.GetAxis("Mouse X");
                    float targetX = currentX - moveAmountX;

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
                        GameObject roomObject = hit2.collider.gameObject.transform.parent.parent.gameObject;
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
            prevWideCameraPos = Camera.transform.position;
            Camera.transform.position = new(room.transform.position.x, room.transform.position.y+4, -10);
        }

        public void ZoomOut()
        {
            selectedRoom = null;
            Camera.transform.position = prevWideCameraPos;
        }
    }
}
