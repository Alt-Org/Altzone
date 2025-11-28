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

    [Header("Password")]
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
    [SerializeField] private GameObject _languagePopup;

    [Header("Values")]
    [SerializeField] private ValueSelectionController _valueSelection;
    [SerializeField] private ClanValuePanel _values;
    [SerializeField] private GameObject _valuesPopup;

    [Header("Heart Selector")]
    [SerializeField] private ClanHeartColorChanger _heartColorChanger;
    [SerializeField] private ClanHeartColorSetter _heartColorSetter;
    [SerializeField] private GameObject _editHeartPanel;

    [Header("Other settings fields")]
    //[SerializeField] private ClanRightsPanel _clanRightsPanel;
    [SerializeField] private GameObject _swipeBlockOverlay;
    [SerializeField] private GameObject _chatboxBlockOverlay;

    [Header("Other Buttons")]
    [SerializeField] private Button _saveButton;

    [Header("Tabline buttons")]
    [SerializeField] private GameObject _editViewButtons;
    [SerializeField] private GameObject _buttonsInClan;

    [Header("Popups")]
    [SerializeField] private PopupController _errorPopup;
    [SerializeField] private GameObject _cancelConfirmationPopup;

    [Header("Panels")]
    [SerializeField] private GameObject _mainSettingsPanel;

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
            _languagePopup.SetActive(false);
            _valuesPopup.SetActive(false);
            _cancelConfirmationPopup.SetActive(false);

            // Initialize settings
            _clanName.text = clan.Name;
            _clanMembers.text = "Jäsenmäärä: " + clan.Members.Count.ToString();
            //_clanActivityRanking.text = _clanWinsRanking.text = "-1";

            //Motto init
            _clanPhraseText.text = clan.Phrase;
            _clanPhraseField.text = clan.Phrase;
            _clanPhrasePopup.SetActive(false);

            // Open/closed init TODO password handling
            _clanOpenToggle.isOn = !clan.IsOpen;
            _clanPassword.SetActive(!clan.IsOpen);

            // Rules init
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

            //Age init     
            _ageSelection.Initialize(clan.ClanAge);
            _agePopup.SetActive(false);
            UpdateAgeDisplay();

            //_goalSelection.Initialize(clan.Goals);
            //_clanRightsPanel.InitializeRightsToggles(clan.ClanRights);

            // Language init
            _languageList.Initialize(clan.Language);
            _flagImageSetter.SetFlag(clan.Language);

            // Values init
            _values.SetValues(clan.Values);
            _valueSelection.SetSelected(clan.Values);
            _selectedValues = clan.Values;

            // Heart init
            clan.ClanHeartPieces ??= new();
            _heartPieces = clan.ClanHeartPieces;
            _heartColorChanger.InitializeClanHeart(_heartPieces);
            _heartColorSetter.SetHeartColors(_heartPieces);

            _saveButton.onClick.RemoveAllListeners();
            _saveButton.onClick.AddListener(SaveClanSettings);
        });
    }

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
                ShowProfileTablineButtons();
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
                ShowPopup(_cancelConfirmationPopup);
            }
           else
            {
                ShowProfileTablineButtons();
            }
        });
    }

    public void OnClickContinueEditingClanSettings()
    {
        HidePopup(_cancelConfirmationPopup);
    }

    public void OnClickDiscardClanSettingEdits()
    {
        HidePopup(_cancelConfirmationPopup);

        ResetHeartColorChanger();

        ShowProfileTablineButtons();
    }

    private void ShowPopup(GameObject popup)
    {
        if(_swipeBlockOverlay != null)
            _swipeBlockOverlay.SetActive(true);

        if(_chatboxBlockOverlay != null)
            _chatboxBlockOverlay.SetActive(true);

        if (popup != null)
            popup.SetActive(true);
    }

    private void HidePopup(GameObject popup)
    {
        if (_swipeBlockOverlay != null)
            _swipeBlockOverlay.SetActive(false);

        if (_chatboxBlockOverlay != null)
            _chatboxBlockOverlay.SetActive(false);

        if (popup != null)
            popup.SetActive(false);
    }

    private void ShowProfileTablineButtons()
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

    public void SetClanHeartFromColorChanges()
    {
        _heartPieces = _heartColorChanger.GetHeartPieceDatas();
        _heartColorSetter.SetHeartColors(_heartPieces);
    }

    public void ResetHeartColorChanger() => _heartColorChanger.InitializeClanHeart(_heartPieces);

    public void OpenHeartEditPopup()
    {
        ResetHeartColorChanger();

        ShowPopup(_editHeartPanel);
    }

    public void ConfirmHeartEdit()
    {
        SetClanHeartFromColorChanges();
        HidePopup(_editHeartPanel);
    }

    public void CancelHeartEdit()
    {
        ResetHeartColorChanger();
        HidePopup(_editHeartPanel);
    }

    public void OpenClanPhrasePopup()
    {
        _clanPhraseField.text = _clanPhraseText.text;
        ShowPopup(_clanPhrasePopup);
    }

    public void ConfirmClanPhraseEdit()
    {
        _clanPhraseText.text = _clanPhraseField.text;
        HidePopup(_clanPhrasePopup);
    }

    public void CancelClanPhraseEdit()
    {
        HidePopup(_clanPhrasePopup);
    }

    public void OpenClanRulesPopup()
    {       
        _rule1Input.text = _rule1Text.text;
        _rule2Input.text = _rule2Text.text;
        _rule3Input.text = _rule3Text.text;
        ShowPopup(_rulesPopup);
    }

    public void ConfirmClanRulesEdit()
    {
        _rule1Text.text = _rule1Input.text;
        _rule2Text.text = _rule2Input.text;
        _rule3Text.text = _rule3Input.text;
        HidePopup(_rulesPopup);
    }

    public void CancelClanRulesEdit()
    {
        HidePopup(_rulesPopup);
    }

    public void OpenClanAgePopup()
    {
        ShowPopup(_agePopup);
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
        HidePopup(_agePopup);
    }

    public void CancelClanAgeEdit()
    {
        HidePopup(_agePopup);
    }

    public void OpenLanguagePopup()
    {
        ShowPopup(_languagePopup);
    }

    public void ConfirmLanguageEdit()
    {
        _flagImageSetter.SetFlag(_languageList.SelectedLanguage);
        HidePopup(_languagePopup);
    }

    public void CancelLanguagePopup()
    {
        HidePopup(_languagePopup);
    }

    public void OpenValuesPopup()
    {
        _valueSelection.SetSelected(_selectedValues);

        ShowPopup(_valuesPopup);
    }

    public void ConfirmValuesEdit()
    {
        _selectedValues = _valueSelection.SelectedValues;
        _values.SetValues(_selectedValues);

        HidePopup(_valuesPopup);
    }

    public void CancelValuesEdit()
    {
        HidePopup(_valuesPopup);
    }

}
