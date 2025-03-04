using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Prg.Scripts.Common.Unity
{
    public class ToggleSwitchHandler : MonoBehaviour
    {
        [SerializeField] Toggle _toggle;
        [SerializeField] Slider _sliderSwitch;

        public delegate void ToggleStateChanged(bool isOn);
        public event ToggleStateChanged OnToggleStateChanged;

        private void Start()
        {
            if(_toggle != null && _sliderSwitch != null)
            {
                if(_toggle.isOn) _sliderSwitch.value = _sliderSwitch.maxValue;
                else _sliderSwitch.value = _sliderSwitch.minValue;

                _toggle.onValueChanged.AddListener(ChangeState);
            }
        }
        private void OnDestroy()
        {
            _toggle.onValueChanged.RemoveAllListeners();
        }

        public void ChangeState(bool value)
        {
            if(value) _sliderSwitch.value = _sliderSwitch.maxValue;
            else _sliderSwitch.value = _sliderSwitch.minValue;

            OnToggleStateChanged?.Invoke(value);
        }

        public void SetState(bool value)
        {
            if (value == _toggle.isOn) return;
            _toggle.isOn = value;
        }
    }
}
