using System;
using Altzone.Scripts.Model.Poco.Clan;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClanSearchFiltersPanel : MonoBehaviour
{
    [SerializeField] private TMP_InputField _clanNameField;
    [SerializeField] private TMP_Dropdown _ageDropdown;
    [SerializeField] private TMP_Dropdown _activityDropdown;
    [SerializeField] private TMP_Dropdown _languageDropdown;
    [SerializeField] private TMP_Dropdown _goalDropdown;
    [SerializeField] private Toggle _removeLockedToggle;

    public Action<ClanSearchFilters> OnFiltersChanged;

    private void OnEnable()
    {
        InitDropdowns();
        SetToggleListeners();
        _clanNameField.onEndEdit.RemoveAllListeners();
        _clanNameField.onEndEdit.AddListener((value) => UpdateFilters());
        UpdateFilters();
    }

    private void InitDropdowns()
    {
        _languageDropdown.onValueChanged.RemoveAllListeners();
        _languageDropdown.value = 0;
        _languageDropdown.options.Clear();
        foreach (Language language in Enum.GetValues(typeof(Language)))
        {
            string text = ClanDataTypeConverter.GetLanguageText(language);
            _languageDropdown.options.Add(new TMP_Dropdown.OptionData(text));
        }
        _languageDropdown.onValueChanged.AddListener((value) => UpdateFilters());

        _goalDropdown.onValueChanged.RemoveAllListeners();
        _goalDropdown.value = 0;
        _goalDropdown.options.Clear();
        foreach (Goals goal in Enum.GetValues(typeof(Goals)))
        {
            string text = ClanDataTypeConverter.GetGoalText(goal);
            _goalDropdown.options.Add(new TMP_Dropdown.OptionData(text));
        }
        _goalDropdown.onValueChanged.AddListener((value) => UpdateFilters());

        _ageDropdown.onValueChanged.RemoveAllListeners();
        _ageDropdown.value = 0;
        _ageDropdown.options.Clear();
        foreach (ClanAge age in Enum.GetValues(typeof(ClanAge)))
        {
            string text = ClanDataTypeConverter.GetAgeText(age);
            _ageDropdown.options.Add(new TMP_Dropdown.OptionData(text));
        }
        _ageDropdown.onValueChanged.AddListener((value) => UpdateFilters());

        _activityDropdown.onValueChanged.RemoveAllListeners();
        _activityDropdown.value = 0;
        _activityDropdown.options.Clear();
        foreach (ClanActivity activity in Enum.GetValues(typeof(ClanActivity)))
        {
            string text = ClanDataTypeConverter.GetActivityText(activity);
            _activityDropdown.options.Add(new TMP_Dropdown.OptionData(text));
        }
        _activityDropdown.onValueChanged.AddListener((value) => UpdateFilters());
    }

    private void SetToggleListeners()
    {
        _removeLockedToggle.isOn = false;
        _removeLockedToggle.onValueChanged.RemoveAllListeners();
        _removeLockedToggle.onValueChanged.AddListener((isOn) =>
        {
            if (isOn)
            {
                ColorBlock colors = _removeLockedToggle.colors;
                colors.normalColor = Color.yellow;
                _removeLockedToggle.colors = colors;
            }
            else
            {
                ColorBlock colors = _removeLockedToggle.colors;
                colors.normalColor = Color.white;
                _removeLockedToggle.colors = colors;
            }
            UpdateFilters();
        });
    }

    private void UpdateFilters()
    {
        OnFiltersChanged?.Invoke(new ClanSearchFilters()
        {
            clanName = _clanNameField.text,
            activity = (ClanActivity)_activityDropdown.value,
            age = (ClanAge)_ageDropdown.value,
            language = (Language)_languageDropdown.value,
            goal = (Goals)_goalDropdown.value,
            removeLocked = _removeLockedToggle.isOn
        });
    }
}
