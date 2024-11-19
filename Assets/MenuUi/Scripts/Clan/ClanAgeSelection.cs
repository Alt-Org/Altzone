using System;
using System.Linq;
using Altzone.Scripts.Model.Poco.Clan;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClanAgeSelection : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _ageText;
    [SerializeField] private Button _nextOption;
    [SerializeField] private Button _previousOption;

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
}
