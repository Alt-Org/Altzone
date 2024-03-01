using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace MenuUI.Scripts.SoulHome
{
    public enum ClickState
    {
        Start,
        Hold,
        Move,
        End
    }


    public class CameraController : MonoBehaviour
    {
        [SerializeField]
        private CameraRaycast secondCamera;
        [SerializeField]
        private Camera _camera;

        Vector3 startPosition;
        float startTime;

        Vector3 currentPosition;
        private bool moving;

        float backDelay = 0;
        float inDelay = 0;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            if(Input.GetMouseButton(0))
            if (!Mouse.current.position.ReadValue().Equals(currentPosition))
            {
                moving = true;
            }
            else moving = false;

            Touch touch =new();
            if (Input.touchCount > 0) touch = Input.GetTouch(0);


            if (Input.GetMouseButtonDown(0) || (Input.touchCount > 0 && touch.phase == UnityEngine.TouchPhase.Began))
            {
                if(Input.touchCount >= 1) startPosition = touch.position;
                else startPosition = Mouse.current.position.ReadValue();
                startTime = Time.time;
                //Debug.Log(startPosition);
                //Debug.Log(startTime);
                RayPoint(ClickState.Start);
            }
            else if ((Input.GetMouseButtonUp(0) || touch.phase is UnityEngine.TouchPhase.Ended or UnityEngine.TouchPhase.Canceled))
            {
                Vector2 endPosition;
                if (Input.touchCount >= 1) endPosition = touch.position;
                else endPosition = Mouse.current.position.ReadValue();
                float endTime = Time.time;

                //Debug.Log(endPosition);
                //Debug.Log(endTime);

                //if (endTime - startTime > 0.2f || Mathf.Abs(startPosition.x-endPosition.x)+Mathf.Abs(startPosition.y - endPosition.y) > 1) return;

                RayPoint(ClickState.End);
                startPosition = Vector3.zero;
            }
            else if (((moving && Input.GetMouseButton(0)) || touch.phase == UnityEngine.TouchPhase.Moved) )
            {
                Vector2 endPosition;
                if (Input.touchCount >= 1) endPosition = touch.position;
                else currentPosition = Mouse.current.position.ReadValue();
                float endTime = Time.time;

                //Debug.Log(endPosition);
                //Debug.Log(endTime);

                //if (endTime - startTime > 0.2f || Mathf.Abs(startPosition.x - currentPosition.x) + Mathf.Abs(startPosition.y - currentPosition.y) > 1) return;

                RayPoint(ClickState.Move);
            }
            else if (((!moving && Input.GetMouseButton(0)) || touch.phase == UnityEngine.TouchPhase.Stationary) )
            {
                Vector2 endPosition;
                if (Input.touchCount >= 1) endPosition = touch.position;
                else endPosition = Mouse.current.position.ReadValue();
                float endTime = Time.time;

                //Debug.Log(endPosition);
                //Debug.Log(endTime);

                //if (endTime - startTime > 0.2f || Mathf.Abs(startPosition.x - endPosition.x) + Mathf.Abs(startPosition.y - endPosition.y) > 1) return;

                RayPoint(ClickState.Hold);
            }
            if ((Input.GetMouseButtonUp(1) || touch.tapCount > 1) && backDelay + 0.4f < Time.time)
            {
                secondCamera.ZoomOut();
                //inDelay = Time.time;
            }
        }


        private void RayPoint(ClickState click)
        {
            Debug.Log(click);
            Touch touch = new();
            Ray ray;
            if (Input.touchCount >= 1)
            {
                touch = Input.GetTouch(0);
                ray = _camera.ScreenPointToRay(touch.position);
            }
            else ray = _camera.ScreenPointToRay(Input.mousePosition);

            RaycastHit2D hit;
            hit = Physics2D.GetRayIntersection(ray, 1000);
            if (hit.collider != null)
            {
                //Debug.Log(hit.collider.gameObject.name);
                Vector3 hitPoint = hit.transform.InverseTransformPoint(hit.point);
                //Debug.Log(hitPoint);
                float x = hit.transform.GetComponent<RectTransform>().rect.width;
                float y = hit.transform.GetComponent<RectTransform>().rect.height;
                Vector2 relPos = new((x / 2 + hitPoint.x) / x, (y / 2 + hitPoint.y) / y);
                //Debug.Log(relPos);
                bool check = secondCamera.FindRayPoint(relPos, click);
                //if(check)backDelay = Time.time;
            }
        }
    }
}
