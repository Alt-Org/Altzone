using UnityEngine;
using UnityEngine.UI;

public class ToggleColor : MonoBehaviour
{
    [SerializeField] private Color _toggleOnColor;
    [SerializeField] private Color _toggleOffColor;
    [SerializeField] private Image _image;
    [SerializeField] private Toggle _toggle;

    private void Start()
    {
        _image.color = _toggle.isOn ? _toggleOnColor : _toggleOffColor;
        _toggle.onValueChanged.AddListener(ToggleImageOnAndOff);
    }

    public void ToggleImageOnAndOff(bool isOn)
    {
        _image.color = isOn ? _toggleOnColor : _toggleOffColor;
    }
}
