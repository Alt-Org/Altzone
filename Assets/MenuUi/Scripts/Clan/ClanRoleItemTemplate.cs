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
    [SerializeField] private GameObject _selectedHighlight;

    private string _roleName;
    private Action<string, bool> _onChanged;

    public string RoleName => _roleName;

    public void Init(
        string roleName,
        string displayName,
        Sprite icon,
        bool isOn,
        Action<string, bool> onChanged,
        ToggleGroup toggleGroup = null)
    {
        _roleName = roleName;
        _onChanged = onChanged;

        if (_label != null) _label.text = displayName;
        if (_icon != null) _icon.sprite = icon;

        if (_toggle == null)
        {
            Debug.LogError($"{nameof(ClanRoleItemTemplate)}: Toggle reference missing on {gameObject.name}");
            return;
        }

        _toggle.group = toggleGroup;

        _toggle.onValueChanged.RemoveAllListeners();
        _toggle.isOn = isOn;

        SetHighlight(isOn);

        _toggle.onValueChanged.AddListener(v =>
        {
            SetHighlight(v);
            _onChanged?.Invoke(_roleName, v);
        });
    }

    private void SetHighlight(bool on)
    {
        if (_selectedHighlight != null)
            _selectedHighlight.SetActive(on);
    }
}



