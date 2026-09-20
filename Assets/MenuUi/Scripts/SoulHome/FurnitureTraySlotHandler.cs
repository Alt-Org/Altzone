using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Audio; // ----------------------
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace MenuUI.Scripts.SoulHome
{
    public class FurnitureTraySlotHandler : SmartListItem //IBeginDragHandler, IEndDragHandler // MonoBehaviour
    {
        [SerializeField]
        private SoulHomeFurnitureReference _furnitureRefrence;
        [SerializeField]
        private GameObject _furnitureIconObject; // ----------------------
        [SerializeField]
        private TextMeshProUGUI _name;
        [SerializeField]
        private TextMeshProUGUI _amountField;
        private FurnitureListObject _furnitureList;
        private GameObject trayFurniture;
        private int _savedCount = 0;
        public TextMeshProUGUI Name { get => _name; set => _name = value; }
        public FurnitureListObject FurnitureList { get => _furnitureList;
            set
            {
                _furnitureList = value;
                _savedCount = _furnitureList.Count - _furnitureList.GetInRoomCount();
            }
        }

        public int SavedCount { get => _savedCount; }

        // Start is called before the first frame update
        void Start()
        {
            if (_name == null) _name = transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        }

        // Update is called once per frame
        void Update()
        {
            UpdateFurnitureCount(); //This should probably be moved elsewhere, cause we probably don't need to check this every frame.
        }

        public int UpdateFurnitureCount()
        {
            if (_furnitureList != null)
            {
                int value = _furnitureList.Count - _furnitureList.GetInRoomCount();
                if (value > 1)
                    _amountField.text = "x" + value.ToString();
                else
                    _amountField.text = "";
                if(value <= 0) gameObject.SetActive(false);
                else gameObject.SetActive(true);
                return value;
            }
            return -1;
        }

        public void UpdateFurniture()
        {
            if (_furnitureList == null) return;
            //Destroy(trayFurniture); // ----------------------
            _name.text = _furnitureList.Name;
            int value = _furnitureList.Count - _furnitureList.GetInRoomCount();
            if (value > 1)
                _amountField.text = "x" + value.ToString();
            else
                _amountField.text = "";
            GameObject furnitureObject = _furnitureRefrence.GetSoulHomeTrayFurnitureObject(_furnitureList.Name);
            //trayFurniture = Instantiate(furnitureObject, SelfRectTransform.transform);
            _furnitureIconObject.GetComponent<Image>().sprite = furnitureObject.GetComponent<Image>().sprite;
            _furnitureIconObject.GetComponent<Image>().preserveAspect = true;
        }

        public override void SetData<T1>(T1 data)
        {
            if (!CheckClassType<T1, FurnitureListObject>(data, out FurnitureListObject furnitureData)) return;
            _furnitureList = furnitureData;
            UpdateFurniture();
        }

        public override void ClearData()
        {
            _furnitureList = null;
            _savedCount = 0;
            if (_name != null) _name.text = "";
            if (_amountField != null) _amountField.text = "";
            gameObject.SetActive(false);
        }

        public void SaveCount()
        {
            if (_furnitureList == null) return;
            _savedCount = _furnitureList.Count - _furnitureList.GetInRoomCount();
        }
    }
}
