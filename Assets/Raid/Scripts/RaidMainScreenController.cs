using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.InputSystem;
using Prg.Scripts.Common;
using UnityEngine.UI;

public class RaidMainScreenController : MonoBehaviour
{
    //[SerializeField]
    //private SoulHomeController _soulHomeController;
    [SerializeField]
    private RaidTowerController _raidTower;
    [SerializeField]
    private Camera _camera;
    // Start is called before the first frame update
    void Start()
    {
        EnhancedTouchSupport.Enable();
    }

    // Update is called once per frame
    void Update()
    {
        //if (!CheckInteractableStatus()) return;

        ClickState clickState = ClickStateHandler.GetClickState();
        if (clickState is not ClickState.None)
        {
            Debug.Log(Touch.activeFingers[0].screenPosition);
            if (Touch.activeTouches.Count == 1 || (Mouse.current != null && Mouse.current.leftButton.isPressed && Mouse.current.scroll.ReadValue() == Vector2.zero))
            RayPoint(clickState);
            else if (Touch.activeTouches.Count <= 2 || (Mouse.current != null && Mouse.current.scroll.ReadValue() != Vector2.zero))
            {
                float distance;
                if (Touch.activeTouches.Count <= 2)
                {
                    Vector2 touch1 = Touch.activeFingers[0].screenPosition;
                    Vector2 touch2 = Touch.activeFingers[1].screenPosition;

                    distance = Vector2.Distance(touch1, touch2);
                    //_soulHomeTower.PinchZoom(distance, false);
                }
                else
                {
                    distance = Mouse.current.scroll.ReadValue().y;
                    //_soulHomeTower.PinchZoom(distance, true);
                }
            }

        }
    }

    private void RayPoint(ClickState click)
    {
        Debug.Log(click);
        Debug.Log(Screen.orientation);
        Ray ray;
        if (Touch.activeFingers.Count >= 1)
        {
            Touch touch = Touch.activeTouches[0];
            ray = _camera.ScreenPointToRay(touch.screenPosition);
        }
        else ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());

        RaycastHit2D[] hit;
        hit = Physics2D.GetRayIntersectionAll(ray, 1000);
        bool overlayHit = false;
        foreach (RaycastHit2D hit1 in hit)
        {
            Debug.LogWarning(hit1.collider.gameObject.ToString());
            if (hit1.collider.gameObject.CompareTag("Overlay")) overlayHit = true;
        }
        if (!overlayHit)
        {
            foreach (RaycastHit2D hit2 in hit)
            {
                if (hit2.collider.gameObject.CompareTag("SoulHomeScreen"))
                {
                    if (_raidTower.SelectedFurniture != null)
                    {
                        if (!_raidTower.SelectedFurniture.GetComponent<SpriteRenderer>().enabled)
                        {
                            _raidTower.SelectedFurniture.GetComponent<SpriteRenderer>().enabled = true;
                            _raidTower.SelectedFurniture.GetComponent<BoxCollider2D>().enabled = true;
                        }
                    }

                    //Debug.Log(hit.collider.gameObject.name);
                    Vector3 hitPoint = hit2.transform.InverseTransformPoint(hit2.point);
                    //Debug.Log(hitPoint);
                    float x = hit2.transform.GetComponent<RectTransform>().rect.width;
                    float y = hit2.transform.GetComponent<RectTransform>().rect.height;
                    Vector2 relPos = new((x / 2 + hitPoint.x) / x, (y / 2 + hitPoint.y) / y);
                    //Debug.Log(relPos);
                    bool check = _raidTower.FindRayPoint(relPos, click);
                    //if (check) _backDelay = Time.time;
                    if (_raidTower.SelectedFurniture != null)
                    {
                        //if (_selectedFurnitureTray == null)
                        //{
                            //SetFurniture();
                        //}
                        //if (_selectedFurnitureTray.GetComponent<Image>().enabled) _selectedFurnitureTray.GetComponent<Image>().enabled = false;
                        //_selectedFurnitureTray.transform.position = hit2.point;
                    }
                }
            }
        }
    }
    /*private bool CheckInteractableStatus()
    {
        if (_soulHomeController.ExitPending) return false;
        if (_soulHomeController.ConfirmPopupOpen) return false;

        return true;
    }*/
}
