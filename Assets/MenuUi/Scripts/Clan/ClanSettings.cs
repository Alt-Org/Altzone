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

public class ClanSettings : AltMonoBehaviour
{
    [Header("Motto")]
    [SerializeField] private TextMeshProUGUI _clanPhraseText;
    [SerializeField] private TMP_InputField _clanPhraseField;
    [SerializeField] private GameObject _clanPhrasePopup;
    [SerializeField] private Button _mottoCloseButton;

    [Header("Rules (display texts)")]
    [SerializeField] private TextMeshProUGUI _rule1Text;
    [SerializeField] private TextMeshProUGUI _rule2Text;
    [SerializeField] private TextMeshProUGUI _rule3Text;

    [Header("Rules (popup inputs)")]
    [SerializeField] private TMP_InputField _rule1Input;
    [SerializeField] private TMP_InputField _rule2Input;
    [SerializeField] private TMP_InputField _rule3Input;
    [SerializeField] private RulesSelectionController _ruleSelection;
    [SerializeField] private GameObject _rulesPopup;
    [SerializeField] private Button _rulesCloseButton;

    [Header("Password")]
    [SerializeField] private GameObject _clanPassword;
    [SerializeField] private TMP_InputField _clanPasswordField;
    [SerializeField] private Toggle _clanOpenToggle;

    [SerializeField] private GameObject _lockClanPopup;
    [SerializeField] private GameObject _openClanPopup;
    [SerializeField] private GameObject _createClanPasswordPopup;

    [SerializeField] private Button _lockClanConfirmButton;
    [SerializeField] private Button _lockClanCancelButton;

    [SerializeField] private Button _openClanConfirmButton;
    [SerializeField] private Button _openClanCancelButton;

    [SerializeField] private Button _createPasswordConfirmButton;
    [SerializeField] private Button _createPasswordCancelButton;

    [SerializeField] private ToggleImage _clanLockToggleImage;

    [Header("Age")]
    //[SerializeField] private ClanAgeSelection _ageSelection;
    [SerializeField] private ClanAgeList _ageSelection;
    [SerializeField] private Image _ageImage;
    [SerializeField] private GameObject _agePopup;
    [SerializeField] private Button _ageCloseButton;

    [Header("Language")]
    [SerializeField] private ClanLanguageList _languageList;
    [SerializeField] private LanguageFlagImage _flagImageSetter;
    [SerializeField] private GameObject _languagePopup;
    [SerializeField] private Button _languageCloseButton;

    [Header("Values")]
    [SerializeField] private ValueSelectionController _valueSelection;
    [SerializeField] private ClanValuePanel _values;
    [SerializeField] private GameObject _valuesPopup;
    [SerializeField] private Button _valuesCloseButton;

    [Header("Roles")]
    //[SerializeField] private GameObject _rolesPopup;

    [Header("Heart Selector")]
    [SerializeField] private ClanHeartColorChanger _heartColorChanger;
    [SerializeField] private ClanHeartColorSetter _heartColorSetter;
    [SerializeField] private GameObject _editHeartPanel;

    [SerializeField] private Toggle _fillWholeHeartToggle;
    [SerializeField] private TMP_Text _heartModeLabel;
    [SerializeField] private string _wholeHeartText = "Tila: väritä koko sydän";
    [SerializeField] private string _pieceModeText = "Tila: muokkaa palaa kerrallaan";
    [SerializeField] private Image _toggleIcon;
    [SerializeField] private Sprite _iconWholeHeart;
    [SerializeField] private Sprite _iconPieceMode;

    [Header("Other settings fields")]
    //[SerializeField] private ClanRightsPanel _clanRightsPanel;
    [SerializeField] private GameObject _swipeBlockOverlay;

    [Header("Buttons")]
    [SerializeField] private Button _saveButton;
    [SerializeField] private Button _cancelEditsButton;
    [SerializeField] private Button _closeButton;

    [SerializeField] private Button _openMottoButton;
    [SerializeField] private Button _mottoConfirmButton;
    [SerializeField] private Button _mottoCancelButton;

    [SerializeField] private Button _openRulesButton;
    [SerializeField] private Button _rulesConfirmButton;
    [SerializeField] private Button _rulesCancelButton;

    [SerializeField] private Button _openAgeButton;
    [SerializeField] private Button _ageConfirmButton;
    [SerializeField] private Button _ageCancelButton;

    [SerializeField] private Button _openLanguageButton;
    [SerializeField] private Button _languageConfirmButton;
    [SerializeField] private Button _languageCancelButton;

    [SerializeField] private Button _openValuesButton;
    [SerializeField] private Button _valuesConfirmButton;
    [SerializeField] private Button _valuesCancelButton;

    //[SerializeField] private Button _openRolesButton;
    //[SerializeField] private Button _rolesConfirmButton;
    //[SerializeField] private Button _rolesCancelButton;

    //[SerializeField] private Button _openHeartEditButton;
    //[SerializeField] private Button _heartConfirmButton;
    //[SerializeField] private Button _heartCancelButton;

    [SerializeField] private Button _cancelConfirmContinueButton;
    [SerializeField] private Button _cancelConfirmDiscardButton;

    [Header("Popups")]
    [SerializeField] private PopupController _errorPopup;
    [SerializeField] private GameObject _cancelConfirmationPopup;

    [Header("Settings Popup")]
    [SerializeField] private GameObject _clanSettingsPopup;
    [SerializeField] private ClanMainView _clanMainView;

    [Header("Values / Roles / Rules Carousel")]
    [SerializeField] private Button _valuesRolesRulesLeftButton;
    [SerializeField] private Button _valuesRolesRulesRightButton;

    [SerializeField] private TextMeshProUGUI _valuesRolesRulesTitle;

    [SerializeField] private GameObject _valuesPanel;
    [SerializeField] private GameObject _rolesPanel;
    [SerializeField] private GameObject _rulesPanel;

    [SerializeField] private Image _valuesDot;
    [SerializeField] private Image _rolesDot;
    [SerializeField] private Image _rulesDot;

    [SerializeField] private Color _activeDotColor = new Color(1f, 0.6f, 0f, 1f);
    [SerializeField] private Color _inactiveDotColor = new Color(0.75f, 0.85f, 0.9f, 1f);

    private int _valuesRolesRulesIndex = 0;

    private const int ValuesIndex = 0;
    private const int RolesIndex = 1;
    private const int RulesIndex = 2;

    private List<HeartPieceData> _heartPieces;
    private List<ClanValues> _selectedValues = new();
    private Language _selectedLanguage;

    private ClanData _currentClanData;

    private string _originalPhrase;
    private List<Rules> _originalRules = new();
    private ClanAge _originalAge;
    private ClanAge _selectedAge;
    private Language _originalLanguage;
    private List<ClanValues> _originalValues = new();
    private List<HeartPieceData> _originalHeartPieces = new();
    private List<Rules> _selectedRules = new();
    private bool _originalIsOpen;
    private bool _selectedIsOpen;
    private string _selectedPassword = string.Empty;

    private void OnEnable()
    {
        RegisterUiListeners();

        _valuesRolesRulesIndex = ValuesIndex;
        UpdateValuesRolesRulesPanel();

        Storefront.Get().GetClanData(ServerManager.Instance.Clan._id, (clan) =>
        {
            _currentClanData = clan;

            clan.Rules ??= new List<Rules>();
            clan.Values ??= new List<ClanValues>();
            clan.Members ??= new List<ClanMember>();
            clan.ClanHeartPieces ??= new List<HeartPieceData>();

            SaveOriginalState(clan);

            // Show correct popups closed
            if (_editHeartPanel != null)
                _editHeartPanel.SetActive(false);

            if (_languagePopup != null)
                _languagePopup.SetActive(false);

            if (_agePopup != null)
                _agePopup.SetActive(false);

            if (_valuesPopup != null)
                _valuesPopup.SetActive(false);

            if (_rulesPopup != null)
                _rulesPopup.SetActive(false);

            /*if (_rolesPopup != null)
                _rolesPopup.SetActive(false);*/

            if (_clanPhrasePopup != null)
                _clanPhrasePopup.SetActive(false);

            if (_cancelConfirmationPopup != null)
                _cancelConfirmationPopup.SetActive(false);

            if (_lockClanPopup != null)
                _lockClanPopup.SetActive(false);

            if (_openClanPopup != null)
                _openClanPopup.SetActive(false);

            if (_createClanPasswordPopup != null)
                _createClanPasswordPopup.SetActive(false);

            //Motto init
            _clanPhraseText.text = clan.Phrase;
            _clanPhraseField.text = clan.Phrase;
            _clanPhrasePopup.SetActive(false);

            // Open/closed init TODO password handling
            _selectedIsOpen = clan.IsOpen;
            _selectedPassword = string.Empty;

            UpdateClanOpenUi();

            // Rules init
            string rule1 = clan.Rules.Count > 0
                ? ClanDataTypeConverter.GetRulesText(clan.Rules[0])
                : string.Empty;
            string rule2 = clan.Rules.Count > 1
                ? ClanDataTypeConverter.GetRulesText(clan.Rules[1])
                : string.Empty;
            string rule3 = clan.Rules.Count > 2
                ? ClanDataTypeConverter.GetRulesText(clan.Rules[2])
                : string.Empty;

            _rule1Text.text = rule1;
            _rule2Text.text = rule2;
            _rule3Text.text = rule3;

            _selectedRules = new List<Rules>(clan.Rules);
            _ruleSelection.SetSelected(new List<Rules>(_selectedRules));
            //_ruleSelection.SetSelected(clan.Rules);
            _rulesPopup.SetActive(false);

            //Age init     
            _selectedAge = clan.ClanAge;
            _ageSelection.Initialize(_selectedAge);
            _agePopup.SetActive(false);
            UpdateAgeDisplay();

            //_goalSelection.Initialize(clan.Goals);
            //_clanRightsPanel.InitializeRightsToggles(clan.ClanRights);

            // Language init
            _languageList.Initialize(clan.Language);
            _selectedLanguage = clan.Language;
            _flagImageSetter.SetFlag(clan.Language);

            // Values init
            _values.SetValues(clan.Values);
            //_valueSelection.SetSelected(clan.Values);
            _selectedValues = new List<ClanValues>(clan.Values);

            // Heart init
            clan.ClanHeartPieces ??= new();
            _heartPieces = new List<HeartPieceData>(clan.ClanHeartPieces);

            if (_heartColorChanger != null && _heartPieces != null)
                _heartColorChanger.InitializeClanHeart(_heartPieces);

            if (_heartColorSetter != null && _heartPieces != null)
                _heartColorSetter.SetHeartColors(_heartPieces);

            if (_fillWholeHeartToggle != null)
            {
                _fillWholeHeartToggle.isOn = false;
                OnFillWholeHeartToggleChanged(_fillWholeHeartToggle.isOn);
            }
            else if (_heartColorChanger != null)
            {
                _heartColorChanger.SetFillWholeHeart(true);
            }
        });
    }

    private void OnDisable()
    {
        UnregisterUiListeners();
    }

    private void SaveOriginalState(ClanData clan)
    {
        if (clan == null) return;

        _originalPhrase = clan.Phrase ?? string.Empty;
        _originalRules = clan.Rules != null ? new List<Rules>(clan.Rules) : new List<Rules>();
        _originalAge = clan.ClanAge;
        _originalLanguage = clan.Language;
        _originalValues = clan.Values != null ? new List<ClanValues>(clan.Values) : new List<ClanValues>();
        _originalIsOpen = clan.IsOpen;

        clan.ClanHeartPieces ??= new List<HeartPieceData>();
        _originalHeartPieces = new List<HeartPieceData>(clan.ClanHeartPieces);
    }

    private void ResetSettingsToOriginalState()
    {
        _clanPhraseText.text = _originalPhrase;
        _clanPhraseField.text = _originalPhrase;

        _selectedRules = new List<Rules>(_originalRules);
        _ruleSelection.SetSelected(new List<Rules>(_originalRules));

        string rule1 = _originalRules.Count > 0
            ? ClanDataTypeConverter.GetRulesText(_originalRules[0])
            : string.Empty;
        string rule2 = _originalRules.Count > 1
            ? ClanDataTypeConverter.GetRulesText(_originalRules[1])
            : string.Empty;
        string rule3 = _originalRules.Count > 2
            ? ClanDataTypeConverter.GetRulesText(_originalRules[2])
            : string.Empty;

        _rule1Text.text = rule1;
        _rule2Text.text = rule2;
        _rule3Text.text = rule3;

        _selectedAge = _originalAge;
        _ageSelection.Initialize(_selectedAge);
        UpdateAgeDisplay();

        _selectedLanguage = _originalLanguage;
        _languageList.Initialize(_originalLanguage);
        _flagImageSetter.SetFlag(_originalLanguage);

        _selectedValues = new List<ClanValues>(_originalValues);
        _values.SetValues(_selectedValues);
        //_valueSelection.SetSelected(new List<ClanValues>(_originalValues));

        _selectedIsOpen = _originalIsOpen;
        _selectedPassword = string.Empty;

        if (_clanPasswordField != null)
            _clanPasswordField.text = string.Empty;

        UpdateClanOpenUi();

        _heartPieces = new List<HeartPieceData>(_originalHeartPieces);
        ResetHeartColorChanger();
    }

    private bool CanCurrentPlayerEditClan(ClanData clanData)
    {
        var serverManager = ServerManager.Instance;
        if (serverManager == null) return false;

        var player = serverManager.Player;
        if (player == null) return false;

        if (player.clan_id != clanData.Id) return false;

        var roleId = player.clanRole_id;
        if (string.IsNullOrEmpty(roleId)) return false;

        var role = clanData.ClanRoles?.Find(r => r._id == roleId);
        if (role == null || role.rights == null) return false;

        return role.rights.edit_clan_data;
    }


    public void SaveClanSettings()
    {
        _saveButton.interactable = false;

        if (_valueSelection.SelectedValues.Count == 0)
        {
            SignalBus.OnChangePopupInfoSignal("Valitse vähintään yksi arvo klaanille.");
            _saveButton.interactable = true;
            return;
        }

        if (_currentClanData == null)
        {
            SignalBus.OnChangePopupInfoSignal("Klaanin tietoja ei löytynyt.");
            _saveButton.interactable = true;
            return;
        }

        if (!CanCurrentPlayerEditClan(_currentClanData))
        {
            SignalBus.OnChangePopupInfoSignal("Mites pääsit tähän ikkunaan? Sinulla ei ole oikeuksia muokata klaanin asetuksia.");
            _saveButton.interactable = true;
            return;
        }

        string previousPhrase = _currentClanData.Phrase;

        _currentClanData.Phrase = _clanPhraseField.text;
        _currentClanData.Language = _selectedLanguage;
        //_currentClanData.Goals = _goalSelection.GoalsRange;
        _currentClanData.ClanAge = _selectedAge;
        _currentClanData.Rules = new List<Rules>(_selectedRules);
        _currentClanData.Values = new List<ClanValues>(_selectedValues);
        _currentClanData.ClanHeartPieces = _heartPieces;

        // These are not saved at the moment
        _currentClanData.IsOpen = _selectedIsOpen;

        // TODO: add password handling to clan data and saving. For now, password changes are not saved to server and only used for UI purposes
        string password = _selectedPassword;

        //_currentClanData.ClanRights = _clanRightsPanel.ClanRights;

        StartCoroutine(ServerManager.Instance.UpdateClanToServer(_currentClanData, success =>
        {
            _saveButton.interactable = true;

            if (success)
            {
                SaveOriginalState(_currentClanData);
                if (_clanMainView != null)
                {
                    _clanMainView.UpdateProfileFromSettings(_currentClanData);
                    
                }

                CloseSettingsPopup();

                if (!string.IsNullOrEmpty(previousPhrase) && previousPhrase != _currentClanData.Phrase)
                    gameObject.GetComponent<DailyTaskProgressListener>().UpdateProgress("1");

                gameObject.GetComponent<ClanCulturalPractices>().SettingsChanged(_currentClanData);
            }
            else
            {
                SignalBus.OnChangePopupInfoSignal("Asetusten tallentaminen ei onnistunut.");
            }
        }));
    }

    private void RegisterUiListeners()
    {
        if (_saveButton) _saveButton.onClick.AddListener(SaveClanSettings);

        if (_fillWholeHeartToggle) _fillWholeHeartToggle.onValueChanged.AddListener(OnFillWholeHeartToggleChanged);

        if (_cancelEditsButton) _cancelEditsButton.onClick.AddListener(OnClickCancelClanSettingEdits);

        if (_closeButton) _closeButton.onClick.AddListener(OnClickCancelClanSettingEdits);

        if (_valuesRolesRulesLeftButton)
            _valuesRolesRulesLeftButton.onClick.AddListener(ShowPreviousValuesRolesRulesPanel);

        if (_valuesRolesRulesRightButton)
            _valuesRolesRulesRightButton.onClick.AddListener(ShowNextValuesRolesRulesPanel);

        // Motto popup
        if (_openMottoButton) _openMottoButton.onClick.AddListener(OpenClanPhrasePopup);
        if (_mottoConfirmButton) _mottoConfirmButton.onClick.AddListener(ConfirmClanPhraseEdit);
        if (_mottoCancelButton) _mottoCancelButton.onClick.AddListener(CancelClanPhraseEdit);
        if (_mottoCloseButton) _mottoCloseButton.onClick.AddListener(CancelClanPhraseEdit);

        // Rules popup
        if (_openRulesButton) _openRulesButton.onClick.AddListener(OpenClanRulesPopup);
        if (_rulesConfirmButton) _rulesConfirmButton.onClick.AddListener(ConfirmClanRulesEdit);
        if (_rulesCancelButton) _rulesCancelButton.onClick.AddListener(CancelClanRulesEdit);
        if (_rulesCloseButton) _rulesCloseButton.onClick.AddListener(CancelClanRulesEdit);

        // Age popup
        if (_openAgeButton) _openAgeButton.onClick.AddListener(OpenClanAgePopup);
        if (_ageConfirmButton) _ageConfirmButton.onClick.AddListener(OnAgeSelectionConfirmed);
        if (_ageCancelButton) _ageCancelButton.onClick.AddListener(CancelClanAgeEdit);
        if (_ageCloseButton) _ageCloseButton.onClick.AddListener(CancelClanAgeEdit);

        // Language popup
        if (_openLanguageButton) _openLanguageButton.onClick.AddListener(OpenLanguagePopup);
        if (_languageConfirmButton) _languageConfirmButton.onClick.AddListener(ConfirmLanguageEdit);
        if (_languageCancelButton) _languageCancelButton.onClick.AddListener(CancelLanguagePopup);
        if (_languageCloseButton) _languageCloseButton.onClick.AddListener(CancelLanguagePopup);

        // Values popup
        if (_openValuesButton) _openValuesButton.onClick.AddListener(OpenValuesPopup);
        if (_valuesConfirmButton) _valuesConfirmButton.onClick.AddListener(ConfirmValuesEdit);
        if (_valuesCancelButton) _valuesCancelButton.onClick.AddListener(CancelValuesEdit);
        if (_valuesCloseButton) _valuesCloseButton.onClick.AddListener(CancelValuesEdit);

        // Heart popup
        /* if (_openHeartEditButton) _openHeartEditButton.onClick.AddListener(OpenHeartEditPopup);
        if (_heartConfirmButton) _heartConfirmButton.onClick.AddListener(ConfirmHeartEdit);
        if (_heartCancelButton) _heartCancelButton.onClick.AddListener(CancelHeartEdit);

        // Roles popup
       if (_openRolesButton) _openRolesButton.onClick.AddListener(OpenClanRolesPopup);
        if (_rolesConfirmButton) _rolesConfirmButton.onClick.AddListener(ConfirmClanRolesEdit);
        if (_rolesCancelButton) _rolesCancelButton.onClick.AddListener(CancelClanRolesEdit);*/

        // Open / locked clan popups
        if (_clanOpenToggle) _clanOpenToggle.onValueChanged.AddListener(OnClanOpenToggleChanged);

        if (_lockClanConfirmButton) _lockClanConfirmButton.onClick.AddListener(ConfirmLockClanPopup);
        if (_lockClanCancelButton) _lockClanCancelButton.onClick.AddListener(CancelClanLockChange);

        if (_openClanConfirmButton) _openClanConfirmButton.onClick.AddListener(ConfirmOpenClanPopup);
        if (_openClanCancelButton) _openClanCancelButton.onClick.AddListener(CancelClanOpenChange);

        if (_createPasswordConfirmButton) _createPasswordConfirmButton.onClick.AddListener(ConfirmCreateClanPassword);
        if (_createPasswordCancelButton) _createPasswordCancelButton.onClick.AddListener(CancelCreateClanPassword);

        // Cancel confirmation popup
        if (_cancelConfirmContinueButton) _cancelConfirmContinueButton.onClick.AddListener(OnClickContinueEditingClanSettings);
        if (_cancelConfirmDiscardButton) _cancelConfirmDiscardButton.onClick.AddListener(OnClickDiscardClanSettingEdits);
    }

    private void UnregisterUiListeners()
    {
        if (_saveButton) _saveButton.onClick.RemoveListener(SaveClanSettings);

        if (_fillWholeHeartToggle) _fillWholeHeartToggle.onValueChanged.RemoveListener(OnFillWholeHeartToggleChanged);

        if (_cancelEditsButton) _cancelEditsButton.onClick.RemoveListener(OnClickCancelClanSettingEdits);

        if (_closeButton) _closeButton.onClick.RemoveListener(OnClickCancelClanSettingEdits);

        if (_valuesRolesRulesLeftButton)
            _valuesRolesRulesLeftButton.onClick.RemoveListener(ShowPreviousValuesRolesRulesPanel);

        if (_valuesRolesRulesRightButton)
            _valuesRolesRulesRightButton.onClick.RemoveListener(ShowNextValuesRolesRulesPanel);

        if (_openMottoButton) _openMottoButton.onClick.RemoveListener(OpenClanPhrasePopup);
        if (_mottoConfirmButton) _mottoConfirmButton.onClick.RemoveListener(ConfirmClanPhraseEdit);
        if (_mottoCancelButton) _mottoCancelButton.onClick.RemoveListener(CancelClanPhraseEdit);
        if (_mottoCloseButton) _mottoCloseButton.onClick.RemoveListener(CancelClanPhraseEdit);

        if (_openRulesButton) _openRulesButton.onClick.RemoveListener(OpenClanRulesPopup);
        if (_rulesConfirmButton) _rulesConfirmButton.onClick.RemoveListener(ConfirmClanRulesEdit);
        if (_rulesCancelButton) _rulesCancelButton.onClick.RemoveListener(CancelClanRulesEdit);
        if (_rulesCloseButton) _rulesCloseButton.onClick.RemoveListener(CancelClanRulesEdit);

        if (_openAgeButton) _openAgeButton.onClick.RemoveListener(OpenClanAgePopup);
        if (_ageConfirmButton) _ageConfirmButton.onClick.RemoveListener(OnAgeSelectionConfirmed);
        if (_ageCancelButton) _ageCancelButton.onClick.RemoveListener(CancelClanAgeEdit);
        if (_ageCloseButton) _ageCloseButton.onClick.RemoveListener(CancelClanAgeEdit);

        if (_openLanguageButton) _openLanguageButton.onClick.RemoveListener(OpenLanguagePopup);
        if (_languageConfirmButton) _languageConfirmButton.onClick.RemoveListener(ConfirmLanguageEdit);
        if (_languageCancelButton) _languageCancelButton.onClick.RemoveListener(CancelLanguagePopup);
        if (_languageCloseButton) _languageCloseButton.onClick.RemoveListener(CancelLanguagePopup);

        if (_openValuesButton) _openValuesButton.onClick.RemoveListener(OpenValuesPopup);
        if (_valuesConfirmButton) _valuesConfirmButton.onClick.RemoveListener(ConfirmValuesEdit);
        if (_valuesCancelButton) _valuesCancelButton.onClick.RemoveListener(CancelValuesEdit);
        if (_valuesCloseButton) _valuesCloseButton.onClick.RemoveListener(CancelValuesEdit);

        /*if (_openHeartEditButton) _openHeartEditButton.onClick.RemoveListener(OpenHeartEditPopup);
        if (_heartConfirmButton) _heartConfirmButton.onClick.RemoveListener(ConfirmHeartEdit);
        if (_heartCancelButton) _heartCancelButton.onClick.RemoveListener(CancelHeartEdit);

        if (_openRolesButton) _openRolesButton.onClick.RemoveListener(OpenClanRolesPopup);
        if (_rolesConfirmButton) _rolesConfirmButton.onClick.RemoveListener(ConfirmClanRolesEdit);
        if (_rolesCancelButton) _rolesCancelButton.onClick.RemoveListener(CancelClanRolesEdit);*/

        if (_clanOpenToggle) _clanOpenToggle.onValueChanged.RemoveListener(OnClanOpenToggleChanged);

        if (_lockClanConfirmButton) _lockClanConfirmButton.onClick.RemoveListener(ConfirmLockClanPopup);
        if (_lockClanCancelButton) _lockClanCancelButton.onClick.RemoveListener(CancelClanLockChange);

        if (_openClanConfirmButton) _openClanConfirmButton.onClick.RemoveListener(ConfirmOpenClanPopup);
        if (_openClanCancelButton) _openClanCancelButton.onClick.RemoveListener(CancelClanOpenChange);

        if (_createPasswordConfirmButton) _createPasswordConfirmButton.onClick.RemoveListener(ConfirmCreateClanPassword);
        if (_createPasswordCancelButton) _createPasswordCancelButton.onClick.RemoveListener(CancelCreateClanPassword);

        if (_cancelConfirmContinueButton) _cancelConfirmContinueButton.onClick.RemoveListener(OnClickContinueEditingClanSettings);
        if (_cancelConfirmDiscardButton) _cancelConfirmDiscardButton.onClick.RemoveListener(OnClickDiscardClanSettingEdits);
    }

    public void OnClickContinueEditingClanSettings()
    {
        HidePopup(_cancelConfirmationPopup);
    }

    public void OnClickCancelClanSettingEdits()
    {
        bool hasMadeEdits = _heartColorChanger.IsAnyPieceChanged()
            || _originalPhrase != _clanPhraseField.text
            || _originalLanguage != _selectedLanguage
            || _originalAge != _selectedAge
            || _originalIsOpen != _selectedIsOpen
            || !_originalRules.SequenceEqual(_selectedRules)
            || !_originalValues.SequenceEqual(_selectedValues);

        if (hasMadeEdits)
        {
            ShowPopup(_cancelConfirmationPopup);
        }
        else
        {
            CloseSettingsPopup();
        }
    }

    public void OnClickDiscardClanSettingEdits()
    {
        HidePopup(_cancelConfirmationPopup);

        ResetSettingsToOriginalState();
        CloseSettingsPopup();
    }

    private void ShowPopup(GameObject popup)
    {
        if (_swipeBlockOverlay != null)
            _swipeBlockOverlay.SetActive(true);

        if (popup != null)
            popup.SetActive(true);
    }

    private void HidePopup(GameObject popup)
    {
        if (_swipeBlockOverlay != null)
            _swipeBlockOverlay.SetActive(false);

        if (popup != null)
            popup.SetActive(false);
    }

    private void CloseSettingsPopup()
    {
        if (_cancelConfirmationPopup != null)
            _cancelConfirmationPopup.SetActive(false);

        if (_swipeBlockOverlay != null)
            _swipeBlockOverlay.SetActive(false);

        if (_clanSettingsPopup != null)
            _clanSettingsPopup.SetActive(false);
    }

    private void OnFillWholeHeartToggleChanged(bool isOn)
    {
        if (_heartModeLabel != null)
        {
            _heartModeLabel.text = isOn ? _pieceModeText : _wholeHeartText;
        }

        if (_toggleIcon != null)
        {
            _toggleIcon.sprite = isOn ? _iconPieceMode : _iconWholeHeart;
        }

        if (_heartColorChanger != null)
        {
            bool fillWholeHeart = !isOn;
            _heartColorChanger.SetFillWholeHeart(fillWholeHeart);
        }
    }

    public void SetClanHeartFromColorChanges()
    {
        _heartPieces = _heartColorChanger.GetHeartPieceDatas();
        _heartColorSetter.SetHeartColors(_heartPieces);
    }

    public void ResetHeartColorChanger()
    {
        if (_heartColorChanger != null && _heartPieces != null)
        {
            _heartColorChanger.InitializeClanHeart(_heartPieces);
        }

        if (_heartColorSetter != null && _heartPieces != null)
        {
            _heartColorSetter.SetHeartColors(_heartPieces);
        }
    }

    public void OpenHeartEditPopup()
    {
        ResetHeartColorChanger();

        if (_fillWholeHeartToggle != null)
        {
            OnFillWholeHeartToggleChanged(_fillWholeHeartToggle.isOn);
        }

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
        if (_clanPhraseField != null && _clanPhraseText != null)
        {
            _clanPhraseField.text = _clanPhraseText.text;
        }

        ShowPopup(_clanPhrasePopup);
    }

    private void TryUpdateClanMottoDailyTaskProgress(string newPhrase)
    {
        if (DailyTaskProgressManager.Instance == null)
            return;

        if (DailyTaskProgressManager.Instance.CurrentPlayerTask == null)
            return;

        if (DailyTaskProgressManager.Instance.CurrentPlayerTask.EducationSocialType !=
            Altzone.Scripts.Model.Poco.Game.TaskEducationSocialType.ChangeClanMotto)
            return;

        if (string.IsNullOrWhiteSpace(newPhrase))
            return;

        StartCoroutine(GetClanData(clan =>
        {
            if (clan == null)
                return;

            if (clan.Phrase != newPhrase)
            {
                DailyTaskProgressManager.Instance.UpdateTaskProgress(
                    Altzone.Scripts.Model.Poco.Game.TaskEducationSocialType.ChangeClanMotto,
                    "1"
                );
            }
        }));
    }

    public void ConfirmClanPhraseEdit()
    {
        if (_clanPhraseField == null || _clanPhraseText == null)
        {
            HidePopup(_clanPhrasePopup);
            return;
        }

        string newPhrase = _clanPhraseField.text;

        TryUpdateClanMottoDailyTaskProgress(newPhrase);

        _clanPhraseText.text = newPhrase;

        HidePopup(_clanPhrasePopup);
    }

    public void CancelClanPhraseEdit()
    {
        if (_clanPhraseField != null && _clanPhraseText != null)
        {
            _clanPhraseField.text = _clanPhraseText.text;
        }

        HidePopup(_clanPhrasePopup);
    }

    public void OpenClanRulesPopup()
    {
        _rule1Input.text = _rule1Text.text;
        _rule2Input.text = _rule2Text.text;
        _rule3Input.text = _rule3Text.text;

        ShowPopup(_rulesPopup);

        Canvas.ForceUpdateCanvases();

        _ruleSelection.SetSelected(new List<Rules>(_selectedRules));

        var popupRect = _rulesPopup.GetComponent<RectTransform>();
        if (popupRect != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(popupRect);

        Canvas.ForceUpdateCanvases();
    }

    public void ConfirmClanRulesEdit()
    {
        _selectedRules = new List<Rules>(_ruleSelection.SelectedRules);

        string rule1 = _selectedRules.Count > 0
            ? ClanDataTypeConverter.GetRulesText(_selectedRules[0])
            : string.Empty;
        string rule2 = _selectedRules.Count > 1
            ? ClanDataTypeConverter.GetRulesText(_selectedRules[1])
            : string.Empty;
        string rule3 = _selectedRules.Count > 2
            ? ClanDataTypeConverter.GetRulesText(_selectedRules[2])
            : string.Empty;

        if (rule1 != _rule1Text.text || rule2 != _rule2Text.text || rule3 != _rule3Text.text)
        {
            if (DailyTaskProgressManager.Instance.CurrentPlayerTask?.EducationCultureType ==
                Altzone.Scripts.Model.Poco.Game.TaskEducationCultureType.ClanCulturalGuideline)
            {
                DailyTaskProgressManager.Instance.UpdateTaskProgress(
                    Altzone.Scripts.Model.Poco.Game.TaskEducationCultureType.ClanCulturalGuideline, "1");
            }
        }

        _rule1Text.text = rule1;
        _rule2Text.text = rule2;
        _rule3Text.text = rule3;

        HidePopup(_rulesPopup);
    }

    public void CancelClanRulesEdit()
    {
        _ruleSelection.SetSelected(new List<Rules>(_selectedRules));

        string rule1 = _ruleSelection.SelectedRules.Count > 0 ?
        ClanDataTypeConverter.GetRulesText(_ruleSelection.SelectedRules[0]) : string.Empty;
        string rule2 = _ruleSelection.SelectedRules.Count > 1 ?
        ClanDataTypeConverter.GetRulesText(_ruleSelection.SelectedRules[1]) : string.Empty;
        string rule3 = _ruleSelection.SelectedRules.Count > 2 ?
        ClanDataTypeConverter.GetRulesText(_ruleSelection.SelectedRules[2]) : string.Empty;

        if (rule1 != _rule1Text.text || rule2 != _rule2Text.text || rule3 != _rule3Text.text)
        {
            if (DailyTaskProgressManager.Instance.CurrentPlayerTask?.EducationCultureType == Altzone.Scripts.Model.Poco.Game.TaskEducationCultureType.ClanCulturalGuideline)
                DailyTaskProgressManager.Instance.UpdateTaskProgress(Altzone.Scripts.Model.Poco.Game.TaskEducationCultureType.ClanCulturalGuideline, "1");
        }

        _rule1Text.text = rule1;
        _rule2Text.text = rule2;
        _rule3Text.text = rule3;

        Canvas.ForceUpdateCanvases();

        var popupRect = _rulesPopup.GetComponent<RectTransform>();
        if (popupRect != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(popupRect);

        HidePopup(_rulesPopup);
    }

    public void OpenClanAgePopup()
    {
        if (_ageSelection != null)
        {
            _ageSelection.Initialize(_selectedAge);
        }

        ShowPopup(_agePopup);
    }

    private void UpdateAgeDisplay()
    {
        if (_ageSelection == null) return;

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
        if (_ageSelection != null)
        {
            _selectedAge = _ageSelection.ClanAgeRange;
        }

        UpdateAgeDisplay();
        HidePopup(_agePopup);
    }

    public void CancelClanAgeEdit()
    {
        if (_ageSelection != null)
        {
            _ageSelection.Initialize(_selectedAge);
        }

        HidePopup(_agePopup);
    }

    private void UpdateClanOpenUi()
    {
        if (_clanOpenToggle != null)
            _clanOpenToggle.SetIsOnWithoutNotify(!_selectedIsOpen);

        if (_clanPassword != null)
            _clanPassword.SetActive(!_selectedIsOpen);

        if (_clanLockToggleImage != null)
            _clanLockToggleImage.RefreshImage();
    }

    private void OnClanOpenToggleChanged(bool isLocked)
    {
        bool wantsOpen = !isLocked;

        UpdateClanOpenUi();

        if (wantsOpen == _selectedIsOpen)
            return;

        if (wantsOpen)
        {
            ShowPopup(_openClanPopup);
        }
        else
        {
            ShowPopup(_lockClanPopup);
        }
    }

    public void ConfirmLockClanPopup()
    {
        if (_lockClanPopup != null)
            _lockClanPopup.SetActive(false);

        if (_clanPasswordField != null)
            _clanPasswordField.text = _selectedPassword;

        ShowPopup(_createClanPasswordPopup);
    }

    public void CancelClanLockChange()
    {
        HidePopup(_lockClanPopup);
        UpdateClanOpenUi();
    }

    public void ConfirmCreateClanPassword()
    {
        string password = _clanPasswordField != null ? _clanPasswordField.text : string.Empty;

        if (string.IsNullOrWhiteSpace(password))
        {
            SignalBus.OnChangePopupInfoSignal("Lisää klaanille salasana.");
            return;
        }

        _selectedPassword = password;
        _selectedIsOpen = false;

        UpdateClanOpenUi();
        HidePopup(_createClanPasswordPopup);
    }

    public void CancelCreateClanPassword()
    {
        if (_clanPasswordField != null)
            _clanPasswordField.text = _selectedPassword;

        HidePopup(_createClanPasswordPopup);
        UpdateClanOpenUi();
    }

    public void ConfirmOpenClanPopup()
    {
        _selectedIsOpen = true;
        _selectedPassword = string.Empty;

        if (_clanPasswordField != null)
            _clanPasswordField.text = string.Empty;

        UpdateClanOpenUi();
        HidePopup(_openClanPopup);
    }

    public void CancelClanOpenChange()
    {
        HidePopup(_openClanPopup);
        UpdateClanOpenUi();
    }

    public void OpenLanguagePopup()
    {
        if (_languageList != null)
        {
            _languageList.Initialize(_selectedLanguage);
        }

        ShowPopup(_languagePopup);
    }

    public void ConfirmLanguageEdit()
    {
        if (_languageList != null)
        {
            _selectedLanguage = _languageList.SaveLanguage();
        }

        if (_flagImageSetter != null)
        {
            _flagImageSetter.SetFlag(_selectedLanguage);
        }

        HidePopup(_languagePopup);
    }

    public void CancelLanguagePopup()
    {
        if (_languageList != null)
        {
            _languageList.Initialize(_selectedLanguage);
        }

        HidePopup(_languagePopup);
    }

    public void OpenValuesPopup()
    {
        ShowPopup(_valuesPopup);

        Canvas.ForceUpdateCanvases();

        _valueSelection.SetSelected(new List<ClanValues>(_selectedValues));

        var popupRect = _valuesPopup.GetComponent<RectTransform>();
        if (popupRect != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(popupRect);

        Canvas.ForceUpdateCanvases();
    }

    public void ConfirmValuesEdit()
    {
        _selectedValues = new List<ClanValues>(_valueSelection.SelectedValues);
        _values.SetValues(_selectedValues);

        HidePopup(_valuesPopup);
    }

    public void CancelValuesEdit()
    {
        _values.SetValues(_selectedValues);
        HidePopup(_valuesPopup);
    }

    /*public void OpenClanRolesPopup()
    {
        ShowPopup(_rolesPopup);
    }

    public void ConfirmClanRolesEdit()
    {
        HidePopup(_rolesPopup);
    }

    public void CancelClanRolesEdit()
    {
        HidePopup(_rolesPopup);
    }*/

    private void ShowPreviousValuesRolesRulesPanel()
    {
        Debug.Log("LEFT ARROW CLICKED");

        _valuesRolesRulesIndex--;

        if (_valuesRolesRulesIndex < ValuesIndex)
        {
            _valuesRolesRulesIndex = RulesIndex;
        }

        Debug.Log("Values/Roles/Rules index: " + _valuesRolesRulesIndex);

        UpdateValuesRolesRulesPanel();
    }

    private void ShowNextValuesRolesRulesPanel()
    {
        _valuesRolesRulesIndex++;

        if (_valuesRolesRulesIndex > RulesIndex)
        {
            _valuesRolesRulesIndex = ValuesIndex;
        }

        UpdateValuesRolesRulesPanel();
    }

    private void UpdateValuesRolesRulesPanel()
    {
        if (_valuesPanel != null)
            _valuesPanel.SetActive(_valuesRolesRulesIndex == ValuesIndex);

        if (_rolesPanel != null)
            _rolesPanel.SetActive(_valuesRolesRulesIndex == RolesIndex);

        if (_rulesPanel != null)
            _rulesPanel.SetActive(_valuesRolesRulesIndex == RulesIndex);

        if (_valuesRolesRulesTitle != null)
        {
            switch (_valuesRolesRulesIndex)
            {
                case ValuesIndex:
                    _valuesRolesRulesTitle.text = "Klaanin arvot";
                    break;

                case RolesIndex:
                    _valuesRolesRulesTitle.text = "Klaanin roolit";
                    break;

                case RulesIndex:
                    _valuesRolesRulesTitle.text = "Klaanin säännöt";
                    break;
            }
        }

        UpdateValuesRolesRulesDots();
    }

    private void UpdateValuesRolesRulesDots()
    {
        SetDotColor(_valuesDot, _valuesRolesRulesIndex == ValuesIndex);
        SetDotColor(_rolesDot, _valuesRolesRulesIndex == RolesIndex);
        SetDotColor(_rulesDot, _valuesRolesRulesIndex == RulesIndex);
    }

    private void SetDotColor(Image dot, bool isActive)
    {
        if (dot == null) return;

        dot.color = isActive ? _activeDotColor : _inactiveDotColor;
    }
}
