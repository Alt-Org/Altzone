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
        }
        private void EmptyToggle(bool toggle)
        {
            if (toggle)
            {
                _includeEmpty = true;
                ChangeColor(_emptyToggle, Color.white);
            }
            else
            {
                _includeEmpty = false;
                ChangeColor(_emptyToggle, Color.grey);
            }
            if (_filterType is FilterType.LogBox) _controller.SetMsgTypeFilter(_client, _typeOptions);
            else _controller.SetTimelineFilter(_typeOptions, _includeEmpty);
        }

        private void InfoToggle(bool toggle)
        {
            if (toggle)
            {
                _typeOptions |= MessageTypeOptions.Info;
                ChangeColor(_infoToggle, Color.white);
            }
            else
            {
                _typeOptions &= ~MessageTypeOptions.Info;
                ChangeColor(_infoToggle, Color.grey);
            }
            if(_filterType is FilterType.LogBox)_controller.SetMsgTypeFilter(_client, _typeOptions);
            else _controller.SetTimelineFilter(_typeOptions, _includeEmpty);
        }
        private void WarningToggle(bool toggle)
        {
            if (toggle)
            {
                _typeOptions |= MessageTypeOptions.Warning;
                ChangeColor(_warningToggle, Color.white);
            }
            else
            {
                _typeOptions &= ~MessageTypeOptions.Warning;
                ChangeColor(_warningToggle, Color.grey);
            }
            if (_filterType is FilterType.LogBox) _controller.SetMsgTypeFilter(_client, _typeOptions);
            else _controller.SetTimelineFilter(_typeOptions, _includeEmpty);
        }
        private void ErrorToggle(bool toggle)
        {
            if (toggle)
            {
                _typeOptions |= MessageTypeOptions.Error;
                ChangeColor(_errorToggle, Color.white);
            }
            else
            {
                _typeOptions &= ~MessageTypeOptions.Error;
                ChangeColor(_errorToggle, Color.grey);
            }
            if (_filterType is FilterType.LogBox) _controller.SetMsgTypeFilter(_client, _typeOptions);
            else _controller.SetTimelineFilter(_typeOptions, _includeEmpty);
        }

        private void ChangeColor(Toggle toggle, Color colour)
        {
            ColorBlock block = toggle.colors;
            block.normalColor = colour;
            block.selectedColor = colour;
            toggle.colors = block;
        }
    }
}
