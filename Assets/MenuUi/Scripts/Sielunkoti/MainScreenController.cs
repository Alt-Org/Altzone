using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using Prg.Scripts.Common;

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

        private bool _rotated = false;

        float _prevTapTime = 0;
        float _backDelay = 0;
        float inDelay = 0;

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
        }

        // Update is called once per frame
        void Update()
        {
            CheckTrayButtonStatus();
            //CheckHoverButtons();
            CheckFurnitureButtons();
            if (!CheckInteractableStatus()) return;

            if (transform.Find("Screen").GetComponent<RectTransform>().rect.width != transform.Find("Screen").GetComponent<BoxCollider2D>().size.x || transform.Find("Screen").GetComponent<RectTransform>().rect.height != transform.Find("Screen").GetComponent<BoxCollider2D>().size.y)
            //if ((Screen.orientation == ScreenOrientation.LandscapeLeft && !rotated) || (Screen.orientation == ScreenOrientation.Portrait && rotated))
            {
                _rotated = !_rotated;
                StartCoroutine(SetColliderSize());
            }

            ClickState clickState = ClickStateHandler.GetClickState();
            if (clickState is not ClickState.None)
            {
                RayPoint(clickState);
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
            bool soulHomeHit = false;
            foreach (RaycastHit2D hit1 in hit)
            {
                if (hit1.collider.gameObject.CompareTag("Overlay")) overlayHit = true;
                else if (!hit1.collider.gameObject.CompareTag("SoulHomeScreen"))
                {
                    if (click is ClickState.Start) {
                        if(_soulHomeTower.SelectedFurniture != null) _soulHomeTower.DeselectFurniture();
                    }
                }
            }
            bool trayHit = false;
            if (!overlayHit)
            {
                foreach (RaycastHit2D hit2 in hit)
                {
                    if (hit2.collider.gameObject.CompareTag("SoulHomeScreen"))
                    {
                        soulHomeHit = true;
                        transform.Find("Itemtray/Scroll View").gameObject.GetComponent<ScrollRect>().StopMovement();
                        transform.Find("Itemtray/Scroll View").gameObject.GetComponent<ScrollRect>().enabled = false;
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
                                HideTrayItem(_selectedFurnitureTray);
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
                        transform.Find("Itemtray/Scroll View").gameObject.GetComponent<ScrollRect>().enabled = true;
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
                            if (_soulHomeTower.SelectedFurniture.GetComponent<SpriteRenderer>().enabled)
                            {
                                _soulHomeTower.SelectedFurniture.GetComponent<SpriteRenderer>().enabled = false;
                                _soulHomeTower.SelectedFurniture.GetComponent<BoxCollider2D>().enabled = false;
                            }

                            if (click is ClickState.End)
                            {
                                if (_selectedFurnitureTray != null && !_selectedFurnitureTray.transform.parent.CompareTag("FurnitureTrayItem"))
                                {
                                    //Destroy(_selectedFurnitureTray); //This is temporaty setup until a create the handling to up the furniture into the tray.
                                    Debug.Log("Check1");
                                    if(!CheckAndRevealTrayItem(_selectedFurnitureTray)) AddTrayItem(_selectedFurnitureTray.GetComponent<TrayFurniture>().Furniture);
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
                            _tempSelectedFurnitureTray = hit2.collider.transform.GetChild(1).gameObject;
                            //_selectedFurnitureTray = _tempSelectedFurnitureTray;
                            //transform.Find("Itemtray/Scroll View").gameObject.GetComponent<ScrollRect>().StopMovement();
                            //transform.Find("Itemtray/Scroll View").gameObject.GetComponent<ScrollRect>().enabled = false;
                        }
                    }
                }
                if (!trayHit)
                {
                    transform.Find("Itemtray/Scroll View").gameObject.GetComponent<ScrollRect>().StopMovement();
                    transform.Find("Itemtray/Scroll View").gameObject.GetComponent<ScrollRect>().enabled = false;
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
                    transform.Find("Itemtray/Scroll View").gameObject.GetComponent<ScrollRect>().enabled = true;

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
                        RevealTrayItem();
                    }
                    if (!soulHomeHit && _soulHomeTower.TempSelectedFurniture != null)
                    {
                        if (_soulHomeTower.SelectedFurniture.GetComponent<FurnitureHandling>().TempSlot != null)
                        {
                            _soulHomeTower.SelectedFurniture.GetComponent<FurnitureHandling>().ResetFurniturePosition();
                            _soulHomeTower.SelectedFurniture.GetComponent<SpriteRenderer>().enabled = true;
                            _soulHomeTower.SelectedFurniture.GetComponent<BoxCollider2D>().enabled = true;
                            _soulHomeTower.UnfocusFurniture();
                            //_selectedFurnitureTray.GetComponent<Image>().enabled = false;
                        }
                        else
                        {
                            RevealTrayItem();
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
            transform.Find("Itemtray").GetComponent<FurnitureTrayHandler>().ResetChanges();
            _soulHomeController.ShowInfoPopup("Muutokset palautettu");
        }

        public void SaveChanges()
        {
            _soulHomeTower.SaveChanges();
            transform.Find("Itemtray").GetComponent<FurnitureTrayHandler>().SaveChanges();
            _soulHomeController.ShowInfoPopup("Muutokset tallennettu");
        }

        public void ToggleTray()
        {
            float width = transform.Find("Screen").GetComponent<BoxCollider2D>().size.x;
            GameObject tray = transform.Find("Itemtray").gameObject;
            if (!_trayOpen)
            {
                tray.transform.localPosition = new Vector2(tray.transform.localPosition.x - width * 0.8f + tray.transform.Find("EditButton").GetComponent<RectTransform>().rect.width, tray.transform.localPosition.y);
                _trayOpen = true;
                //transform.Find("ChangeHandleButtons/SaveChangesButton").gameObject.SetActive(true);
                //if (!_soulHomeTower.EditingMode) _soulHomeTower.ToggleEdit();
            }
            else
            {
                tray.transform.localPosition = new Vector2(tray.transform.localPosition.x + width * 0.8f - tray.transform.Find("EditButton").GetComponent<RectTransform>().rect.width, tray.transform.localPosition.y);
                _trayOpen = false;
                //transform.Find("ChangeHandleButtons/SaveChangesButton").gameObject.SetActive(false);
                //if (_soulHomeTower.EditingMode) _soulHomeTower.ToggleEdit();
            }
        }
        public void EnableTray(bool enable)
        {
            if (enable)
            {
                GameObject tray = transform.Find("Itemtray").gameObject;
                tray.SetActive(true);
                transform.Find("ChangeHandleButtons").gameObject.SetActive(true);
                transform.Find("FurnitureButtons").gameObject.SetActive(true);
            }
            else
            {
                GameObject tray = transform.Find("Itemtray").gameObject;
                tray.SetActive(false);
                transform.Find("ChangeHandleButtons").gameObject.SetActive(false);
                transform.Find("FurnitureButtons").gameObject.SetActive(false);
            }
        }

        public void SetFurniture()
        {
            if (_selectedFurnitureTray == null && _tempSelectedFurnitureTray != null) _selectedFurnitureTray = _tempSelectedFurnitureTray;
        }

        public void SetFurniture(GameObject trayFurniture)
        {
            GameObject furnitureObject = Instantiate(trayFurniture.GetComponent<FurnitureHandling>().TrayFurnitureObject, transform.Find("Itemtray"));
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
            transform.Find("Itemtray").GetComponent<FurnitureTrayHandler>().AddFurniture(furniture);
            if (_selectedFurnitureTray != null)
            {
                Destroy(_selectedFurnitureTray);
                _selectedFurnitureTray = null;
                _tempSelectedFurnitureTray = null;
            }
        }

        public void RemoveTrayItem(GameObject trayFurniture)
        {
            transform.Find("Itemtray").GetComponent<FurnitureTrayHandler>().RemoveFurniture(trayFurniture);
            _selectedFurnitureTray = null;
            if(_tempSelectedFurnitureTray != null && !_tempSelectedFurnitureTray.transform.parent.CompareTag("FurnitureTrayItem"))Destroy(_tempSelectedFurnitureTray);
            _tempSelectedFurnitureTray = null;
        }

        public void HideTrayItem(GameObject trayFurniture)
        {
            transform.Find("Itemtray").GetComponent<FurnitureTrayHandler>().HideFurnitureSlot(trayFurniture);
        }
        public void RevealTrayItem()
        {
            transform.Find("Itemtray").GetComponent<FurnitureTrayHandler>().RevealFurnitureSlot();
            if (_selectedFurnitureTray != null && !_selectedFurnitureTray.transform.parent.CompareTag("FurnitureTrayItem"))
            {
                Destroy(_selectedFurnitureTray);
                _selectedFurnitureTray = null;
                _tempSelectedFurnitureTray = null;
            }
        }
        public bool CheckAndRevealTrayItem(GameObject trayFurniture)
        {
            bool value = transform.Find("Itemtray").GetComponent<FurnitureTrayHandler>().CheckAndRevealHiddenSlot(trayFurniture);
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
            if (_soulHomeTower.ChangedFurnitureList.Count > 0 && CheckInteractableStatus())
            {
                transform.Find("ChangeHandleButtons/DiscardChangesButton").GetComponent<Button>().interactable = true;
                transform.Find("ChangeHandleButtons/SaveChangesButton").GetComponent<Button>().interactable = true;
            }
            else
            {
                transform.Find("ChangeHandleButtons/DiscardChangesButton").GetComponent<Button>().interactable = false;
                transform.Find("ChangeHandleButtons/SaveChangesButton").GetComponent<Button>().interactable = false;
            }
        }

        private bool CheckInteractableStatus()
        {
            if(_soulHomeController.ExitPending) return false;
            if (_soulHomeController.ConfirmPopupOpen) return false;

            return true;
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
                transform.Find("FurnitureButtons").Find("RotateFurniture").GetComponent<Button>().interactable = true;
                transform.Find("FurnitureButtons").Find("SetFurniture").GetComponent<Button>().interactable = true;
            }
            else
            {
                transform.Find("FurnitureButtons").Find("RotateFurniture").GetComponent<Button>().interactable = false;
                transform.Find("FurnitureButtons").Find("SetFurniture").GetComponent<Button>().interactable = false;
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
    }
}
