using System;
using Altzone.Scripts.Model.Poco.Clan;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Toggle))]
public class ClanAgeListItem : MonoBehaviour
{
    [SerializeField] private Image _iconImage;
    [SerializeField] private TextMeshProUGUI _label;

    private Toggle _toggle;
    private ClanAge _age;

    public void Initialize(
        ClanAge age,
        Sprite icon,
        bool isSlected,
        UnityAction<bool> onvalueChanged)
    {
        _age = age;

        if (_label != null)
        {
            _label.text = ClanDataTypeConverter.GetAgeText(age);
        }

        if (_iconImage != null)
        {
            _iconImage.sprite = icon;
            _iconImage.preserveAspect = true;
            _iconImage.enabled = icon != null;
        }

        _toggle = GetComponent<Toggle>();
        _toggle.isOn = isSlected;
        _toggle.onValueChanged.RemoveAllListeners();
        _toggle.onValueChanged.AddListener(isOn =>
        {
            onvalueChanged?.Invoke(isOn);
        });
    }
}
