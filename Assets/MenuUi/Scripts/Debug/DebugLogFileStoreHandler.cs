using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DebugUi.Scripts.BattleAnalyzer
{
    public class DebugLogFileStoreHandler : MonoBehaviour
    {
        private DebugLogStoreField[] _fieldList = new DebugLogStoreField[4];

        public int FieldListCount {
            get
            {
                int logs = 0;
                for(int i=0 ; i<_fieldList.Length; i++)
                {
                    if (_fieldList[i].Address == null) continue;
                    logs++;
                }
                return logs;
            }
        }
        // Start is called before the first frame update
        void Start()
        {
            int i = 0;
            foreach(Transform t in transform)
            {
                if (t.GetComponent<DebugLogStoreField>() == null) return;
                t.GetComponent<DebugLogStoreField>().Client = i;
                _fieldList[i] = t.GetComponent<DebugLogStoreField>();
                i++;
                if (i == _fieldList.Length) break;
            }
        }

        private void OnEnable()
        {
            DebugLogStoreField.OnAddressDeleted += ReorderAddresses;
        }
        private void OnDisable()
        {
            DebugLogStoreField.OnAddressDeleted -= ReorderAddresses;
        }

        public void SetLogAddress(string address)
        {
            foreach(DebugLogStoreField field in _fieldList)
            {
                if(string.IsNullOrWhiteSpace(field.Address))
                {
                    field.Address = address;
                    break;
                }
            }
        }

        public void SetLogAddress(string address, int client)
        {
            _fieldList[client].Address = address;
            ReorderAddresses();
        }

        public string GetAddress(int client)
        {
            return _fieldList[client].Address;
        }

        private void ReorderAddresses(int client=0)
        {
            for(int i = client; i < FieldListCount; i++)
            {
                if (_fieldList[i].Address == null)
                {
                    for(int j=i+1;j<FieldListCount; j++)
                    {
                        if(_fieldList[j].Address == null)
                        {
                            continue;
                        }
                        else
                        {
                            _fieldList[i].Address = _fieldList[j].Address;
                            _fieldList[j].Address = null;
                            break;
                        }
                    }
                }
                else continue;
            }
        }
    }
}
