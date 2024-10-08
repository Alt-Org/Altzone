using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using Prg.Scripts.Common;
using UnityEngine.InputSystem;

namespace MenuUI.Scripts.SoulHome
{
    public class TowerController : MonoBehaviour
    {
        private Camera _camera;
        private GameObject selectedRoom = null;
        private GameObject tempSelectedRoom = null;
        [Tooltip("The Mainscreen Script that handles the inputs to the actual screen that is shown to you."), SerializeField]
        private MainScreenController _mainScreen;
        [Tooltip("The script that loads and sets the starting state of Soulhome."),SerializeField]
        private SoulHomeLoad _loadScript;
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
        [Tooltip("The screen that the output of the TowerCamera is printed onto."),SerializeField]
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
        public GameObject SelectedFurniture { get => _selectedFurniture; private set
            {
                _selectedFurniture?.GetComponent<FurnitureHandling>().SetOutline(false);
                _selectedFurniture = value;
                //if (_tempSelectedFurniture != _selectedFurniture) _tempSelectedFurniture = value;
                if(_selectedFurniture != null) _selectedFurniture.GetComponent<FurnitureHandling>().SetOutline(true);
                _soulHomeController.SetFurniture(_selectedFurniture?.GetComponent<FurnitureHandling>().Furniture);
            }
        }
        public GameObject TempSelectedFurniture { get => _tempSelectedFurniture;}
        public List<GameObject> ChangedFurnitureList { get => _changedFurnitureList; set => _changedFurnitureList = value; }
        public bool EditingMode { get => editingMode;}
        public bool Rotated { get => _rotated;}
        public BoxCollider2D RoomBounds { get => _roomBounds; set { if(_roomBounds == null) _roomBounds = value; } }

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

            SetScrollSpeed();

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

        // Update is called once per frame
        void Update()
        {
            if (!_startFinished) return;
            CheckScreenRotationStatus();
            SetScrollSpeed();
            //Debug.Log(cameraWidth+" : "+ _mainScreen.transform.GetComponent<RectTransform>().rect.width);
            //Debug.Log(cameraMove);
            if (!CheckInteractableStatus()) return;

            Touch touch = new();
            if(Touch.activeFingers.Count > 0) touch = Touch.activeTouches[0];
            if (Touch.activeFingers.Count > 0 && (touch.phase == UnityEngine.InputSystem.TouchPhase.Began || prevp == Vector2.zero)) prevp = touch.screenPosition;
            if (((AppPlatform.IsDesktop && !AppPlatform.IsSimulator && Mouse.current.leftButton.isPressed) || Touch.activeFingers.Count == 1) && cameraMove)
            {
                if (_tempSelectedFurniture == null)
                {
                    Vector3 bl = _camera.ViewportToWorldPoint(new Vector3(0, 0, Mathf.Abs(_camera.transform.position.z)));
                    float currentY = transform.position.y;
                    float offsetY = Mathf.Abs(currentY-bl.y);
                    float targetY;
                    float currentX = transform.position.x;
                    float offsetX = Mathf.Abs(currentX - bl.x);
                    float targetX;
                    if (Touch.activeFingers.Count == 1)
                    {
                        //touch = Input.GetTouch(0);
                        if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began || prevp == Vector2.zero) prevp = touch.screenPosition;
                        Vector2 lp = touch.screenPosition;
                        targetY = currentY + (prevp.y - lp.y) / _scrollSpeed;
                        targetX = currentX + (prevp.x - lp.x) / _scrollSpeed;
                        //Debug.Log("Touch: Y: "+(prevp.y - lp.y));
                        if (touch.phase is UnityEngine.InputSystem.TouchPhase.Ended or UnityEngine.InputSystem.TouchPhase.Canceled)
                        {
                            startScrollSlide = new Vector2(Mathf.Abs(prevp.x - lp.x) / _scrollSpeed, Mathf.Abs(prevp.y - lp.y) / _scrollSpeed);
                            currentScrollSlide = startScrollSlide;
                            currentScrollSlideDirection = new Vector2(prevp.x - lp.x, prevp.y - lp.y);
                            currentScrollSlideDirection.Normalize();
                            prevp = Vector2.zero;
                        }
                        else
                        {
                            prevp = touch.screenPosition;
                        }
                    }
                    else
                    {
                        float moveAmountY = Mouse.current.position.ReadValue().y - Mouse.current.position.ReadValueFromPreviousFrame().y;
                        targetY = currentY - moveAmountY * _scrollSpeedMouse;
                        float moveAmountX = Mouse.current.position.ReadValue().x - Mouse.current.position.ReadValueFromPreviousFrame().x;
                        targetX = currentX - moveAmountX * _scrollSpeedMouse;
                        startScrollSlide = new Vector2(Mathf.Abs(moveAmountX) * _scrollSpeedMouse, Mathf.Abs(moveAmountY * _scrollSpeedMouse));
                        currentScrollSlide = startScrollSlide;
                        currentScrollSlideDirection = new Vector2(-1 * moveAmountX, -1 * moveAmountY);
                        currentScrollSlideDirection.Normalize();
                    }
                    float y;
                    if (cameraMinY + offsetY < cameraMaxY - offsetY)
                    {
                        y = Mathf.Clamp(targetY, cameraMinY + offsetY, cameraMaxY - offsetY);
                    }
                    else
                    {
                        y = (cameraMinY+ cameraMaxY) /2;
                    }
                    float x;
                    if (cameraMinX + offsetX < cameraMaxX - offsetX)
                    {
                        x = Mathf.Clamp(targetX, cameraMinX + offsetX, cameraMaxX - offsetX);
                    }
                    else
                    {
                        x = (cameraMinX + cameraMaxX) / 2;
                    }
                    transform.position = new(x, y, transform.position.z);
                }
                /*else if(_tempSelectedFurniture == null)
                {
                    Bounds roomCameraBounds = selectedRoom.GetComponent<BoxCollider2D>().bounds;
                    float roomCameraMinX = roomCameraBounds.min.x;
                    float roomCameraMinY = roomCameraBounds.min.y;
                    float roomCameraMaxX = roomCameraBounds.max.x;
                    float roomCameraMaxY = roomCameraBounds.max.y;
                    Vector3 bl = Camera.ViewportToWorldPoint(new Vector3(0, 0, Mathf.Abs(Camera.transform.position.z)));
                    float currentX = transform.position.x;
                    float offsetX = Mathf.Abs(currentX - bl.x);
                    float targetX;
                    if (Touch.activeFingers.Count == 1)
                    {
                        //touch = Input.GetTouch(0);
                        if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began || prevp == Vector2.zero) prevp = touch.screenPosition;
                        Vector2 lp = touch.screenPosition;
                        targetX = currentX + (prevp.x - lp.x) / scrollSpeed;
                        //Debug.Log("Touch: X: " + (prevp.x - lp.x));
                        if (touch.phase is UnityEngine.InputSystem.TouchPhase.Ended or UnityEngine.InputSystem.TouchPhase.Canceled)
                        {
                            startScrollSlide = new Vector2(Mathf.Abs(prevp.x - lp.x) / scrollSpeed, 0); 
                            currentScrollSlide = startScrollSlide;
                            currentScrollSlideDirection = new Vector2(prevp.x - lp.x, 0);
                            currentScrollSlideDirection.Normalize();
                            prevp = Vector2.zero;
                        }
                        else prevp = touch.screenPosition;
                    }
                    else
                    {
                        float moveAmountY = Mouse.current.position.ReadValue().x - Mouse.current.position.ReadValueFromPreviousFrame().x;
                        targetX = currentX - moveAmountY * scrollSpeedMouse;
                        startScrollSlide = new Vector2(Mathf.Abs(moveAmountY) * scrollSpeedMouse, 0);
                        currentScrollSlide = startScrollSlide;
                        currentScrollSlideDirection = new Vector2(-1*moveAmountY, 0);
                        currentScrollSlideDirection.Normalize();
                    }
                    float x;
                    if (roomCameraMinX + offsetX < roomCameraMaxX - offsetX)
                    {
                        x = Mathf.Clamp(targetX, roomCameraMinX + offsetX, roomCameraMaxX - offsetX);
                    }
                    else
                    {
                        x = (roomCameraMinX + roomCameraMaxX) / 2;
                    }
                    transform.position = new(x, transform.position.y, transform.position.z);
                }*/
                //cameraMove = false;
                Debug.Log(currentScrollSlideDirection);
            }
            else if(!cameraMove)
            {
                if (currentScrollSlideDirection != Vector2.zero)
                {
                    Debug.Log(currentScrollSlide);
                    if (currentScrollSlide.x > 0)
                        currentScrollSlide.x = Mathf.Max(0, currentScrollSlide.x - startScrollSlide.x/20);
                    if (currentScrollSlide.y > 0)
                        currentScrollSlide.y = Mathf.Max(0, currentScrollSlide.y - startScrollSlide.y / 20);

                    if (selectedRoom == null && _selectedFurniture == null)
                    {
                        Vector3 bl = _camera.ViewportToWorldPoint(new Vector3(0, 0, Mathf.Abs(_camera.transform.position.z)));
                        float currentY = transform.position.y;
                        float offsetY = Mathf.Abs(currentY - bl.y);
                        float targetY;

                        targetY = currentY + currentScrollSlideDirection.y * currentScrollSlide.y;
                        float y;
                        if (cameraMinY + offsetY < cameraMaxY - offsetY)
                        {
                            y = Mathf.Clamp(targetY, cameraMinY + offsetY, cameraMaxY - offsetY);
                        }
                        else
                        {
                            y = (cameraMinY + cameraMaxY) / 2;
                        }


                        transform.position = new(transform.position.x, y, transform.position.z);
                    }
                    if (selectedRoom != null)
                    {
                        Bounds roomCameraBounds = selectedRoom.GetComponent<BoxCollider2D>().bounds;
                        float roomCameraMinX = roomCameraBounds.min.x;
                        float roomCameraMaxX = roomCameraBounds.max.x;
                        Vector3 bl = _camera.ViewportToWorldPoint(new Vector3(0, 0, Mathf.Abs(_camera.transform.position.z)));
                        float currentX = transform.position.x;
                        float offsetX = Mathf.Abs(currentX - bl.x);
                        float targetX;

                        targetX = currentX + currentScrollSlideDirection.x * currentScrollSlide.x;

                        float x;
                        if (roomCameraMinX + offsetX < roomCameraMaxX - offsetX)
                        {
                            x = Mathf.Clamp(targetX, roomCameraMinX + offsetX, roomCameraMaxX - offsetX);
                        }
                        else
                        {
                            x = (roomCameraMinX + roomCameraMaxX) / 2;
                        }
                        transform.position = new(x, transform.position.y, transform.position.z);
                    }
                    if(currentScrollSlide == Vector2.zero) currentScrollSlideDirection = Vector2.zero;
                }
            }
            cameraMove = false;
            if (Touch.activeFingers.Count > 0 && (touch.phase is UnityEngine.InputSystem.TouchPhase.Ended or UnityEngine.InputSystem.TouchPhase.Canceled)) prevp = Vector2.zero;
            if(!_pinched && _prevPinchDistance != 0) _prevPinchDistance = 0;
            _pinched = false;
            if(ClickStateHandler.GetClickState() is ClickState.End && _selectedFurniture == null && _tempSelectedFurniture != null) _tempSelectedFurniture = null;
        }

        void OnEnable()
        {
            if (_camera != null)
            {
                _camera.aspect = _displayScreen.GetComponent<RectTransform>().rect.x / _displayScreen.GetComponent<RectTransform>().rect.y;
                HandleScreenRotation();
            }
            _rotated = false;
        }

        void OnDisable()
        {
            if (selectedRoom != null) ZoomOut();
            if (editingMode) ToggleEdit();
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

            if(click == ClickState.Start) exitRoom = true;

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
                            if(furnitureObject == null) furnitureObject = furnitureObjectHit;
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
                                    ZoomIn(selectedRoom);
                                }
                                //enterRoom = true;
                            }
                            else if (selectedRoom != null && selectedRoom != roomObject && tempSelectedRoom != null
                                && _tempRoomHitStart.y > _tempRoomHitEnd.y - 3f && _tempRoomHitStart.y < _tempRoomHitEnd.y + 3f
                                && _tempRoomHitStart.x > _tempRoomHitEnd.x - 3f && _tempRoomHitStart.x < _tempRoomHitEnd.x + 3f)
                            {
                                ZoomOut();
                                selectedRoom = tempSelectedRoom;
                                ZoomIn(selectedRoom);
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
                    if (_selectedFurniture == null) {
                        SelectedFurniture = furnitureObject;
                        _mainScreen.SetFurniture(_selectedFurniture);
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
                    else if(furnitureObject != null && _selectedFurniture != furnitureObject)
                    {
                        DeselectFurniture();
                        SelectedFurniture = furnitureObject;
                        _mainScreen.SetFurniture(_selectedFurniture);
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
                else if (((_startFurnitureTime + 1 <= Time.time && selectedRoom != null) || editingMode) && _tempSelectedFurniture == null && _selectedFurniture != null && _grabbingFurniture)
                {
                    _tempSelectedFurniture = SelectedFurniture;
                    _selectedFurniture.GetComponent<FurnitureHandling>().SetTransparency(0.5f);
                    if (!editingMode) ToggleEdit();
                    _mainScreen.SetFurniture(_selectedFurniture);
                }
                else if (click is ClickState.Hold or ClickState.Move && _selectedFurniture != null && _tempSelectedFurniture != null)
                {
                    PlaceFurniture(hitPoint, true);
                }
                else if (click is ClickState.End /*or ClickState.Move*/ || furnitureObject != _selectedFurniture)
                {
                    if (_tempSelectedFurniture != null) PlaceFurniture(hitPoint, false);
                    _tempSelectedFurniture = null;
                    _grabbingFurniture = false;
                }

                if(_selectedFurniture)_mainScreen.SetHoverButtons(_camera.WorldToViewportPoint(_selectedFurniture.transform.position));

            }
            else if (click is ClickState.End && furnitureObject == null)
            {
                DeselectFurniture();
            }


            if (!hitRoom && selectedRoom != null && exitRoom && click == ClickState.End)
            {
                ZoomOut();
            }
            return enterRoom;
        }

        public void PinchZoom(float pinchDistance, bool scroll)
        {
            _pinched = true;
            if (scroll)
            {
                Debug.Log("Pinch: " + pinchDistance / 100);
                ClampCameraDistance(pinchDistance / 100);
            }
            else
            {
                if (_prevPinchDistance == 0) _prevPinchDistance = pinchDistance;
                else
                {
                    float change = pinchDistance - _prevPinchDistance;
                    float scaledChange = change / Vector2.Distance(_displayScreen.GetComponent<RectTransform>().rect.max, _displayScreen.GetComponent<RectTransform>().rect.min);

                    Debug.Log("Pinch: " + scaledChange);

                    ClampCameraDistance(scaledChange*100);

                    _prevPinchDistance = pinchDistance;
                }
            }
        }

        private void ClampCameraDistance(float change)
        {
            float z = -1 * Mathf.Clamp(Mathf.Abs(_camera.transform.position.z) + change, _minCameraDistance, _maxCameraDistance);

            _camera.transform.position = new(_camera.transform.position.x, _camera.transform.position.y, z);
        }

        public void ZoomIn(GameObject room)
        {
            _soulHomeController.SetRoomName(selectedRoom);
            _camera.transform.position = new(room.transform.position.x, room.transform.position.y + room.GetComponent<BoxCollider2D>().size.y / 2,_camera.transform.position.z);
            
            outDelay = Time.time;

            _mainScreen.LeaveRoomButton.SetActive(true);

            Vector3 bl = _camera.ViewportToWorldPoint(new Vector3(0, 0, Mathf.Abs(_camera.transform.position.z)));

            float offsetY = Mathf.Abs(transform.position.y - bl.y);
            float y = Mathf.Clamp(transform.position.y, cameraMinY + offsetY, cameraMaxY - offsetY);
            float offsetX = Mathf.Abs(transform.position.x - bl.x);
            float x = Mathf.Clamp(transform.position.x, cameraMinX + offsetX, cameraMaxX - offsetX);
            _camera.transform.position = new(x, y, transform.position.z);
            //_mainScreen.EnableTray(true);

            //_displayScreen.GetComponent<RectTransform>().anchorMin = new(_displayScreen.GetComponent<RectTransform>().anchorMin.x, 0.4f);
            //_displayScreen.GetComponent<RectTransform>().anchorMax = new(_displayScreen.GetComponent<RectTransform>().anchorMax.x, 0.6f);
            //Camera.aspect = _displayScreen.GetComponent<RectTransform>().rect.x / _displayScreen.GetComponent<RectTransform>().rect.y;
        }

        public void ZoomOut()
        {
            if (selectedRoom != null && outDelay + 1f < Time.time)
            {
                //if (_mainScreen.TrayOpen) _mainScreen.ToggleTray();
                selectedRoom = null;
                _soulHomeController.SetRoomName(selectedRoom);

                _camera.transform.position = new(_camera.transform.position.x- _camera.transform.localPosition.x, _camera.transform.position.y, _camera.transform.position.z);
                inDelay = Time.time;

                _mainScreen.LeaveRoomButton.SetActive(false);
                Vector3 bl = _camera.ViewportToWorldPoint(new Vector3(0, 0, Mathf.Abs(_camera.transform.position.z)));

                float offsetY = Mathf.Abs(transform.position.y - bl.y);
                float y = Mathf.Clamp(transform.position.y, cameraMinY + offsetY, cameraMaxY - offsetY);
                float offsetX = Mathf.Abs(transform.position.x - bl.x);
                float x = Mathf.Clamp(transform.position.x, cameraMinX + offsetX, cameraMaxX - offsetX);
                _camera.transform.position = new(x, y, transform.position.z);
                //_mainScreen.EnableTray(false);

                //_displayScreen.GetComponent<RectTransform>().anchorMin = new(_displayScreen.GetComponent<RectTransform>().anchorMin.x, 0.2f);
                //_displayScreen.GetComponent<RectTransform>().anchorMax = new(_displayScreen.GetComponent<RectTransform>().anchorMax.x, 0.8f);
                //Camera.aspect = _displayScreen.GetComponent<RectTransform>().rect.x / _displayScreen.GetComponent<RectTransform>().rect.y;
            }
        }

        private void SetScrollSpeed()
        {
            Vector3 bl = _camera.ViewportToWorldPoint(new Vector3(0, 0, Mathf.Abs(_camera.transform.position.z)));
            Vector3 tr = _camera.ViewportToWorldPoint(new Vector3(1, 1, Mathf.Abs(_camera.transform.position.z)));

            cameraHeight = Mathf.Abs(tr.y - bl.y);

            _scrollSpeed = _displayScreen.transform.GetComponent<RectTransform>().rect.height / cameraHeight;
        }

        public void PlaceFurnitureToCurrent(bool hover)
        {
            Vector2 hitPoint = Vector2.negativeInfinity;
            PlaceFurniture(hitPoint, hover);
        }

        public void PlaceFurniture(Vector2 hitPoint, bool hover)
        {
            Vector2 checkPoint;
            Vector2Int size = _selectedFurniture.GetComponent<FurnitureHandling>().GetFurnitureSize();
            bool isFurniturePlaceHolder = _selectedFurniture.GetComponent<FurnitureHandling>().IsPlaceHolder;
            if(hitPoint.Equals(Vector2.negativeInfinity)) hitPoint = _selectedFurniture.transform.position + new Vector3(0, 0.001f);

            if(!isFurniturePlaceHolder)
                checkPoint = hitPoint + new Vector2((_selectedFurniture.transform.localScale.x / 2) + ((_selectedFurniture.transform.localScale.x *size.x)/2)*-1, 0);
            else
                checkPoint = hitPoint + new Vector2((_selectedFurniture.transform.localScale.x / 2) * -1 + _selectedFurniture.transform.localScale.x / (2 * size.x), 0);

            Ray ray2 = new(transform.position, (Vector3)checkPoint - transform.position);
            RaycastHit2D[] hitArray;
            hitArray = Physics2D.GetRayIntersectionAll(ray2, 1000);
            bool hitRoom = false;
            foreach (RaycastHit2D hit2 in hitArray)
            {
                if (hit2.collider.gameObject.CompareTag("Room")) { hitRoom = true; break; }
            }
            if (!hitRoom)
            {
                ray2 = new(transform.position, (Vector3)hitPoint - transform.position);
                hitArray = Physics2D.GetRayIntersectionAll(ray2, 1000);
            }
            bool check = false;
            foreach (RaycastHit2D hit2 in hitArray)
            {
                if (hit2.collider.gameObject.CompareTag("Room"))
                {
                    check = hit2.collider.GetComponent<RoomData>().HandleFurniturePosition(hitArray, _selectedFurniture, hover, hitPoint, hitRoom);
                }
            }
            if (hover)
            {
                if (!check) _selectedFurniture.transform.position = hitPoint + new Vector2(0, (_selectedFurniture.transform.localScale.y / 2) * -1);
            }
            else
            {
                if (check)
                {
                    CheckFurnitureList(_selectedFurniture);
                    if (_mainScreen.SelectedFurnitureTray != null) _mainScreen.RemoveTrayItem(_mainScreen.SelectedFurnitureTray);
                }
                else
                {
                    if (_selectedFurniture.GetComponent<FurnitureHandling>().TempSlot != null)
                    {
                        int id = _selectedFurniture.GetComponent<FurnitureHandling>().TempSlot.roomId;
                        _rooms.transform.GetChild(id).GetChild(0).GetComponent<RoomData>().ResetPosition(_selectedFurniture, true);
                    }
                    else if (_selectedFurniture.GetComponent<FurnitureHandling>().Slot == null)
                    {
                        _changedFurnitureList.Remove(_selectedFurniture);
                        Destroy(_selectedFurniture);
                        _mainScreen.RevealTrayItem();
                        SelectedFurniture = null;
                    }
                }
                if (_selectedFurniture != null) {
                    _selectedFurniture.GetComponent<FurnitureHandling>().SetTransparency(1f);
                    _selectedFurniture.GetComponent<FurnitureHandling>().ResetFurniturePosition();
                    //SelectedFurniture = null;
                }
                //_mainScreen.DeselectTrayFurniture();
            }
        }

        public void SetFurniture(GameObject furniture)
        {
            GameObject furnitureObject = Instantiate(furniture.GetComponent<TrayFurniture>().FurnitureObject);
            furnitureObject.GetComponent<FurnitureHandling>().Furniture = furniture.GetComponent<TrayFurniture>().Furniture;
            furnitureObject.GetComponent<FurnitureHandling>().Position = new(-1, -1);
            furnitureObject.GetComponent<FurnitureHandling>().Slot = null;
            _tempSelectedFurniture = furnitureObject;
            SelectedFurniture = _tempSelectedFurniture;
            _selectedFurniture.GetComponent<FurnitureHandling>().SetTransparency(0.5f);
        }

        public void RemoveFurniture()
        {
            if (_selectedFurniture.GetComponent<FurnitureHandling>().Slot != null)
                _rooms.transform.GetChild(_selectedFurniture.GetComponent<FurnitureHandling>().Slot.roomId).GetChild(0).GetComponent<RoomData>().FreeFurnitureSlots(_selectedFurniture.GetComponent<FurnitureHandling>(), _selectedFurniture.GetComponent<FurnitureHandling>().Slot);
            else if (_selectedFurniture.GetComponent<FurnitureHandling>().TempSlot != null)
                _rooms.transform.GetChild(_selectedFurniture.GetComponent<FurnitureHandling>().TempSlot.roomId).GetChild(0).GetComponent<RoomData>().FreeFurnitureSlots(_selectedFurniture.GetComponent<FurnitureHandling>(), _selectedFurniture.GetComponent<FurnitureHandling>().TempSlot);
            _selectedFurniture.SetActive(false);
            _selectedFurniture.GetComponent<FurnitureHandling>().TempSlot = null;
            if (_selectedFurniture.GetComponent<FurnitureHandling>().Slot != null) ChangedFurnitureList.Add(_selectedFurniture);
            else if(ChangedFurnitureList.Contains(_selectedFurniture)) ChangedFurnitureList.Remove(_selectedFurniture);
            if(_selectedFurniture.GetComponent<FurnitureHandling>().Slot == null)
            {
                Destroy(_selectedFurniture);
                SelectedFurniture = null;
                return;
            }
            SelectedFurniture = null;
        }

        public void CheckFurnitureList(GameObject selectedFurniture)
        {
            if (!_changedFurnitureList.Contains(selectedFurniture))
            {
                if(selectedFurniture.GetComponent<FurnitureHandling>().Slot == null
                    || (!selectedFurniture.GetComponent<FurnitureHandling>().Slot.Equals(selectedFurniture.GetComponent<FurnitureHandling>().TempSlot)
                    || selectedFurniture.GetComponent<FurnitureHandling>().Slot.Rotated != selectedFurniture.GetComponent<FurnitureHandling>().IsRotated))
                    _changedFurnitureList.Add(selectedFurniture);
            }
            else
            {
                if (selectedFurniture.GetComponent<FurnitureHandling>().Slot == null
                    || (selectedFurniture.GetComponent<FurnitureHandling>().Slot.Equals(selectedFurniture.GetComponent<FurnitureHandling>().TempSlot)
                    && selectedFurniture.GetComponent<FurnitureHandling>().Slot.Rotated == selectedFurniture.GetComponent<FurnitureHandling>().IsRotated))
                    _changedFurnitureList.Remove(selectedFurniture);
            }
        }

        public void UnfocusFurniture()
        {
            _tempSelectedFurniture = null;
        }

        public void DeselectFurniture()
        {
            _tempSelectedFurniture = null;
            _mainScreen.DeselectTrayFurniture();
            if (_selectedFurniture != null)
            {
                if (_selectedFurniture.GetComponent<FurnitureHandling>().TempSlot == null) RemoveFurniture();
                else
                {
                    _rooms.transform.GetChild(_selectedFurniture.GetComponent<FurnitureHandling>().TempSlot.roomId).GetChild(0).GetComponent<RoomData>().ResetPosition(SelectedFurniture, true);
                    _selectedFurniture.GetComponent<FurnitureHandling>().SetTransparency(1f);
                    _selectedFurniture.GetComponent<FurnitureHandling>().ResetFurniturePosition();
                    SelectedFurniture = null;
                }
            }
        }

        public void ResetChanges()
        {
            foreach (GameObject furniture in ChangedFurnitureList)
            {
                if (furniture.GetComponent<FurnitureHandling>().TempSlot != null)
                {
                    int roomId = furniture.GetComponent<FurnitureHandling>().TempSlot.roomId;
                    _rooms.transform.GetChild(roomId).GetChild(0).GetComponent<RoomData>().FreeFurnitureSlots(furniture.GetComponent<FurnitureHandling>(), furniture.GetComponent<FurnitureHandling>().TempSlot);
                }
                furniture.GetComponent<FurnitureHandling>().ResetSlot();
                furniture.GetComponent<FurnitureHandling>().ResetDirection();
            }
            foreach (GameObject furniture in ChangedFurnitureList)
            {
                if (furniture.GetComponent<FurnitureHandling>().Slot == null)
                {
                    furniture.GetComponent<FurnitureHandling>().TempSlot = null;
                    Destroy(furniture);
                    continue;
                }
                int roomId = furniture.GetComponent<FurnitureHandling>().Slot.roomId;
                _rooms.transform.GetChild(roomId).GetChild(0).GetComponent<RoomData>().SetFurnitureSlots(furniture.GetComponent<FurnitureHandling>());
                _rooms.transform.GetChild(roomId).GetChild(0).GetComponent<RoomData>().ResetPosition(furniture, false);
                furniture.GetComponent<FurnitureHandling>().ResetFurniturePosition();
                furniture.GetComponent<FurnitureHandling>().SetScale();
                furniture.GetComponent<FurnitureHandling>().SetTransparency(1f);
                furniture.GetComponent<FurnitureHandling>().TempSlot = furniture.GetComponent<FurnitureHandling>().Slot;
                if (!furniture.activeInHierarchy)
                {
                    furniture.SetActive(true);
                    furniture.GetComponent<SpriteRenderer>().enabled = true;
                    furniture.GetComponent<BoxCollider2D>().enabled = true;
                }
            }
            ChangedFurnitureList.Clear();
            SelectedFurniture = null;
        }

        public void SaveChanges()
        {
            foreach (GameObject furniture in ChangedFurnitureList)
            {
                furniture.GetComponent<FurnitureHandling>().SaveDirection();
                FurnitureSlot oldSlot = furniture.GetComponent<FurnitureHandling>().Slot;
                furniture.GetComponent<FurnitureHandling>().SaveSlot();
                int roomId;
                if (furniture.GetComponent<FurnitureHandling>().Slot == null)
                {
                    roomId = oldSlot.roomId;
                    _rooms.transform.GetChild(roomId).GetChild(0).GetComponent<RoomData>().FreeFurnitureSlots(furniture.GetComponent<FurnitureHandling>(), oldSlot);
                    Destroy(furniture);
                    continue;
                }
                roomId = furniture.GetComponent<FurnitureHandling>().Slot.roomId;
                _rooms.transform.GetChild(roomId).GetChild(0).GetComponent<RoomData>().SetFurnitureSlots(furniture.GetComponent<FurnitureHandling>());
            }
            ChangedFurnitureList.Clear();
        }

        public void ToggleEdit()
        {
            editingMode = !editingMode;
            if (!_mainScreen.TrayOpen && editingMode) _mainScreen.ToggleTray(null);
            else if (_mainScreen.TrayOpen && !editingMode) _mainScreen.ToggleTray(null);
            if (editingMode)
            {
                _mainScreen.EnableTray(true);
                _soulHomeController.EditModeTrayHandle(true);
            }
            else if (!editingMode)
            {
                //DeselectFurniture();
                _mainScreen.EnableTray(false);
                _soulHomeController.EditModeTrayHandle(false);
            }
            //if(!editingMode) ResetChanges();
        }

        public void RotateFurniture()
        {
            _selectedFurniture.GetComponent<FurnitureHandling>().RotateFurniture();
            PlaceFurnitureToCurrent(false);
        }

        private void CheckScreenRotationStatus()
        {
            Debug.Log(Screen.orientation);
            if ((AppPlatform.IsMobile || AppPlatform.IsEditor) && Screen.orientation == ScreenOrientation.LandscapeLeft /*&& !rotated*/)
            {
                _rotated = true;

                HandleScreenRotation();

            }
            else if ((AppPlatform.IsMobile || AppPlatform.IsEditor) && Screen.orientation == ScreenOrientation.Portrait /*&& rotated*/)
            {
                _rotated = false;

                HandleScreenRotation();
            }
        }

        private void HandleScreenRotation()
        {
            float newAspect = _displayScreen.GetComponent<RectTransform>().rect.x / _displayScreen.GetComponent<RectTransform>().rect.y;
            if (_camera.aspect.Equals(newAspect)) return;
            _camera.aspect = newAspect;

            _camera.fieldOfView = 90;

            /*if (SelectedRoom != null)
                transform.position = new(transform.position.x, transform.position.y, -1 * GetCameraYDistance());
            else
                transform.position = new(transform.position.x, transform.position.y, -1 * GetCameraMaxDistance());*/

            _maxCameraDistance = GetCameraMaxDistance();
            _minCameraDistance = GetCameraMinDistance();
            if(!_rotated)
                transform.position = new(transform.position.x, transform.position.y, GetCameraXDistance());
            else
                transform.position = new(transform.position.x, transform.position.y, GetCameraYDistance());

            ClampCameraDistance(0);

            Vector3 bl = _camera.ViewportToWorldPoint(new Vector3(0, 0, Mathf.Abs(_camera.transform.position.z)));
            float currentX = transform.position.x;
            float currentY = transform.position.y;
            float offsetX = Mathf.Abs(currentX - bl.x);
            float offsetY = Mathf.Abs(currentY - bl.y);

            float y;
            if (cameraMinY + offsetY < cameraMaxY - offsetY)
                y = Mathf.Clamp(currentY, cameraMinY + offsetY, cameraMaxY - offsetY);
            else
                y = (cameraMinY + cameraMaxY) / 2;
            float x;
            if (cameraMinX + offsetX < cameraMaxX - offsetX)
                x = Mathf.Clamp(currentX, cameraMinX + offsetX, cameraMaxX - offsetX);
            else
                x = (cameraMinX+cameraMaxX)/2;
            Debug.Log("CurrentX:"+currentX+", CameraMinX:"+ cameraMinX + ", OffsetX:" + offsetX + ", CameraMaxX:" + cameraMaxX +", OffsetX:" + offsetX);
            transform.position = new(x, y, transform.position.z);

        }

        private bool CheckInteractableStatus()
        {
            if (_soulHomeController.ExitPending) return false;
            if (_soulHomeController.ConfirmPopupOpen) return false;

            return true;
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
}
