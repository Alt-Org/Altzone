using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class FurnitureTrayCategoryButton : MonoBehaviour
{
    [SerializeField] private Image _buttonImageObject;
    [SerializeField] private Image _iconImageObject;
    [SerializeField] private Color _defaultColour = Color.white;
    [SerializeField] private Color _defaultIconColour = new Color32(0x6F, 0xC6, 0xDE, 0xFF); //6FC6DE
    [SerializeField] private Color _selectedButtonColour = new Color32(0x9A, 0xE0, 0xF4, 0xFF); // 9AE0F4

    private bool _isSelected = false;

    public void Select()
    {
        if (_buttonImageObject != null) _buttonImageObject.color = _selectedButtonColour;
        if (_iconImageObject != null)_iconImageObject.color = _defaultColour;
        _isSelected = true;

    }
    public void DeSelect()
    {
        if (_buttonImageObject != null) _buttonImageObject.color = _defaultColour;
        if (_iconImageObject != null) _iconImageObject.color = _defaultIconColour;
        _isSelected = false;
    }
}
