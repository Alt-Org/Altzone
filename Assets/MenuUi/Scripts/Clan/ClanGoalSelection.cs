using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Clan;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClanGoalSelection : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _goalText;
    [SerializeField] private Button _nextOption;
    [SerializeField] private Button _previousOption;

    public Goals GoalsRange { get; private set; } = Goals.None;

    private void Awake()
    {
        _nextOption.onClick.AddListener(OnNextOptionClicked);
        _previousOption.onClick.AddListener(OnPreviousOptionClicked);
    }

    public void Initialize(Goals initialGoalValue)
    {
        GoalsRange = initialGoalValue;
        _goalText.text = ClanDataTypeConverter.GetGoalText(GoalsRange);
    }

    private void OnNextOptionClicked()
    {
        bool isLast = GoalsRange == Enum.GetValues(typeof(Goals)).Cast<Goals>().Last();
        GoalsRange = isLast ? (Goals)1 : GoalsRange + 1;
        _goalText.text = ClanDataTypeConverter.GetGoalText(GoalsRange);
    }

    private void OnPreviousOptionClicked()
    {
        bool isFirst = (int)GoalsRange <= 1;
        GoalsRange = isFirst ? Enum.GetValues(typeof(Goals)).Cast<Goals>().Last() : GoalsRange - 1;
        _goalText.text = ClanDataTypeConverter.GetGoalText(GoalsRange);
    }
}
