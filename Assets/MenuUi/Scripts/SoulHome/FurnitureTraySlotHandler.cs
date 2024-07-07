using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace MenuUI.Scripts.SoulHome
{
    public class FurnitureTraySlotHandler : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI _name;
        [SerializeField]
        private TextMeshProUGUI _amountField;
        private FurnitureListObject _furnitureList;
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

        public void SaveCount()
        {
            _savedCount = _furnitureList.Count - _furnitureList.GetInRoomCount();
        }
    }
}
