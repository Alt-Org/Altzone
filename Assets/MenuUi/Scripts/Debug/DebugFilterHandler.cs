using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DebugUi.Scripts.BattleAnalyzer
{
    enum FilterType
    {
        LogBox,
        Timeline
    }
    public class DebugFilterHandler : MonoBehaviour
    {
        [SerializeField] private FilterType _filterType;
        [SerializeField] private Toggle _emptyToggle;
        [SerializeField] private Toggle _infoToggle;
        [SerializeField] private Toggle _warningToggle;
        [SerializeField] private Toggle _errorToggle;

        [SerializeField] private DebugFilterSyncHandler _filterSync;
        [SerializeField] LogBoxController _controller;
        [SerializeField] int _client = 0;

        private MessageTypeOptions _typeOptions;
        private bool _includeEmpty = true;
        // Start is called before the first frame update
        void Start()
        {
            if(_filterType == FilterType.Timeline) _emptyToggle.isOn = true;
            _infoToggle.isOn = true;
            _warningToggle.isOn = true;
            _errorToggle.isOn = true;
            _typeOptions = MessageTypeOptions.Info | MessageTypeOptions.Warning | MessageTypeOptions.Error;

            if (_filterType == FilterType.Timeline) _emptyToggle.onValueChanged.AddListener(EmptyToggle);
            _infoToggle.onValueChanged.AddListener(InfoToggle);
            _warningToggle.onValueChanged.AddListener(WarningToggle);
            _errorToggle.onValueChanged.AddListener(ErrorToggle);
            _filterSync.InitializeFilterCalls(EmptyToggle, InfoToggle, WarningToggle, ErrorToggle);
        }
        private void EmptyToggle(bool toggle) => EmptyToggle(toggle, false);
        private void EmptyToggle(bool toggle,bool fromSync)
        {
            if (toggle)
            {
                _includeEmpty = true;
                ChangeColor(_emptyToggle, Color.white);
                if(!_emptyToggle.isOn)_emptyToggle.isOn = true;
            }
            else
            {
                _includeEmpty = false;
                ChangeColor(_emptyToggle, Color.grey);
                if (_emptyToggle.isOn) _emptyToggle.isOn = false;
            }
            if (!fromSync) SendFilters(MessageType.None, toggle);
        }

        private void InfoToggle(bool toggle) => InfoToggle(toggle, false);
        private void InfoToggle(bool toggle, bool fromSync)
        {
            if (toggle)
            {
                _typeOptions |= MessageTypeOptions.Info;
                ChangeColor(_infoToggle, Color.white);
                if (!_infoToggle.isOn) _infoToggle.isOn = true;
            }
            else
            {
                _typeOptions &= ~MessageTypeOptions.Info;
                ChangeColor(_infoToggle, Color.grey);
                if (_infoToggle.isOn) _infoToggle.isOn = false;
            }
            if(!fromSync)SendFilters(MessageType.Info, toggle);
        }

        private void WarningToggle(bool toggle) => WarningToggle(toggle, false);
        private void WarningToggle(bool toggle, bool fromSync)
        {
            if (toggle)
            {
                _typeOptions |= MessageTypeOptions.Warning;
                ChangeColor(_warningToggle, Color.white);
                if (!_warningToggle.isOn) _warningToggle.isOn = true;
            }
            else
            {
                _typeOptions &= ~MessageTypeOptions.Warning;
                ChangeColor(_warningToggle, Color.grey);
                if (_warningToggle.isOn) _warningToggle.isOn = false;
            }
            if (!fromSync) SendFilters(MessageType.Warning, toggle);
        }

        private void ErrorToggle(bool toggle) => ErrorToggle(toggle, false);
        private void ErrorToggle(bool toggle, bool fromSync)
        {
            if (toggle)
            {
                _typeOptions |= MessageTypeOptions.Error;
                ChangeColor(_errorToggle, Color.white);
                if (!_errorToggle.isOn) _errorToggle.isOn = true;
            }
            else
            {
                _typeOptions &= ~MessageTypeOptions.Error;
                ChangeColor(_errorToggle, Color.grey);
                if (_errorToggle.isOn) _errorToggle.isOn = false;
            }
            if (!fromSync) SendFilters(MessageType.Error, toggle);
        }

        private void ChangeColor(Toggle toggle, Color colour)
        {
            ColorBlock block = toggle.colors;
            block.normalColor = colour;
            block.selectedColor = colour;
            toggle.colors = block;
        }

        private void SendFilters(MessageType type, bool value)
        {
            if(type != MessageType.None)
                _filterSync.SendFilters(type, value);
            /*else
                _filterSync.SendFilters(empty:_includeEmpty);*/
            if (_filterType is FilterType.LogBox) _controller.SetMsgTypeFilter(_client, _typeOptions);
            else _controller.SetTimelineFilter(_typeOptions, _includeEmpty);
        }
    }
}
