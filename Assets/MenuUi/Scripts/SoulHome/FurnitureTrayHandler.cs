using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
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
        private List<FurnitureListObject> _candidateListObjects = new(); // Matches the category/set filter
        private List<FurnitureListObject> _furnitureListObjects = new(); // What the tray shows
        [SerializeField] private FurnitureTrayCategoryButtonsHandler _decorateModeButtons;
        [SerializeField] private FurnitureTrayCategoryButton _decorateModeSetsButton;
        [SerializeField, Range(0f, 1f)] private float _minimizedHeightRatio = 0.65f;
        
        [SerializeField] private FurnitureTrayCategoryButtonsHandler _renovateModeButtons;
        [SerializeField] private TextMeshProUGUI _categoryText;
        [SerializeField] private TextMeshProUGUI _modeText;
        [SerializeField] private GameObject _setSelectMenu;
        private bool _isOnDecorateMode = true;
        private bool _isTrayMinimized = false;

        private FurnitureTrayCategoryButton _previousCategoryButton = null;

        public SmartHorizontalObjectList SmartList => _smartList;
        public string _currentSelectedSet;
        public int _currentSelectedCategory = 1; // shows all by default

        // Start is called before the first frame update
        void Awake()
        {
            _decorateModeButtons.OnCategoryButtonSelected += CategoryButtonClicked; // Connect the category button click event to the handler
            _renovateModeButtons.OnCategoryButtonSelected += CategoryButtonClicked;
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
            if (_isOnDecorateMode)
            {
                if (_category == 0) 
                {
                    if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.Finnish) _categoryText.text = "Lattia";
                    else if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.English) _categoryText.text = "Floor";
                    else _categoryText.text = "Lattia"; // Default to Finnish if language not set
                }
                else if (_category == 1) // Displays all
                {
                    if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.Finnish) _categoryText.text = "Kaikki";
                    else if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.English) _categoryText.text = "All";
                    else _categoryText.text = "Kaikki"; // Default to Finnish if language not set
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
                    FillSelectionButtonList(filtered_list); // TODO - replace with special, currently shows nothing (empty list)
                    if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.Finnish) _categoryText.text = "Erikoiset";
                    else if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.English) _categoryText.text = "Special";
                    else _categoryText.text = "Erikoiset"; // Default to Finnish if language not set
                    _smartList.ScrollToStart(); // Reset FurnitureTray position
                    return;
                }
                else if (_category == 3) {
                    if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.Finnish) _categoryText.text = "Katto";
                    else if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.English) _categoryText.text = "Ceiling";
                    else _categoryText.text = "Katto"; // Default to Finnish if language not set
                }
                else if (_category == 4) {
                    if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.Finnish) _categoryText.text = "Seinät";
                    else if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.English) _categoryText.text = "Walls";
                    else _categoryText.text = "Seinät"; // Default to Finnish if language not set
                }

                if (_currentSelectedSet != null) // if has a set filter active
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
            }
            else
            {
                if (_category == 0) {
                    if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.Finnish) _categoryText.text = "Lattia";
                    else if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.English) _categoryText.text = "Floor";
                    else _categoryText.text = "Lattia"; // Default to Finnish if language not set
                }
                //else if (_category == 1) Does not exist in renovate mode
                else if (_category == 2) {
                    if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.Finnish) _categoryText.text = "Reunat";
                    else if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.English) _categoryText.text = "Frames";
                    else _categoryText.text = "Reunat"; // Default to Finnish if language not set
                }
                //else if (_category == 3) Does not exist in renovate mode
                else if (_category == 4) {
                    if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.Finnish) _categoryText.text = "Seinät";
                    else if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.English) _categoryText.text = "Walls";
                    else _categoryText.text = "Seinät"; // Default to Finnish if language not set
                }
            }
            FillSelectionButtonList(filtered_list); // Replaces the existing slots with the filtered list
            Debug.Log("FurnitureTrayHandler: FilterTrayObjects() - Category: " + _category + ", Set: " + _currentSelectedSet);
            _smartList.ScrollToStart(); // Reset FurnitureTray position
        }

        public void ToggleMinimizeTray()
        {
            RectTransform rect = GetComponent<RectTransform>();
            _isTrayMinimized = !_isTrayMinimized;

            rect.anchorMax = new Vector2(rect.anchorMax.x, _isTrayMinimized ? _minimizedHeightRatio : 1f);
            rect.offsetMax = new Vector2(rect.offsetMax.x, 0f);
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
            //if (_previousCategoryButton != null) _previousCategoryButton.DeSelect();
            if (!_isOnDecorateMode)
            {
                _isOnDecorateMode = true;
                if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.Finnish) _modeText.text = "Sisustus";
                else if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.English) _modeText.text = "Decorate";
                else _modeText.text = "Sisustus"; // Default to Finnish if language not set
                _decorateModeButtons.gameObject.SetActive(true);
                _renovateModeButtons.gameObject.SetActive(false);
                if (!_decorateModeButtons.SelectCategory(_currentSelectedCategory))
                {
                    _currentSelectedCategory = 1; // All
                    _decorateModeButtons.SelectCategory(_currentSelectedCategory);
                }
            }
            else
            {
                _isOnDecorateMode = false;
                if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.Finnish) _modeText.text = "Remontti";
                else if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.English) _modeText.text = "Renovation";
                else _modeText.text = "Remontti"; // Default to Finnish if language not set
                _decorateModeButtons.gameObject.SetActive(false);
                _renovateModeButtons.gameObject.SetActive(true);
                if (!_renovateModeButtons.SelectCategory(_currentSelectedCategory))
                {
                    _currentSelectedCategory = 0; // Floor
                    _renovateModeButtons.SelectCategory(_currentSelectedCategory);
                }
            }
            _modeButton.GetComponent<ModeButtonHandler>().ToggleMode();
            FilterTrayObjects(_currentSelectedCategory); // refresh tray slots
        }
    }
}
