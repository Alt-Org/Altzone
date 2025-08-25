using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Clan;
using UnityEngine;
using UnityEngine.UI;

public class IconButtonHandler : MonoBehaviour
{

    [SerializeField]
    private Button _button;
    [SerializeField]
    private Image _backgroundSelector;
    [SerializeField]
    private Image _valueIcon;

    private ClanValues _value;

    private string _selectedColorHex = "#00FF00";   // Vihreä
    private string _unselectedColorHex = "#FFFFFF"; // Valkoinen

    private Action<ClanValues, Action<bool>> _selectionMethod;

    // Start is called before the first frame update
    void Start()
    {
        _button.onClick.AddListener(OnValueSelected);
    }

    public void Initialize(Sprite sprite, ClanValues value, string selectedColour, string unselectedColour, Action<ClanValues, Action<bool>> selectionMethod)
    {
        _valueIcon.sprite = sprite;
        _value = value;
        _selectedColorHex = selectedColour;
        _unselectedColorHex = unselectedColour;
        _selectionMethod = selectionMethod;
    }

    void OnValueSelected()
    {
        bool active = false;
        _selectionMethod.Invoke(_value, x => active = x);
        UpdateButtonVisual(active);
    }

    public void UpdateButtonVisual(bool isSelected)
    {
        if (_backgroundSelector != null)
        {
            string hexColor = isSelected ? _selectedColorHex : _unselectedColorHex;

            if (ColorUtility.TryParseHtmlString(hexColor, out Color parsedColor))
            {
                _backgroundSelector.color = parsedColor;
            }
            else
            {
                Debug.LogWarning($"Virheellinen v�riarvo: {hexColor}");
            }
        }
    }
}
