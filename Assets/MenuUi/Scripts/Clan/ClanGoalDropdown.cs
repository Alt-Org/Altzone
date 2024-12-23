using System;
using Altzone.Scripts.Model.Poco.Clan;
using TMPro;
using UnityEngine;

public class ClanGoalDropdown : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown _dropdown;

    public void Initialize(Goals initialValue)
    {
        _dropdown.options.Clear();
        foreach (Goals goal in Enum.GetValues(typeof(Goals)))
        {
            if (goal == Goals.None) continue;
            string text = ClanDataTypeConverter.GetGoalText(goal);
            _dropdown.options.Add(new TMP_Dropdown.OptionData(text));
        }

        _dropdown.value = EnumToDropdown(initialValue);
        _dropdown.RefreshShownValue();
    }

    public Goals GetSelected() => DropdownToGoal(_dropdown.value);

    // To skip over the None value
    private int EnumToDropdown<T>(T value) where T : Enum => Convert.ToInt32(value) - 1;
    private Goals DropdownToGoal(int goal) => (Goals)goal + 1;
}
