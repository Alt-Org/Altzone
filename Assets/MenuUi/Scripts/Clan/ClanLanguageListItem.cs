using System;
using Altzone.Scripts.Model.Poco.Clan;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClanLanguageListItem : MonoBehaviour
{
    [SerializeField] Toggle _toggle;
    [SerializeField] TextMeshProUGUI _languageText;
    [SerializeField] Image _flagImage;
    [SerializeField] Image _selectedImage;

    public void Initialize(Language language, Sprite flag, bool isSelected, Action<bool> OnToggle)
    {
        _languageText.text = ClanDataTypeConverter.GetLanguageText(language);
        _flagImage.sprite = flag;
        _selectedImage.enabled = isSelected;
        _toggle.isOn = isSelected;
        _toggle.onValueChanged.AddListener((isOn) => OnToggle(isOn));
        _toggle.onValueChanged.AddListener(ToggleSelectedImage);
    }
    private void ToggleSelectedImage(bool isOn) => _selectedImage.enabled = isOn;
}
