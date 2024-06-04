using System.Collections;
using System.Collections.Generic;
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
        private FurnitureTrayRefrence _furnitureRefrence;
        [SerializeField]
        private GameObject _trayContent;
        private List<GameObject> _changedTrayItemList = new();
        private GameObject _hiddenSlot = null;

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

        // Update is called once per frame
        void Update()
        {

        }

        private void OnEnable()
        {
            SetEditInfo();
        }

        public GameObject GetTrayContent()
        {
            return _trayContent;
        }

        public void AddFurnitureInitial(Furniture furniture)
        {
            if (furniture == null) return;

            GameObject furnitureObject = _furnitureRefrence.GetFurniture(furniture.Name);
            if (furnitureObject == null) return;
            if (_trayContent == null) _trayContent = transform.Find("Scroll View").GetChild(0).GetChild(0).gameObject;
            GameObject furnitureSlot = Instantiate(_traySlotObject, _trayContent.transform);
            furnitureSlot.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = furniture.Name;

            float slotSize = _trayContent.GetComponent<RectTransform>().rect.height * 0.9f;
            furnitureSlot.GetComponent<RectTransform>().sizeDelta = new(slotSize, slotSize);
            furnitureSlot.GetComponent<ResizeCollider>().Resize();

            GameObject trayFurniture = Instantiate(furnitureObject, furnitureSlot.transform);
            trayFurniture.GetComponent<TrayFurniture>().Furniture = furniture;
        }
        public void AddFurniture(Furniture furniture)
        {
            if (furniture == null) return;
            Debug.LogWarning("Check");
            GameObject furnitureObject = CheckChangeList(furniture);

            if (furnitureObject != null)
            {
                furnitureObject.SetActive(true);
                if (!furnitureObject.transform.GetChild(1).GetComponent<Image>().enabled) furnitureObject.transform.GetChild(1).GetComponent<Image>().enabled = true;
                furnitureObject.transform.GetChild(1).GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                _changedTrayItemList.Remove(furnitureObject);
            }
            else
            {
                furnitureObject = _furnitureRefrence.GetFurniture(furniture.Name);
                if (furnitureObject == null) return;
                if (_trayContent == null) _trayContent = transform.Find("Scroll View").GetChild(0).GetChild(0).gameObject;
                GameObject furnitureSlot = Instantiate(_traySlotObject, _trayContent.transform);
                furnitureSlot.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = furniture.Name;

                float slotSize = _trayContent.GetComponent<RectTransform>().rect.height * 0.9f;
                furnitureSlot.GetComponent<RectTransform>().sizeDelta = new(slotSize, slotSize);
                furnitureSlot.GetComponent<ResizeCollider>().Resize();

                Instantiate(furnitureObject, furnitureSlot.transform);
                _changedTrayItemList.Add(furnitureSlot);
            }
        }

        public void RemoveFurniture(Furniture furniture)
        {
            if (furniture == null) return;

            int trayFurnitureAmount = _trayContent.transform.childCount;
            Debug.LogWarning("TrayContent: "+ trayFurnitureAmount);

            for (int i = 0; i < trayFurnitureAmount; i++)
            {
                if (_trayContent.transform.GetChild(i).GetChild(1).GetComponent<TrayFurniture>().Furniture.Id == furniture.Id)
                {
                    GameObject itemToRemove = _trayContent.transform.GetChild(i).gameObject;
                    if (CheckChangeList(itemToRemove))
                    {
                        _changedTrayItemList.Remove(itemToRemove);
                        Destroy(itemToRemove);
                    }
                    else
                    {
                        itemToRemove.SetActive(false);
                        _changedTrayItemList.Add(itemToRemove);
                        _hiddenSlot = null;
                    }
                    break;
                }
            }

        }
        public void RemoveFurniture(GameObject trayFurniture)
        {
            if(trayFurniture == null) return;

            int trayFurnitureAmount = _trayContent.transform.childCount;

            for (int i = 0; i < trayFurnitureAmount; i++)
            {
                if (Object.ReferenceEquals(_trayContent.transform.GetChild(i).GetChild(1).gameObject, trayFurniture))
                {
                    GameObject itemToRemove = _trayContent.transform.GetChild(i).gameObject;
                    if (CheckChangeList(itemToRemove))
                    {
                        _changedTrayItemList.Remove(itemToRemove);
                        Destroy(itemToRemove);
                    }
                    else
                    {
                        itemToRemove.SetActive(false);
                        _changedTrayItemList.Add(itemToRemove);
                        _hiddenSlot = null;
                    }
                    break;
                }
            }
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
            for(int i = 0;i < amount; i++)
            {
                if(Object.ReferenceEquals(_changedTrayItemList[i].transform.GetChild(1).gameObject, item)) return true;
            }
            return false;
        }

        public GameObject CheckChangeList(Furniture furniture)
        {
            int amount = _changedTrayItemList.Count;
            Debug.Log("amount: "+ amount);
            for (int i = 0; i < amount; i++)
            {
                if (Object.ReferenceEquals(_changedTrayItemList[i].transform.GetChild(1).GetComponent<TrayFurniture>().Furniture, furniture)) return _changedTrayItemList[i];
            }
            return null;
        }

        public void ResetChanges()
        {
            int amount = _changedTrayItemList.Count;
            for (int i = 0; i < amount; i++)
            {
                if (_changedTrayItemList[i].activeSelf == false)
                {
                    _changedTrayItemList[i].SetActive(true);
                    if (!_changedTrayItemList[i].transform.GetChild(1).GetComponent<Image>().enabled) _changedTrayItemList[i].transform.GetChild(1).GetComponent<Image>().enabled = true;
                    _changedTrayItemList[i].transform.GetChild(1).GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
                }
                else Destroy(_changedTrayItemList[i]);
            }
            _changedTrayItemList.Clear();
        }

        public void SaveChanges()
        {
            int amount = _changedTrayItemList.Count;
            for (int i = 0; i < amount; i++)
            {
                if (_changedTrayItemList[i].activeSelf == false) Destroy(_changedTrayItemList[i]);
            }
            _changedTrayItemList.Clear();
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
