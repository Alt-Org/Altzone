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
    [Header("Static text fields")]
    [SerializeField] private TextMeshProUGUI _clanName;
    [SerializeField] private TextMeshProUGUI _clanMembers;
    [SerializeField] private TextMeshProUGUI _clanWinsRanking;
    [SerializeField] private TextMeshProUGUI _clanActivityRanking;

    [Header("Inputs")]
    [SerializeField] private TMP_InputField _clanPhraseField;
    [SerializeField] private GameObject _clanPassword;
    [SerializeField] private TMP_InputField _clanPasswordField;
    [SerializeField] private Toggle _clanOpenToggle;
    [SerializeField] private ClanGoalSelection _goalSelection;
    [SerializeField] private ClanAgeSelection _ageSelection;

    [Header("Language")]
    [SerializeField] private ClanLanguageList _languageList;
    [SerializeField] private LanguageFlagImage _flagImageSetter;

    [Header("Other settings fields")]
    [SerializeField] private ClanRightsPanel _clanRightsPanel;
    [SerializeField] private ClanHeartColorChanger _heartColorChanger;
    [SerializeField] private ClanHeartColorSetter _heartColorSetter;
    [SerializeField] private ValueSelectionController _valueSelection;
    [SerializeField] private ClanValuePanel _valuePanel;

    [Header("Buttons")]
    [SerializeField] private Button _openValueSelectionButton;
    [SerializeField] private Button _setLanguageButton;
    [SerializeField] private Button _setValuesButton;
    [SerializeField] private Button _saveButton;

    [Header("Popups")]
    [SerializeField] private PopupController _errorPopup;
    [SerializeField] private GameObject _cancelConfirmationPopup;

    [Header("Panels")]
    [SerializeField] private GameObject _mainSettingsPanel;
    [SerializeField] private GameObject _selectLanguagePanel;
    [SerializeField] private GameObject _editHeartPanel;
    [SerializeField] private GameObject _editValuesPanel;

    private List<HeartPieceData> _heartPieces;
    private List<ClanValues> _selectedValues = new();

    private void OnEnable()
    {
        Storefront.Get().GetClanData(ServerManager.Instance.Clan._id, (clan) =>
        {
            // Show correct panel
            _mainSettingsPanel.SetActive(true);
            _editHeartPanel.SetActive(false);
            _selectLanguagePanel.SetActive(false);
            _editValuesPanel.SetActive(false);
            _cancelConfirmationPopup.SetActive(false);

            // Initialize settings
            _clanName.text = clan.Name;
            _clanMembers.text = "Jäsenmäärä: " + clan.Members.Count.ToString();
            _clanActivityRanking.text = _clanWinsRanking.text = "-1";

            _clanPhraseField.text = clan.Phrase;
            _clanOpenToggle.isOn = !clan.IsOpen;
            _clanPassword.SetActive(!clan.IsOpen);

            _goalSelection.Initialize(clan.Goals);
            _ageSelection.Initialize(clan.ClanAge);

            _clanRightsPanel.InitializeRightsToggles(clan.ClanRights);

            _languageList.Initialize(clan.Language);
            _flagImageSetter.SetFlag(clan.Language);
            _setLanguageButton.onClick.AddListener(() => _flagImageSetter.SetFlag(_languageList.SelectedLanguage));

            // Values init
            List<ClanValues> defaultVals = new() {
                ClanValues.Huumorintajuiset,
                ClanValues.Syvalliset,
                ClanValues.Tasapainoiset
            };
            _valuePanel.SetValues(clan.Values);
            _valueSelection.SetSelected(clan.Values);
            _selectedValues = clan.Values;
            _openValueSelectionButton.onClick.AddListener(() => _valueSelection.SetSelected(_selectedValues));
            _setValuesButton.onClick.AddListener(() =>
            {
                _selectedValues = _valueSelection.SelectedValues;
                _valuePanel.SetValues(_selectedValues);
            });

            // Heart init
            clan.ClanHeartPieces ??= new();
            _heartPieces = clan.ClanHeartPieces;
            _heartColorChanger.InitializeClanHeart(_heartPieces);
            _heartColorSetter.SetHeartColors(_heartPieces);

            _saveButton.onClick.AddListener(SaveClanSettings);
        });
    }

    public void SetClanHeartFromColorChanges()
    {
        _heartPieces = _heartColorChanger.GetHeartPieceDatas();
        _heartColorSetter.SetHeartColors(_heartPieces);
    }
    public void ResetHeartColorChanger() => _heartColorChanger.InitializeClanHeart(_heartPieces);

    public void SaveClanSettings()
    {
        _saveButton.interactable = false;

        Storefront.Get().GetClanData(ServerManager.Instance.Clan._id, (clanData) =>
        {
            string previousPhrase = clanData.Phrase;
            clanData.Phrase = _clanPhraseField.text;
            clanData.Language = _languageList.SelectedLanguage;
            clanData.Goals = _goalSelection.GoalsRange;
            clanData.ClanAge = _ageSelection.ClanAgeRange;

            clanData.Values = _valueSelection.SelectedValues;
            clanData.ClanHeartPieces = _heartPieces;

            // These are not saved at the moment
            bool isOpen = !_clanOpenToggle.isOn;
            string password = _clanPasswordField.text;
            clanData.ClanRights = _clanRightsPanel.ClanRights;
            
            StartCoroutine(ServerManager.Instance.UpdateClanToServer(clanData, success =>
            {
                _saveButton.interactable = true;
                if (success)
                {
                    WindowManager.Get().GoBack();

                    if (!string.IsNullOrEmpty(previousPhrase) && previousPhrase != clanData.Phrase)
                        gameObject.GetComponent<DailyTaskProgressListener>().UpdateProgress("1");
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
            bool hasMadeEdits = _heartColorChanger.IsAnyPieceChanged()
                || clanData.Phrase != _clanPhraseField.text
                || clanData.Language != _languageList.SelectedLanguage
                || clanData.Goals != _goalSelection.GoalsRange
                || clanData.ClanAge != _ageSelection.ClanAgeRange
                || !clanData.ClanRights.SequenceEqual(_clanRightsPanel.ClanRights)
                || clanData.Values != _selectedValues;

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
