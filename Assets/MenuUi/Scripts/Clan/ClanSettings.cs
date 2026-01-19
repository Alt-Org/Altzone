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
            string rule1 = clan.Rules.Count > 0 ?
            _rule1Text.text = ClanDataTypeConverter.GetRulesText(clan.Rules[0]) : string.Empty;
            string rule2 = clan.Rules.Count > 1 ?
            _rule1Text.text = ClanDataTypeConverter.GetRulesText(clan.Rules[1]) : string.Empty;
            string rule3 = clan.Rules.Count > 2 ?
            _rule1Text.text = ClanDataTypeConverter.GetRulesText(clan.Rules[2]) : string.Empty;

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
            _flagImageSetter.SetFlag(clan.Language);

            // Values init
            _values.SetValues(clan.Values);
            _valueSelection.SetSelected(clan.Values);
            _selectedValues = clan.Values;

            // Heart init
            clan.ClanHeartPieces ??= new();
            _heartPieces = clan.ClanHeartPieces;

            if(_heartColorChanger != null && _heartPieces != null)
                _heartColorChanger.InitializeClanHeart(_heartPieces);

            if(_heartColorSetter != null && _heartPieces != null)
                _heartColorSetter.SetHeartColors(_heartPieces);

            if (_fillWholeHeartToggle != null)
            {
                _fillWholeHeartToggle.onValueChanged.RemoveAllListeners();
                _fillWholeHeartToggle.onValueChanged.AddListener(OnFillWholeHeartToggleChanged);

                _fillWholeHeartToggle.isOn = false;
                OnFillWholeHeartToggleChanged(_fillWholeHeartToggle.isOn);
            }
            else if (_heartColorChanger != null)
            {
                _heartColorChanger.SetFillWholeHeart(true);
            }

            _saveButton.onClick.RemoveAllListeners();
            _saveButton.onClick.AddListener(SaveClanSettings);
        });
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

        Storefront.Get().GetClanData(ServerManager.Instance.Clan._id, (clanData) =>
        {
            if (!CanCurrentPlayerEditClan(clanData))
            {
                SignalBus.OnChangePopupInfoSignal("Mites pääsit tähän ikkunaan? Sinulla ei ole oikeuksia muokata klaanin asetuksia.");
                _saveButton.interactable = true;
                return;
            }

            string previousPhrase = clanData.Phrase;
            clanData.Phrase = _clanPhraseField.text;
            clanData.Language = _languageList.SelectedLanguage;
            //clanData.Goals = _goalSelection.GoalsRange;
            clanData.ClanAge = _ageSelection.ClanAgeRange;
            clanData.Rules = _ruleSelection.SelectedRules;
            clanData.Values = _valueSelection.SelectedValues;
            clanData.ClanHeartPieces = _heartPieces;

            // These are not saved at the moment
            bool isOpen = !_clanOpenToggle.isOn;
            string password = _clanPasswordField.text;
            //clanData.ClanRights = _clanRightsPanel.ClanRights;

            StartCoroutine(ServerManager.Instance.UpdateClanToServer(clanData, success =>
            {
                _saveButton.interactable = true;
                if (success)
                {
                    if (_clanMainView != null)
                    {
                        _clanMainView.UpdateProfileFromSettings(clanData);
                        ShowProfileTablineButtons();
                    }

                    if (!string.IsNullOrEmpty(previousPhrase) && previousPhrase != clanData.Phrase)
                        gameObject.GetComponent<DailyTaskProgressListener>().UpdateProgress("1");

                    gameObject.GetComponent<ClanCulturalPractices>().SettingsChanged(clanData);
                }
                else
                {
                    SignalBus.OnChangePopupInfoSignal("Asetusten tallentaminen ei onnistunut.");
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
            _clanMainView.SetCurrentPageToProfile(); 
            _clanMainView.SetSwipeEnabled(true);
        }

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

        if(_fillWholeHeartToggle != null)
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
        if(DailyTaskProgressManager.Instance.CurrentPlayerTask != null
            && DailyTaskProgressManager.Instance.CurrentPlayerTask.EducationSocialType == Altzone.Scripts.Model.Poco.Game.TaskEducationSocialType.ChangeClanMotto)
        {
            StartCoroutine(GetClanData(c =>
            {
                if(c != null)
                {
                    if(!string.IsNullOrWhiteSpace(_clanPhraseField.text) && c.Phrase != _clanPhraseField.text)
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

        if (rule1 != _rule1Text.text || rule2 != _rule2Text.text|| rule3 != _rule3Text.text)
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
