using System;
using System.Collections;
using System.Collections.Generic;
using MenuUi.Scripts.Window;
using MenuUi.Scripts.Window.ScriptableObjects;
using MenuUI.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts.Model.Poco.Clan;

public class ClanCreateNew : MonoBehaviour
{
    [SerializeField] private List<string> _valuesList;
    [SerializeField] private GameObject _valuePrefab;
    [SerializeField] private GameObject _valuesContainer;

    [Header("Input fields")]
    [SerializeField] private TMP_InputField _clanNameField;
    [SerializeField] private TMP_InputField _clanPhraseField;
    [SerializeField] private TMP_InputField _clanPasswordField;

    [Header("Toggles")]
    [SerializeField] private Toggle _ageToddlersToggle;
    [SerializeField] private Toggle _ageTeensToggle;
    [SerializeField] private Toggle _ageAdultsToggle;
    [SerializeField] private Toggle _ageEveryoneToggle;
    [SerializeField] private Toggle _openClanButton;

    [Header("Warnings")]
    [SerializeField] private GameObject _clanNameWarningOutline;
    [SerializeField] private GameObject _clanPasswordWarningOutline;
    [SerializeField] private GameObject _clanLanguageWarningOutline;
    [SerializeField] private GameObject _clanGoalWarningOutline;
    [SerializeField] private GameObject _clanAgeWarningOutline;
    [SerializeField] private PopupController _warningPopup;

    [Header("Buttons")]
    [SerializeField] private Button _returnToMainClanViewButton;
    [SerializeField] private Button _clanValues;
    [SerializeField] private Button _buttonLogo;

    [Header("Dropdowns")]
    [SerializeField] private TMP_Dropdown _clanLanguageDropdown;
    [SerializeField] private TMP_Dropdown _clanGoalDropdown;

    [Header("Other Inputs")]
    [SerializeField] private ClanRightsPanel _clanRightsPanel;

    [Header("Navigation")]
    [SerializeField] protected WindowDef _naviTarget;

    private ClanAge _clanAgeRange = ClanAge.None;
    private ClanRoleRights[] _defaultRights = new ClanRoleRights[3]  {
        ClanRoleRights.None,
        ClanRoleRights.EditSoulHome,
        ClanRoleRights.EditClanSettings | ClanRoleRights.EditSoulHome | ClanRoleRights.EditMemberRights
    };

    private void Reset()
    {
        StopAllCoroutines();
        _clanNameField.text = "";
        _openClanButton.isOn = false;

        _clanNameWarningOutline.SetActive(false);
        _clanPasswordWarningOutline.SetActive(false);
        _clanLanguageWarningOutline.SetActive(false);
        _clanGoalWarningOutline.SetActive(false);
        _clanAgeWarningOutline.SetActive(false);

        SetLanguageDropdown();
        SetValuesPanel();
        SetGoalsDropDown();

        ConfigureAgeToggle(_ageToddlersToggle, ClanAge.Toddlers);
        ConfigureAgeToggle(_ageTeensToggle, ClanAge.Teenagers);
        ConfigureAgeToggle(_ageAdultsToggle, ClanAge.Adults);
        ConfigureAgeToggle(_ageEveryoneToggle, ClanAge.All);

        _clanRightsPanel.InitializeRightsToggles(_defaultRights);
    }

    private void OnEnable()
    {
        Reset();
    }

    private void SetLanguageDropdown()
    {
        _clanLanguageDropdown.options.Clear();
        foreach (Language language in Enum.GetValues(typeof(Language)))
        {
            string text = ClanDataTypeConverter.GetLanguageText(language);
            _clanLanguageDropdown.options.Add(new TMP_Dropdown.OptionData(text));
        }
    }

    private void SetGoalsDropDown()
    {
        _clanGoalDropdown.options.Clear();
        foreach (Goals goal in Enum.GetValues(typeof(Goals)))
        {
            string text = ClanDataTypeConverter.GetGoalText(goal);
            _clanGoalDropdown.options.Add(new TMP_Dropdown.OptionData(text));
        }
    }

    private void ConfigureAgeToggle(Toggle toggle, ClanAge clanAge)
    {
        SetToggleColor(toggle, Color.white);
        toggle.onValueChanged.RemoveAllListeners();
        toggle.onValueChanged.AddListener((value) =>
        {
            if (value)
            {
                SetClanAge(clanAge);
                SetToggleColor(toggle, Color.yellow);
            }
            else SetToggleColor(toggle, Color.white);
        });
    }
    private void SetClanAge(ClanAge age) => _clanAgeRange = age;
    private void SetToggleColor(Toggle toggle, Color color)
    {
        ColorBlock colors = toggle.colors;
        colors.normalColor = color;
        toggle.colors = colors;
    }

    private void SetValuesPanel()
    {
        _valuesContainer.GetComponent<ValuePanelController>().CreateLabels();
    }

    public void PostClanToServer()
    {
        string clanName = _clanNameField.text;
        string phrase = _clanPhraseField.text;
        bool isOpen = !_openClanButton.isOn;
        string password = _clanPasswordField.text;
        Language language = (Language)_clanLanguageDropdown.value;
        Goals goal = (Goals)_clanGoalDropdown.value;
        ClanAge age = _clanAgeRange;
        ClanRoleRights[] clanRights = _clanRightsPanel.ClanRights;

        if (!CheckClanValuesValidity(clanName, phrase, isOpen, password, language, goal, age))
        {
            return;
        }

        StartCoroutine(ServerManager.Instance.PostClanToServer(clanName, clanName.Trim().Substring(0, 4), isOpen, null, age, goal, phrase, language, clan =>
        {
            if (clan == null)
            {
                return;
            }

            Debug.Log($"naviTarget {_naviTarget} isCurrentPopOutWindow {true}", _naviTarget);
            var windowManager = WindowManager.Get();
            //windowManager.PopCurrentWindow();

            // Check if navigation target window is already in window stack and we area actually going back to it via button.
            var windowCount = windowManager.WindowCount;
            if (windowCount > 1)
            {
                var targetIndex = windowManager.FindIndex(_naviTarget);
                if (targetIndex == 1)
                {
                    windowManager.GoBack();
                    return;
                }
                if (targetIndex > 1)
                {
                    windowManager.Unwind(_naviTarget);
                    windowManager.GoBack();
                    return;
                }
            }
            windowManager.ShowWindow(_naviTarget);
        }));
    }

    private bool CheckClanValuesValidity(string clanName, string phrase, bool isOpen, string password, Language language, Goals goal, ClanAge age)
    {
        bool validInputs = true;

        if (clanName == string.Empty)
        {
            _clanNameWarningOutline.SetActive(true);
            _warningPopup.ActivatePopUp("Lisää klaanin nimi");
            validInputs = false;
        }
        else _clanNameWarningOutline.SetActive(false);

        if (!isOpen && password == string.Empty)
        {
            _clanPasswordWarningOutline.SetActive(true);
            _warningPopup.ActivatePopUp("Lukituilla klaaneilla tulee olla salasana");
            validInputs = false;
        }
        else _clanPasswordWarningOutline.SetActive(false);

        if (language == Language.None)
        {
            _clanLanguageWarningOutline.SetActive(true);
            validInputs = false;
            _warningPopup.ActivatePopUp("Valitse klaanin kieli");
        }
        else _clanLanguageWarningOutline.SetActive(false);

        if (goal == Goals.None)
        {
            _clanGoalWarningOutline.SetActive(true);
            validInputs = false;
            _warningPopup.ActivatePopUp("Valitse klaanin tavoite");
        }
        else _clanGoalWarningOutline.SetActive(false);

        if (age == ClanAge.None)
        {
            _clanAgeWarningOutline.SetActive(true);
            validInputs = false;
            _warningPopup.ActivatePopUp("Valitse klaanin ikäryhmä");
        }
        else _clanAgeWarningOutline.SetActive(false);

        return validInputs;
    }
}
