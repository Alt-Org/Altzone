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
    //[SerializeField] private TextMeshProUGUI _clanActivityRanking;

    [Header("Motto")]
    [SerializeField] private TextMeshProUGUI _clanPhraseText;
    [SerializeField] private TMP_InputField _clanPhraseField;
    [SerializeField] private GameObject _clanPhrasePopup;

    [Header("Rules (display texts)")]
    [SerializeField] private TextMeshProUGUI _rule1Text;
    [SerializeField] private TextMeshProUGUI _rule2Text;
    [SerializeField] private TextMeshProUGUI _rule3Text;

    [Header("Rules (popup inputs)")]
    [SerializeField] private TMP_InputField _rule1Input;
    [SerializeField] private TMP_InputField _rule2Input;
    [SerializeField] private TMP_InputField _rule3Input;

    [SerializeField] private GameObject _rulesPopup;

    [Header("Inputs")]
    //[SerializeField] private TMP_InputField _clanPhraseField;
    [SerializeField] private GameObject _clanPassword;
    [SerializeField] private TMP_InputField _clanPasswordField;
    [SerializeField] private Toggle _clanOpenToggle;
    //[SerializeField] private ClanGoalSelection _goalSelection;

    [Header("Age")]
    //[SerializeField] private ClanAgeSelection _ageSelection;
    [SerializeField] private ClanAgeList _ageSelection;
    [SerializeField] private Image _ageImage;

    [SerializeField] private GameObject _agePopup;

    [Header("Language")]
    [SerializeField] private ClanLanguageList _languageList;
    [SerializeField] private LanguageFlagImage _flagImageSetter;

    [Header("Other settings fields")]
    //[SerializeField] private ClanRightsPanel _clanRightsPanel;
    [SerializeField] private ClanHeartColorChanger _heartColorChanger;
    [SerializeField] private ClanHeartColorSetter _heartColorSetter;
    [SerializeField] private ValueSelectionController _valueSelection;
    [SerializeField] private ClanValuePanel _valuePanel;

    [Header("Buttons")]
    [SerializeField] private Button _openValueSelectionButton;
    [SerializeField] private Button _setLanguageButton;
    [SerializeField] private Button _setValuesButton;
    [SerializeField] private Button _saveButton;

    [Header("Tabline buttons")]
    [SerializeField] private GameObject _editViewButtons;
    [SerializeField] private GameObject _buttonsInClan;

    [Header("Popups")]
    [SerializeField] private PopupController _errorPopup;
    [SerializeField] private GameObject _cancelConfirmationPopup;

    [Header("Panels")]
    [SerializeField] private GameObject _mainSettingsPanel;
    [SerializeField] private GameObject _selectLanguagePanel;
    [SerializeField] private GameObject _editHeartPanel;
    [SerializeField] private GameObject _editValuesPanel;

    [SerializeField] private ClanMainView _clanMainView;

    private List<HeartPieceData> _heartPieces;
    private List<ClanValues> _selectedValues = new();

    private void OnEnable()
    {

        Storefront.Get().GetClanData(ServerManager.Instance.Clan._id, (clan) =>
        {
            // Show correct panel
            _mainSettingsPanel.SetActive(false);
            _editHeartPanel.SetActive(false);
            _selectLanguagePanel.SetActive(false);
            _editValuesPanel.SetActive(false);
            _cancelConfirmationPopup.SetActive(false);

            // Initialize settings
            _clanName.text = clan.Name;
            _clanMembers.text = "Jäsenmäärä: " + clan.Members.Count.ToString();
            //_clanActivityRanking.text = _clanWinsRanking.text = "-1";

            //Motto
            _clanPhraseText.text = clan.Phrase;
            _clanPhraseField.text = clan.Phrase;
            _clanPhrasePopup.SetActive(false);

            _clanOpenToggle.isOn = !clan.IsOpen;
            _clanPassword.SetActive(!clan.IsOpen);

            // Rules
            /*List<string> rules = clan.Rules ?? new List<string>();

            string rule1 = rules.Count > 0 ? rules[0] : string.Empty;
            string rule2 = rules.Count > 1 ? rules[1] : string.Empty;
            string rule3 = rules.Count > 2 ? rules[2] : string.Empty;

            _rule1Text.text = rule1;
            _rule2Text.text = rule2;
            _rule3Text.text = rule3;

            _rule1Input.text = rule1;
            _rule2Input.text = rule2;
            _rule3Input.text = rule3;*/

            _rulesPopup.SetActive(false);

            //Age           
            _ageSelection.Initialize(clan.ClanAge);
            _agePopup.SetActive(false);
            UpdateAgeDisplay();

            //_goalSelection.Initialize(clan.Goals);
            //_clanRightsPanel.InitializeRightsToggles(clan.ClanRights);

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

            _saveButton.onClick.RemoveAllListeners();
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

        if (_valueSelection.SelectedValues.Count == 0)
        {
            _errorPopup.ActivatePopUp("Valitse vähintään yksi arvo klaanille.");
            _saveButton.interactable = true;
            return;
        }

        Storefront.Get().GetClanData(ServerManager.Instance.Clan._id, (clanData) =>
        {
            string previousPhrase = clanData.Phrase;
            clanData.Phrase = _clanPhraseField.text;
            clanData.Language = _languageList.SelectedLanguage;
            //clanData.Goals = _goalSelection.GoalsRange;
            clanData.ClanAge = _ageSelection.ClanAgeRange;

            clanData.Values = _valueSelection.SelectedValues;
            clanData.ClanHeartPieces = _heartPieces;

            if (_clanMainView != null)
            {                
                _clanMainView.UpdateProfileFromSettings(clanData);
                ShowProfileAndNormalButtons();
            }

            // These are not saved at the moment
            bool isOpen = !_clanOpenToggle.isOn;
            string password = _clanPasswordField.text;
            //clanData.ClanRights = _clanRightsPanel.ClanRights;

            StartCoroutine(ServerManager.Instance.UpdateClanToServer(clanData, success =>
            {
                _saveButton.interactable = true;
                if (success)
                {               
                    if (!string.IsNullOrEmpty(previousPhrase) && previousPhrase != clanData.Phrase)
                        gameObject.GetComponent<DailyTaskProgressListener>().UpdateProgress("1");

                    gameObject.GetComponent<ClanCulturalPractices>().SettingsChanged(clanData);
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
                /*|| clanData.Goals != _goalSelection.GoalsRange*/
                || clanData.ClanAge != _ageSelection.ClanAgeRange
                /*|| !clanData.ClanRights.SequenceEqual(_clanRightsPanel.ClanRights)*/
                || !clanData.Values.SequenceEqual(_selectedValues) /*clanData.Values != _selectedValues*/;

            if (hasMadeEdits)
            {
                _cancelConfirmationPopup.SetActive(true);
            }
           else
            {
                ShowProfileAndNormalButtons();
            }
        });
    }
    public void OnClickContinueEditingClanSettings() => _cancelConfirmationPopup.SetActive(false);

    public void OnClickDiscardClanSettingEdits()
    {
        _cancelConfirmationPopup.SetActive(false);

        ResetHeartColorChanger();

        ShowProfileAndNormalButtons();
    }

    private void ShowProfileAndNormalButtons()
    {
        if (_clanMainView != null)
        {
            _clanMainView.ShowProfilePage();
        }

        if (_editViewButtons != null)
            _editViewButtons.SetActive(false);

        if (_buttonsInClan != null)
            _buttonsInClan.SetActive(true);
    }


    private void UpdateAgeDisplay()
    {
        if (_ageSelection == null) return;

        var age = _ageSelection.ClanAgeRange;

        if (_ageImage != null)
        {
            var sprite = _ageSelection.GetSelectedAgeSprite();
            _ageImage.sprite = sprite;
            _ageImage.preserveAspect = true;
            _ageImage.enabled = sprite != null;
        }
    }

    public void OnAgeSelectionConfirmed()
    {
        UpdateAgeDisplay();
    }


    public void OpenClanPhrasePopup()
    {
        _clanPhrasePopup.SetActive(true);
        _clanPhraseField.text = _clanPhraseText.text;
    }

    public void ConfirmClanPhraseEdit()
    {
        _clanPhraseText.text = _clanPhraseField.text;
        _clanPhrasePopup.SetActive(false);
    }

    public void CancelClanPhraseEdit()
    {
        _clanPhrasePopup.SetActive(false);
    }

    public void OpenClanRulesPopup()
    {
        _rulesPopup.SetActive(true);
        _rule1Input.text = _rule1Text.text;
        _rule2Input.text = _rule2Text.text;
        _rule3Input.text = _rule3Text.text;
    }

    public void ConfirmClanRulesEdit()
    {
        _rule1Text.text = _rule1Input.text;
        _rule2Text.text = _rule2Input.text;
        _rule3Text.text = _rule3Input.text;
        _rulesPopup.SetActive(false);
    }

    public void CancelClanRulesEdit()
    {
        _rulesPopup.SetActive(false);
    }

    public void OpenClanAgePopup()
    {
        _agePopup.SetActive(true);
    }
}
