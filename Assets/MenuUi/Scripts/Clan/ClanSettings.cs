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
    [SerializeField] private RulesSelectionController _ruleSelection;
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
    [SerializeField] private GameObject _chatboxBlockOverlay;

    [Header("Buttons")]
    [SerializeField] private Button _saveButton;
    [SerializeField] private Button _cancelEditsButton;

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

    [SerializeField] private Button _openHeartEditButton;
    [SerializeField] private Button _heartConfirmButton;
    [SerializeField] private Button _heartCancelButton;

    [SerializeField] private Button _cancelConfirmContinueButton;
    [SerializeField] private Button _cancelConfirmDiscardButton;

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
    private Language _selectedLanguage;

    private ClanData _currentClanData;

    private string _originalPhrase;
    private List<Rules> _originalRules = new();
    private ClanAge _originalAge;
    private Language _originalLanguage;
    private List<ClanValues> _originalValues = new();
    private List<HeartPieceData> _originalHeartPieces = new();
    private bool _originalIsOpen;

    private void OnEnable()
    {
        RegisterUiListeners();

        Storefront.Get().GetClanData(ServerManager.Instance.Clan._id, (clan) =>
        {
            _currentClanData = clan;

            clan.Rules ??= new List<Rules>();
            clan.Values ??= new List<ClanValues>();
            clan.Members ??= new List<ClanMember>();
            clan.ClanHeartPieces ??= new List<HeartPieceData>();

            SaveOriginalState(clan);

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

            _ruleSelection.SetSelected(clan.Rules);
            _rulesPopup.SetActive(false);

            //Age init     
            _ageSelection.Initialize(clan.ClanAge);
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
            _valueSelection.SetSelected(clan.Values);
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

        _ageSelection.Initialize(_originalAge);
        UpdateAgeDisplay();

        _selectedLanguage = _originalLanguage;
        _languageList.Initialize(_originalLanguage);
        _flagImageSetter.SetFlag(_originalLanguage);

        _selectedValues = new List<ClanValues>(_originalValues);
        _values.SetValues(_selectedValues);
        _valueSelection.SetSelected(new List<ClanValues>(_originalValues));

        _clanOpenToggle.isOn = !_originalIsOpen;
        _clanPassword.SetActive(!_originalIsOpen);

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
        _currentClanData.ClanAge = _ageSelection.ClanAgeRange;
        _currentClanData.Rules = _ruleSelection.SelectedRules;
        _currentClanData.Values = _valueSelection.SelectedValues;
        _currentClanData.ClanHeartPieces = _heartPieces;

        // These are not saved at the moment
        bool isOpen = !_clanOpenToggle.isOn;
        string password = _clanPasswordField.text;
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
                    ShowProfileTablineButtons();
                }

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

        // Motto popup
        if (_openMottoButton) _openMottoButton.onClick.AddListener(OpenClanPhrasePopup);
        if (_mottoConfirmButton) _mottoConfirmButton.onClick.AddListener(ConfirmClanPhraseEdit);
        if (_mottoCancelButton) _mottoCancelButton.onClick.AddListener(CancelClanPhraseEdit);

        // Rules popup
        if (_openRulesButton) _openRulesButton.onClick.AddListener(OpenClanRulesPopup);
        if (_rulesConfirmButton) _rulesConfirmButton.onClick.AddListener(ConfirmClanRulesEdit);
        if (_rulesCancelButton) _rulesCancelButton.onClick.AddListener(CancelClanRulesEdit);

        // Age popup
        if (_openAgeButton) _openAgeButton.onClick.AddListener(OpenClanAgePopup);
        if (_ageConfirmButton) _ageConfirmButton.onClick.AddListener(OnAgeSelectionConfirmed);
        if (_ageCancelButton) _ageCancelButton.onClick.AddListener(CancelClanAgeEdit);

        // Language popup
        if (_openLanguageButton) _openLanguageButton.onClick.AddListener(OpenLanguagePopup);
        if (_languageConfirmButton) _languageConfirmButton.onClick.AddListener(ConfirmLanguageEdit);
        if (_languageCancelButton) _languageCancelButton.onClick.AddListener(CancelLanguagePopup);

        // Values popup
        if (_openValuesButton) _openValuesButton.onClick.AddListener(OpenValuesPopup);
        if (_valuesConfirmButton) _valuesConfirmButton.onClick.AddListener(ConfirmValuesEdit);
        if (_valuesCancelButton) _valuesCancelButton.onClick.AddListener(CancelValuesEdit);

        // Heart popup
        if (_openHeartEditButton) _openHeartEditButton.onClick.AddListener(OpenHeartEditPopup);
        if (_heartConfirmButton) _heartConfirmButton.onClick.AddListener(ConfirmHeartEdit);
        if (_heartCancelButton) _heartCancelButton.onClick.AddListener(CancelHeartEdit);

        // Cancel confirmation popup
        if (_cancelConfirmContinueButton) _cancelConfirmContinueButton.onClick.AddListener(OnClickContinueEditingClanSettings);
        if (_cancelConfirmDiscardButton) _cancelConfirmDiscardButton.onClick.AddListener(OnClickDiscardClanSettingEdits);
    }

    private void UnregisterUiListeners()
    {
        if (_saveButton) _saveButton.onClick.RemoveListener(SaveClanSettings);

        if (_fillWholeHeartToggle) _fillWholeHeartToggle.onValueChanged.RemoveListener(OnFillWholeHeartToggleChanged);

        if (_cancelEditsButton) _cancelEditsButton.onClick.RemoveListener(OnClickCancelClanSettingEdits);

        if (_openMottoButton) _openMottoButton.onClick.RemoveListener(OpenClanPhrasePopup);
        if (_mottoConfirmButton) _mottoConfirmButton.onClick.RemoveListener(ConfirmClanPhraseEdit);
        if (_mottoCancelButton) _mottoCancelButton.onClick.RemoveListener(CancelClanPhraseEdit);

        if (_openRulesButton) _openRulesButton.onClick.RemoveListener(OpenClanRulesPopup);
        if (_rulesConfirmButton) _rulesConfirmButton.onClick.RemoveListener(ConfirmClanRulesEdit);
        if (_rulesCancelButton) _rulesCancelButton.onClick.RemoveListener(CancelClanRulesEdit);

        if (_openAgeButton) _openAgeButton.onClick.RemoveListener(OpenClanAgePopup);
        if (_ageConfirmButton) _ageConfirmButton.onClick.RemoveListener(OnAgeSelectionConfirmed);
        if (_ageCancelButton) _ageCancelButton.onClick.RemoveListener(CancelClanAgeEdit);

        if (_openLanguageButton) _openLanguageButton.onClick.RemoveListener(OpenLanguagePopup);
        if (_languageConfirmButton) _languageConfirmButton.onClick.RemoveListener(ConfirmLanguageEdit);
        if (_languageCancelButton) _languageCancelButton.onClick.RemoveListener(CancelLanguagePopup);

        if (_openValuesButton) _openValuesButton.onClick.RemoveListener(OpenValuesPopup);
        if (_valuesConfirmButton) _valuesConfirmButton.onClick.RemoveListener(ConfirmValuesEdit);
        if (_valuesCancelButton) _valuesCancelButton.onClick.RemoveListener(CancelValuesEdit);

        if (_openHeartEditButton) _openHeartEditButton.onClick.RemoveListener(OpenHeartEditPopup);
        if (_heartConfirmButton) _heartConfirmButton.onClick.RemoveListener(ConfirmHeartEdit);
        if (_heartCancelButton) _heartCancelButton.onClick.RemoveListener(CancelHeartEdit);

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
            || _originalAge != _ageSelection.ClanAgeRange
            || !_originalRules.SequenceEqual(_ruleSelection.SelectedRules)
            || !_originalValues.SequenceEqual(_selectedValues);

        if (hasMadeEdits)
        {
            ShowPopup(_cancelConfirmationPopup);
        }
        else
        {
            ShowProfileTablineButtons();
        }
    }

    public void OnClickDiscardClanSettingEdits()
    {
        HidePopup(_cancelConfirmationPopup);

        ResetSettingsToOriginalState();

        ShowProfileTablineButtons();
    }

    private void ShowPopup(GameObject popup)
    {
        if (_swipeBlockOverlay != null)
            _swipeBlockOverlay.SetActive(true);

        if (_chatboxBlockOverlay != null)
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
        _clanMainView.ExitSettingsToProfile();

        if (_editViewButtons != null)
            _editViewButtons.SetActive(false);

        if (_buttonsInClan != null)
            _buttonsInClan.SetActive(true);
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
        _clanPhraseField.text = _clanPhraseText.text;
        ShowPopup(_clanPhrasePopup);
    }

    public void ConfirmClanPhraseEdit()
    {
        if (DailyTaskProgressManager.Instance.CurrentPlayerTask != null
            && DailyTaskProgressManager.Instance.CurrentPlayerTask.EducationSocialType == Altzone.Scripts.Model.Poco.Game.TaskEducationSocialType.ChangeClanMotto)
        {
            StartCoroutine(GetClanData(c =>
            {
                if (c != null)
                {
                    if (!string.IsNullOrWhiteSpace(_clanPhraseField.text) && c.Phrase != _clanPhraseField.text)
                        DailyTaskProgressManager.Instance.UpdateTaskProgress(Altzone.Scripts.Model.Poco.Game.TaskEducationSocialType.ChangeClanMotto, "1");
                }
            }));
        }
        _clanPhraseText.text = _clanPhraseField.text;
        HidePopup(_clanPhrasePopup);
    }

    public void CancelClanPhraseEdit()
    {
        if (DailyTaskProgressManager.Instance.CurrentPlayerTask != null
            && DailyTaskProgressManager.Instance.CurrentPlayerTask.EducationSocialType == Altzone.Scripts.Model.Poco.Game.TaskEducationSocialType.ChangeClanMotto)
        {
            StartCoroutine(GetClanData(c =>
            {
                if (c != null)
                {
                    if (!string.IsNullOrWhiteSpace(_clanPhraseField.text) && c.Phrase != _clanPhraseField.text)
                        DailyTaskProgressManager.Instance.UpdateTaskProgress(Altzone.Scripts.Model.Poco.Game.TaskEducationSocialType.ChangeClanMotto, "1");
                }
            }));
        }
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
        HidePopup(_rulesPopup);
    }

    public void CancelClanRulesEdit()
    {
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
        _selectedLanguage = _languageList.SaveLanguage();
        _flagImageSetter.SetFlag(_selectedLanguage);
        HidePopup(_languagePopup);
    }

    public void CancelLanguagePopup()
    {
        _languageList.Initialize(_selectedLanguage);
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
        _valueSelection.SetSelected(new List<ClanValues>(_selectedValues));

        Canvas.ForceUpdateCanvases();

        var popupRect = _valuesPopup.GetComponent<RectTransform>();
        if (popupRect != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(popupRect);

        HidePopup(_valuesPopup);
    }
}
