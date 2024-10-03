using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static SettingsCarrier;

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
    }

    public void ChangeTextSize(bool nextSize)
    {
        TextSize size = _settingsCarrier.Textsize;
        switch (size)
        {
            case TextSize.None:
                if (nextSize) _settingsCarrier.SetTextSize(TextSize.Small);
                else _settingsCarrier.SetTextSize(TextSize.Large);
                break;
            case TextSize.Small:
                if (nextSize) _settingsCarrier.SetTextSize(TextSize.Medium);
                else _settingsCarrier.SetTextSize(TextSize.None);
                break;
            case TextSize.Medium:
                if (nextSize) _settingsCarrier.SetTextSize(TextSize.Large);
                else _settingsCarrier.SetTextSize(TextSize.Small);
                break;
            case TextSize.Large:
                if (nextSize) _settingsCarrier.SetTextSize(TextSize.None);
                else _settingsCarrier.SetTextSize(TextSize.Medium);
                break;
        }
        switch (_settingsCarrier.Textsize)
        {
            case TextSize.None:
                _currentSize.text = "Pois p‰‰lt‰";
                break;
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
