using MenuUi.Scripts.Clan;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts.Model.Poco.Clan;

public class ValueLabelHandler : MonoBehaviour
{
    [SerializeField] private LabelReference _reference;
    [SerializeField] private TextMeshProUGUI _textLabel;
    [SerializeField] private Image _labelImage;
    [field: SerializeField] public Button _selectButton { get; private set; }

    public LabelInfoObject labelInfo { get; private set; }

    [Header("Colors")]
    [SerializeField] private string _selectedColorHex = "#0dd236";   // Vihreä
    [SerializeField] private string _unselectedColorHex = "#a0a0a0"; // Harmaa

    private Color _selectedColor;
    private Color _unselectedColor; 

    void Start()
    {
        CheckLabelSize();
    }

    void Awake()
    {
        if(!ColorUtility.TryParseHtmlString(_selectedColorHex, out _selectedColor))
        {            
            _selectedColor = Color.green; // Default color
        }

        if(!ColorUtility.TryParseHtmlString(_unselectedColorHex, out _unselectedColor))
        {
            _unselectedColor = Color.gray; // Default color
        }
    }

    public void SetLabelInfo(ClanValues value, bool showName)
    {
        labelInfo = _reference.GetLabelInfo(value);
        _labelImage.sprite = labelInfo.Image;

        if (showName)
        {
            _textLabel.enabled = true;
            _textLabel.text = labelInfo.Name;   
        }
        else
        {
            _textLabel.enabled = false;
        }

        SetUnselectedVisuals();
    }

    public void CheckLabelSize()
    {
        float imagewidth = _labelImage.GetComponent<RectTransform>().sizeDelta.x;
        float imageleftpos = _labelImage.GetComponent<RectTransform>().localPosition.x;
        _textLabel.GetComponent<RectTransform>().offsetMin = new(imagewidth + imageleftpos + 10, _textLabel.GetComponent<RectTransform>().offsetMin.y);
    }

    public void Select()
    {
        _textLabel.text = labelInfo.Name;
        SetSelectedVisuals();
    }
    public void Unselect()
    {
        _textLabel.text = labelInfo.Name;
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
