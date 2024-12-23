using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


namespace MenuUi.Scripts.Settings
{
    [RequireComponent(typeof(TMP_Text))]
    public class LabelVisibilityHandler : MonoBehaviour // handles label visibility and font size
    {
        private readonly SettingsCarrier _carrier = SettingsCarrier.Instance;
        private TMP_Text _text;

        private void Awake()
        {
            _text = GetComponent<TMP_Text>();
            _text.enableAutoSizing = false;
        }

        private void Start()
        {
            _carrier.OnButtonLabelVisibilityChange += SetVisibility;
            _carrier.OnTextSizeChange += SetFontSize;
            SetVisibility();
            SetFontSize();
        }

        private void SetVisibility()
        {
            gameObject.SetActive(_carrier.ShowButtonLabels);
        }

        private void SetFontSize()
        {
            switch (_carrier.Textsize)
            {
                case SettingsCarrier.TextSize.Small:
                    _text.fontSize = _carrier.TextSizeSmall;
                    break;
                case SettingsCarrier.TextSize.Medium:
                    _text.fontSize = _carrier.TextSizeMedium;
                    break;
                case SettingsCarrier.TextSize.Large:
                    _text.fontSize = _carrier.TextSizeLarge;
                    break;
            }
        }
    }
}
