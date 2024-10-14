using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DebugUi.Scripts.BattleAnalyzer
{
    public class DebugFilterToggleHandler : MonoBehaviour
    {
        [SerializeField] Toggle _button;
        [SerializeField] TextMeshProUGUI _text;
        private int _filter = -1;
        private Action<int, bool> _setFilter;
        private bool _listenerBlock=false;

        private void Start()
        {
            _button.onValueChanged.AddListener(OnToggle);
        }

        public void SetAllFilter(Action<int, bool> setFilter)
        {
            _text.text = "Kaikki";
            _setFilter = setFilter;
        }

        public void SetFilter(int filter, string text, Action<int, bool> setFilter)
        {
            _filter = filter;
            _text.text = text;
            _setFilter = setFilter;
        }

        public IEnumerator SetValue(bool value, bool triggerListener = false)
        {
            _listenerBlock = true;
            GetComponent<Toggle>().isOn = value;
            if (!triggerListener)
            {
                yield return new WaitForEndOfFrame();
            }
            _listenerBlock = false;
        }

        private void OnToggle(bool toggle)
        {
            if (_listenerBlock) return;
            _setFilter.Invoke(_filter, toggle);
        }
    }
}
