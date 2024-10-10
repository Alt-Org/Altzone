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
        private int _filter;
        private Action<int, bool> _setFilter;

        private void Start()
        {
            _button.onValueChanged.AddListener(OnToggle);
        }

        public void SetFilter(int filter, string text, Action<int, bool> setFilter)
        {
            _filter = filter;
            _text.text = text;
            _setFilter = setFilter;
        }

        private void OnToggle(bool toggle)
        {
            _setFilter.Invoke(_filter, toggle);
        }
    }
}
