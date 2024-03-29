using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

namespace MenuUI.Scripts.SoulHome
{
    public class TowerController : MonoBehaviour
    {
        private Camera Camera;
        private Vector3 prevWideCameraPos = new(0,0);
        private float prevWideCameraFoV;
        private GameObject selectedRoom = null;
        private GameObject tempSelectedRoom = null;
        [SerializeField]
        private MainScreenController _mainScreen;
        [SerializeField]
        private float scrollSpeed = 2f;
        [SerializeField]
        private float scrollSpeedMouse = 2f;
        [SerializeField]
        private SpriteRenderer backgroundSprite;
        [SerializeField]
        private bool isometric = false;
        [SerializeField]
        private SoulHomeController _soulHomeController;
        [SerializeField]
        private RawImage _displayScreen;
        [SerializeField]
        private GameObject _rooms;

        private List<GameObject> _changedFurnitureList = new();

        private bool rotated = false;

        private GameObject _selectedFurniture;
        private GameObject _tempSelectedFurniture;
        private float _startFurnitureTime;
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

        private Vector2 prevp;
        private bool cameraMove = false;
        private Vector2 startScrollSlide = Vector2.zero;
        private Vector2 currentScrollSlide = Vector2.zero;
        private Vector2 currentScrollSlideDirection = Vector2.zero;

        private float outDelay = 0;
        private float inDelay = 0;

        public GameObject SelectedRoom { get => selectedRoom; private set => selectedRoom = value; }
        public GameObject SelectedFurniture { get => _selectedFurniture; private set => _selectedFurniture = value; }
        public List<GameObject> ChangedFurnitureList { get => _changedFurnitureList; set => _changedFurnitureList = value; }
        public bool EditingMode { get => editingMode;}

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
            if (Application.platform is RuntimePlatform.Android or RuntimePlatform.IPhonePlayer
                || (Application.platform == RuntimePlatform.WebGLPlayer && Screen.fullScreenMode != FullScreenMode.FullScreenWindow)
                || AppPlatform.IsSimulator) Camera.fieldOfView = 90;
            else if (AppPlatform.IsEditor
                || (Application.platform is RuntimePlatform.WebGLPlayer && Screen.fullScreenMode == FullScreenMode.FullScreenWindow)
                || !Application.isMobilePlatform) Camera.fieldOfView = 45;
            transform.localPosition = new(0, 0, transform.position.z);
            Vector3 bl = Camera.ViewportToWorldPoint(new Vector3(0, 0, Mathf.Abs(Camera.transform.position.z)));
            Vector3 tr = Camera.ViewportToWorldPoint(new Vector3(1, 1, Mathf.Abs(Camera.transform.position.z)));
            float currentX = transform.position.x;
            float currentY = transform.position.y;
            float offsetX = Mathf.Abs(currentX - bl.x);
            float offsetY = Mathf.Abs(currentY - bl.y);

            cameraWidth = Mathf.Abs(tr.x - bl.x);
            cameraHeight = Mathf.Abs(tr.y - bl.y);

            scrollSpeed = _mainScreen.transform.GetComponent<RectTransform>().rect.height / cameraHeight;

            Debug.Log(currentY + " : " + (cameraMinY + offsetY) + " : " + (cameraMaxY - offsetY));
            Debug.Log(currentX + " : " + (cameraMinX + offsetX) + " : " + (cameraMaxX - offsetX));

            float y = Mathf.Clamp(currentY, cameraMinY + offsetY, cameraMaxY - offsetY);
            float x = Mathf.Clamp(currentX, cameraMinX + offsetX, cameraMaxX - offsetX);
            transform.position = new(x, y, transform.position.z);
            EnhancedTouchSupport.Enable();
        }
            // Update is called once per frame
            void Update()
        {
            /*if (AppPlatform.IsMobile && Screen.orientation == ScreenOrientation.LandscapeLeft && !rotated)
            {
                Camera.aspect = _displayScreen.GetComponent<RectTransform>().rect.x / _displayScreen.GetComponent<RectTransform>().rect.y;
                if (!selectedRoom) Camera.fieldOfView = 45;
                else
                {
                    Camera.fieldOfView = 55;
                    prevWideCameraFoV = 45;
                }
                rotated = true;

                Vector3 bl = Camera.ViewportToWorldPoint(new Vector3(0, 0, Mathf.Abs(Camera.transform.position.z)));
                float currentX = transform.position.x;
                float currentY = transform.position.y;
                float offsetX = Mathf.Abs(currentX - bl.x);
                float offsetY = Mathf.Abs(currentY - bl.y);

                float y = Mathf.Clamp(currentY, cameraMinY + offsetY, cameraMaxY - offsetY);
                float x = Mathf.Clamp(currentX, cameraMinX + offsetX, cameraMaxX - offsetX);
                transform.position = new(x, y, transform.position.z);

            }
            else if (AppPlatform.IsMobile && Screen.orientation == ScreenOrientation.Portrait && rotated)
            {
                Camera.aspect = _displayScreen.GetComponent<RectTransform>().rect.x / _displayScreen.GetComponent<RectTransform>().rect.y;
                if (!selectedRoom) Camera.fieldOfView = 90;
                else
                {
                    Camera.fieldOfView = 60;
                    prevWideCameraFoV = 90;
                }
                rotated = false;

                Vector3 bl = Camera.ViewportToWorldPoint(new Vector3(0, 0, Mathf.Abs(Camera.transform.position.z)));
                float currentX = transform.position.x;
                float currentY = transform.position.y;
                float offsetX = Mathf.Abs(currentX - bl.x);
                float offsetY = Mathf.Abs(currentY - bl.y);

                float y = Mathf.Clamp(currentY, cameraMinY + offsetY, cameraMaxY - offsetY);
                float x = Mathf.Clamp(currentX, cameraMinX + offsetX, cameraMaxX - offsetX);
                transform.position = new(x, y, transform.position.z);

            }*/
            //Debug.Log(cameraWidth+" : "+ _mainScreen.transform.GetComponent<RectTransform>().rect.width);
            //Debug.Log(cameraMove);
            Touch touch = new();
            if(Touch.activeFingers.Count > 0) touch = Touch.activeTouches[0];
            if (Touch.activeFingers.Count > 0 && (touch.phase == UnityEngine.InputSystem.TouchPhase.Began || prevp == Vector2.zero)) prevp = touch.screenPosition;
            if (((AppPlatform.IsDesktop && !AppPlatform.IsSimulator && Input.GetMouseButton(0)) || Touch.activeFingers.Count == 1) && cameraMove)
            {
                if (selectedRoom == null && _selectedFurniture == null)
                {
                    Vector3 bl = Camera.ViewportToWorldPoint(new Vector3(0, 0, Mathf.Abs(Camera.transform.position.z)));
                    float currentY = transform.position.y;
                    float offsetY = Mathf.Abs(currentY-bl.y);
                    float targetY;
                    if (Touch.activeFingers.Count == 1)
                    {
                        //touch = Input.GetTouch(0);
                        if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began || prevp == Vector2.zero) prevp = touch.screenPosition;
                        Vector2 lp = touch.screenPosition;
                        targetY = currentY + (prevp.y - lp.y) / scrollSpeed;
                        //Debug.Log("Touch: Y: "+(prevp.y - lp.y));
                        if (touch.phase is UnityEngine.InputSystem.TouchPhase.Ended or UnityEngine.InputSystem.TouchPhase.Canceled)
                        {
                            startScrollSlide = new Vector2(0, Mathf.Abs((prevp.y - lp.y) / scrollSpeed));
                            currentScrollSlide = startScrollSlide;
                            currentScrollSlideDirection = new Vector2(0, prevp.y - lp.y);
                            currentScrollSlideDirection.Normalize();
                            prevp = Vector2.zero;
                        }
                        else prevp = touch.screenPosition;
                    }
                    else
                    {
                        float moveAmountY = Input.GetAxis("Mouse Y");
                        targetY = currentY - moveAmountY * scrollSpeedMouse;
                        currentScrollSlide = new Vector2(0, Mathf.Abs(moveAmountY * scrollSpeedMouse));
                        currentScrollSlideDirection = new Vector2(0,-1 * moveAmountY);
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

                    float currentX = transform.position.x;
                    float offsetX = Mathf.Abs(currentX - bl.x);
                    //float moveAmountX = Input.GetAxis("Mouse X");
                    float moveAmountX = 0;
                    float targetX = currentX - moveAmountX * scrollSpeedMouse;

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
                else if(_selectedFurniture == null)
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
                        float moveAmountY = Input.GetAxis("Mouse X");
                        targetX = currentX - moveAmountY * scrollSpeedMouse;
                        currentScrollSlide = new Vector2(Mathf.Abs(moveAmountY) * scrollSpeedMouse, 0);
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
                }
                //cameraMove = false;
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
                        Vector3 bl = Camera.ViewportToWorldPoint(new Vector3(0, 0, Mathf.Abs(Camera.transform.position.z)));
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
                        Vector3 bl = Camera.ViewportToWorldPoint(new Vector3(0, 0, Mathf.Abs(Camera.transform.position.z)));
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
        }

        void OnEnable()
        {
            Camera.aspect = _displayScreen.GetComponent<RectTransform>().rect.x / _displayScreen.GetComponent<RectTransform>().rect.y;
        }

        void OnDisable()
        {
            if(selectedRoom != null) ZoomOut();
            if (editingMode) ToggleEdit();
        }

        public bool FindRayPoint(Vector2 relPoint, ClickState click)
        {
            //if (click == ClickState.Start) cameraMove = true;
            //if(click is ClickState.Hold or ClickState.Move) cameraMove = true;
            //if (click == ClickState.End) cameraMove = false;
            cameraMove = true;


            Ray ray = Camera.ViewportPointToRay(relPoint);
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
                    if (hit2.collider.gameObject.tag == "ScrollRectCanvas")
                    {
                        hitPoint = hit2.point;
                        continue;
                    }

                    if (hit2.collider.gameObject.tag == "Furniture")
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

                    if (hit2.collider.gameObject.tag == "Room")
                    {
                        exitRoom = false;
                        //Debug.Log("Camera2: " + hit2.collider.gameObject.name);
                        //Vector3 hitPoint = hit2.transform.InverseTransformPoint(hit2.point);
                        //Debug.Log("Camera2: " + hitPoint);
                        //Debug.Log("Camera2: " + click);
                        GameObject roomObject;
                        if (isometric)
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
                            _tempRoomHitStart = Input.mousePosition;
                        }
                        else if (click is ClickState.Move or ClickState.Hold && tempSelectedRoom != null)
                        {
                            if (tempSelectedRoom != roomObject) tempSelectedRoom = null;
                        }

                        else if (click == ClickState.End && _selectedFurniture == null)
                        {
                            Vector2 _tempRoomHitEnd = new();
                            if (Touch.activeFingers.Count > 0)
                            {
                                Touch touch = Touch.activeTouches[0];
                                _tempRoomHitEnd = touch.screenPosition;
                            }
                            else if (AppPlatform.IsDesktop && !AppPlatform.IsSimulator)
                                _tempRoomHitEnd = Input.mousePosition;
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
                            else if (selectedRoom != null && selectedRoom != roomObject && tempSelectedRoom != null )
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
            if (((AppPlatform.IsDesktop && !AppPlatform.IsSimulator && (Input.GetMouseButton(0) || Input.GetMouseButtonUp(0))) || Touch.activeFingers.Count >= 1) && (furnitureObject != null || _selectedFurniture != null))
            {
                Debug.Log(furnitureObject);
                //Touch touch = Input.GetTouch(0);
                if (click == ClickState.Start && (selectedRoom != null || editingMode))
                {
                        _tempSelectedFurniture = furnitureObject;
                        _startFurnitureTime = Time.time;
                }
                else if (((_startFurnitureTime + 1 <= Time.time && selectedRoom != null) || editingMode) && _tempSelectedFurniture != null && _selectedFurniture == null)
                {
                    _selectedFurniture = _tempSelectedFurniture;
                    _selectedFurniture.GetComponent<FurnitureHandling>().SetTransparency(0.5f);
                    if (!editingMode) ToggleEdit();
                }
                else if (click is ClickState.Hold or ClickState.Move && _selectedFurniture != null)
                {
                    Vector2 checkPoint;
                    FurnitureSize size = _selectedFurniture.GetComponent<FurnitureHandling>().Furniture.Size;
                    if (size is FurnitureSize.OneXOne)
                        checkPoint = hitPoint + new Vector2(0, (_selectedFurniture.transform.localScale.y / 2) * -1);
                    else if(size is FurnitureSize.OneXTwo)
                        checkPoint = hitPoint + new Vector2((_selectedFurniture.transform.localScale.x / 2)/2 * -1, (_selectedFurniture.transform.localScale.y / 2) * -1);
                    else checkPoint = hitPoint + new Vector2(0, (_selectedFurniture.transform.localScale.y / 2) * -1);

                    //Debug.Log("HitPoint: "+ hitPoint);
                    //Debug.Log("CheckPoint: " + checkPoint);

                    Ray ray2 = new(transform.position, (Vector3)checkPoint - transform.position);
                    RaycastHit2D[] hitArray;
                    hitArray = Physics2D.GetRayIntersectionAll(ray2, 1000);
                    bool check = false;
                    foreach (RaycastHit2D hit2 in hitArray)
                    {
                        if (hit2.collider.gameObject.tag == "Room")
                        {
                            check = hit2.collider.GetComponent<RoomData>().HandleFurniturePosition(hitArray, _selectedFurniture, true);
                        }
                    }

                    if (!check)_selectedFurniture.transform.position = hitPoint + new Vector2(0,(_selectedFurniture.transform.localScale.y/2)*-1);
                }
                else if (click is ClickState.End /*or ClickState.Move*/ || furnitureObject != _tempSelectedFurniture)
                {
                    _tempSelectedFurniture = null;
                    if (_selectedFurniture != null) {

                        Vector2 checkPoint;
                        FurnitureSize size = _selectedFurniture.GetComponent<FurnitureHandling>().Furniture.Size;
                        if (size is FurnitureSize.OneXOne)
                            checkPoint = hitPoint + new Vector2(0, (_selectedFurniture.transform.localScale.y / 2) * -1);
                        else if (size is FurnitureSize.OneXTwo)
                            checkPoint = hitPoint + new Vector2((_selectedFurniture.transform.localScale.x / 2) / 2 * -1, (_selectedFurniture.transform.localScale.y / 2) * -1);
                        else checkPoint = hitPoint + new Vector2(0, (_selectedFurniture.transform.localScale.y / 2) * -1);

                        Ray ray2 = new(transform.position, (Vector3)checkPoint - transform.position);
                        RaycastHit2D[] hitArray;
                        hitArray = Physics2D.GetRayIntersectionAll(ray2, 1000);
                        bool check = false;
                        foreach (RaycastHit2D hit2 in hitArray)
                        {
                            if (hit2.collider.gameObject.tag == "Room")
                            {
                                check = hit2.collider.GetComponent<RoomData>().HandleFurniturePosition(hitArray, _selectedFurniture, false);
                            }
                        }

                        //bool check = selectedRoom.GetComponent<RoomData>().HandleFurniturePosition(checkPoint, Camera, _selectedFurniture, false);

                        if (check)
                        {
                            _changedFurnitureList.Add(_selectedFurniture);
                            if(_mainScreen.SelectedFurnitureTray != null) _mainScreen.RemoveTrayItem(_mainScreen.SelectedFurnitureTray);
                        }
                        else
                        {
                            if (_selectedFurniture.GetComponent<FurnitureHandling>().TempSlot != null)
                            {
                                int id = _selectedFurniture.GetComponent<FurnitureHandling>().TempSlot.roomId;
                                _rooms.transform.GetChild(id).GetChild(0).GetComponent<RoomData>().ResetPosition(_selectedFurniture, true);
                            }
                        }

                        _selectedFurniture.GetComponent<FurnitureHandling>().SetTransparency(1f);
                        _selectedFurniture.GetComponent<FurnitureHandling>().ResetFurniturePosition();
                        _selectedFurniture = null;
                    }
                }

            }


            if (!hitRoom && selectedRoom != null && exitRoom && click == ClickState.End)
            {
                ZoomOut();
            }
            return enterRoom;
        }

        public void ZoomIn(GameObject room)
        {
            _soulHomeController.SetRoomName(selectedRoom);
            prevWideCameraPos = Camera.transform.position;
            prevWideCameraFoV = Camera.fieldOfView;
            Camera.transform.position = new(room.transform.position.x, room.transform.position.y + 10f, -27.5f);
            if (Application.platform is RuntimePlatform.Android or RuntimePlatform.IPhonePlayer or RuntimePlatform.WebGLPlayer || AppPlatform.IsSimulator)
            {
                if((Application.platform is RuntimePlatform.WebGLPlayer && Screen.fullScreenMode != FullScreenMode.FullScreenWindow) || AppPlatform.IsSimulator) Camera.fieldOfView = 60;
                else if (Screen.orientation == ScreenOrientation.LandscapeLeft
                    || (Application.platform is RuntimePlatform.WebGLPlayer && Screen.fullScreenMode == FullScreenMode.FullScreenWindow)
                    || AppPlatform.IsEditor) Camera.fieldOfView = 55;
                else Camera.fieldOfView = 60;
            }
            else if (AppPlatform.IsEditor) Camera.fieldOfView = 55;
            outDelay = Time.time;

            Vector3 bl = Camera.ViewportToWorldPoint(new Vector3(0, 0, Mathf.Abs(Camera.transform.position.z)));
            Vector3 tr = Camera.ViewportToWorldPoint(new Vector3(1, 1, Mathf.Abs(Camera.transform.position.z)));

            cameraWidth = Mathf.Abs(tr.x - bl.x);
            cameraHeight = Mathf.Abs(tr.y - bl.y);

            scrollSpeed = _mainScreen.transform.GetComponent<RectTransform>().rect.width / cameraWidth;
            Debug.Log(_mainScreen.transform.GetComponent<RectTransform>().rect.width + ": " + cameraWidth + ": " + scrollSpeed);

            _mainScreen.EnableTray(true);

            //_displayScreen.GetComponent<RectTransform>().anchorMin = new(_displayScreen.GetComponent<RectTransform>().anchorMin.x, 0.4f);
            //_displayScreen.GetComponent<RectTransform>().anchorMax = new(_displayScreen.GetComponent<RectTransform>().anchorMax.x, 0.6f);
            //Camera.aspect = _displayScreen.GetComponent<RectTransform>().rect.x / _displayScreen.GetComponent<RectTransform>().rect.y;
        }

        public void ZoomOut()
        {
            if (selectedRoom != null && outDelay + 1f < Time.time)
            {
                if (_mainScreen.TrayOpen) _mainScreen.ToggleTray();
                selectedRoom = null;
                _soulHomeController.SetRoomName(selectedRoom);
                if (Application.platform is RuntimePlatform.WebGLPlayer && Screen.fullScreenMode != FullScreenMode.FullScreenWindow || AppPlatform.IsSimulator) Camera.fieldOfView = 90;
                else if (Screen.orientation == ScreenOrientation.LandscapeLeft
                    || (Application.platform is RuntimePlatform.WebGLPlayer && Screen.fullScreenMode == FullScreenMode.FullScreenWindow)
                    || AppPlatform.IsEditor) Camera.fieldOfView = 45;
                else Camera.fieldOfView = 90;

                Camera.transform.position = new(Camera.transform.position.x- Camera.transform.localPosition.x, Camera.transform.position.y, -50f);
                inDelay = Time.time;

                Vector3 bl = Camera.ViewportToWorldPoint(new Vector3(0, 0, Mathf.Abs(Camera.transform.position.z)));
                Vector3 tr = Camera.ViewportToWorldPoint(new Vector3(1, 1, Mathf.Abs(Camera.transform.position.z)));

                cameraWidth = Mathf.Abs(tr.x - bl.x);
                cameraHeight = Mathf.Abs(tr.y - bl.y);

                scrollSpeed = _mainScreen.transform.GetComponent<RectTransform>().rect.height / cameraHeight;
                Debug.Log(_mainScreen.transform.GetComponent<RectTransform>().rect.height + ": " + cameraWidth + ": " + scrollSpeed);

                _mainScreen.EnableTray(false);

                //_displayScreen.GetComponent<RectTransform>().anchorMin = new(_displayScreen.GetComponent<RectTransform>().anchorMin.x, 0.2f);
                //_displayScreen.GetComponent<RectTransform>().anchorMax = new(_displayScreen.GetComponent<RectTransform>().anchorMax.x, 0.8f);
                //Camera.aspect = _displayScreen.GetComponent<RectTransform>().rect.x / _displayScreen.GetComponent<RectTransform>().rect.y;
            }
        }

        public void SetFurniture(GameObject furniture)
        {
            var furnitureObject = Instantiate(furniture.GetComponent<TrayFurniture>().FurnitureObject);
            furnitureObject.GetComponent<FurnitureHandling>().Furniture = furniture.GetComponent<TrayFurniture>().Furniture;
            furnitureObject.GetComponent<FurnitureHandling>().Position = new(-1, -1);
            furnitureObject.GetComponent<FurnitureHandling>().Slot = null;
            _selectedFurniture = furnitureObject;
            _selectedFurniture.GetComponent<FurnitureHandling>().SetTransparency(0.5f);
        }

        public void RemoveFurniture()
        {
            if(_selectedFurniture.GetComponent<FurnitureHandling>().Slot != null)
                _rooms.transform.GetChild(_selectedFurniture.GetComponent<FurnitureHandling>().TempSlot.roomId).GetChild(0).GetComponent<RoomData>().FreeFurnitureSlots(_selectedFurniture.GetComponent<FurnitureHandling>().Furniture.Size, _selectedFurniture.GetComponent<FurnitureHandling>().Slot);
            else if (_selectedFurniture.GetComponent<FurnitureHandling>().TempSlot != null)
                _rooms.transform.GetChild(_selectedFurniture.GetComponent<FurnitureHandling>().TempSlot.roomId).GetChild(0).GetComponent<RoomData>().FreeFurnitureSlots(_selectedFurniture.GetComponent<FurnitureHandling>().Furniture.Size, _selectedFurniture.GetComponent<FurnitureHandling>().TempSlot);
            _selectedFurniture.SetActive(false);
            _selectedFurniture.GetComponent<FurnitureHandling>().TempSlot = null;
            ChangedFurnitureList.Add(_selectedFurniture);
            _selectedFurniture = null;
        }


        public void DeselectFurniture()
        {
            _tempSelectedFurniture = null;
            if (_selectedFurniture != null)
            {
                _rooms.transform.GetChild(_selectedFurniture.GetComponent<FurnitureHandling>().TempSlot.roomId).GetChild(0).GetComponent<RoomData>().ResetPosition(SelectedFurniture, true);
                _selectedFurniture.GetComponent<FurnitureHandling>().SetTransparency(1f);
                _selectedFurniture.GetComponent<FurnitureHandling>().ResetFurniturePosition();
                _selectedFurniture = null;
            }
        }

        public void ResetChanges()
        {
            foreach (GameObject furniture in ChangedFurnitureList)
            {
                if (furniture.GetComponent<FurnitureHandling>().TempSlot != null)
                {
                    int roomId = furniture.GetComponent<FurnitureHandling>().TempSlot.roomId;
                    _rooms.transform.GetChild(roomId).GetChild(0).GetComponent<RoomData>().FreeFurnitureSlots(
                        furniture.GetComponent<FurnitureHandling>().Furniture.Size,
                        furniture.GetComponent<FurnitureHandling>().TempSlot
                    );
                }
                furniture.GetComponent<FurnitureHandling>().ResetSlot();
            }
            foreach (GameObject furniture in ChangedFurnitureList)
            {
                if (furniture.GetComponent<FurnitureHandling>().Slot == null)
                {
                    Destroy(furniture);
                    continue;
                }
                int roomId = furniture.GetComponent<FurnitureHandling>().Slot.roomId;
                _rooms.transform.GetChild(roomId).GetChild(0).GetComponent<RoomData>().SetFurnitureSlots(
                    furniture.GetComponent<FurnitureHandling>().Furniture.Size,
                    furniture.GetComponent<FurnitureHandling>().Slot,
                    furniture.GetComponent<FurnitureHandling>().Furniture
                );
                _rooms.transform.GetChild(roomId).GetChild(0).GetComponent<RoomData>().ResetPosition(furniture, false);
                furniture.GetComponent<FurnitureHandling>().ResetFurniturePosition();
                furniture.GetComponent<FurnitureHandling>().SetScale();
                furniture.GetComponent<FurnitureHandling>().SetTransparency(1f);
                if (!furniture.activeInHierarchy)
                {
                    furniture.SetActive(true);
                    furniture.GetComponent<SpriteRenderer>().enabled = true;
                    furniture.GetComponent<BoxCollider2D>().enabled = true;
                }
            }
            ChangedFurnitureList.Clear();
        }

        public void SaveChanges()
        {
            foreach (GameObject furniture in ChangedFurnitureList)
            {
                furniture.GetComponent<FurnitureHandling>().SaveSlot();
                if (furniture.GetComponent<FurnitureHandling>().Slot == null)
                {
                    Destroy(furniture);
                }
            }
            ChangedFurnitureList.Clear();
        }

        public void ToggleEdit()
        {
            editingMode = !editingMode;
            if (!_mainScreen.TrayOpen && editingMode) _mainScreen.ToggleTray();
            else if (_mainScreen.TrayOpen && !editingMode) _mainScreen.ToggleTray();
            if(editingMode) _mainScreen.EnableTray(true);
            else if (!editingMode && selectedRoom == null) _mainScreen.EnableTray(false);
            if(!editingMode) ResetChanges();
        }
    }
}
