using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Prg.Scripts.Common.Unity
{
    public class ToggleSwitchHandler : MonoBehaviour
    {
        [SerializeField] Toggle _toggle;
        [SerializeField] Image _toggleImage;

        public delegate void ToggleStateChanged(bool isOn);
        public event ToggleStateChanged OnToggleStateChanged;

        public bool IsOn { get => _toggle.isOn; }

        private void Start()
        {
            if(_toggle != null)
            {
                if(_toggleImage != null)
                {
                    if (_toggle.isOn) _toggleImage.gameObject.SetActive(true);
                    else _toggleImage.gameObject.SetActive(false);
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
            if (_toggleImage != null)
            {
                if (value) _toggleImage.gameObject.SetActive(true);
                else _toggleImage.gameObject.SetActive(false);
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
