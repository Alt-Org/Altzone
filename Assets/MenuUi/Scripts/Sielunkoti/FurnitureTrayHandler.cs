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
        private GameObject _trayContent;
        private List<GameObject> _changedTrayItemList = new();
        // Start is called before the first frame update
        void Awake()
        {
            _trayContent = transform.Find("Scroll View").GetChild(0).GetChild(0).gameObject;
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void AddFurnitureInitial(Furniture furniture)
        {
            if (furniture == null) return;

            GameObject furnitureObject = _furnitureRefrence.GetFurniture(furniture.Name);
            if (furnitureObject == null) return;
            if (_trayContent == null) _trayContent = transform.Find("Scroll View").GetChild(0).GetChild(0).gameObject;
            GameObject furnitureSlot = Instantiate(_traySlotObject, _trayContent.transform);
            furnitureSlot.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = furniture.Name;

            GameObject trayFurniture = Instantiate(furnitureObject, furnitureSlot.transform);
            trayFurniture.GetComponent<TrayFurniture>().Furniture = furniture;
        }
        public void AddFurniture(Furniture furniture)
        {
            if (furniture == null) return;

            GameObject furnitureObject = CheckChangeList(furniture);

            if (furnitureObject != null)
            {
                furnitureObject.SetActive(true);
                _changedTrayItemList.Remove(furnitureObject);
            }
            else
            {
                furnitureObject = _furnitureRefrence.GetFurniture(furniture.Name);
                if (furnitureObject == null) return;
                if (_trayContent == null) _trayContent = transform.Find("Scroll View").GetChild(0).GetChild(0).gameObject;
                GameObject furnitureSlot = Instantiate(_traySlotObject, _trayContent.transform);
                furnitureSlot.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = furniture.Name;

                Instantiate(furnitureObject, furnitureSlot.transform);
                _changedTrayItemList.Add(furnitureSlot);
            }
        }

        public void RemoveFurniture(Furniture furniture)
        {
            if (furniture == null) return;

            int trayFurnitureAmount = _trayContent.transform.childCount;

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
                if (_trayContent.transform.GetChild(i).GetChild(1).gameObject == trayFurniture)
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
                    }
                    break;
                }
            }
        }

        public bool CheckChangeList(GameObject item)
        {
            int amount = _changedTrayItemList.Count;
            for(int i = 0;i < amount; i++)
            {
                if(_changedTrayItemList[i] == item) return true;
            }
            return false;
        }

        public GameObject CheckChangeList(Furniture furniture)
        {
            int amount = _changedTrayItemList.Count;
            Debug.Log("amount: "+ amount);
            for (int i = 0; i < amount; i++)
            {
                if (_changedTrayItemList[i].transform.GetChild(1).GetComponent<TrayFurniture>().Furniture == furniture) return _changedTrayItemList[i];
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
    }
}
