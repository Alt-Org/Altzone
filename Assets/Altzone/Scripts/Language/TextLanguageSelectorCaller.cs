using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Altzone.Scripts.Language
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TextLanguageSelectorCaller : TextLanguageSelector
    {
        protected override void OnEnable()
        {
            _textField = GetComponent<TextMeshProUGUI>();
        }

        public void SetText(SettingsCarrier.LanguageType language, string[] additions)
        {
            if(_textField == null) _textField = GetComponent<TextMeshProUGUI>();
            if (language is SettingsCarrier.LanguageType.Finnish) _textField.text = string.Format(_finnishText, additions);
            else if (language is SettingsCarrier.LanguageType.English) _textField.text = string.Format(_englishText, additions);
            else _textField.text = _finnishText;
        }

        public void SetText(string text)
        {
            if (_textField == null) _textField = GetComponent<TextMeshProUGUI>();
            if (!string.IsNullOrWhiteSpace(text)) _textField.text = text;
            else
            {
                if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.Finnish) _textField.text = _finnishText;
                else if (SettingsCarrier.Instance.Language is SettingsCarrier.LanguageType.English) _textField.text = _englishText;
            }
        }
    }
}
