using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using Prg.Scripts.Common;
using Altzone.Scripts.Model.Poco.Game;

namespace MenuUI.Scripts.SoulHome
{
    public class MainScreenController : MonoBehaviour
    {
        [SerializeField]
        private SoulHomeController _soulHomeController;
        [SerializeField]
        private TowerController _soulHomeTower;
        [SerializeField]
        private Camera _camera;
        [SerializeField]
        private GameObject _hoverButtons;
        [SerializeField]
        private GameObject _leaveRoomButton;
        [SerializeField]
        private GameObject _furnitureButtonTray;
        [SerializeField]
        private GameObject _changeHandleButtonTray;
        [SerializeField]
        private GameObject _overlayBar;
        [SerializeField]
        private GameObject _verticalItemTray;
        [SerializeField]
        private GameObject _horizontalItemTray;
        [SerializeField]
        private GameObject _furnitureButtons;

        private bool _rotated = false;

        float _prevTapTime = 0;
        float _backDelay = 0;
        //float _inDelay = 0;

        private bool _trayOpen = false;
        private GameObject _selectedFurnitureTray = null;
        private GameObject _tempSelectedFurnitureTray = null;

        internal bool TrayOpen { get => _trayOpen; set => _trayOpen = value; }
        internal GameObject SelectedFurnitureTray { get => _selectedFurnitureTray;}
        public GameObject LeaveRoomButton { get => _leaveRoomButton;}
        public GameObject TempSelectedFurnitureTray { get => _tempSelectedFurnitureTray;}

        // Start is called before the first frame update
        void Start()
        {
            if (AppPlatform.IsMobile || AppPlatform.IsSimulator)
            {
                Screen.autorotateToPortrait = true;
                Screen.autorotateToPortraitUpsideDown = false;
                Screen.autorotateToLandscapeRight = false;
                Screen.autorotateToLandscapeLeft = true;
                Screen.orientation = ScreenOrientation.AutoRotation;
            }
            EnhancedTouchSupport.Enable();
            EnableTray(false);
            //transform.Find("Itemtray").GetComponent<RectTransform>().sizeDelta = new(GetComponent<RectTransform>().sizeDelta.x * 0.8f, transform.Find("Itemtray").GetComponent<RectTransform>().sizeDelta.y);
        }

        // Update is called once per frame
        void Update()
        {
            CheckTrayButtonStatus();
            //CheckHoverButtons();
            CheckFurnitureButtons();
            if (!_rotated)
            {
                GetTray().GetComponent<RectTransform>().sizeDelta = new(GetComponent<RectTransform>().rect.width * 0.8f - 50, GetTray().GetComponent<RectTransform>().sizeDelta.y);
                GetTrayHandler().SetTrayContentSize();
                GetTrayHandler().GetComponent<ResizeCollider>().Resize();
            }
            else
            {
                GetTray().GetComponent<RectTransform>().sizeDelta = new(GetComponent<RectTransform>().rect.width * 0.2f - 50, GetTray().GetComponent<RectTransform>().sizeDelta.y);
                GetTrayHandler().SetTrayContentSize();
                GetTrayHandler().GetComponent<ResizeCollider>().Resize();
            }
            if (!_soulHomeController.CheckInteractableStatus()) return;

            if (transform.Find("Screen").GetComponent<RectTransform>().rect.width != transform.Find("Screen").GetComponent<BoxCollider2D>().size.x || transform.Find("Screen").GetComponent<RectTransform>().rect.height != transform.Find("Screen").GetComponent<BoxCollider2D>().size.y)
            //if ((Screen.orientation == ScreenOrientation.LandscapeLeft && !rotated) || (Screen.orientation == ScreenOrientation.Portrait && rotated))
            {
                if(Screen.orientation is ScreenOrientation.LandscapeLeft) _rotated = true;
                else if(Screen.orientation is ScreenOrientation.Portrait) _rotated = false;
                StartCoroutine(ScreenRotation());
                StartCoroutine(SetColliderSize());
            }

            ClickState clickState = ClickStateHandler.GetClickState();
            if (clickState is not ClickState.None)
            {
                //Debug.Log(Touch.activeFingers[0].screenPosition);
                if (ClickStateHandler.GetClickType() is ClickType.Click)
                RayPoint(clickState);
                else if(ClickStateHandler.GetClickType() is ClickType.TwoFingerOrScroll)
                {
                    Debug.LogWarning("Scroll Test");
                    Debug.LogWarning(Mouse.current.scroll.ReadValue());
                    float distance = ClickStateHandler.GetPinchDistance();
                    if (ClickStateHandler.GetClickType(ClickInputDevice.Touch) is ClickType.TwoFingerOrScroll)
                    {
                        _soulHomeTower.PinchZoom(distance, false);
                    }
                    else
                    {
                        _soulHomeTower.PinchZoom(distance, true);
                    }
                }
                    
            }
            bool doubleTap = false;
            if(Touch.activeFingers.Count > 0 && clickState is ClickState.End)
            {
                if(Time.time < _prevTapTime+0.2f) doubleTap = true;
                _prevTapTime = Time.time;
            }
            if (((AppPlatform.IsDesktop && !AppPlatform.IsSimulator && Mouse.current.rightButton.wasReleasedThisFrame) || doubleTap /*(Touch.activeFingers.Count > 0 && touch.tapCount > 1)*/) && _backDelay + 0.4f < Time.time)
            {
                if(!_soulHomeTower.EditingMode)_soulHomeTower.ZoomOut();
                //inDelay = Time.time;
            }

        }

        private void OnEnable()
        {
            if (AppPlatform.IsMobile || AppPlatform.IsSimulator)
            {
                Screen.autorotateToPortrait = true;
                Screen.autorotateToPortraitUpsideDown = false;
                Screen.autorotateToLandscapeRight = false;
                Screen.autorotateToLandscapeLeft = true;
                Screen.orientation = ScreenOrientation.AutoRotation;
            }
            EnableTray(false);
        }

        private void OnDisable()
        {
            if (AppPlatform.IsMobile || AppPlatform.IsSimulator) Screen.orientation = ScreenOrientation.Portrait;
            _soulHomeTower.ResetChanges();
        }

        private void RayPoint(ClickState click)
        {
            //Debug.Log(click);
            //Debug.Log(Screen.orientation);

            Ray ray = _camera.ScreenPointToRay(ClickStateHandler.GetClickPosition());

            RaycastHit2D[] hit;
            hit = Physics2D.GetRayIntersectionAll(ray, 1000);
            bool overlayHit = false;
            bool soulHomeHit = false;
            foreach (RaycastHit2D hit1 in hit)
            {
                //Debug.LogWarning(hit1.collider.gameObject.ToString());
                if (hit1.collider.gameObject.CompareTag("Overlay")) overlayHit = true;
                /*else if (!hit1.collider.gameObject.CompareTag("SoulHomeScreen"))
                {
                    if (click is ClickState.Start) {
                        if(_soulHomeTower.SelectedFurniture != null) _soulHomeTower.DeselectFurniture();
                    }
                }*/
            }
            bool trayHit = false;
            if (!overlayHit)
            {
                foreach (RaycastHit2D hit2 in hit)
                {
                    if (hit2.collider.gameObject.CompareTag("SoulHomeScreen"))
                    {
                        soulHomeHit = true;
                        GetTray().transform.Find("Furniture Scroll View").gameObject.GetComponent<ScrollRect>().StopMovement();
                        GetTray().transform.Find("Furniture Scroll View").gameObject.GetComponent<ScrollRect>().enabled = false;
                        if (_tempSelectedFurnitureTray != null || _selectedFurnitureTray != null)
                        {
                            if (_tempSelectedFurnitureTray != null)
                            {
                                _selectedFurnitureTray = _tempSelectedFurnitureTray;
                                _tempSelectedFurnitureTray = null;
                            }
                            if (_selectedFurnitureTray.GetComponent<Image>().enabled) _selectedFurnitureTray.GetComponent<Image>().enabled = false;
                            if (_soulHomeTower.SelectedFurniture == null)
                            {
                                _soulHomeTower.SetFurniture(_selectedFurnitureTray);
                                //HideTrayItem(_selectedFurnitureTray);
                            }
                        }
                        if (_soulHomeTower.SelectedFurniture != null)
                        {
                            if (!_soulHomeTower.SelectedFurniture.GetComponent<SpriteRenderer>().enabled)
                            {
                                _soulHomeTower.SelectedFurniture.GetComponent<SpriteRenderer>().enabled = true;
                                _soulHomeTower.SelectedFurniture.GetComponent<BoxCollider2D>().enabled = true;
                            }
                        }

                        //Debug.Log(hit.collider.gameObject.name);
                        Vector3 hitPoint = hit2.transform.InverseTransformPoint(hit2.point);
                        //Debug.Log(hitPoint);
                        float x = hit2.transform.GetComponent<RectTransform>().rect.width;
                        float y = hit2.transform.GetComponent<RectTransform>().rect.height;
                        Vector2 relPos = new((x / 2 + hitPoint.x) / x, (y / 2 + hitPoint.y) / y);
                        //Debug.Log(relPos);
                        bool check = _soulHomeTower.FindRayPoint(relPos, click);
                        if (check) _backDelay = Time.time;
                        if (_soulHomeTower.SelectedFurniture != null)
                        {
                            if (_selectedFurnitureTray == null)
                            {
                                //SetFurniture();
                            }
                                if (_selectedFurnitureTray.GetComponent<Image>().enabled) _selectedFurnitureTray.GetComponent<Image>().enabled = false;
                                //_selectedFurnitureTray.transform.position = hit2.point;
                        }
                    }
                    if (hit2.collider.gameObject.CompareTag("FurnitureTray"))
                    {
                        trayHit = true;
                        GetTray().transform.Find("Furniture Scroll View").gameObject.GetComponent<ScrollRect>().enabled = true;
                        if (_selectedFurnitureTray == null)
                        {
                            if (_soulHomeTower.SelectedFurniture != null)
                            {
                                //SetFurniture(_soulHomeTower.SelectedFurniture);
                                //_selectedFurnitureTray.transform.position = hit2.point;
                            }
                        }
                        else
                        {
                            if (!_selectedFurnitureTray.GetComponent<Image>().enabled) _selectedFurnitureTray.GetComponent<Image>().enabled = true;
                            _selectedFurnitureTray.transform.position = hit2.point;

                        }
                        if (_soulHomeTower.SelectedFurniture != null)
                        {
                            //Debug.Log("Check2");
                            if (click is ClickState.Start)
                            {
                                _soulHomeTower.DeselectFurniture();
                                RevealTrayItem();

                            }
                            else if (_soulHomeTower.SelectedFurniture.GetComponent<SpriteRenderer>().enabled)
                            {
                                _soulHomeTower.SelectedFurniture.GetComponent<SpriteRenderer>().enabled = false;
                                _soulHomeTower.SelectedFurniture.GetComponent<BoxCollider2D>().enabled = false;
                            }

                            if (click is ClickState.End)
                            {
                                //Debug.Log("Check3");
                                if (_selectedFurnitureTray != null /*&& !_selectedFurnitureTray.transform.parent.CompareTag("FurnitureTrayItem")*/)
                                {
                                    //Destroy(_selectedFurnitureTray); //This is temporaty setup until a create the handling to up the furniture into the tray.
                                    //Debug.Log("Check1");
                                    if(!CheckAndRevealTrayItem(_selectedFurnitureTray)) AddTrayItem(_selectedFurnitureTray.GetComponent<TrayFurniture>().Furniture);
                                    Destroy(_selectedFurnitureTray);
                                }
                                _soulHomeTower.RemoveFurniture();
                                _selectedFurnitureTray = null;
                            }
                        }
                    }
                    if (hit2.collider.gameObject.CompareTag("FurnitureTrayItem"))
                    {
                        if (click is ClickState.Start)
                        {
                            if(_soulHomeTower.SelectedFurniture != null) _soulHomeTower.DeselectFurniture();
                            RevealTrayItem();
                            string furnitureName = hit2.collider.GetComponent<FurnitureTraySlotHandler>().FurnitureList.Name;
                            GameObject furnitureObject = GetTrayHandler().TakeFurnitureFromTray(furnitureName);
                            if(furnitureObject != null) _tempSelectedFurnitureTray = furnitureObject;
                            //_selectedFurnitureTray = _tempSelectedFurnitureTray;
                            //transform.Find("Itemtray/Scroll View").gameObject.GetComponent<ScrollRect>().StopMovement();
                            //transform.Find("Itemtray/Scroll View").gameObject.GetComponent<ScrollRect>().enabled = false;
                        }
                    }
                }
                if (!trayHit)
                {
                    GetTray().transform.Find("Furniture Scroll View").gameObject.GetComponent<ScrollRect>().StopMovement();
                    GetTray().transform.Find("Furniture Scroll View").gameObject.GetComponent<ScrollRect>().enabled = false;
                    if (_selectedFurnitureTray != null && _selectedFurnitureTray.GetComponent<Image>().enabled) _selectedFurnitureTray.GetComponent<Image>().enabled = false;

                }
                if (click is ClickState.End)
                {
                    if (_selectedFurnitureTray != null)
                    {
                        if (_selectedFurnitureTray.transform.parent.CompareTag("FurnitureTrayItem"))
                        {
                            if (!_selectedFurnitureTray.GetComponent<Image>().enabled) _selectedFurnitureTray.GetComponent<Image>().enabled = true;
                            _selectedFurnitureTray.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                            //_selectedFurnitureTray = null;
                        }
                        //else Destroy(_selectedFurnitureTray);
                    }
                    //_selectedFurnitureTray = null;
                    GetTray().transform.Find("Furniture Scroll View").gameObject.GetComponent<ScrollRect>().enabled = true;

                    /*if (_soulHomeTower.SelectedFurniture != null)
                    {
                        if (!_soulHomeTower.SelectedFurniture.GetComponent<SpriteRenderer>().enabled)
                        {
                            _soulHomeTower.SelectedFurniture.GetComponent<SpriteRenderer>().enabled = true;
                            _soulHomeTower.SelectedFurniture.GetComponent<BoxCollider2D>().enabled = true;
                        }
                        _soulHomeTower.DeselectFurniture();
                    }*/
                    if (_soulHomeTower.SelectedFurniture == null)
                    {
                        //RevealTrayItem();
                    }
                    if (!soulHomeHit && _soulHomeTower.SelectedFurniture != null)
                    {
                        if (_soulHomeTower.SelectedFurniture.GetComponent<FurnitureHandling>().TempSlot != null)
                        {
                            _soulHomeTower.SelectedFurniture.GetComponent<FurnitureHandling>().ResetFurniturePosition(_soulHomeTower.SelectedFurniture.GetComponent<FurnitureHandling>().TempSlot.furnitureGrid is FurnitureGrid.LeftWall);
                            _soulHomeTower.SelectedFurniture.GetComponent<SpriteRenderer>().enabled = true;
                            _soulHomeTower.SelectedFurniture.GetComponent<BoxCollider2D>().enabled = true;
                            _soulHomeTower.UnfocusFurniture();
                            //_selectedFurnitureTray.GetComponent<Image>().enabled = false;
                        }
                        else
                        {
                            //RevealTrayItem();
                            _soulHomeTower.DeselectFurniture();
                            DeselectTrayFurniture();
                        }
                    }
                    _tempSelectedFurnitureTray = null;
                }
            }
        }

        public void ResetChanges()
        {
            _soulHomeTower.ResetChanges();
            GetTray().GetComponent<FurnitureTrayHandler>().ResetChanges();
            if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.Finnish) _soulHomeController.ShowInfoPopup("Muutokset palautettu");
            else if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.English) _soulHomeController.ShowInfoPopup("Changes reverted.");
        }

        public void SaveChanges()
        {
            _soulHomeTower.SaveChanges();
            GetTray().GetComponent<FurnitureTrayHandler>().SaveChanges();
            if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.Finnish) _soulHomeController.ShowInfoPopup("Muutokset tallennettu");
            else if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.English) _soulHomeController.ShowInfoPopup("Changes saved.");

            // Checks if the interior is matching
            string furnitureStyle = null;
            bool matchingStyle = false;
            foreach (var furniture in _soulHomeController.FurnitureList.List)
            {
                if (furniture.GetInRoomCount() > 0)
                {
                    string[] parts = furniture.Name.Split('_');
                    string style = parts[1];

                    if (furnitureStyle == null)
                    {
                        furnitureStyle = style;
                        matchingStyle = true;
                    }
                    else if (furnitureStyle != style)
                    {
                        matchingStyle = false;
                        break;
                    }
                }
            }
            if (matchingStyle) gameObject.GetComponent<DailyTaskProgressListener>().UpdateProgress("1");
        }

        public void ToggleTray(GameObject tray)
        {
            float width = transform.GetComponent<RectTransform>().rect.width;
            if(tray is null) tray = GetTray();
            if (!_trayOpen)
            {
                if(!_rotated)tray.transform.localPosition = new Vector2(tray.transform.localPosition.x - width * 0.8f, tray.transform.localPosition.y);
                else tray.transform.localPosition = new Vector2(tray.transform.localPosition.x - width * 0.2f + tray.transform.Find("EditButton").GetComponent<RectTransform>().rect.width, tray.transform.localPosition.y);
                _trayOpen = true;
                RectTransform furnitureRectTransform = tray.transform.Find("FurnitureButton").GetComponent<RectTransform>();
                furnitureRectTransform.sizeDelta = new(width *0.2f, furnitureRectTransform.sizeDelta.y);
                furnitureRectTransform.gameObject.SetActive(true);
                RectTransform trapRectTransform = tray.transform.Find("TrapButton").GetComponent<RectTransform>();
                trapRectTransform.sizeDelta = new(width * 0.2f, trapRectTransform.sizeDelta.y);
                trapRectTransform.gameObject.SetActive(true);
                //transform.Find("ChangeHandleButtons/SaveChangesButton").gameObject.SetActive(true);
                //if (!_soulHomeTower.EditingMode) _soulHomeTower.ToggleEdit();
            }
            else
            {
                tray.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                _trayOpen = false;
                tray.transform.Find("FurnitureButton").GetComponent<RectTransform>().gameObject.SetActive(false);
                tray.transform.Find("TrapButton").GetComponent<RectTransform>().gameObject.SetActive(false);
                //transform.Find("ChangeHandleButtons/SaveChangesButton").gameObject.SetActive(false);
                //if (_soulHomeTower.EditingMode) _soulHomeTower.ToggleEdit();
            }
        }
        public void EnableTray(bool enable)
        {
            GameObject tray = GetTray();
            if (enable)
            {
                _overlayBar.SetActive(false);
                tray.SetActive(true);
                _changeHandleButtonTray.SetActive(true);
                _furnitureButtonTray.SetActive(true);
                SetFurnitureButtons();
            }
            else
            {
                if(!_rotated)
                _overlayBar.SetActive(true);
                tray.SetActive(false);
                _changeHandleButtonTray.SetActive(false);
                _furnitureButtonTray.SetActive(false);
            }
            CheckEditMode();
        }

        public void CheckEditMode()
        {
            if (_soulHomeTower.EditingMode)
            {
                if(!_rotated) _soulHomeController.FurnitureName.gameObject.SetActive(false);
                else
                {
                    if (_soulHomeTower.SelectedFurniture != null) _soulHomeController.FurnitureName.gameObject.SetActive(true);
                    else _soulHomeController.FurnitureName.gameObject.SetActive(false);
                }
            }
            else
            {
                if (_soulHomeTower.SelectedFurniture != null) _soulHomeController.FurnitureName.gameObject.SetActive(true);
                else _soulHomeController.FurnitureName.gameObject.SetActive(false);
            }
        }

        private FurnitureTrayHandler GetTrayHandler()
        {
            return GetTray().GetComponent<FurnitureTrayHandler>();
        }

        private FurnitureTrayHandler GetVerticalTrayHandler()
        {
            return GetVerticalTray().GetComponent<FurnitureTrayHandler>();
        }

        private FurnitureTrayHandler GetHorizontalTrayHandler()
        {
            return GetHorizontalTray().GetComponent<FurnitureTrayHandler>();
        }

        private GameObject GetTray()
        {
            if (!_rotated) return GetHorizontalTray();
            else return GetVerticalTray();
        }

        private GameObject GetVerticalTray()
        {
            return _verticalItemTray.gameObject;
        }
        private GameObject GetHorizontalTray()
        {
            return _horizontalItemTray.gameObject;
        }

        private void SetFurnitureButtons()
        {
            float width = _furnitureButtonTray.GetComponent<RectTransform>().rect.width;
            float height = _furnitureButtonTray.GetComponent<RectTransform>().rect.height;

            GameObject setButton = _furnitureButtonTray.transform.GetChild(0).gameObject;
            GameObject rotateButton = _furnitureButtonTray.transform.GetChild(1).gameObject;

            if (!_rotated)
            {
                setButton.GetComponent<RectTransform>().anchorMax = new(0.5f,0.75f);
                setButton.GetComponent<RectTransform>().anchorMin = new(0.5f, 0.75f);
                setButton.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                float buttonSizeHeight = (height/2)*0.9f;
                float buttonSizeWidth = width * 0.9f;
                float buttonSize;
                if (buttonSizeHeight > buttonSizeWidth) buttonSize = buttonSizeWidth;
                else buttonSize = buttonSizeHeight;
                setButton.GetComponent<RectTransform>().sizeDelta = new(buttonSize, buttonSize);
                rotateButton.GetComponent<RectTransform>().anchorMax = new(0.5f, 0.25f);
                rotateButton.GetComponent<RectTransform>().anchorMin = new(0.5f, 0.25f);
                rotateButton.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                rotateButton.GetComponent<RectTransform>().sizeDelta = new(buttonSize, buttonSize);
            }
            else
            {
                setButton.GetComponent<RectTransform>().anchorMax = new(0.25f, 0.5f);
                setButton.GetComponent<RectTransform>().anchorMin = new(0.25f, 0.5f);
                setButton.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                float buttonSizeWidth = (width / 2) * 0.9f;
                float buttonSizeHeight = height * 0.9f;
                float buttonSize;
                if (buttonSizeHeight < buttonSizeWidth) buttonSize = buttonSizeHeight;
                else buttonSize = buttonSizeWidth;
                setButton.GetComponent<RectTransform>().sizeDelta = new(buttonSize, buttonSize);
                rotateButton.GetComponent<RectTransform>().anchorMax = new(0.75f, 0.5f);
                rotateButton.GetComponent<RectTransform>().anchorMin = new(0.75f, 0.5f);
                rotateButton.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                rotateButton.GetComponent<RectTransform>().sizeDelta = new(buttonSize, buttonSize);
            }
        }

        private void SetBottomButtons()
        {

            GameObject discardButton = _changeHandleButtonTray.transform.GetChild(0).gameObject;
            GameObject saveButton = _changeHandleButtonTray.transform.GetChild(1).gameObject;

            if (!_rotated)
            {
                discardButton.GetComponent<RectTransform>().anchorMax = new(0.45f, 0.8f);
                discardButton.GetComponent<RectTransform>().anchorMin = new(0.1f, 0.2f);
                discardButton.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                saveButton.GetComponent<RectTransform>().anchorMax = new(0.9f, 0.8f);
                saveButton.GetComponent<RectTransform>().anchorMin = new(0.55f, 0.2f);
                saveButton.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }
            else
            {
                discardButton.GetComponent<RectTransform>().anchorMax = new(0.175f, 0.8f);
                discardButton.GetComponent<RectTransform>().anchorMin = new(0.025f, 0.2f);
                discardButton.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                saveButton.GetComponent<RectTransform>().anchorMax = new(0.975f, 0.8f);
                saveButton.GetComponent<RectTransform>().anchorMin = new(0.825f, 0.2f);
                saveButton.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            }
        }

        private void SetScreenSize()
        {

            GameObject screen = transform.Find("Screen").gameObject;

            if (!_rotated)
            {
                screen.GetComponent<RectTransform>().anchorMax = new(1f, 1f);
                screen.GetComponent<RectTransform>().anchorMin = new(0f, 0.1f);
                screen.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }
            else
            {
                screen.GetComponent<RectTransform>().anchorMax = new(0.8f, 1f);
                screen.GetComponent<RectTransform>().anchorMin = new(0.2f, 0f);
                screen.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            }
        }

        public void SetFurnitureInfo()
        {
            GameObject furnitureInfo = _soulHomeController.FurnitureName.gameObject;
            if (!_rotated)
            {
                furnitureInfo.GetComponent<RectTransform>().anchorMax = new(1f, 0.4f);
                furnitureInfo.GetComponent<RectTransform>().anchorMin = new(0f, 0.3f);
                furnitureInfo.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }
            else
            {
                furnitureInfo.GetComponent<RectTransform>().anchorMax = new(0.2f, 0.95f);
                furnitureInfo.GetComponent<RectTransform>().anchorMin = new(0f, 0.85f);
                furnitureInfo.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }
        }

        public void SetFurniture()
        {
            if (_selectedFurnitureTray == null && _tempSelectedFurnitureTray != null) _selectedFurnitureTray = _tempSelectedFurnitureTray;
        }

        public void SetFurniture(GameObject trayFurniture)
        {
            GameObject furnitureObject = Instantiate(trayFurniture.GetComponent<FurnitureHandling>().TrayFurnitureObject, GetTray().transform);
            furnitureObject.GetComponent<TrayFurniture>().Furniture = trayFurniture.GetComponent<FurnitureHandling>().Furniture;
            _selectedFurnitureTray = furnitureObject;
            //_tempSelectedFurnitureTray = _selectedFurnitureTray;
        }

        public void DeselectTrayFurniture()
        {
            if (_selectedFurnitureTray != null && !_selectedFurnitureTray.transform.parent.CompareTag("FurnitureTrayItem")) Destroy(_selectedFurnitureTray);
            _selectedFurnitureTray = null;
        }

        public void AddTrayItem(Furniture furniture)
        {
            GetTrayHandler().AddFurnitureToTray(furniture);
            if (_selectedFurnitureTray != null)
            {
                Destroy(_selectedFurnitureTray);
                _selectedFurnitureTray = null;
                _tempSelectedFurnitureTray = null;
            }
        }

        public void RemoveTrayItem(GameObject trayFurniture)
        {
            GetTrayHandler().RemoveFurnitureObject(trayFurniture);
            //_selectedFurnitureTray = null;
            if(_tempSelectedFurnitureTray != null && !_tempSelectedFurnitureTray.transform.parent.CompareTag("FurnitureTrayItem"))Destroy(_tempSelectedFurnitureTray);
            _tempSelectedFurnitureTray = null;
        }

        private void SwitchTray( GameObject prevContent, GameObject newContent)
        {
            int childCount = prevContent.transform.childCount;

            for (int i = 0; i < childCount; i++)
            {
                GameObject slotObject = prevContent.transform.GetChild(0).gameObject;
                slotObject.transform.SetParent(newContent.transform);
            }

        }

        public void HideTrayItem(GameObject trayFurniture)
        {
            GetTrayHandler().HideFurnitureSlot(trayFurniture);
        }
        public void RevealTrayItem()
        {
            GetTrayHandler().RevealFurnitureSlot();
            if (_selectedFurnitureTray != null && !_selectedFurnitureTray.transform.parent.CompareTag("FurnitureTrayItem"))
            {
                Destroy(_selectedFurnitureTray);
                _tempSelectedFurnitureTray = null;
            }
            _selectedFurnitureTray = null;
        }
        public bool CheckAndRevealTrayItem(GameObject trayFurniture)
        {
            bool value = GetTrayHandler().CheckAndRevealHiddenSlot(trayFurniture);
            if (value)
            {
                Destroy(_selectedFurnitureTray);
                _selectedFurnitureTray = null;
                _tempSelectedFurnitureTray = null;
                return true;
            }
            return false;
        }

        private IEnumerator SetColliderSize()
        {
            yield return new WaitForEndOfFrame();
            RectTransform rect = transform.Find("Screen").GetComponent<RectTransform>();
            float x = rect.rect.width;
            float y = rect.rect.height;
            transform.Find("Screen").GetComponent<BoxCollider2D>().size = new(x, y);
        }
        private void CheckTrayButtonStatus()
        {
            if (_soulHomeTower.ChangedFurnitureList.Count > 0 && _soulHomeController.CheckInteractableStatus())
            {
                _changeHandleButtonTray.transform.Find("DiscardChangesButton").GetComponent<Button>().interactable = true;
                _changeHandleButtonTray.transform.Find("SaveChangesButton").GetComponent<Button>().interactable = true;
            }
            else
            {
                _changeHandleButtonTray.transform.Find("DiscardChangesButton").GetComponent<Button>().interactable = false;
                _changeHandleButtonTray.transform.Find("SaveChangesButton").GetComponent<Button>().interactable = false;
            }
        }

        private void CheckHoverButtons()
        {
            if(_soulHomeTower.SelectedFurniture != null && _soulHomeTower.SelectedFurniture.GetComponent<SpriteRenderer>().enabled) _hoverButtons.SetActive(true);
            else _hoverButtons.SetActive(false);
        }
        private void CheckFurnitureButtons()
        {
            if (_soulHomeTower.SelectedFurniture != null)
            {
                _furnitureButtons.transform.Find("RotateFurniture").GetComponent<Button>().interactable = true;
                _furnitureButtons.transform.Find("SetFurniture").GetComponent<Button>().interactable = true;
            }
            else
            {
                _furnitureButtons.transform.Find("RotateFurniture").GetComponent<Button>().interactable = false;
                _furnitureButtons.transform.Find("SetFurniture").GetComponent<Button>().interactable = false;
            }
        }
        public void SetHoverButtons(Vector3 relPos)
        {
            float x = transform.Find("Screen").GetComponent<RectTransform>().rect.width;
            float y = transform.Find("Screen").GetComponent<RectTransform>().rect.height;
            Vector2 localPosition = new(x * relPos.x - x / 2, y * relPos.y - y / 2);
            Vector2 position = transform.Find("Screen").TransformPoint(localPosition);
            _hoverButtons.transform.position = position;
        }

        public IEnumerator ScreenRotation()
        {
            yield return new WaitForEndOfFrame();
            SetFurnitureButtons();
            SetBottomButtons();
            SetScreenSize();
            SetFurnitureInfo();
            if (_trayOpen)
            {
                if (_rotated)
                {
                    GetVerticalTray().SetActive(true);
                    GetHorizontalTray().SetActive(false);
                    ToggleTray(GetHorizontalTray());
                }
                else
                {
                    GetVerticalTray().SetActive(false);
                    GetHorizontalTray().SetActive(true);
                    ToggleTray(GetVerticalTray());
                }
                ToggleTray(GetTray());
            }
            GameObject verticalContent = GetVerticalTrayHandler().GetTrayContent();
            GameObject horizontalContent = GetHorizontalTrayHandler().GetTrayContent();
            if (_rotated)
            {
                _overlayBar.gameObject.SetActive(false);
                SwitchTray(horizontalContent, verticalContent);
            }
            else
            {
                if(!_trayOpen)_overlayBar.gameObject.SetActive(true);
                SwitchTray(verticalContent, horizontalContent);
            }
            GetTrayHandler().GetComponent<ResizeCollider>().Resize();
            GetTrayHandler().SetTrayContentSize();
            _soulHomeController.EditModeTrayResize();
        }
    }
}
