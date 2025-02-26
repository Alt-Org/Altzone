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

    void Start()
    {
        CheckLabelSize();
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
    }

    public void CheckLabelSize()
    {
        float imagewidth = _labelImage.GetComponent<RectTransform>().sizeDelta.x;
        float imageleftpos = _labelImage.GetComponent<RectTransform>().localPosition.x;
        _textLabel.GetComponent<RectTransform>().offsetMin = new(imagewidth + imageleftpos + 10, _textLabel.GetComponent<RectTransform>().offsetMin.y);
    }

    public void Select()
    {
        _textLabel.text = labelInfo.Name + " (Valittu)";
    }
    public void Unselect()
    {
        _textLabel.text = labelInfo.Name;
    }
}
