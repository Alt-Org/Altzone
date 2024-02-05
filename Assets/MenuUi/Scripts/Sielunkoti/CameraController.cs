using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MenuUI.Scripts.SoulHome
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField]
        private CameraRaycast secondCamera;
        [SerializeField]
        private Camera _camera;

        Vector3 startPosition;
        float startTime;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                startPosition = Mouse.current.position.ReadValue();
                startTime = Time.time;
                Debug.Log(startPosition);
                Debug.Log(startTime);
            }
            if (Input.GetMouseButtonUp(0))
            {
                Vector3 endPosition = Mouse.current.position.ReadValue();
                float endTime = Time.time;

                Debug.Log(endPosition);
                Debug.Log(endTime);

                if (endTime - startTime > 0.2f || Mathf.Abs(startPosition.x-endPosition.x)+Mathf.Abs(startPosition.y - endPosition.y) > 1) return;

                Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
                //Vector2 click2D = new Vector2(clickpos.x, clickpos.y);
                //Debug.Log(ray);
                // Debug.Log(click2D);
                RaycastHit2D hit;
                //Physics.Raycast(ray, out hit);
                hit = Physics2D.GetRayIntersection(ray, 1000);
                if (hit.collider != null)
                {
                    Debug.Log(hit.collider.gameObject.name);
                    Vector3 hitPoint = hit.transform.InverseTransformPoint(hit.point);
                    Debug.Log(hitPoint);
                    float x = hit.transform.GetComponent<RectTransform>().rect.width;
                    float y = hit.transform.GetComponent<RectTransform>().rect.height;
                    Vector2 relPos = new((x / 2 + hitPoint.x) / x, (y / 2 + hitPoint.y) / y);
                    Debug.Log(relPos);
                    secondCamera.FindRayPoint(relPos);
                }
            }
            if (Input.GetMouseButtonUp(1))
            {
                secondCamera.ZoomOut();
            }
        }
    }
}
