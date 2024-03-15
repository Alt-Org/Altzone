using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

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

        private bool trayOpen = false;
        private GameObject _selectedFurnitureTray = null;

        public bool TrayOpen { get => trayOpen; set => trayOpen = value; }

        // Start is called before the first frame update
        void Start()
        {
            EnableTray(false);
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
                if (Input.touchCount >= 1) currentPosition = touch.position;
                else currentPosition = Mouse.current.position.ReadValue();

                RayPoint(ClickState.Move);
            }
            else if (((!moving && Input.GetMouseButton(0)) || touch.phase == UnityEngine.TouchPhase.Stationary) )
            {
                if (Input.touchCount >= 1) currentPosition = touch.position;
                else currentPosition = Mouse.current.position.ReadValue();

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

            RaycastHit2D[] hit;
            hit = Physics2D.GetRayIntersectionAll(ray, 1000);
            foreach (RaycastHit2D hit2 in hit)
            {
                if (hit2.collider != null)
                {
                    if (hit2.collider.gameObject.CompareTag("SoulHomeScreen"))
                    {
                        if (_selectedFurnitureTray != null)
                        {
                            if(_selectedFurnitureTray.GetComponent<Image>().enabled) _selectedFurnitureTray.GetComponent<Image>().enabled = false;
                            if(secondCamera.SelectedFurniture == null)
                            {
                                secondCamera.SetFurniture(_selectedFurnitureTray);
                            }
                        }
                        if (secondCamera.SelectedFurniture != null)
                        {
                            if (!secondCamera.SelectedFurniture.GetComponent<SpriteRenderer>().enabled)
                            {
                                secondCamera.SelectedFurniture.GetComponent<SpriteRenderer>().enabled = true;
                                secondCamera.SelectedFurniture.GetComponent<BoxCollider2D>().enabled = true;
                            }
                        }

                        //Debug.Log(hit.collider.gameObject.name);
                        Vector3 hitPoint = hit2.transform.InverseTransformPoint(hit2.point);
                        //Debug.Log(hitPoint);
                        float x = hit2.transform.GetComponent<RectTransform>().rect.width;
                        float y = hit2.transform.GetComponent<RectTransform>().rect.height;
                        Vector2 relPos = new((x / 2 + hitPoint.x) / x, (y / 2 + hitPoint.y) / y);
                        //Debug.Log(relPos);
                        bool check = secondCamera.FindRayPoint(relPos, click);
                        //if(check)backDelay = Time.time;
                    }
                    if (hit2.collider.gameObject.CompareTag("FurnitureTray"))
                    {
                        if (_selectedFurnitureTray != null)
                        {
                            if (!_selectedFurnitureTray.GetComponent<Image>().enabled) _selectedFurnitureTray.GetComponent<Image>().enabled = true;
                            _selectedFurnitureTray.transform.position = hit2.point;

                        }
                        if (secondCamera.SelectedFurniture != null)
                        {
                            if (secondCamera.SelectedFurniture.GetComponent<SpriteRenderer>().enabled)
                            {
                                secondCamera.SelectedFurniture.GetComponent<SpriteRenderer>().enabled = false;
                                secondCamera.SelectedFurniture.GetComponent<BoxCollider2D>().enabled = false;
                            }

                            if (click is ClickState.End)
                            {
                                secondCamera.RemoveFurniture();
                            }
                        }
                    }
                    if (hit2.collider.gameObject.CompareTag("FurnitureTrayItem"))
                    {
                        if (_selectedFurnitureTray == null && click is ClickState.Start)
                        {
                            _selectedFurnitureTray = hit2.collider.transform.GetChild(0).gameObject;
                            transform.GetChild(0).Find("Scroll View").gameObject.GetComponent<ScrollRect>().StopMovement();
                            transform.GetChild(0).Find("Scroll View").gameObject.GetComponent<ScrollRect>().enabled = false;
                        }
                    }
                }
            }
            if (click is ClickState.End)
            {
                if (_selectedFurnitureTray != null)
                {
                    if (!_selectedFurnitureTray.GetComponent<Image>().enabled) _selectedFurnitureTray.GetComponent<Image>().enabled = true;
                    _selectedFurnitureTray.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                    _selectedFurnitureTray = null;
                    transform.GetChild(0).Find("Scroll View").gameObject.GetComponent<ScrollRect>().enabled = true;
                }

                if (secondCamera.SelectedFurniture != null)
                {
                    if (!secondCamera.SelectedFurniture.GetComponent<SpriteRenderer>().enabled)
                    {
                        secondCamera.SelectedFurniture.GetComponent<SpriteRenderer>().enabled = true;
                        secondCamera.SelectedFurniture.GetComponent<BoxCollider2D>().enabled = true;
                    }
                    secondCamera.DeselectFurniture();
                }
            }
        }

        public void ToggleTray()
        {
            if (secondCamera.SelectedRoom == null) return;
            float width = GetComponent<BoxCollider2D>().size.x;
            GameObject tray = transform.GetChild(0).gameObject;
            if (!trayOpen)
            {
                tray.transform.localPosition = new Vector2(tray.transform.localPosition.x -width+100, tray.transform.localPosition.y);
                trayOpen = true;
            }
            else
            {
                tray.transform.localPosition = new Vector2(tray.transform.localPosition.x + width - 100, tray.transform.localPosition.y);
                trayOpen = false;
            }
        }
        public void EnableTray(bool enable)
        {
            if (enable)
            {
                GameObject tray = transform.GetChild(0).gameObject;
                tray.SetActive(true);
            }
            else
            {
                GameObject tray = transform.GetChild(0).gameObject;
                tray.SetActive(false);
            }
        }

    }
}
