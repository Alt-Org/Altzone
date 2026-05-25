using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Clan;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class IconButtonHandler : MonoBehaviour
{

    [SerializeField]
    private Button _button;

    [SerializeField]
    private GameObject _backgroundHighlight;

    [SerializeField]
    private Image _valueIcon;

    [SerializeField]
    private TMP_Text _labelText;

    [Header("Text Colors")]
    [SerializeField]
    private Color _selectedTextColor = new Color(1f, 0.55f, 0f); // orange

    [SerializeField]
    private Color _unselectedTextColor = Color.black;

    private ClanValues _value;

    private Action<ClanValues, Action<bool>> _selectionMethod;

    // Start is called before the first frame update
    void Start()
    {
        if (_button != null)
        {
            _button.onClick.AddListener(OnValueSelected);
        }          
    }

    public void Initialize(
        Sprite sprite,
        ClanValues value,
        string label,
        Action<ClanValues, Action<bool>> selectionMethod)
    {
        _valueIcon.sprite = sprite;
        _value = value;
        _selectionMethod = selectionMethod;

        if (_labelText != null)
        {
            _labelText.text = label;
        }

        UpdateButtonVisual(false);
    }

    void OnValueSelected()
    {
        bool active = false;
        _selectionMethod.Invoke(_value, x => active = x);
        UpdateButtonVisual(active);
    }

    public void UpdateButtonVisual(bool isSelected)
    {
        if (_backgroundHighlight != null)
        {
            _backgroundHighlight.SetActive(isSelected);
        }

        if (_labelText != null)
        {
            _labelText.color = isSelected ? _selectedTextColor : _unselectedTextColor;
        }
    }
}
