using System.Collections;
using System.Collections.Generic;
using MenuUI.Scripts.SoulHome;
using Prg.Scripts.Common;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class RaidTowerController : MonoBehaviour
{
    private Camera _camera;
    private GameObject selectedRoom = null;
    private GameObject tempSelectedRoom = null;
    [Tooltip("The Mainscreen Script that handles the inputs to the actual screen that is shown to you."), SerializeField]
    private RaidMainScreenController _mainScreen;
    [Tooltip("The script that loads and sets the starting state of Soulhome."), SerializeField]
    private RaidRoomLoad _loadScript;
    [SerializeField]
    private float _scrollSpeed = 2f;
    [SerializeField]
    private float _scrollSpeedMouse = 2f;
    [SerializeField]
    private SpriteRenderer _backgroundSprite;
    //[Tooltip("Not in use, don't activate"),SerializeField]
    private bool _isometric = false;
    [Tooltip("The Controller Script for the SoulHome that includes everything not directly related screen inputs or the FurnitureTray."), SerializeField]
    private SoulHomeController _soulHomeController;
    [Tooltip("The screen that the output of the TowerCamera is printed onto."), SerializeField]
    private RawImage _displayScreen;
    [SerializeField]
    private GameObject _rooms;

    private BoxCollider2D _roomBounds;

    private List<GameObject> _changedFurnitureList = new();

    private bool _startFinished = false;
    private bool _rotated = false;
    private float _prevPinchDistance = 0;

    private GameObject _selectedFurniture;
    private GameObject _tempSelectedFurniture;
    private float _startFurnitureTime;
    private bool _grabbingFurniture = false;
    private Vector2 _tempRoomHitStart;
    private bool exitRoom = false;
    private bool editingMode = false;

    private Bounds cameraBounds;
    private float cameraMinX;
    private float cameraMinY;
    private float cameraMaxX;
    private float cameraMaxY;
    private float cameraWidth;
    private float cameraHeight;

    private float _maxCameraDistance;
    private float _minCameraDistance;

    private Vector2 prevp;
    private bool cameraMove = false;
    private bool _pinched = false;
    private Vector2 startScrollSlide = Vector2.zero;
    private Vector2 currentScrollSlide = Vector2.zero;
    private Vector2 currentScrollSlideDirection = Vector2.zero;

    private float outDelay = 0;
    private float inDelay = 0;

    public GameObject SelectedRoom { get => selectedRoom; private set => selectedRoom = value; }
    public GameObject SelectedFurniture
    {
        get => _selectedFurniture; private set
        {
            _selectedFurniture?.GetComponent<FurnitureHandling>().SetOutline(false);
            _selectedFurniture = value;
            //if (_tempSelectedFurniture != _selectedFurniture) _tempSelectedFurniture = value;
            if (_selectedFurniture != null) _selectedFurniture.GetComponent<FurnitureHandling>().SetOutline(true);
            _soulHomeController.SetFurniture(_selectedFurniture?.GetComponent<FurnitureHandling>().Furniture);
        }
    }
    public GameObject TempSelectedFurniture { get => _tempSelectedFurniture; }
    public List<GameObject> ChangedFurnitureList { get => _changedFurnitureList; set => _changedFurnitureList = value; }
    public bool EditingMode { get => editingMode; }
    public bool Rotated { get => _rotated; }
    public BoxCollider2D RoomBounds { get => _roomBounds; set { if (_roomBounds == null) _roomBounds = value; } }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(StartActions());

    }

    private IEnumerator StartActions()
    {
        yield return new WaitUntil(() => _loadScript.LoadFinished);
        _camera = GetComponent<Camera>();
        SetCameraBounds();

        Debug.Log(_displayScreen.GetComponent<RectTransform>().rect.x /*.sizeDelta.x*/ + " : " + _displayScreen.GetComponent<RectTransform>().rect.y /*.sizeDelta.y*/);
        //Camera.aspect = _displayScreen.GetComponent<RectTransform>().sizeDelta.x / _displayScreen.GetComponent<RectTransform>().sizeDelta.y;
        _camera.aspect = _displayScreen.GetComponent<RectTransform>().rect.x / _displayScreen.GetComponent<RectTransform>().rect.y;
        _camera.fieldOfView = 90f;
        transform.localPosition = new(0, 0, transform.position.z);
        _camera.transform.localPosition = new(_camera.transform.localPosition.x, _camera.transform.localPosition.y, -1 * GetCameraMaxDistance());
        Vector3 bl = _camera.ViewportToWorldPoint(new Vector3(0, 0, Mathf.Abs(_camera.transform.position.z)));
        float currentX = transform.position.x;
        float currentY = transform.position.y;
        float offsetX = Mathf.Abs(currentX - bl.x);
        float offsetY = Mathf.Abs(currentY - bl.y);

        //SetScrollSpeed();

        Debug.Log(currentY + " : " + (cameraMinY + offsetY) + " : " + (cameraMaxY - offsetY));
        Debug.Log(currentX + " : " + (cameraMinX + offsetX) + " : " + (cameraMaxX - offsetX));

        float y = Mathf.Clamp(currentY, cameraMinY + offsetY, cameraMaxY - offsetY);
        float x = Mathf.Clamp(currentX, cameraMinX + offsetX, cameraMaxX - offsetX);
        transform.position = new(x, y, transform.position.z);
        EnhancedTouchSupport.Enable();
        _maxCameraDistance = GetCameraMaxDistance();
        _minCameraDistance = GetCameraMinDistance();
        _startFinished = true;
    }

    public bool FindRayPoint(Vector2 relPoint, ClickState click)
    {
        //if (click == ClickState.Start) cameraMove = true;
        //if(click is ClickState.Hold or ClickState.Move) cameraMove = true;
        //if (click == ClickState.End) cameraMove = false;
        cameraMove = true;


        Ray ray = _camera.ViewportPointToRay(relPoint);
        RaycastHit2D[] hit;
        //Debug.Log("Camera2: " + ray);
        hit = Physics2D.GetRayIntersectionAll(ray, 1000);
        bool hitRoom = false;
        bool enterRoom = false;
        Vector2 hitPoint = Vector2.zero;
        GameObject furnitureObject = null;

        if (click == ClickState.Start) exitRoom = true;

        foreach (RaycastHit2D hit2 in hit)
        {
            if (hit2.collider != null)
            {
                if (hit2.collider.gameObject.CompareTag("ScrollRectCanvas"))
                {
                    hitPoint = hit2.point;
                    continue;
                }

                if (hit2.collider.gameObject.CompareTag("Furniture"))
                {
                    Debug.Log("Furniture");
                    //if(selectedRoom == null) continue;
                    //else
                    {
                        GameObject furnitureObjectHit = hit2.collider.gameObject;
                        if (furnitureObject == null) furnitureObject = furnitureObjectHit;
                        else
                        {
                            if (furnitureObjectHit.GetComponent<FurnitureHandling>().checkTopCollider(hit2.point.y))
                            {
                                if (furnitureObject.GetComponent<FurnitureHandling>().checkTopCollider(hit2.point.y)) //Check for an edgecase where the contact point is at the top of the collider on both objects.
                                {
                                    if (furnitureObject.GetComponent<SpriteRenderer>().sortingOrder < furnitureObjectHit.GetComponent<SpriteRenderer>().sortingOrder)
                                    {
                                        furnitureObject = furnitureObjectHit;
                                    }
                                }
                            }
                            else
                            if (furnitureObject.GetComponent<SpriteRenderer>().sortingOrder < furnitureObjectHit.GetComponent<SpriteRenderer>().sortingOrder)
                            {
                                furnitureObject = furnitureObjectHit;
                            }
                        }
                        //_furnitureList.Add(furnitureObject);
                    }
                }

                if (hit2.collider.gameObject.CompareTag("Room"))
                {
                    exitRoom = false;
                    //Debug.Log("Camera2: " + hit2.collider.gameObject.name);
                    //Vector3 hitPoint = hit2.transform.InverseTransformPoint(hit2.point);
                    //Debug.Log("Camera2: " + hitPoint);
                    //Debug.Log("Camera2: " + click);
                    GameObject roomObject;
                    if (_isometric)
                        roomObject = hit2.collider.gameObject.transform.parent.parent.gameObject;
                    else
                        roomObject = hit2.collider.gameObject;
                    if (click == ClickState.Start)
                    {
                        tempSelectedRoom = roomObject;
                        if (Touch.activeFingers.Count > 0)
                        {
                            Touch touch = Touch.activeTouches[0];
                            _tempRoomHitStart = touch.screenPosition;
                        }
                        else if (AppPlatform.IsDesktop && !AppPlatform.IsSimulator)
                            _tempRoomHitStart = Mouse.current.position.ReadValue();
                    }
                    else if (click is ClickState.Move or ClickState.Hold && tempSelectedRoom != null)
                    {
                        if (tempSelectedRoom != roomObject) tempSelectedRoom = null;
                    }

                    else if (click == ClickState.End /*&& _selectedFurniture == null*/)
                    {
                        Vector2 _tempRoomHitEnd = new();
                        if (Touch.activeFingers.Count > 0)
                        {
                            Touch touch = Touch.activeTouches[0];
                            _tempRoomHitEnd = touch.screenPosition;
                        }
                        else if (AppPlatform.IsDesktop && !AppPlatform.IsSimulator)
                            _tempRoomHitEnd = Mouse.current.position.ReadValue();
                        if (selectedRoom == null && tempSelectedRoom != null
                            && _tempRoomHitStart.y > _tempRoomHitEnd.y - 3f && _tempRoomHitStart.y < _tempRoomHitEnd.y + 3f
                            && _tempRoomHitStart.x > _tempRoomHitEnd.x - 3f && _tempRoomHitStart.x < _tempRoomHitEnd.x + 3f)
                        {
                            if (inDelay + 1f < Time.time)
                            {
                                selectedRoom = tempSelectedRoom;
                                //ZoomIn(selectedRoom);
                            }
                            //enterRoom = true;
                        }
                        else if (selectedRoom != null && selectedRoom != roomObject && tempSelectedRoom != null
                            && _tempRoomHitStart.y > _tempRoomHitEnd.y - 3f && _tempRoomHitStart.y < _tempRoomHitEnd.y + 3f
                            && _tempRoomHitStart.x > _tempRoomHitEnd.x - 3f && _tempRoomHitStart.x < _tempRoomHitEnd.x + 3f)
                        {
                            //ZoomOut();
                            selectedRoom = tempSelectedRoom;
                            //ZoomIn(selectedRoom);
                        }
                    }
                    hitRoom = true;
                }
            }

        }
        if (((AppPlatform.IsDesktop && !AppPlatform.IsSimulator && (Mouse.current.leftButton.isPressed || Mouse.current.leftButton.wasReleasedThisFrame)) || Touch.activeFingers.Count >= 1) && (furnitureObject != null || _tempSelectedFurniture != null))
        {
            Debug.Log(furnitureObject);
            //Touch touch = Input.GetTouch(0);
            if (click == ClickState.Start && (selectedRoom != null || editingMode))
            {
                if (_selectedFurniture == null)
                {
                    SelectedFurniture = furnitureObject;
                    //_mainScreen.SetFurniture(_selectedFurniture);
                    if (editingMode)
                    {
                        _tempSelectedFurniture = SelectedFurniture;
                        _tempSelectedFurniture.GetComponent<FurnitureHandling>().SetTransparency(0.5f);
                        //if (!editingMode) ToggleEdit();
                    }
                    else
                    {
                        _startFurnitureTime = Time.time;
                        _grabbingFurniture = true;
                    }
                }
                else if (furnitureObject != null && _selectedFurniture != furnitureObject)
                {
                    DeselectFurniture();
                    SelectedFurniture = furnitureObject;
                    //_mainScreen.SetFurniture(_selectedFurniture);
                    if (EditingMode)
                    {
                        _tempSelectedFurniture = SelectedFurniture;
                        _selectedFurniture.GetComponent<FurnitureHandling>().SetTransparency(0.5f);
                    }
                }
                else if (_selectedFurniture == furnitureObject && EditingMode)
                {
                    _tempSelectedFurniture = furnitureObject;
                    _selectedFurniture.GetComponent<FurnitureHandling>().SetTransparency(0.5f);
                }
            }
            else if (click is ClickState.End /*or ClickState.Move*/ || furnitureObject != _selectedFurniture)
            {
                //if (_tempSelectedFurniture != null) PlaceFurniture(hitPoint, false);
                _tempSelectedFurniture = null;
                _grabbingFurniture = false;
            }

            //if (_selectedFurniture) _mainScreen.SetHoverButtons(_camera.WorldToViewportPoint(_selectedFurniture.transform.position));

        }
        else if (click is ClickState.End && furnitureObject == null)
        {
            DeselectFurniture();
        }

        return enterRoom;
    }

    public void DeselectFurniture()
    {
        _tempSelectedFurniture = null;
        if (_selectedFurniture != null)
        {
            _rooms.transform.GetChild(_selectedFurniture.GetComponent<FurnitureHandling>().TempSlot.roomId).GetChild(0).GetComponent<RoomData>().ResetPosition(SelectedFurniture, true);
            _selectedFurniture.GetComponent<FurnitureHandling>().SetTransparency(1f);
            _selectedFurniture.GetComponent<FurnitureHandling>().ResetFurniturePosition();
            SelectedFurniture = null;
        }
    }

    public void SetCameraBounds()
    {
        cameraBounds = _backgroundSprite.bounds;
        Bounds roomBounds = _roomBounds.bounds;
        cameraMinX = roomBounds.min.x;
        cameraMinY = cameraBounds.min.y;
        cameraMaxX = roomBounds.max.x;
        cameraMaxY = cameraBounds.max.y;
    }

    public float GetCameraMaxDistance()
    {
        float distanceMaxX = GetCameraXDistance();

        float distanceMaxY = GetCameraYDistance();

        if (distanceMaxX < distanceMaxY) return distanceMaxY;
        else return distanceMaxX;
    }

    public float GetCameraMinDistance()
    {
        float distanceMaxX = GetCameraXDistance();

        float distanceMaxY = GetCameraYDistance();

        if (distanceMaxX > distanceMaxY) return distanceMaxY;
        else return distanceMaxX;
    }

    public float GetCameraYDistance()
    {
        float heightToEdge = _roomBounds.size.y / 2 + 2;
        float cameraAngleVertical = _camera.fieldOfView / 2;
        float distanceMaxY = heightToEdge / Mathf.Tan(cameraAngleVertical * (Mathf.PI / 180));
        //Debug.Log(heightToEdge + ":" + cameraAngleVertical + ":" + Mathf.Tan(cameraAngleVertical * (Mathf.PI / 180)) + ":" + distanceMaxY);
        return distanceMaxY;
    }

    public float GetCameraXDistance()
    {
        float widthToEdge = _roomBounds.size.x / 2;
        float cameraAngle = Camera.VerticalToHorizontalFieldOfView(_camera.fieldOfView, _camera.aspect) / 2;
        float distanceMaxX = widthToEdge / Mathf.Tan(cameraAngle * (Mathf.PI / 180));
        //Debug.Log(widthToEdge + ":" + cameraAngle + ":" + Mathf.Tan(cameraAngle * (Mathf.PI / 180)) + ":" + distanceMaxX);
        return distanceMaxX;
    }
}
