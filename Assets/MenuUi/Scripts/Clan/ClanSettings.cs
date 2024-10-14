using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Altzone.Scripts.Model.Poco.Clan;
using System;
using MenuUI.Scripts;
using MenuUi.Scripts.Window;

public class ClanSettings : MonoBehaviour
{
    [Header("Text fields")]
    [SerializeField] private TextMeshProUGUI _clanName;
    [SerializeField] private TextMeshProUGUI _clanMembers;
    [SerializeField] private TextMeshProUGUI _clanCoins;
    [SerializeField] private TextMeshProUGUI _clanTrophies;
    [SerializeField] private TextMeshProUGUI _clanGlobalRanking;

    [Header("Input fields")]
    [SerializeField] private TMP_InputField _clanPhraseField;
    [SerializeField] private TMP_InputField _clanPasswordField;

    [Header("Toggles and dropdowns")]
    [SerializeField] private Toggle _clanOpenToggle;
    [SerializeField] private TMP_Dropdown _clanLanguageDropdown;
    [SerializeField] private TMP_Dropdown _clanGoalDropdown;
    [SerializeField] private TMP_Dropdown _clanAgeDropdown;

    [Header("Other settings fields")]
    [SerializeField] private Transform _valueRowFirst;
    [SerializeField] private Transform _valueRowSecond;

    [Header("Buttons")]
    [SerializeField] private Button _saveButton;

    [Header("Popups")]
    [SerializeField] private PopupController errorPopup;

    [Header("Prefabs")]
    [SerializeField] private GameObject _valuePrefab;

    private void OnEnable()
    {
        if (ServerManager.Instance.Clan != null)
        {
            SetPanelValues(ServerManager.Instance.Clan);
            SetInitialSettingValues(ServerManager.Instance.Clan);
        }
    }

    private void SetPanelValues(ServerClan clan)
    {
        _clanName.text = clan.name;
        _clanMembers.text = "Jäsenmäärä: " + clan.playerCount.ToString();
        _clanCoins.text = clan.gameCoins.ToString();
        _clanTrophies.text = "-1";
        _clanGlobalRanking.text = "-1";
    }

    private void SetInitialSettingValues(ServerClan clan)
    {
        _clanPhraseField.text = clan.phrase;
        // _clanPasswordField.text = ;
        _clanOpenToggle.isOn = !clan.isOpen;

        InitLanguageDropdown();
        _clanLanguageDropdown.value = EnumToDropdown(clan.language);
        _clanLanguageDropdown.RefreshShownValue();

        InitGoalsDropDown();
        _clanGoalDropdown.value = EnumToDropdown(clan.goals);
        _clanGoalDropdown.RefreshShownValue();

        InitAgeDropDown();
        _clanAgeDropdown.value = EnumToDropdown(clan.clanAge);
        _clanAgeDropdown.RefreshShownValue();
    }

    private void InitLanguageDropdown()
    {
        _clanLanguageDropdown.options.Clear();
        foreach (Language language in Enum.GetValues(typeof(Language)))
        {
            if (language == Language.None) continue;
            string text = ClanDataTypeConverter.GetLanguageText(language);
            _clanLanguageDropdown.options.Add(new TMP_Dropdown.OptionData(text));
        }
    }

    private void InitGoalsDropDown()
    {
        _clanGoalDropdown.options.Clear();
        foreach (Goals goal in Enum.GetValues(typeof(Goals)))
        {
            if (goal == Goals.None) continue;
            string text = ClanDataTypeConverter.GetGoalText(goal);
            _clanGoalDropdown.options.Add(new TMP_Dropdown.OptionData(text));
        }
    }

    private void InitAgeDropDown()
    {
        _clanAgeDropdown.options.Clear();
        foreach (ClanAge age in Enum.GetValues(typeof(ClanAge)))
        {
            if (age == ClanAge.None) continue;
            string text = ClanDataTypeConverter.GetAgeText(age);
            _clanAgeDropdown.options.Add(new TMP_Dropdown.OptionData(text));
        }
    }

    // To skip over the None value
    private int EnumToDropdown<T>(T value) where T : Enum => Convert.ToInt32(value) - 1;
    private Language DropdownToLanguage(int lang) => (Language)lang + 1;
    private ClanAge DropdownToAge(int age) => (ClanAge)age + 1;
    private Goals DropdownToGoal(int goal) => (Goals)goal + 1;

    public void SaveClanSettings()
    {
        _saveButton.interactable = false;

        ClanData clanData = new ClanData(ServerManager.Instance.Clan);
        clanData.Phrase = _clanPhraseField.text;
        clanData.Language = DropdownToLanguage(_clanLanguageDropdown.value);
        clanData.Goals = DropdownToGoal(_clanGoalDropdown.value);
        clanData.ClanAge = DropdownToAge(_clanAgeDropdown.value);

        // These are not saved at the moment
        bool isOpen = !_clanOpenToggle.isOn;
        string password = _clanPasswordField.text;

        StartCoroutine(ServerManager.Instance.UpdateClanToServer(clanData, success =>
        {
            _saveButton.interactable = true;
            if (success)
            {
                WindowManager.Get().GoBack();
            }
            else
            {
                errorPopup.ActivatePopUp("Asetusten tallentaminen ei onnistunut.");
            }
        }));
    }
}
