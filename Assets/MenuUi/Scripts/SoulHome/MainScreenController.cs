using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using MenuUi.Scripts.Window;
using Prg.Scripts.Common;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

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
        private GameObject _changeHandleButtonTray;
        [SerializeField]
        private GameObject _horizontalItemTray;
        [SerializeField] private RectTransform _screen;

        float _prevTapTime = 0;
        float _backDelay = 0;

        private bool _trayOpen = false;
        private GameObject _selectedFurnitureTray = null;
        private GameObject _tempSelectedFurnitureTray = null;
        private SmartHorizontalObjectList _trayGestureList;
        private string _trayGestureFurnitureName;
        private bool _trayFurnitureDragStarted;
        private bool _uiOwnsPointer;
        private readonly List<RaycastResult> _uiRaycastResults = new();
        private BoxCollider2D _screenCollider;
        private bool _colliderResizePending;

        internal bool TrayOpen { get => _trayOpen; set => _trayOpen = value; }
        internal GameObject SelectedFurnitureTray { get => _selectedFurnitureTray;}
        public GameObject TempSelectedFurnitureTray { get => _tempSelectedFurnitureTray;}

        public GameObject HoverButtons { get => _hoverButtons; }

        // Start is called before the first frame update
        void Start()
        {
            EnhancedTouchSupport.Enable();
            _screenCollider = _screen.GetComponent<BoxCollider2D>();
            EnableTray(false);
            _hoverButtons.GetComponent<SoulHomeHoverButtonHandler>()._screen = _screen;
        }

        // Update is called once per frame
        void Update()
        {
            CheckTrayButtonStatus();
            if (!_soulHomeController.CheckInteractableStatus())
            {
                CancelTrayGesture();
                return;
            }

            if (!_colliderResizePending && _screen.rect.size != _screenCollider.size) StartCoroutine(SetColliderSize());

            ClickState clickState = ClickStateHandler.GetClickState();
            if (clickState == ClickState.None || Touch.activeTouches.Count > 1)
            {
                CancelTrayGesture();
                _uiOwnsPointer = true;
            }
            if (clickState is not ClickState.None)
            {
                ClickType clickType = ClickStateHandler.GetClickType();
                if (clickType is ClickType.Click) RayPoint(clickState);
                // Mouse wheel or pinch over a menu (e.g. scrolling the set select list) belongs to that menu, not the tower zoom.
                else if (clickType is ClickType.TwoFingerOrScroll && !IsPointerOverUi())
                {
                    float distance = ClickStateHandler.GetPinchDistance();
                    bool scroll = ClickStateHandler.GetClickType(ClickInputDevice.Touch) is not ClickType.TwoFingerOrScroll;
                    _soulHomeTower.PinchZoom(distance, scroll);
                }
            }
            bool doubleTap = false;
            if(Touch.activeFingers.Count > 0 && clickState is ClickState.End)
            {
                if(Time.time < _prevTapTime+0.2f) doubleTap = true;
                _prevTapTime = Time.time;
            }
            bool rightClick = AppPlatform.IsDesktop && !AppPlatform.IsSimulator && Mouse.current != null && Mouse.current.rightButton.wasReleasedThisFrame;
            if ((rightClick || doubleTap) && _backDelay + 0.4f < Time.time)
            {
                if(!_soulHomeTower.EditingMode)_soulHomeTower.ZoomOut();
            }
        }

        private void OnEnable()
        {
            EnableTray(false);
        }

        private void OnDisable()
        {
            CancelTrayGesture();
            _soulHomeTower.ResetChanges();
            _colliderResizePending = false; // Disabling stops the resize coroutine before it clears the flag.
        }

        private void CancelTrayGesture()
        {
            if (_trayGestureList != null) _trayGestureList.CancelGesture();
            if (_trayFurnitureDragStarted)
            {
                if (_soulHomeTower.SelectedFurniture != null) _soulHomeTower.DeselectFurniture();
                else DeselectTrayFurniture();
            }
            ResetTrayGestureState();
        }

        private void ResetTrayGestureState()
        {
            _trayGestureList = null;
            _trayGestureFurnitureName = null;
            _trayFurnitureDragStarted = false;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
            {
                CancelTrayGesture();
                _uiOwnsPointer = true;
            }
        }

        private bool IsPointerOverUi()
        {
            if (EventSystem.current == null) return false;
            var pointer = new PointerEventData(EventSystem.current) {position = ClickStateHandler.GetClickPosition()};
            _uiRaycastResults.Clear();
            EventSystem.current.RaycastAll(pointer, _uiRaycastResults);
            if (_uiRaycastResults.Count == 0) return false;
            // Stop scroll input from going through the gaps
            GameObject target = _uiRaycastResults[0].gameObject;
            if (target.GetComponentInParent<FurnitureTraySlotHandler>() != null) return false;
            if (target.GetComponentInParent<Selectable>() != null) return true;
            ScrollRect scrollRect = target.GetComponentInParent<ScrollRect>();
            return scrollRect != null && scrollRect.GetComponentInParent<FurnitureTrayHandler>() == null;
        }

        private void RayPoint(ClickState click)
        {
            if (click == ClickState.Start)
            {
                CancelTrayGesture();
                _uiOwnsPointer = IsPointerOverUi();
            }
            try
            {
                if (!_uiOwnsPointer) ProcessRayPoint(click);
            }
            finally
            {
                if (click == ClickState.End)
                {
                    if (_trayFurnitureDragStarted && _soulHomeTower.SelectedFurniture == null) DeselectTrayFurniture();
                    ResetTrayGestureState();
                    _uiOwnsPointer = false;
                    if (_tempSelectedFurnitureTray != null) Destroy(_tempSelectedFurnitureTray);
                    _tempSelectedFurnitureTray = null;
                }
            }
        }

        private void ProcessRayPoint(ClickState click)
        {
            Vector2 position = ClickStateHandler.GetClickPosition();
            RaycastHit2D[] hits = Physics2D.GetRayIntersectionAll(_camera.ScreenPointToRay(position), 1000);
            if (HasTag(hits, "Overlay")) return;

            if (click == ClickState.Start) BeginTrayGesture(hits, position);
            if (_trayGestureList != null && !ContinueTrayGesture(click, position)) return;

            // A room collider behind the tray must not also process this position.
            bool trayHit = HasTag(hits, "FurnitureTray");
            bool soulHomeHit = false;
            foreach (RaycastHit2D hit in hits)
            {
                if (!trayHit && hit.collider.CompareTag("SoulHomeScreen"))
                {
                    soulHomeHit = true;
                    HandleScreenHit(hit, click);
                }
                if (hit.collider.CompareTag("FurnitureTray")) HandleTrayHit(hit, click);
                if (hit.collider.CompareTag("FurnitureTrayItem")) HandleTrayItemHit(hit, click);
            }
            if (!trayHit) SetTrayPreviewVisible(false);
            if (click is ClickState.End) FinishClick(soulHomeHit);
        }

        private void BeginTrayGesture(RaycastHit2D[] hits, Vector2 position)
        {
            foreach (RaycastHit2D hit in hits)
            {
                if (!hit.collider.CompareTag("FurnitureTrayItem")) continue;
                FurnitureTrayHandler tray = hit.collider.GetComponentInParent<FurnitureTrayHandler>();
                if (tray == null || tray.SmartList == null) continue;
                _trayGestureList = tray.SmartList;
                var slot = hit.collider.GetComponent<FurnitureTraySlotHandler>();
                if (slot != null && slot.FurnitureList != null)
                {
                    _trayGestureFurnitureName = slot.FurnitureList.Name;
                    break;
                }
            }
            if (_trayGestureList != null) _trayGestureList.BeginGesture(position);
        }

        // Returns false when the tray gesture keeps this position to itself (scrolling the tray, or the drag didn't get a furniture).
        private bool ContinueTrayGesture(ClickState click, Vector2 position)
        {
            var intent = _trayGestureList.ResolveGesture(position);
            if (intent != SmartHorizontalObjectList.DragIntent.Furniture || _trayGestureFurnitureName == null) return false;
            if (_trayFurnitureDragStarted) return true;
            if (click == ClickState.End) return false;

            _trayFurnitureDragStarted = true;
            if (_soulHomeTower.SelectedFurniture != null) _soulHomeTower.DeselectFurniture();
            RevealTrayItem();
            _selectedFurnitureTray = GetTrayHandler().TakeFurnitureFromTray(_trayGestureFurnitureName);
            return _selectedFurnitureTray != null;
        }

        private void HandleScreenHit(RaycastHit2D hit, ClickState click)
        {
            if (_tempSelectedFurnitureTray != null)
            {
                _selectedFurnitureTray = _tempSelectedFurnitureTray;
                _tempSelectedFurnitureTray = null;
            }
            if (_selectedFurnitureTray != null)
            {
                SetTrayPreviewVisible(false);
                if (_soulHomeTower.SelectedFurniture == null) _soulHomeTower.SetFurniture(_selectedFurnitureTray);
            }
            if (_soulHomeTower.SelectedFurniture != null) SetFurnitureVisible(_soulHomeTower.SelectedFurniture, true);

            if (_soulHomeTower.FindRayPoint(GetNormalizedScreenPoint(hit), click)) _backDelay = Time.time;
            if (_soulHomeTower.SelectedFurniture != null) SetTrayPreviewVisible(false);
        }

        private void HandleTrayHit(RaycastHit2D hit, ClickState click)
        {
            if (_selectedFurnitureTray != null)
            {
                SetTrayPreviewVisible(true);
                _selectedFurnitureTray.transform.position = hit.point;
            }

            GameObject selectedFurniture = _soulHomeTower.SelectedFurniture;
            if (selectedFurniture == null) return;

            if (click is ClickState.Start)
            {
                _soulHomeTower.DeselectFurniture();
                RevealTrayItem();
            }
            else SetFurnitureVisible(selectedFurniture, false);

            if (click is ClickState.End)
            {
                // Destroys the preview as well.
                if (_selectedFurnitureTray != null) AddTrayItem(_selectedFurnitureTray.GetComponent<TrayFurniture>().Furniture);
                _soulHomeTower.RemoveFurniture();
                _selectedFurnitureTray = null;
            }
        }

        private void HandleTrayItemHit(RaycastHit2D hit, ClickState click)
        {
            if (click is not ClickState.Start || _trayGestureList != null) return;

            if (_soulHomeTower.SelectedFurniture != null) _soulHomeTower.DeselectFurniture();
            RevealTrayItem();
            var slot = hit.collider.GetComponent<FurnitureTraySlotHandler>();
            if (slot == null || slot.FurnitureList == null) return;
            GameObject furnitureObject = GetTrayHandler().TakeFurnitureFromTray(slot.FurnitureList.Name);
            if (furnitureObject != null) _tempSelectedFurnitureTray = furnitureObject;
        }

        private void FinishClick(bool soulHomeHit)
        {
            // A preview that is still in its tray slot goes back to its place.
            if (_selectedFurnitureTray != null && _selectedFurnitureTray.transform.parent.CompareTag("FurnitureTrayItem"))
            {
                SetTrayPreviewVisible(true);
                _selectedFurnitureTray.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }

            GameObject selectedFurniture = _soulHomeTower.SelectedFurniture;
            if (!soulHomeHit && selectedFurniture != null)
            {
                FurnitureHandling furnitureHandling = selectedFurniture.GetComponent<FurnitureHandling>();
                if (furnitureHandling.TempSlot != null)
                {
                    furnitureHandling.ResetFurniturePosition(furnitureHandling.TempSlot.furnitureGrid is FurnitureGrid.LeftWall);
                    SetFurnitureVisible(selectedFurniture, true);
                    _soulHomeTower.UnfocusFurniture();
                }
                else
                {
                    _soulHomeTower.DeselectFurniture();
                    DeselectTrayFurniture();
                }
            }
            _tempSelectedFurnitureTray = null;
        }

        private static bool HasTag(RaycastHit2D[] hits, string tag)
        {
            foreach (RaycastHit2D hit in hits)
                if (hit.collider.CompareTag(tag)) return true;
            return false;
        }

        // Where the hit is on the Screen, from (0, 0) at its bottom left to (1, 1) at its top right.
        private static Vector2 GetNormalizedScreenPoint(RaycastHit2D hit)
        {
            Rect rect = hit.transform.GetComponent<RectTransform>().rect;
            Vector2 point = hit.transform.InverseTransformPoint(hit.point);
            return new((point.x - rect.xMin) / rect.width, (point.y - rect.yMin) / rect.height);
        }

        private void SetTrayPreviewVisible(bool visible)
        {
            if (_selectedFurnitureTray != null) _selectedFurnitureTray.GetComponent<Image>().enabled = visible;
        }

        private static void SetFurnitureVisible(GameObject furniture, bool visible)
        {
            furniture.GetComponent<SpriteRenderer>().enabled = visible;
            furniture.GetComponent<BoxCollider2D>().enabled = visible;
        }

        public void ResetChanges()
        {
            _soulHomeTower.ResetChanges();
            GetTray().GetComponent<FurnitureTrayHandler>().ResetChanges();
            if (Localization.Language is LanguageType.Finnish) _soulHomeController.ShowInfoPopup("Muutokset palautettu");
            else if (Localization.Language is LanguageType.English) _soulHomeController.ShowInfoPopup("Changes reverted.");
        }

        public void SaveChanges()
        {
            _soulHomeTower.SaveChanges();
            GetTray().GetComponent<FurnitureTrayHandler>().SaveChanges();
            if (Localization.Language is LanguageType.Finnish) _soulHomeController.ShowInfoPopup("Muutokset tallennettu");
            else if (Localization.Language is LanguageType.English) _soulHomeController.ShowInfoPopup("Changes saved.");

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
            if(tray is null) tray = GetTray();
            if (!_trayOpen) _trayOpen = true;
            else
            {
                tray.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                _trayOpen = false;
            }
        }
        public void EnableTray(bool enable)
        {
            GameObject tray = GetTray();
            if (enable)
            {
                OverlayPanelCheck.Instance?.ToggleBottomBar(false);
                tray.SetActive(true);
                _changeHandleButtonTray.SetActive(true);
            }
            else
            {
                OverlayPanelCheck.Instance?.ToggleBottomBar(true);
                tray.SetActive(false);
                _changeHandleButtonTray.SetActive(false);
            }
            CheckEditMode();
        }

        public void CheckEditMode()
        {
            if (_soulHomeTower.EditingMode) _soulHomeController.FurnitureName.gameObject.SetActive(false);
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

        private GameObject GetTray()
        {
            return _horizontalItemTray;
        }

        public void SetFurnitureInfo()
        {
            GameObject furnitureInfo = _soulHomeController.FurnitureName.gameObject;
            furnitureInfo.GetComponent<RectTransform>().anchorMax = new(1f, 0.4f);
            furnitureInfo.GetComponent<RectTransform>().anchorMin = new(0f, 0.3f);
            furnitureInfo.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }

        public void SetFurniture()
        {
            if (_selectedFurnitureTray == null && _tempSelectedFurnitureTray != null) _selectedFurnitureTray = _tempSelectedFurnitureTray;
        }

        public void SetFurniture(GameObject trayFurniture)
        {
            DeselectTrayFurniture(); // Don't leave the previous preview behind when it's replaced.
            GameObject furnitureObject = Instantiate(trayFurniture.GetComponent<FurnitureHandling>().TrayFurnitureObject, GetTray().transform);
            furnitureObject.GetComponent<TrayFurniture>().Furniture = trayFurniture.GetComponent<FurnitureHandling>().Furniture;
            _selectedFurnitureTray = furnitureObject;
            string name = furnitureObject.GetComponent<TrayFurniture>().Furniture.Name;
            gameObject.GetComponent<FindSymbolicFurniture>().FurniturePressed(name);
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
            if(_tempSelectedFurnitureTray != null && !_tempSelectedFurnitureTray.transform.parent.CompareTag("FurnitureTrayItem"))Destroy(_tempSelectedFurnitureTray);
            _tempSelectedFurnitureTray = null;
        }

        // Drops the furniture preview that was taken from the tray.
        public void RevealTrayItem()
        {
            if (_selectedFurnitureTray != null && !_selectedFurnitureTray.transform.parent.CompareTag("FurnitureTrayItem"))
            {
                Destroy(_selectedFurnitureTray);
                _tempSelectedFurnitureTray = null;
            }
            _selectedFurnitureTray = null;
        }

        private IEnumerator SetColliderSize()
        {
            _colliderResizePending = true;
            yield return new WaitForEndOfFrame();
            _screenCollider.size = _screen.rect.size;
            _colliderResizePending = false;
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
        public void CheckHoverButtons()
        {
            if(_soulHomeTower.SelectedFurniture != null /*&& _soulHomeTower.SelectedFurniture.GetComponent<SpriteRenderer>().enabled*/) _hoverButtons.SetActive(true);
            else _hoverButtons.SetActive(false);
        }
    }
}
