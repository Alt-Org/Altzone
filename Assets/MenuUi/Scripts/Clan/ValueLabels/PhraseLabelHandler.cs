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
    [SerializeField] private string _selectedColorHex = "#FFA000";

    private Color _selectedColor;
    private readonly Color _unselectedColor = new Color(1f, 1f, 1f, 0f);

    private void Awake()
    {
        if (!ColorUtility.TryParseHtmlString(_selectedColorHex, out _selectedColor))
        {
            _selectedColor = new Color(1f, 0.6f, 0f, 1f);
        }
    }

    public void SetLabelInfo(Phrases value)
    {
        labelData = value;

        if (_textLabel != null)
        {
            _textLabel.enabled = true;
            _textLabel.text = ClanDataTypeConverter.GetPhraseText(labelData);
        }

        if (_selectButton != null)
        {
            _selectButton.gameObject.SetActive(true);

            if (_selectButton.targetGraphic != null)
            {
                _selectButton.targetGraphic.gameObject.SetActive(true);
            }
        }

        SetUnselectedVisuals();
    }

    public void Select()
    {
        if (_textLabel != null)
        {
            _textLabel.text = ClanDataTypeConverter.GetPhraseText(labelData);
        }

        SetSelectedVisuals();
    }

    public void Unselect()
    {
        if (_textLabel != null)
        {
            _textLabel.text = ClanDataTypeConverter.GetPhraseText(labelData);
        }

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
