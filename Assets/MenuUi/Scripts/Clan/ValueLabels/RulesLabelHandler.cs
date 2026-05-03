using Altzone.Scripts.Model.Poco.Clan;
using MenuUi.Scripts.Clan;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RulesLabelHandler : MonoBehaviour
{
    [SerializeField] private LabelReference _reference;
    [SerializeField] private TextMeshProUGUI _textLabel;
    [field: SerializeField] public Button _selectButton { get; private set; }

    [Header("Checkbox")]
    [SerializeField] private GameObject _checkedObject;
    [SerializeField] private GameObject _uncheckedObject;

    public Rules labelData { get; private set; }

    public void SetLabelInfo(Rules value)
    {
        labelData = value;

        if (_textLabel != null)
        {
            _textLabel.enabled = true;
            _textLabel.text = ClanDataTypeConverter.GetRulesText(labelData);
        }

        SetUnselectedVisuals();
    }

    public void Select()
    {
        if (_textLabel != null)
            _textLabel.text = ClanDataTypeConverter.GetRulesText(labelData);

        SetSelectedVisuals();
    }

    public void Unselect()
    {
        if (_textLabel != null)
            _textLabel.text = ClanDataTypeConverter.GetRulesText(labelData);

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
