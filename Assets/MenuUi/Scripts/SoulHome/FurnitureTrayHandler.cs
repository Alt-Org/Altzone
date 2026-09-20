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
        private GameObject _traySlotObject;
        [SerializeField]
        private SoulHomeFurnitureReference _furnitureRefrence;
        [SerializeField]
        private GameObject _trayContent;
        private List<GameObject> _changedTrayItemList = new();
        private GameObject _hiddenSlot = null;
        [SerializeField]
        private SoulHomeController _controller;

        private List<Furniture> _furnitureList = new();

        [SerializeField]
        private FurnitureTrayHandler _otherTray;
        [SerializeField]
        private bool _vertical = false;

        [SerializeField] private SmartHorizontalObjectList _smartList; //-----------------
        private List<FurnitureListObject> _furnitureListObjects = new(); //-----------------
        [SerializeField] private GameObject _decorateModeButtons; //-----------------
        [SerializeField] private GameObject _renovateModeButtons; //-----------------
        [SerializeField] private TextMeshProUGUI _categoryText; //-----------------
        [SerializeField] private TextMeshProUGUI _modeText; //-----------------
        private bool _isOnDecorateMode = true; // ----------------

        private GameObject _previousCategoryButton = null; // -----------------

        public GameObject HiddenSlot { get => _hiddenSlot;}
        public List<GameObject> ChangedTrayItemList { get => _changedTrayItemList;}
        public SmartHorizontalObjectList SmartList => _smartList;

        // Start is called before the first frame update
        void Awake()
        {
            if(_trayContent == null) _trayContent = transform.Find("Scroll View").GetChild(0).GetChild(0).gameObject;
        }

        private void OnEnable()
        {
            SetEditInfo();
        }

        public GameObject GetTrayContent()
        {
            return _trayContent;
        }

        public void InitializeTray()
        {
            FurnitureList list = _controller.FurnitureList;
            //Debug.Log("Count: "+list.Count);
            if (list == null) return;

            // FurnitureList = how many furniture items in total, FurnitureListObject = how many of that type?, Furniture = the actual furniture object
            FillSelectionButtonList(list); // ----------------
        }

        public void FilterTrayObjects(int _category) // ------------------------------------
        {
            FurnitureList list = _controller.FurnitureList;
            if (list == null) return;
            FurnitureList filtered_list = new();
            //FillSelectionButtonList(filtered_list); // Clear the existing slots before filtering
            
            if (_category == 0) _categoryText.text = "Lattia";
            else if (_category == 1) // Should display sets (sets not implemented?)
            {
                // currently shows all items
                FillSelectionButtonList(list);
                _trayContent.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0); // Reset FurnitureTray position
                _categoryText.text = "Sarjat";
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
                _trayContent.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0); // Reset FurnitureTray position
                return;
            }
            else if (_category == 3) _categoryText.text = "Katto";
            else if (_category == 4) _categoryText.text = "Seinät";

            foreach (var _furnitureListObject in list.List) // Adds all furniture objects that match the category to a new list
            {
                if (_furnitureListObject.GetListObjectType() == (FurniturePlacement)_category)
                {
                    filtered_list.List.Add(_furnitureListObject);
                }
            }
            FillSelectionButtonList(filtered_list); // Replaces the existing slots with the filtered list
            _trayContent.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0); // Reset FurnitureTray position
        }

         private void FillSelectionButtonList(FurnitureList furnitureList) // ----------------------
        {
            _smartList.OnNewDataRequested -= UpdateButtonHandlerData;
            _smartList.OnNewDataRequested += UpdateButtonHandlerData;
            _furnitureListObjects.Clear();

            foreach (FurnitureListObject listObject in furnitureList.Get())
            {
                _furnitureListObjects.Add(listObject);
            }
            _smartList.Setup<FurnitureListObject>(_furnitureListObjects);
        }

        private void UpdateButtonHandlerData(int targetIndex) // ----------------------
        {
            if (targetIndex < 0 || targetIndex >= _furnitureListObjects.Count) return;
            _smartList.UpdateContent<FurnitureListObject>(targetIndex, _furnitureListObjects[targetIndex]);
        }

        private void OnDestroy()
        {
            if (_smartList != null) _smartList.OnNewDataRequested -= UpdateButtonHandlerData;
        }

        public void AddFurnitureInitial(Furniture furniture)
        {
            if (furniture == null) return;

            GameObject furnitureObject = _furnitureRefrence.GetSoulHomeTrayFurnitureObject(furniture.Name);
            if (furnitureObject == null) return;

            _furnitureList.Add(furniture);

            if (_trayContent == null) _trayContent = transform.Find("Scroll View").GetChild(0).GetChild(0).gameObject;
            GameObject furnitureSlot = Instantiate(_traySlotObject, _trayContent.transform);
            furnitureSlot.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = furniture.Name;

            float slotSize = _trayContent.GetComponent<RectTransform>().rect.height * 0.9f;
            furnitureSlot.GetComponent<RectTransform>().sizeDelta = new(slotSize, slotSize);
            furnitureSlot.GetComponent<ResizeCollider>().Resize();

            GameObject trayFurniture = Instantiate(furnitureObject, furnitureSlot.transform);
            trayFurniture.GetComponent<TrayFurniture>().Furniture = furniture;
        }
        public void AddFurnitureToTray(Furniture furniture)
        {
            if (furniture == null) return;
            //Debug.LogWarning("Check");
            if (_trayContent == null) _trayContent = transform.Find("Scroll View").GetChild(0).GetChild(0).gameObject;

            //furniture.ResetPosition();

            //Debug.LogWarning("Name: " + furniture.Name);
            //Debug.LogWarning("Name: " + furniture.Position);
            foreach (Transform furnitureSlot in _trayContent.transform)
            {
                FurnitureListObject list = furnitureSlot.GetComponent<FurnitureTraySlotHandler>().FurnitureList;
                if (list != null && list.Name.Equals(furniture.Name))
                {
                    foreach (Furniture furnitureInList in list.List)
                    {
                        if(furnitureInList.Id == furniture.Id) furnitureInList.ResetPosition();
                    }

                    int count = furnitureSlot.GetComponent<FurnitureTraySlotHandler>().UpdateFurnitureCount();
                    //Debug.LogWarning("Total Count: " + furnitureSlot.GetComponent<FurnitureTraySlotHandler>().FurnitureList.Count);
                    //Debug.LogWarning("Count: " + count);
                    GameObject furnitureObject = CheckChangeList(furniture);
                    if (furnitureObject != null)
                    {
                        if (count == furnitureSlot.GetComponent<FurnitureTraySlotHandler>().SavedCount) _changedTrayItemList.Remove(furnitureSlot.gameObject);
                    }
                    else
                    {
                        if (count != furnitureSlot.GetComponent<FurnitureTraySlotHandler>().SavedCount) _changedTrayItemList.Add(furnitureSlot.gameObject);
                    }
                    return;
                }
            }
            _smartList.scroll_enabled = true; // ----------------------
        }

        public GameObject TakeFurnitureFromTray(string furnitureName)
        {
            if (string.IsNullOrWhiteSpace(furnitureName)) return null;

            if (_trayContent == null) _trayContent = transform.Find("Scroll View").GetChild(0).GetChild(0).gameObject;

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

            //return RemoveFurniture(trayFurniture.GetComponent<TrayFurniture>().Furniture);

            int trayFurnitureAmount = _trayContent.transform.childCount;

            foreach (Transform furnitureSlot in _trayContent.transform)
            {
                if (furnitureSlot.GetComponent<FurnitureTraySlotHandler>().FurnitureList?.Name == trayFurniture.GetComponent<TrayFurniture>().Furniture.Name)
                {
                    int count = furnitureSlot.GetComponent<FurnitureTraySlotHandler>().UpdateFurnitureCount();
                    if (CheckChangeList(furnitureSlot.gameObject))
                    {
                        if (count == furnitureSlot.GetComponent<FurnitureTraySlotHandler>().SavedCount) _changedTrayItemList.Remove(furnitureSlot.gameObject);
                    }
                    else
                    {
                        if (count != furnitureSlot.GetComponent<FurnitureTraySlotHandler>().SavedCount) _changedTrayItemList.Add(furnitureSlot.gameObject);
                        _hiddenSlot = null;
                    }
                    return true;
                }
            }
            return false;
        }

        public void HideFurnitureSlot(GameObject trayFurniture)
        {
            if (!trayFurniture.transform.parent.CompareTag("FurnitureTrayItem")) return;
            _hiddenSlot = trayFurniture.transform.parent.gameObject;
            _hiddenSlot.SetActive(false);
        }

        public void RevealFurnitureSlot()
        {
            if (_hiddenSlot != null)
            {
                _hiddenSlot.SetActive(true);
                if (!_hiddenSlot.transform.GetChild(1).GetComponent<Image>().enabled) _hiddenSlot.transform.GetChild(1).GetComponent<Image>().enabled = true;
                _hiddenSlot.transform.GetChild(1).GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            }
        }

        public bool CheckAndRevealHiddenSlot(GameObject trayFurniture)
        {
            if(_hiddenSlot == null) return false;
            if (Object.ReferenceEquals(_hiddenSlot.transform.GetChild(1).GetComponent<TrayFurniture>().Furniture, trayFurniture.GetComponent<TrayFurniture>().Furniture))
            {
                RevealFurnitureSlot();
                return true;
            }
            return false;
        }

        public bool CheckChangeList(GameObject item)
        {
            int amount = _changedTrayItemList.Count;
            foreach (GameObject furnitureSlot in _changedTrayItemList)
            {
                if (Object.ReferenceEquals(furnitureSlot, item)) return true;
            }
            return false;
        }

        public GameObject CheckChangeList(Furniture furniture)
        {
            foreach (GameObject furnitureSlot in _changedTrayItemList)
            {
                FurnitureListObject list = furnitureSlot.GetComponent<FurnitureTraySlotHandler>().FurnitureList;

                if (list == null) continue;
                foreach (Furniture furnitureInList in list.List)
                {
                    if (Object.ReferenceEquals(furnitureInList, furniture)) return furnitureSlot;
                }
            }
            return null;
        }

        public void ResetChanges()
        {
            int amount = _changedTrayItemList.Count;
            for (int i = 0; i < amount; i++)
            {
                _changedTrayItemList[i].GetComponent<FurnitureTraySlotHandler>().UpdateFurnitureCount();
            }
            _changedTrayItemList.Clear();
        }

        public void SaveChanges()
        {
            int amount = _changedTrayItemList.Count;
            for (int i = 0; i < amount; i++)
            {
                _changedTrayItemList[i].GetComponent<FurnitureTraySlotHandler>().SaveCount();
            }
            _changedTrayItemList.Clear();

            int itemsLeft = 0;
            foreach (Transform furnitureSlot in _trayContent.transform)
            {
                if (furnitureSlot.gameObject.activeSelf) itemsLeft++;
            }

            if (itemsLeft == 0) gameObject.GetComponent<DailyTaskProgressListener>().UpdateProgress("1");
        }

        private void SetEditInfo()
        {
            _hiddenSlot = _otherTray.HiddenSlot;
            _changedTrayItemList = _otherTray.ChangedTrayItemList;
        }

        public void SetTrayContentSize()
        {
            if (!_vertical)
            {
                int childCount = _trayContent.transform.childCount;
                float slotSize = _trayContent.GetComponent<RectTransform>().rect.height * 0.9f;

                for (int i = 0; i < childCount; i++)
                {
                    GameObject slotObject = _trayContent.transform.GetChild(i).gameObject;
                    slotObject.GetComponent<RectTransform>().sizeDelta = new(slotSize * 0.6f, slotSize);
                    slotObject.GetComponent<BoxCollider2D>().size = new(slotSize * 0.6f, slotSize);
                    slotObject.GetComponent<ResizeCollider>().Resize();
                }
            }
            else
            {
                int childCount = _trayContent.transform.childCount;
                float slotSize = (GetComponent<RectTransform>().rect.width * 0.5f) - 50f;
                for (int i = 0; i < childCount; i++)
                {
                    GameObject slotObject = _trayContent.transform.GetChild(i).gameObject;
                    _trayContent.GetComponent<GridLayoutGroup>().cellSize = new(slotSize, slotSize); ;
                    slotObject.GetComponent<RectTransform>().sizeDelta = new(slotSize, slotSize);
                    slotObject.GetComponent<BoxCollider2D>().size = new(slotSize, slotSize);
                    slotObject.GetComponent<ResizeCollider>().Resize();
                }
            }
        }

        public void CategoryButtonClicked(GameObject button) // sets button and child icon color
        {
            if (button == null) return;
            if (_previousCategoryButton != null)
            {
                _previousCategoryButton.transform.GetChild(0).GetComponent<Image>().color = new Color32(111, 198, 222, 255); // light blue
            }
            _previousCategoryButton = button;
            _previousCategoryButton.transform.GetChild(0).GetComponent<Image>().color = new Color32(255, 255, 255, 255); // white
            button.GetComponent<Button>().Select();
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
                CategoryButtonClicked(_renovateModeButtons.transform.GetChild(0).gameObject);
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
                CategoryButtonClicked(_decorateModeButtons.transform.GetChild(0).gameObject);
                // TODO - FillSelectionButtonList() with decorate sets category filter
            }
            //_isOnDecorateMode = !_isOnDecorateMode;
        }
    }
}
