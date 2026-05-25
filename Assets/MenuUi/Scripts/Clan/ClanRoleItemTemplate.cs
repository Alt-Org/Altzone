using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClanRoleItemTemplate : MonoBehaviour
{
    [Header("Required")]
    [SerializeField] private Toggle _toggle;

    [Header("Optional visuals")]
    [SerializeField] private TextMeshProUGUI _label;
    [SerializeField] private Image _icon;

    [Header("Checkbox visuals")]
    [SerializeField] private GameObject _checked;
    [SerializeField] private GameObject _unchecked;

    private string _roleName;
    private Action<string, bool> _onChanged;

    public string RoleName => _roleName;

    public void Init(string roleName, string displayName, Sprite icon, bool isOn, Action<string, bool> onToggled)
    {
        _roleName = roleName;
        _onChanged = onToggled;

        if (_label != null)
            _label.text = displayName;

        if (_icon != null)
            _icon.sprite = icon;

        if (_toggle != null)
        {
            _toggle.onValueChanged.RemoveAllListeners();
            SetSelectedWithoutNotify(isOn);

            _toggle.onValueChanged.AddListener(value =>
            {
                SetSelectedWithoutNotify(value);
                _onChanged?.Invoke(_roleName, value);
            });
        }
    }

    private void UpdateVisuals(bool isOn)
    {
        if (_checked != null)
            _checked.SetActive(isOn);

        if (_unchecked != null)
            _unchecked.SetActive(!isOn);
    }

    public void SetSelectedWithoutNotify(bool isOn)
    {
        if (_toggle != null)
            _toggle.SetIsOnWithoutNotify(isOn);

        if (_checked != null)
            _checked.SetActive(isOn);

        if (_unchecked != null)
            _unchecked.SetActive(!isOn);
    }
}



