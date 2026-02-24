using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static SettingsCarrier;

public class LanguageToggle : MonoBehaviour
{
    [SerializeField] private LanguageFlagMap _flagMap;
    [SerializeField] private Image _flagImage;
    [SerializeField] private Button _button;

    private List<LanguageType> _languages = new (){ LanguageType.Finnish, LanguageType.English };
    private LanguageType _language;

    private void OnEnable()
    {
        _language = SettingsCarrier.Instance.Language;
        UpdateSprite(_language);
        _button.onClick.AddListener(ChangeLanguage);
        SettingsCarrier.OnLanguageChanged += UpdateSprite;
    }

    private void OnDisable()
    {
        SettingsCarrier.OnLanguageChanged -= UpdateSprite;
        _button.onClick.RemoveListener(ChangeLanguage);
    }

    private void UpdateSprite(LanguageType language)
    {
        _flagImage.sprite = _flagMap.GetFlag(language);
    }

    private void ChangeLanguage()
    {
        _language = SettingsCarrier.Instance.Language;
        int index = _languages.IndexOf(_language);
        index++;
        if(index >= _languages.Count) index = 0;
        SettingsCarrier.Instance.Language = _languages[index];
    }
}
