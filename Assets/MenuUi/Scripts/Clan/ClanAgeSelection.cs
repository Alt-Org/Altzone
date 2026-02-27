using System;
using System.Linq;
using Altzone.Scripts.Model.Poco.Clan;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ClanAgeSelection : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _ageText;
    [SerializeField] private Button _nextOption;
    [SerializeField] private Button _previousOption;

    [SerializeField] private Image _ageIconImage;
    [SerializeField] private List<AgeIcon> _ageIcons = new List<AgeIcon>();

    [System.Serializable]
    private struct AgeIcon
    {
        public ClanAge age;
        public Sprite icon;
    }

    public ClanAge ClanAgeRange { get; private set; } = ClanAge.None;

    private void Awake()
    {
        _nextOption.onClick.AddListener(OnNextOptionClicked);
        _previousOption.onClick.AddListener(OnPreviousOptionClicked);
    }

    public void Initialize(ClanAge initialAgeValue)
    {
        ClanAgeRange = initialAgeValue;
        _ageText.text = ClanDataTypeConverter.GetAgeText(ClanAgeRange);
    }

    private void OnNextOptionClicked()
    {
        bool isLast = ClanAgeRange == Enum.GetValues(typeof(ClanAge)).Cast<ClanAge>().Last();
        ClanAgeRange = isLast ? (ClanAge)1 : ClanAgeRange + 1;
        _ageText.text = ClanDataTypeConverter.GetAgeText(ClanAgeRange);
    }

    private void OnPreviousOptionClicked()
    {
        bool isFirst = (int)ClanAgeRange <= 1;
        ClanAgeRange = isFirst ? Enum.GetValues(typeof(ClanAge)).Cast<ClanAge>().Last() : ClanAgeRange - 1;
        _ageText.text = ClanDataTypeConverter.GetAgeText(ClanAgeRange);
    }

    private void UpdateAgeUI()
    {
        _ageText.text = ClanDataTypeConverter.GetAgeText(ClanAgeRange);

        if(_ageIconImage == null)
        {
            return;
        }

        var sprite = GetAgeSprite(ClanAgeRange);
        _ageIconImage.sprite = sprite;
        _ageIconImage.preserveAspect = true;
        _ageIconImage.enabled = sprite != null;
    }

    private Sprite GetAgeSprite(ClanAge age)
    {
        foreach (var ageIcon in _ageIcons)
        {
            if (ageIcon.age == age)
            {
                return ageIcon.icon;
            }
        }
        return null;
    }
}
