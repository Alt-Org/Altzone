using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.ReferenceSheets;
using Altzone.Scripts.Model.Poco.Game; // --------------------------------
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUI.Scripts.SoulHome
{
    public class FurnitureTrayHandler : MonoBehaviour
    {
        [SerializeField]
        private SoulHomeFurnitureReference _furnitureRefrence;
        [SerializeField]
        private SoulHomeController _controller;

        [SerializeField] private SmartHorizontalObjectList _smartList;
        private List<FurnitureListObject> _candidateListObjects = new(); // Matches the category/set filter, including sold out ones.
        private List<FurnitureListObject> _furnitureListObjects = new(); // What the tray shows: candidates that still have furniture left.
        [SerializeField] private GameObject _decorateModeButtons; //-----------------
        [SerializeField] private FurnitureTrayCategoryButton _decorateModeSetsButton; //-----------------
        
        [SerializeField] private GameObject _renovateModeButtons; //-----------------
        [SerializeField] private TextMeshProUGUI _categoryText; //-----------------
        [SerializeField] private TextMeshProUGUI _modeText; //-----------------
        [SerializeField] private GameObject _setSelectMenu; //-----------------
        [SerializeField] private GameObject _setSelectButtonContainer; //-----------------
        [SerializeField] private GameObject _setSelectButtonPrefab; //-----------------
        [SerializeField] private StorageFurnitureReference _storageFurnitureReference; // needed for set info? -------------------------
        private bool _isOnDecorateMode = true; // ----------------
        private bool _isTrayMinimized = false; // ----------------

        private string _currentSelectedSet; // -------------------
        private int _currentSelectedCategory = 1; // shows all by default ------------------

        private FurnitureTrayCategoryButton _previousCategoryButton = null; // -----------------

        public SmartHorizontalObjectList SmartList => _smartList;

        // Start is called before the first frame update
        void Awake()
        {
            foreach (FurnitureSetInfo setInfo in _storageFurnitureReference.Info) // creates a set filter button for each set
            {
                GameObject _setSelectButton = Instantiate(_setSelectButtonPrefab, _setSelectButtonContainer.transform);
                TextMeshProUGUI _setName = _setSelectButton.GetComponentInChildren<TextMeshProUGUI>();
                if (setInfo.SetFontName != null)
                {
                   _setSelectButton.transform.GetChild(1).GetComponent<Image>().sprite = setInfo.SetFontName;
                   _setSelectButton.transform.GetChild(1).GetComponent<Image>().gameObject.SetActive(true);
                   _setName.gameObject.SetActive(false);
                }
                else
                {
                    _setName.text = setInfo.SetName;
                    _setName.gameObject.SetActive(true); // probably not needed
                    _setSelectButton.transform.GetChild(1).GetComponent<Image>().gameObject.SetActive(false); // probably not needed
                }
                _setSelectButton.GetComponent<Button>().onClick.AddListener(() => SelectSet(setInfo.SetName));
            }
        }

        private void Update()
        {
            // Furniture is placed and returned from many places, so check if any shown item sold out or came back.
            if (_smartList != null && VisibleListOutdated()) RebuildVisibleList();
        }

        public void InitializeTray()
        {
            FurnitureList list = _controller.FurnitureList;
            if (list == null) return;

            FillSelectionButtonList(list);
        }

        public void FilterTrayObjects(int _category)
        {
            _currentSelectedCategory = _category;
            FurnitureList list = _controller.FurnitureList;
            if (list == null) return;
            FurnitureList filtered_list = new();
            
            if (_category == 0) _categoryText.text = "Lattia";
            else if (_category == 1) // Displays all
            {
                _categoryText.text = "Kaikki";
                if (_currentSelectedSet != null) // if has a set filter active
                {
                    foreach (var _furnitureListObject in list.List)
                    {
                        string[] nameParts = _furnitureListObject.Name.Split('_'); // get set name from furniture name
                        if (nameParts[1] == _currentSelectedSet)
                        {
                            filtered_list.List.Add(_furnitureListObject);
                        }
                    }
                    FillSelectionButtonList(filtered_list); // Replaces the existing slots with the filtered list
                    _smartList.ScrollToStart(); // Reset FurnitureTray position
                    return;
                }
                // else will show all sets
                FillSelectionButtonList(list);
                _smartList.ScrollToStart(); // Reset FurnitureTray position
                return;
            }
            else if (_category == 2) { // Should display special - or frames on renovate mode
                // Special items not implemented?
                if (_isOnDecorateMode) {
                    FillSelectionButtonList(filtered_list); // TODO - replace with special, currently shows nothing (empty list)
                    _categoryText.text = "Erikoiset";
                }
                else
                {
                    FillSelectionButtonList(filtered_list); // TODO - replace with frames
                    _categoryText.text = "Reunat";
                } 
                _smartList.ScrollToStart(); // Reset FurnitureTray position
                return;
            }
            else if (_category == 3) _categoryText.text = "Katto";
            else if (_category == 4) _categoryText.text = "Seinät";

            if (_currentSelectedSet != null) // if has a set filter active // all
            {
                foreach (var _furnitureListObject in list.List)
                {
                    string[] nameParts = _furnitureListObject.Name.Split('_'); // get set name from furniture name
                    if (_furnitureListObject.GetListObjectType() == (FurniturePlacement)_category && nameParts[1] == _currentSelectedSet)
                    {
                        filtered_list.List.Add(_furnitureListObject);
                    }
                }
            }
            else
            {
                foreach (var _furnitureListObject in list.List) // Adds all furniture objects that match the category to a new list
                {
                    if (_furnitureListObject.GetListObjectType() == (FurniturePlacement)_category)
                    {
                        filtered_list.List.Add(_furnitureListObject);
                    }
                }
            }
            
            FillSelectionButtonList(filtered_list); // Replaces the existing slots with the filtered list
            _smartList.ScrollToStart(); // Reset FurnitureTray position
        }
        
        public void SelectSet(string set)
        {
            if (set == "")
            {
               _currentSelectedSet = null; 
               _decorateModeSetsButton.DeSelect();
            }
            
            else
            {
                _currentSelectedSet = set;
                _decorateModeSetsButton.Select(); // if set filter active, make the button "Selected"
            }
            
            FilterTrayObjects(_currentSelectedCategory);
            _setSelectMenu.SetActive(false);
            _setSelectMenu.GetComponentInChildren<ScrollRect>().verticalNormalizedPosition = 1f; // reset scroll to the top
        }

        public void ToggleMinimizeTray()
        {
            if (!_isTrayMinimized)
            {
                RectTransform rect = GetComponent<RectTransform>();
                //rect.offsetMax = new Vector2(rect.offsetMax.x, -400f); // not ideal if screen size or resolution can change??
                _isTrayMinimized = true;

                rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 300f); // 100f
            }
            else
            {
                RectTransform rect = GetComponent<RectTransform>();
                //rect.offsetMax = new Vector2(rect.offsetMax.x, 0f);
                _isTrayMinimized = false;

                rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 462f); // 462f
            }
        }

        private void FillSelectionButtonList(FurnitureList furnitureList)
        {
            _candidateListObjects = furnitureList.Get();
            RebuildVisibleList();
        }

        // Sold out furniture is left out of the list, so it doesn't leave an empty gap in the tray.
        private void RebuildVisibleList()
        {
            _furnitureListObjects.Clear();
            foreach (FurnitureListObject listObject in _candidateListObjects)
            {
                if (listObject.GetAvailableCount() > 0) _furnitureListObjects.Add(listObject);
            }
            _smartList.Setup<FurnitureListObject>(_furnitureListObjects);
        }

        private bool VisibleListOutdated()
        {
            int visibleIndex = 0;
            foreach (FurnitureListObject listObject in _candidateListObjects)
            {
                if (listObject.GetAvailableCount() <= 0) continue;
                if (visibleIndex >= _furnitureListObjects.Count || _furnitureListObjects[visibleIndex] != listObject) return true;
                visibleIndex++;
            }
            return visibleIndex != _furnitureListObjects.Count;
        }

        // Updates which furniture is shown and the amounts on the visible slots.
        private void RefreshTray()
        {
            if (_smartList == null) return;
            if (VisibleListOutdated()) RebuildVisibleList();
            else _smartList.Refresh();
        }

        public void AddFurnitureToTray(Furniture furniture)
        {
            if (furniture == null || _controller.FurnitureList == null) return;

            // Look up the model, not the slots: the furniture's slot may be scrolled out of view, filtered out or sold out.
            foreach (FurnitureListObject list in _controller.FurnitureList.Get())
            {
                if (!list.Name.Equals(furniture.Name)) continue;
                foreach (Furniture furnitureInList in list.List)
                {
                    if (furnitureInList.Id == furniture.Id) furnitureInList.ResetPosition();
                }
                break;
            }
            RefreshTray();
        }

        public GameObject TakeFurnitureFromTray(string furnitureName)
        {
            if (string.IsNullOrWhiteSpace(furnitureName)) return null;

            // Look up the model, not a pooled slot which may have been recycled.
            foreach (FurnitureListObject list in _furnitureListObjects)
            {
                if (list.Name.Equals(furnitureName))
                {
                    foreach (Furniture furnitureInList in list.List)
                    {
                        if (furnitureInList.Position.Equals(new(-1, -1)))
                        {
                            // an extra furnitur icon gets instantiated
                            // make sure not to delete the furniture slot even if count 0
                            GameObject furnitureObject = _furnitureRefrence.GetSoulHomeTrayFurnitureObject(furnitureInList.Name);
                            if (furnitureObject == null) return null;
                            GameObject newObject = Instantiate(furnitureObject, transform);
                            // The preview must not steal raycasts from the gesture's original target.
                            foreach (Graphic graphic in newObject.GetComponentsInChildren<Graphic>(true))
                                graphic.raycastTarget = false;
                            newObject.GetComponent<TrayFurniture>().Furniture = furnitureInList;
                            return newObject;
                        }
                    }
                    Debug.LogWarning("No free furniture available. Something went wrong.");
                    return null;
                }
            }
            return null;
        }

        public bool RemoveFurnitureObject(GameObject trayFurniture)
        {
            if(trayFurniture == null) return false;

            RefreshTray();

            string furnitureName = trayFurniture.GetComponent<TrayFurniture>().Furniture.Name;
            foreach (FurnitureListObject list in _candidateListObjects)
            {
                if (list.Name == furnitureName) return true;
            }
            return false;
        }

        public void ResetChanges()
        {
            RefreshTray();
        }

        public void SaveChanges()
        {
            RefreshTray();

            // Daily task: all furniture has been placed, nothing is left in the tray.
            bool itemsLeft = false;
            if (_controller.FurnitureList != null)
            {
                foreach (FurnitureListObject listObject in _controller.FurnitureList.Get())
                {
                    if (listObject.GetAvailableCount() > 0) { itemsLeft = true; break; }
                }
            }

            if (!itemsLeft) gameObject.GetComponent<DailyTaskProgressListener>().UpdateProgress("1");
        }

        public void CategoryButtonClicked(FurnitureTrayCategoryButton button) // sets button and child icon color
        {
            if (button == null) return;
            if (_previousCategoryButton != null) _previousCategoryButton.DeSelect();
            _previousCategoryButton = button;
            button.Select();
        }

        public void ModeButtonClicked(GameObject _modeButton)
        {
            if (!_isOnDecorateMode)
            {
                _isOnDecorateMode = true;
                _modeText.text = "Sisustus";
                _modeButton.transform.GetChild(0).gameObject.SetActive(true);
                _modeButton.transform.GetChild(1).gameObject.SetActive(false);
                _decorateModeButtons.SetActive(true);
                _renovateModeButtons.SetActive(false);
                CategoryButtonClicked(_decorateModeButtons.transform.GetChild(0).GetComponent<FurnitureTrayCategoryButton>());
                // TODO - FillSelectionButtonList() with renovate sets category filter
            }
            else
            {
                _isOnDecorateMode = false;
                _modeText.text = "Remontti";
                _modeButton.transform.GetChild(0).gameObject.SetActive(false);
                _modeButton.transform.GetChild(1).gameObject.SetActive(true);
                _decorateModeButtons.SetActive(false);
                _renovateModeButtons.SetActive(true);
                CategoryButtonClicked(_renovateModeButtons.transform.GetChild(0).GetComponent<FurnitureTrayCategoryButton>());
                // TODO - FillSelectionButtonList() with decorate sets category filter
            }
        }
    }
}
