using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace DebugUi.Scripts.BattleAnalyzer
{
    public class DebugSourceFilterHandler : MonoBehaviour, ISelectHandler, IDeselectHandler
    {
        [SerializeField] GameObject _togglePrefab;
        [SerializeField] Transform _filtersTransform;
        [SerializeField] TextMeshProUGUI _filterText;
        [SerializeField] GameObject _filtersPanel;

        private int _client = -1;
        private int _msgSourceFilterAll;
        private int _msgSourceFilterCurrent = -1;
        private IReadOnlyMsgStorage _storage;
        private Action<int, int> _setSourceFilter;
        private DebugFilterToggleHandler _toggleAll;

        // Start is called before the first frame update
        void Start()
        {

        }

        internal void SetInitialTimelineFilters(IReadOnlyMsgStorage storage, Action<int, int> setSourceFilter)
        {
            _msgSourceFilterAll = storage.GetSourceAllFlags();
            if(_msgSourceFilterCurrent < 0) _msgSourceFilterCurrent = _msgSourceFilterAll;
            _storage = storage;
            _setSourceFilter = setSourceFilter;

            SetInitialFilters();
        }

        internal void SetInitialLogBoxFilters(int client, IReadOnlyMsgStorage storage, int filter, Action<int, int> setSourceFilter)
        {
            _client = client;
            _msgSourceFilterAll = storage.GetSourceAllFlags();
            _msgSourceFilterCurrent = filter;
            _storage = storage;
            _setSourceFilter = setSourceFilter;

            SetInitialFilters();
        }

        internal void SetInitialFilters()
        {
            IReadOnlyList<int> flagList = _storage.GetSourceFlagList();

            for (int i = _filtersTransform.childCount - 1; i >= 0; i--)
            {
                Destroy(_filtersTransform.GetChild(i).gameObject);
            }
            ((List<int>)flagList).Sort((x, y) => _storage.GetSourceFlagName(x).CompareTo(_storage.GetSourceFlagName(y)));

            GameObject toggleAll = Instantiate(_togglePrefab, _filtersTransform);
            toggleAll.GetComponent<DebugFilterToggleHandler>().SetAllFilter(SetFilter);
            _toggleAll = toggleAll.GetComponent<DebugFilterToggleHandler>();
            StartCoroutine(_toggleAll.SetValue((_msgSourceFilterAll & _msgSourceFilterCurrent) != 0));
            foreach (int flag in flagList)
            {
                GameObject toggle = Instantiate(_togglePrefab, _filtersTransform);
                toggle.GetComponent<DebugFilterToggleHandler>().SetFilter(flag, _storage.GetSourceFlagName(flag), SetFilter);
                StartCoroutine(toggle.GetComponent<DebugFilterToggleHandler>().SetValue((flag & _msgSourceFilterCurrent) != 0));
            }
            SetText();
        }

        private void SetFilter(int flag, bool toggleValue)
        {
            if (flag < 0)
            {
                flag = _msgSourceFilterAll;
                foreach(Transform toggle in _filtersTransform)
                {
                    StartCoroutine(toggle.GetComponent<DebugFilterToggleHandler>().SetValue(toggleValue));
                }
            }
            if(toggleValue)
                _msgSourceFilterCurrent |= flag;
            else
                _msgSourceFilterCurrent &= ~flag;
            SetText();
            if(_msgSourceFilterCurrent != _msgSourceFilterAll)
                StartCoroutine(_toggleAll.SetValue(false));
            else
                StartCoroutine(_toggleAll.SetValue(true));
            _setSourceFilter.Invoke(_client, _msgSourceFilterCurrent);
        }

        private void SetText()
        {
            if(_msgSourceFilterCurrent == _msgSourceFilterAll)
            {
                _filterText.text = "Kaikki";
                return;
            }
            else if (_msgSourceFilterCurrent == 0)
            {
                _filterText.text = "Tyhjä";
                return;
            }
            foreach (int flag in _storage.GetSourceFlagList())
            {
                if(_msgSourceFilterCurrent == flag)
                {
                    _filterText.text = _storage.GetSourceFlagName(flag);
                    return;
                }
            }
            _filterText.text = "Sekoitus";
        }

        public void OnSelect(BaseEventData eventData)
        {
            _filtersPanel.SetActive(true);
        }
        public void OnDeselect(BaseEventData eventData)
        {
            _filtersPanel.SetActive(false);
        }
    }
}
