using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DebugUi.Scripts.BattleAnalyzer
{
    public class DebugLogStoreField : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _textField;
        [SerializeField] private Button _removeButton;
        private string _address = "";
        private int? _client = null;

        public delegate void AddressDeleted(int Client);
        public static event AddressDeleted OnAddressDeleted;

        public string Address { get => _address;
            set
            {
                _address = value;
                if (string.IsNullOrWhiteSpace(value))
                {
                    _address = null;
                    _textField.text = "-";
                }
                else _textField.text = _address;
            }
        }

        public int? Client { get => _client;
            set
            {
                if(_client != null) return;
                _client = value;
            }
        }


        // Start is called before the first frame update
        void Start()
        {
            _removeButton.onClick.AddListener(DeleteAddress);
        }

        private void DeleteAddress()
        {
            Address = null;
            OnAddressDeleted((int)Client);
        }
    }
}
