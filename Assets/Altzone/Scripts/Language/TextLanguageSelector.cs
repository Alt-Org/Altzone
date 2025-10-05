using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(TextMeshProUGUI))]
public class TextLanguageSelector : MonoBehaviour
{
    [SerializeField, TextArea(1, 10)] private string _finnishText;
    [SerializeField, TextArea(1, 10)] private string _englishText;

    private TextMeshProUGUI _textField;
    private void OnEnable()
    {
        _textField = GetComponent<TextMeshProUGUI>();
        SettingsCarrier.OnLanguageChanged += SetText;
        SetText(SettingsCarrier.Instance.Language);
    }

    private void SetText(SettingsCarrier.LanguageType language)
    {
        if (language is SettingsCarrier.LanguageType.Finnish) _textField.text = _finnishText;
        else if (language is SettingsCarrier.LanguageType.English) _textField.text = _englishText;
    }
}
