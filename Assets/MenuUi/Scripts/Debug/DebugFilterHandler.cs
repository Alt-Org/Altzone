using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DebugUi.Scripts.BattleAnalyzer
{
    public class DebugFilterHandler : MonoBehaviour
    {
        [SerializeField] Toggle _infoToggle;
        [SerializeField] Toggle _warningToggle;
        [SerializeField] Toggle _errorToggle;

        [SerializeField] LogBoxController _controller;
        [SerializeField] int _client = 0;

        private MessageTypeOptions _typeOptions;
        // Start is called before the first frame update
        void Start()
        {
            _infoToggle.isOn = true;
            _warningToggle.isOn = true;
            _errorToggle.isOn = true;
            _typeOptions = MessageTypeOptions.Info | MessageTypeOptions.Warning | MessageTypeOptions.Error;

            _infoToggle.onValueChanged.AddListener(InfoToggle);
            _warningToggle.onValueChanged.AddListener(WarningToggle);
            _errorToggle.onValueChanged.AddListener(ErrorToggle);
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
            _controller.SetMsgFilter(_client, _typeOptions);
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
            _controller.SetMsgFilter(_client, _typeOptions);
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
            _controller.SetMsgFilter(_client, _typeOptions);
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
