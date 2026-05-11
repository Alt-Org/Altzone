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

    [Header("Checkbox")]
    [SerializeField] private GameObject _checkedObject;
    [SerializeField] private GameObject _uncheckedObject;

    [field: SerializeField] public Button _selectButton { get; private set; }

    public LabelInfoObject labelInfo { get; private set; }

    public void SetLabelInfo(ClanValues value, bool showName)
    {
        labelInfo = _reference.GetLabelInfo(value);

        if (_labelImage != null)
        {
            _labelImage.sprite = labelInfo.Image;
            _labelImage.preserveAspect = true;
        }

        if (_textLabel != null)
        {
            _textLabel.enabled = showName;
            _textLabel.text = showName ? labelInfo.Name : string.Empty;
        }

        SetUnselectedVisuals();
    }

    public void Select()
    {
        SetSelectedVisuals();
    }

    public void Unselect()
    {
        SetUnselectedVisuals();
    }

    private void SetSelectedVisuals()
    {
        if (_checkedObject != null)
            _checkedObject.SetActive(true);

        if (_uncheckedObject != null)
            _uncheckedObject.SetActive(false);
    }

    private void SetUnselectedVisuals()
    {
        if (_checkedObject != null)
            _checkedObject.SetActive(false);

        if (_uncheckedObject != null)
            _uncheckedObject.SetActive(true);
    }
}
