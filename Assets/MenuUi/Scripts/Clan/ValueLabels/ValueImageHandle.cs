using MenuUi.Scripts.Clan;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts.Model.Poco.Clan;

public class ValueImageHandle : MonoBehaviour
{
    [SerializeField] private LabelReference _reference;
    [SerializeField] private Image _image;
    [SerializeField] private TextMeshProUGUI _labelText;

    public void SetLabelInfo(ClanValues value)
    {
        if (_reference == null)
        {
            return;
        }

        LabelInfoObject labelInfo = _reference.GetLabelInfo(value);

        if (labelInfo == null)
        {
            return;
        }

        if (_image != null)
        {
            _image.sprite = labelInfo.Image;
            _image.preserveAspect = true;
            _image.enabled = labelInfo.Image != null;
        }

        if (_labelText != null)
        {
            _labelText.text = labelInfo.Name;
        }
    }
}
