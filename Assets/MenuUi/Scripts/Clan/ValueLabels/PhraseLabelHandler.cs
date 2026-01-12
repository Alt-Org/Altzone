using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Clan;
using MenuUi.Scripts.Clan;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PhraseLabelHandler : MonoBehaviour
{
    [SerializeField] private LabelReference _reference;
    [SerializeField] private TextMeshProUGUI _textLabel;
    [field: SerializeField] public Button _selectButton { get; private set; }

    public Phrases labelData { get; private set; }

    [Header("Colors")]
    [SerializeField] private string _selectedColorHex = "#0dd236";   // Vihreä
    [SerializeField] private string _unselectedColorHex = "#a0a0a0"; // Harmaa

    private Color _selectedColor;
    private Color _unselectedColor;

    void Awake()
    {
        if (!ColorUtility.TryParseHtmlString(_selectedColorHex, out _selectedColor))
        {
            _selectedColor = Color.green; // Default color
        }

        if (!ColorUtility.TryParseHtmlString(_unselectedColorHex, out _unselectedColor))
        {
            _unselectedColor = Color.gray; // Default color
        }
    }

    public void SetLabelInfo(Phrases value)
    {
        if (_selectButton != null)
        {
            _selectButton.gameObject.SetActive(true);
            if (_selectButton.targetGraphic != null)
                _selectButton.targetGraphic.gameObject.SetActive(true);
        }

        labelData = value;
        _textLabel.enabled = true;
        _textLabel.text = ClanDataTypeConverter.GetPhraseText(labelData);

        SetUnselectedVisuals();
    }

    public void Select()
    {
        _textLabel.text = ClanDataTypeConverter.GetPhraseText(labelData);
        SetSelectedVisuals();
    }
    public void Unselect()
    {
        _textLabel.text = ClanDataTypeConverter.GetPhraseText(labelData);
        SetUnselectedVisuals();
    }

    private void SetSelectedVisuals()
    {
        if (_selectButton != null && _selectButton.image != null)
        {
            _selectButton.image.color = _selectedColor;
        }
    }

    private void SetUnselectedVisuals()
    {
        if (_selectButton != null && _selectButton.image != null)
        {
            _selectButton.image.color = _unselectedColor;
        }
    }
}
