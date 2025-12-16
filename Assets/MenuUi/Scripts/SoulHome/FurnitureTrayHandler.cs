using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.ReferenceSheets;
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

        public GameObject HiddenSlot { get => _hiddenSlot;}
        public List<GameObject> ChangedTrayItemList { get => _changedTrayItemList;}

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
            Debug.Log("Count: "+list.Count);
            if (list == null && list.Count < 1) return;

            foreach (FurnitureListObject listObject in list.Get())
            {
                GameObject furnitureObject = _furnitureRefrence.GetSoulHomeTrayFurnitureObject(listObject.Name);
                if (furnitureObject == null) continue;

                if (_trayContent == null) _trayContent = transform.Find("Scroll View").GetChild(0).GetChild(0).gameObject;
                GameObject furnitureSlot = Instantiate(_traySlotObject, _trayContent.transform);
                furnitureSlot.GetComponent<FurnitureTraySlotHandler>().Name.text = listObject.Name;

                float slotSize = _trayContent.GetComponent<RectTransform>().rect.height * 0.9f;
                furnitureSlot.GetComponent<RectTransform>().sizeDelta = new(slotSize, slotSize);
                furnitureSlot.GetComponent<ResizeCollider>().Resize();

                GameObject trayFurniture = Instantiate(furnitureObject, furnitureSlot.transform);
                furnitureSlot.GetComponent<FurnitureTraySlotHandler>().FurnitureList = listObject;
                if(listObject.Count - listObject.GetInRoomCount() <= 0) furnitureSlot.SetActive(false);
            }

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
                if (list.Name.Equals(furniture.Name))
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
        }

        public GameObject TakeFurnitureFromTray(string furnitureName)
        {
            if (string.IsNullOrWhiteSpace(furnitureName)) return null;

            if (_trayContent == null) _trayContent = transform.Find("Scroll View").GetChild(0).GetChild(0).gameObject;

            foreach (Transform furnitureSlot in _trayContent.transform)
            {
                FurnitureListObject list = furnitureSlot.GetComponent<FurnitureTraySlotHandler>().FurnitureList;
                if (list.Name.Equals(furnitureName))
                {
                    foreach (Furniture furnitureInList in list.List)
                    {
                        if (furnitureInList.Position.Equals(new(-1, -1)))
                        {
                            GameObject furnitureObject = _furnitureRefrence.GetSoulHomeTrayFurnitureObject(furnitureInList.Name);
                            if (furnitureObject == null) return null;
                            GameObject newObject = Instantiate(furnitureObject, furnitureSlot.transform);
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
                if (furnitureSlot.GetComponent<FurnitureTraySlotHandler>().FurnitureList.Name.Equals(trayFurniture.GetComponent<TrayFurniture>().Furniture.Name))
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
                    slotObject.GetComponent<RectTransform>().sizeDelta = new(slotSize, slotSize);
                    slotObject.GetComponent<BoxCollider2D>().size = new(slotSize, slotSize);
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
    }
}
