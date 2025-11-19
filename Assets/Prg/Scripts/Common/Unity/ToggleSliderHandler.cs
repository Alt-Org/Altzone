using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Prg.Scripts.Common.Unity
{
    public class ToggleSliderHandler : MonoBehaviour
    {
        [SerializeField] Toggle _toggle;
        [SerializeField] Slider _sliderSwitch;

        public delegate void ToggleStateChanged(bool isOn);
        public event ToggleStateChanged OnToggleStateChanged;

        private void Start()
        {
            if(_toggle != null)
            {
                if(_sliderSwitch != null)
                {
                    if (_toggle.isOn) _sliderSwitch.value = _sliderSwitch.maxValue;
                    else _sliderSwitch.value = _sliderSwitch.minValue;
                }

                _toggle.onValueChanged.AddListener(ChangeState);
            }
        }
        private void OnDestroy()
        {
            _toggle.onValueChanged.RemoveAllListeners();
        }

        private void ChangeState(bool value)
        {
            if (_sliderSwitch != null)
            {
                if (value) _sliderSwitch.value = _sliderSwitch.maxValue;
                else _sliderSwitch.value = _sliderSwitch.minValue;
                if (_toggle.isOn) _toggle.targetGraphic.color = Color.blue;
                else _toggle.targetGraphic.color = Color.white;
            }

            OnToggleStateChanged?.Invoke(value);
        }

        public void SetState(bool value)
        {
            if (_toggle != null)
            {
                if (value == _toggle.isOn) return;
                _toggle.isOn = value;
            }
            else Debug.LogError($"Toggle {gameObject.name} is not set. Cannot modify set toggleState");
        }
    }
}
