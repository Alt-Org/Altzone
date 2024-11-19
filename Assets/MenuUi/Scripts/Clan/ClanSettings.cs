using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Altzone.Scripts.Model.Poco.Clan;
using System;
using MenuUI.Scripts;
using MenuUi.Scripts.Window;
using Altzone.Scripts;
using System.Linq;

public class ClanSettings : MonoBehaviour
{
    [Header("Text fields")]
    [SerializeField] private TextMeshProUGUI _clanName;
    [SerializeField] private TextMeshProUGUI _clanMembers;
    [SerializeField] private TextMeshProUGUI _clanTrophies;
    [SerializeField] private TextMeshProUGUI _clanWinsRanking;
    [SerializeField] private TextMeshProUGUI _clanActivityRanking;

    [Header("Input fields")]
    [SerializeField] private TMP_InputField _clanPhraseField;
    [SerializeField] private TMP_InputField _clanPasswordField;

    [Header("Toggles and dropdowns")]
    [SerializeField] private Toggle _clanOpenToggle;
    [SerializeField] private TMP_Dropdown _clanGoalDropdown;

    [Header("Language")]
    [SerializeField] private LanguageFlagMap _languageFlagMap;
    [SerializeField] private ClanLanguageList _languageList;
    [SerializeField] Image _flagImage;

    [Header("Other settings fields")]
    [SerializeField] private Transform _valueRowFirst;
    [SerializeField] private Transform _valueRowSecond;
    [SerializeField] private ClanRightsPanel _clanRightsPanel;
    [SerializeField] private ClanAgeSelection _ageSelection;
    [SerializeField] private ClanHeartColorChanger _clanHeartColorChanger;
    [SerializeField] private ClanHeartColorSetter _settingsHeartColorSetter;

    [Header("Buttons")]
    [SerializeField] private Button _closeClanLanguageSelect;
    [SerializeField] private Button _openClanLanguageSelect;
    [SerializeField] private Button _saveButton;

    [Header("Popups")]
    [SerializeField] private PopupController _errorPopup;
    [SerializeField] private GameObject _cancelConfirmationPopup;

    [Header("Prefabs")]
    [SerializeField] private GameObject _valuePrefab;

    private List<HeartPieceData> _heartPieces;

    private void OnEnable()
    {
        Storefront.Get().GetClanData(ServerManager.Instance.Clan._id, (clanData) =>
        {
            _clanHeartColorChanger.gameObject.SetActive(false);
            _languageList.gameObject.SetActive(false);
            _cancelConfirmationPopup.SetActive(false);

            SetPanelValues(clanData);
            _clanRightsPanel.InitializeRightsToggles(clanData.ClanRights);
            _languageList.Initialize(clanData.Language);
            SetFlag(clanData.Language);
            _closeClanLanguageSelect.onClick.AddListener(() => SetFlag(_languageList.SelectedLanguage));
            SetInitialSettingValues(clanData);

            clanData.ClanHeartPieces ??= new();
            _heartPieces = clanData.ClanHeartPieces;
            _clanHeartColorChanger.InitializeClanHeart(_heartPieces);
        });
    }

    private void SetFlag(Language language) => _flagImage.sprite = _languageFlagMap.GetFlag(language);

    private void SetPanelValues(ClanData clan)
    {
        _clanName.text = clan.Name;
        _clanMembers.text = "Jäsenmäärä: " + clan.Members.Count.ToString();
        _clanTrophies.text = "-1";
        _clanActivityRanking.text = _clanWinsRanking.text = "-1";
    }

    private void SetInitialSettingValues(ClanData clan)
    {
        _clanPhraseField.text = clan.Phrase;
        // _clanPasswordField.text = ;
        _clanOpenToggle.isOn = !clan.IsOpen;

        InitGoalsDropDown();
        _clanGoalDropdown.value = EnumToDropdown(clan.Goals);
        _clanGoalDropdown.RefreshShownValue();

        _ageSelection.Initialize(clan.ClanAge);
    }

    public void OpenClanHeartPanel() => _clanHeartColorChanger.gameObject.SetActive(true);
    public void CloseClanHeartPanel()
    {
        _heartPieces = _clanHeartColorChanger.GetHeartPieceDatas();
        _settingsHeartColorSetter.SetHeartColors(_heartPieces);
        _clanHeartColorChanger.gameObject.SetActive(false);
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

    // To skip over the None value
    private int EnumToDropdown<T>(T value) where T : Enum => Convert.ToInt32(value) - 1;
    private Goals DropdownToGoal(int goal) => (Goals)goal + 1;

    public void SaveClanSettings()
    {
        _saveButton.interactable = false;

        Storefront.Get().GetClanData(ServerManager.Instance.Clan._id, (clanData) =>
        {
            clanData.Phrase = _clanPhraseField.text;
            clanData.Language = _languageList.SelectedLanguage;
            clanData.Goals = DropdownToGoal(_clanGoalDropdown.value);
            clanData.ClanAge = _ageSelection.ClanAgeRange;

            // These are not saved at the moment
            bool isOpen = !_clanOpenToggle.isOn;
            string password = _clanPasswordField.text;
            clanData.ClanRights = _clanRightsPanel.ClanRights;
            clanData.ClanHeartPieces = _heartPieces;

            StartCoroutine(ServerManager.Instance.UpdateClanToServer(clanData, success =>
            {
                _saveButton.interactable = true;
                if (success)
                {
                    WindowManager.Get().GoBack();
                }
                else
                {
                    _errorPopup.ActivatePopUp("Asetusten tallentaminen ei onnistunut.");
                }
            }));
        });
    }

    public void OnClickCancelClanSettingEdits()
    {
        Storefront.Get().GetClanData(ServerManager.Instance.Clan._id, (clanData) =>
        {
            bool hasMadeEdits = _clanHeartColorChanger.IsAnyPieceChanged()
                || clanData.Phrase != _clanPhraseField.text
                || clanData.Language != _languageList.SelectedLanguage
                || clanData.Goals != DropdownToGoal(_clanGoalDropdown.value)
                || clanData.ClanAge != _ageSelection.ClanAgeRange
                || !clanData.ClanRights.SequenceEqual(_clanRightsPanel.ClanRights);

            if (hasMadeEdits)
            {
                _cancelConfirmationPopup.SetActive(true);
            }
            else
            {
                WindowManager.Get().GoBack();
            }
        });
    }
    public void OnClickContinueEditingClanSettings() => _cancelConfirmationPopup.SetActive(false);
}
