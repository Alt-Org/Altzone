using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static SettingsCarrier;

namespace MenuUi.Scripts.Settings
{
    public class TextSizeHandler : MonoBehaviour
    {

        [SerializeField] private TextMeshProUGUI _currentSize;
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _prevButton;
        private SettingsCarrier _settingsCarrier = SettingsCarrier.Instance;

        private void Start()
        {
            _nextButton.onClick.AddListener(() => ChangeTextSize(true));
            _prevButton.onClick.AddListener(() => ChangeTextSize(false));
            SetTextSize();
        }

        public void ChangeTextSize(bool nextSize)
        {
            TextSize size = _settingsCarrier.Textsize;
            switch (size)
            {
                case TextSize.Small:
                    if (nextSize) _settingsCarrier.SetTextSize(TextSize.Medium);
                    else _settingsCarrier.SetTextSize(TextSize.Large);
                    break;
                case TextSize.Medium:
                    if (nextSize) _settingsCarrier.SetTextSize(TextSize.Large);
                    else _settingsCarrier.SetTextSize(TextSize.Small);
                    break;
                case TextSize.Large:
                    if (nextSize) _settingsCarrier.SetTextSize(TextSize.Small);
                    else _settingsCarrier.SetTextSize(TextSize.Medium);
                    break;
            }

            SetTextSize();

        }

        private void SetTextSize()
        {
            switch (_settingsCarrier.Textsize)
            {
                case TextSize.Small:
                    _currentSize.text = "Pieni";
                    break;
                case TextSize.Medium:
                    _currentSize.text = "Keskikokoinen";
                    break;
                case TextSize.Large:
                    _currentSize.text = "Suuri";
                    break;
            }
        }

    }
}
