using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Altzone.Scripts.Language
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TextLanguageSelector : MonoBehaviour
    {
        [SerializeField, TextArea(1, 10)] protected string _finnishText;
        [SerializeField, TextArea(1, 10)] protected string _englishText;

        protected TextMeshProUGUI _textField;
        protected virtual void OnEnable()
        {
            _textField = GetComponent<TextMeshProUGUI>();
            SettingsCarrier.OnLanguageChanged += SetText;
            SetText(SettingsCarrier.Instance.Language);
        }

        protected virtual void SetText(SettingsCarrier.LanguageType language)
        {
            if (language is SettingsCarrier.LanguageType.Finnish) _textField.text = _finnishText;
            else if (language is SettingsCarrier.LanguageType.English) _textField.text = _englishText;
            else _textField.text = _finnishText;
        }
    }
}
