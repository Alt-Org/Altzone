using System;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Clan;
using MenuUi.Scripts.Window;
using MenuUi.Scripts.Window.ScriptableObjects;
using MenuUI.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClanCreateNew : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject _mainCreatePanel;
    [SerializeField] private GameObject _languagePanel;
    [SerializeField] private GameObject _clanHeartEditPanel;

    [Header("Setting fields")]
    [SerializeField] private TMP_InputField _clanNameField;
    [SerializeField] private Toggle _openClanToggle;
    [SerializeField] private TMP_InputField _clanPasswordField;
    //[SerializeField] private GameObject _clanPasswordRoot;
    [SerializeField] private ClanLanguageList _languageSelection;
    [SerializeField] private ValueSelectionController _valueSelection;
    [SerializeField] private LanguageFlagImage _flagImage;

    [Header("Age Settings")]
    [SerializeField] private ClanAgeList _ageSelection;
    [SerializeField] private GameObject _clanAgeEditPopup;
    [SerializeField] private ClanAgeList _ageSelectionList;
    [SerializeField] private Image _ageImage;

    [Header("Clan Heart")]
    [SerializeField] private ClanHeartColorSetter _heartColorSetter;
    //[SerializeField] private ClanHeartColorSetter _heartColorSetterPopup;
    [SerializeField] private ClanHeartColorChanger _heartColorChanger;
    [SerializeField] private ColorButton[] _colorButtons;
    [SerializeField] private GameObject _heartEditPopup;
    [SerializeField] private Toggle _fillWholeHeartToggle;
    [SerializeField] private TMP_Text _heartModeLabel;
    [SerializeField] private string _wholeHeartText = "Tila: väritä koko sydän";
    [SerializeField] private string _pieceModeText = "Tila: muokkaa palaa kerrallaan";
    [SerializeField] private Image _toggleIcon;
    [SerializeField] private Sprite _iconWholeHeart;
    [SerializeField] private Sprite _iconPieceMode;

    [Header("Open / Lock Clan Popups")]
    [SerializeField] private GameObject _lockClanPopup;
    [SerializeField] private GameObject _openClanPopup;
    [SerializeField] private GameObject _createClanPasswordPopup;

    [SerializeField] private Button _lockClanConfirmButton;
    [SerializeField] private Button _lockClanCancelButton;
    [SerializeField] private Button _openClanConfirmButton;
    [SerializeField] private Button _openClanCancelButton;
    [SerializeField] private Button _passwordConfirmButton;
    [SerializeField] private Button _passwordCancelButton;

    [Header("Open / Lock Clan Visuals")]
    [SerializeField] private Image _openClanToggleBackground;
    [SerializeField] private Color _openClanBackgroundColor = Color.white;
    [SerializeField] private Color _lockedClanBackgroundColor = Color.red;

    [Header("Warnings")]
    [SerializeField] private GameObject _nameWarningOutline;
    //[SerializeField] private GameObject _passwordWarningOutline;
    [SerializeField] private GameObject _ageWarningOutline;
    [SerializeField] private GameObject _languageWarningOutline;
    [SerializeField] private GameObject _valuesWarningOutline;
    [SerializeField] private PopupController _warningPopup;

    [Header("Buttons")]
    //[SerializeField] private Button _closeLanguageSelect;
    [SerializeField] private Button _createClanButton;
    [SerializeField] private Button _agreementCloseButton;

    [SerializeField] private Button _clanLogoButton;
    [SerializeField] private Button _heartEditSaveButton;
    [SerializeField] private Button _heartEditCancelButton;
    [SerializeField] private Button _heartEditCloseButton;
    [SerializeField] private Button _ageButton;
    [SerializeField] private Button _ageSaveButton;
    [SerializeField] private Button _ageCancelButton;
    [SerializeField] private Button _languageButton;
    [SerializeField] private Button _languageSaveButton;
    [SerializeField] private Button _languageCancelButton;
    [SerializeField] private Button _cancelButton;
    [SerializeField] private Button _confirmButton;
    [SerializeField] private Button _ageCloseButton;
    [SerializeField] private Button _languageCloseButton;

    [Header("Navigation")]
    [SerializeField] protected WindowDef _naviTarget;

    [Header("Popups")]
    [SerializeField] private GameObject _raycastBlocker;
    [SerializeField] private AgreementController _agreementController;
    [SerializeField] private GameObject _agreementPopup;
    [SerializeField] private TMP_Text _agreementClanNameText;

    [Header("Default Icons")]
    [SerializeField] private Sprite _defaultAgeSprite;
    [SerializeField] private Sprite _defaultLanguageSprite;

    [Header("Default Selections")]
    [SerializeField] private ClanAge _defaultAgeSelection = ClanAge.None;
    [SerializeField] private Language _defaultLanguageSelection = Language.None;

    private Color _defaultHeartColor;
    private List<HeartPieceData> _heartPieces;

    private bool _confirmedLockedState;
    private bool _ignoreOpenClanToggleCallback;

    private readonly ClanRoleRights[] _defaultRights = new ClanRoleRights[3]  {
        ClanRoleRights.None,
        ClanRoleRights.EditSoulHome,
        ClanRoleRights.EditClanSettings | ClanRoleRights.EditSoulHome | ClanRoleRights.EditMemberRights
    };

    private void Start()
    {
        if (_openClanToggle != null)
        {
            _confirmedLockedState = _openClanToggle.isOn;

            _openClanToggle.onValueChanged.RemoveListener(OnOpenClanToggleChanged);
            _openClanToggle.onValueChanged.AddListener(OnOpenClanToggleChanged);

            ApplyOpenClanState(_confirmedLockedState);
        }
    }

    private void OnOpenClanToggleChanged(bool isOn)
    {
        if (_ignoreOpenClanToggleCallback)
            return;

        // changing from open to locked
        if (isOn && !_confirmedLockedState)
        {
            ShowPopup(_lockClanPopup);
            RevertToggleToConfirmedState();
            return;
        }

        // changing from locked to open
        if (!isOn && _confirmedLockedState)
        {
            ShowPopup(_openClanPopup);
            RevertToggleToConfirmedState();
            return;
        }
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
            _heartColorChanger.SetFillWholeHeart(isOn);
        }
    }

    private void OnEnable()
    {
        if (_clanLogoButton) _clanLogoButton.onClick.AddListener(OnClanLogoPressed);
        if (_ageButton) _ageButton.onClick.AddListener(OnAgePressed);
        if (_languageButton) _languageButton.onClick.AddListener(OnLanguagePressed);

        if (_cancelButton) _cancelButton.onClick.AddListener(OnCancelPressed);
        if (_confirmButton) _confirmButton.onClick.AddListener(OnConfirmPressed);

        if (_heartEditSaveButton) _heartEditSaveButton.onClick.AddListener(ConfirmHeartEdit);
        if (_heartEditCancelButton) _heartEditCancelButton.onClick.AddListener(CancelHeartEdit);
        if (_heartEditCloseButton) _heartEditCloseButton.onClick.AddListener(CancelHeartEdit);

        if (_ageSaveButton) _ageSaveButton.onClick.AddListener(SaveAgeSelection);
        if (_ageCancelButton) _ageCancelButton.onClick.AddListener(CancelAgeSelection);
        if (_ageCloseButton) _ageCloseButton.onClick.AddListener(CancelAgeSelection);

        if (_languageSaveButton) _languageSaveButton.onClick.AddListener(SaveLanguageSelection);
        if (_languageCancelButton) _languageCancelButton.onClick.AddListener(CancelLanguageSelection);
        if (_languageCloseButton) _languageCloseButton.onClick.AddListener(CancelLanguageSelection);

        if (_lockClanConfirmButton) _lockClanConfirmButton.onClick.AddListener(ConfirmLockClan);
        if (_lockClanCancelButton) _lockClanCancelButton.onClick.AddListener(CancelOpenLockChange);

        if (_openClanConfirmButton) _openClanConfirmButton.onClick.AddListener(ConfirmOpenClan);
        if (_openClanCancelButton) _openClanCancelButton.onClick.AddListener(CancelOpenLockChange);

        if (_passwordConfirmButton) _passwordConfirmButton.onClick.AddListener(ConfirmPasswordPopup);
        if (_passwordCancelButton) _passwordCancelButton.onClick.AddListener(CancelPasswordPopup);

        if (_createClanButton != null)
            _createClanButton.onClick.AddListener(OnAgreementCreatePressed);

        if (_agreementCloseButton != null)
            _agreementCloseButton.onClick.AddListener(OnCancelPressed);

        Reset();
    }

    private void OnDisable()
    {
        if (_clanLogoButton) _clanLogoButton.onClick.RemoveListener(OnClanLogoPressed);
        if (_ageButton) _ageButton.onClick.RemoveListener(OnAgePressed);
        if (_languageButton) _languageButton.onClick.RemoveListener(OnLanguagePressed);

        if (_heartEditSaveButton) _heartEditSaveButton.onClick.RemoveListener(ConfirmHeartEdit);
        if (_heartEditCancelButton) _heartEditCancelButton.onClick.RemoveListener(CancelHeartEdit);
        if (_heartEditCloseButton) _heartEditCloseButton.onClick.RemoveListener(CancelHeartEdit);

        if (_ageSaveButton) _ageSaveButton.onClick.RemoveListener(SaveAgeSelection);
        if (_ageCancelButton) _ageCancelButton.onClick.RemoveListener(CancelAgeSelection);
        if (_ageCloseButton) _ageCloseButton.onClick.RemoveListener(CancelAgeSelection);

        if (_languageSaveButton) _languageSaveButton.onClick.RemoveListener(SaveLanguageSelection);
        if (_languageCancelButton) _languageCancelButton.onClick.RemoveListener(CancelLanguageSelection);
        if (_languageCloseButton) _languageCloseButton.onClick.RemoveListener(CancelLanguageSelection);

        if (_lockClanConfirmButton) _lockClanConfirmButton.onClick.RemoveListener(ConfirmLockClan);
        if (_lockClanCancelButton) _lockClanCancelButton.onClick.RemoveListener(CancelOpenLockChange);

        if (_openClanConfirmButton) _openClanConfirmButton.onClick.RemoveListener(ConfirmOpenClan);
        if (_openClanCancelButton) _openClanCancelButton.onClick.RemoveListener(CancelOpenLockChange);

        if (_passwordConfirmButton) _passwordConfirmButton.onClick.RemoveListener(ConfirmPasswordPopup);
        if (_passwordCancelButton) _passwordCancelButton.onClick.RemoveListener(CancelPasswordPopup);

        if (_cancelButton) _cancelButton.onClick.RemoveListener(OnCancelPressed);
        if (_confirmButton) _confirmButton.onClick.RemoveListener(OnConfirmPressed);

        if (_createClanButton != null)
            _createClanButton.onClick.RemoveListener(OnAgreementCreatePressed);

        if (_agreementCloseButton != null)
            _agreementCloseButton.onClick.RemoveListener(OnCancelPressed);
    }

    private void OnAgreementCreatePressed()
    {
        PostClanToServer();

        if (_agreementPopup != null)
            HidePopup(_agreementPopup);
    }


    private void OnClanLogoPressed()
    {
        ShowPopup(_clanHeartEditPanel);
    }

    private void OnAgePressed()
    {
        ShowAgePopup();
    }

    private void OnLanguagePressed()
    {
        ShowPopup(_languagePanel);
    }

    private void OnCancelPressed()
    {
        HidePopup(_agreementPopup);
    }

    private void OnConfirmPressed()
    {
        UpdateAgreementClanName();
        ShowPopup(_agreementPopup);
    }

    private void Reset()
    {
        CloseAllPopups();

        _mainCreatePanel.SetActive(true);
        _languagePanel.SetActive(false);

        StopAllCoroutines();

        _clanNameField.text = "";

        _confirmedLockedState = false;
        SetToggleWithoutCallback(false);
        ApplyOpenClanState(false);

        _nameWarningOutline.SetActive(false);
        //_passwordWarningOutline.SetActive(false);
        _ageWarningOutline.SetActive(false);
        _languageWarningOutline.SetActive(false);
        _valuesWarningOutline.SetActive(false);

        if(_valueSelection != null)
        {
            _valueSelection.ResetSelection();
        }

        // AGE default selection
        var startAge = _defaultAgeSelection;
        _ageSelection.Initialize(startAge);
        UpdateAgeDisplay();

        // LANGUAGE default selection
        var startLang = _defaultLanguageSelection;
        _languageSelection.Initialize(startLang);
        UpdateLanguageDisplay(startLang);

        // Close button should apply current selection
        /*_closeLanguageSelect.onClick.RemoveAllListeners();
        _closeLanguageSelect.onClick.AddListener(() =>
        {
            UpdateLanguageDisplay(_languageSelection.SaveLanguage());
        });*/

        if (_colorButtons != null && _colorButtons.Length > 0)
        {
            _defaultHeartColor = ColorConstants.GetColorConstant(_colorButtons[0].color);
        }
        else
        {
            _defaultHeartColor = Color.red;
        }

        _heartPieces = new List<HeartPieceData>();
        const int pieceCount = 50;
        for (int i = 0; i < pieceCount; i++)
        {
            _heartPieces.Add(new HeartPieceData(i, _defaultHeartColor));
        }

        if (_heartColorChanger != null)
        {
            _heartColorChanger.InitializeClanHeart(new List<HeartPieceData>(_heartPieces));
        }
        if(_heartColorSetter != null)
        {
            _heartColorSetter.SetHeartColors(new List<HeartPieceData>(_heartPieces));
        }

        if (_fillWholeHeartToggle != null)
        {
            _fillWholeHeartToggle.onValueChanged.RemoveAllListeners();
            _fillWholeHeartToggle.onValueChanged.AddListener(OnFillWholeHeartToggleChanged);

            _fillWholeHeartToggle.isOn = false;
            OnFillWholeHeartToggleChanged(_fillWholeHeartToggle.isOn);
        }
        else if (_heartColorChanger != null)
        {
            OnFillWholeHeartToggleChanged(true);
        }

        if (_agreementController != null)
        {
            _agreementController.ResetState();
        }
    }

    private void CloseAllPopups()
    {
        if (_agreementPopup != null)
            _agreementPopup.SetActive(false);

        if (_languagePanel != null)
            _languagePanel.SetActive(false);

        if (_clanAgeEditPopup != null)
            _clanAgeEditPopup.SetActive(false);

        if (_clanHeartEditPanel != null)
            _clanHeartEditPanel.SetActive(false);

        if (_heartEditPopup != null)
            _heartEditPopup.SetActive(false);

        if (_lockClanPopup != null)
            _lockClanPopup.SetActive(false);

        if (_openClanPopup != null)
            _openClanPopup.SetActive(false);

        if (_createClanPasswordPopup != null)
            _createClanPasswordPopup.SetActive(false);

        if (_raycastBlocker != null)
            _raycastBlocker.SetActive(false);
    }

    public void ShowPopup(GameObject popup)
    {
        if (popup != null)
            popup.SetActive(true);

        if (_raycastBlocker != null)
            _raycastBlocker.SetActive(true);
    }

    public void HidePopup(GameObject popup)
    {
        if (popup != null)
            popup.SetActive(false);

        if (_raycastBlocker != null)
            _raycastBlocker.SetActive(false);
    }

    private void UpdateAgreementClanName()
    {
        if (_agreementClanNameText == null || _clanNameField == null)
            return;

        string clanName = _clanNameField.text.Trim();

        _agreementClanNameText.text = string.IsNullOrWhiteSpace(clanName)
            ? ""
            : $"\"{clanName}\"";
    }

    public void OpenHeartEditPopup()
    {
        if (_heartColorChanger != null && _heartPieces != null)
        {
            _heartColorChanger.InitializeClanHeart(_heartPieces);
            if (_fillWholeHeartToggle != null)
            {
                OnFillWholeHeartToggleChanged(_fillWholeHeartToggle.isOn);
            }
        }
        ShowPopup(_heartEditPopup);
    }

   public void ConfirmHeartEdit()
    {
        if (_heartColorChanger != null)
        {
            _heartPieces = _heartColorChanger.GetHeartPieceDatas();

            if (_heartColorSetter != null)
            {
                _heartColorSetter.SetHeartColors(_heartPieces);
            }
        }

        HidePopup(_heartEditPopup);
    }

    public void CancelHeartEdit()
    {
        if (_heartColorChanger != null && _heartPieces != null)
        {
            _heartColorChanger.InitializeClanHeart(_heartPieces);          
        }

        if (_heartColorSetter != null && _heartPieces != null)
        {
            _heartColorSetter.SetHeartColors(_heartPieces);
        }

        HidePopup(_heartEditPopup);
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

    private void UpdateLanguageDisplay(Language language)
    {
        if (_flagImage != null)
            _flagImage.SetFlag(language);

        if (language == Language.None && _defaultLanguageSprite != null && _flagImage != null)
        {
            var img = _flagImage.GetComponent<Image>();
            if (img != null)
            {
                img.sprite = _defaultLanguageSprite;
                img.preserveAspect = true;
                img.enabled = true;
            }
        }
    }

    public void ShowAgePopup()
    {
        _ageSelectionList.Initialize(_ageSelection.ClanAgeRange);
        ShowPopup(_clanAgeEditPopup);
    }

    public void SaveAgeSelection()
    {
        ClanAge selectedAge = _ageSelectionList.ClanAgeRange;
        _ageSelection.Initialize(selectedAge);
        UpdateAgeDisplay();
        HidePopup(_clanAgeEditPopup);
    }

    public void CancelAgeSelection()
    {
        HidePopup(_clanAgeEditPopup);
    }

    public void SaveLanguageSelection()
    {
        UpdateLanguageDisplay(_languageSelection.SaveLanguage());
        HidePopup(_languagePanel);
    }

    public void CancelLanguageSelection()
    {
        HidePopup(_languagePanel);
    }

    public void PostClanToServer()
    {
        string clanName = _clanNameField.text;
        string phrase = " ";
        bool isOpen = !_openClanToggle.isOn;
        string password = _clanPasswordField.text;
        Language language = _languageSelection.SelectedLanguage;
        ClanAge age = _ageSelection.ClanAgeRange;
        ClanRoleRights[] clanRights = _defaultRights;
        ClanValues[] values = _valueSelection.SelectedValues.ToArray();

        List<HeartPieceData> clanHeartPieces;
        if (_heartPieces != null && _heartPieces.Count > 0)
        {
            clanHeartPieces = new List<HeartPieceData>(_heartPieces);
        }
        else
        {
            clanHeartPieces = new List<HeartPieceData>();
            const int pieceCount = 50;
            for (int i = 0; i < pieceCount; i++) clanHeartPieces.Add(new HeartPieceData(i, _defaultHeartColor));
        }

        if (!CheckClanInputsValidity(clanName, isOpen, password, language, age, values))
        {
            return;
        }

        List<string> serverValues = new ();

        foreach(var value in values)
        {
            string valueString = ClanDataTypeConverter.ClanValuesToString(value);
            serverValues.Add(valueString);
        }

        ClanLogo logo = new ClanLogo();
        logo.logoType = ClanLogoType.Heart;
        logo.pieceColors = new();

        foreach(var piece in clanHeartPieces)
        {
            logo.pieceColors.Add(ColorUtility.ToHtmlStringRGB(piece.pieceColor));
        }

        ServerClan serverClan = new ServerClan
        {
            name = clanName.Trim(),
            tag = clanName.Trim().Substring(0, 3),
            phrase = phrase,
            isOpen = isOpen,
            language = language,
            ageRange = age,
            labels = serverValues,
            clanLogo = logo
        };

        StartCoroutine(ServerManager.Instance.PostClanToServer(serverClan, clan =>
        {
            if (clan == null)
            {
                _warningPopup.ActivatePopUp(
                    "Klaanin luonti epäonnistui.\n" +
                    "Et voi luoda uutta klaania, jos olet jo klaanin jäsen. " +
                    "Poistu ensin nykyisestä klaanistasi ja yritä uudelleen."
                );
                return;
            }
            if (OverlayPanelCheck.Instance) OverlayPanelCheck.Instance.ToggleOverlay(true);
            Debug.Log($"naviTarget {_naviTarget} isCurrentPopOutWindow {true}", _naviTarget);
            IWindowManager windowManager = WindowManager.Get();
            if (windowManager == null)
            {
                Debug.LogError("WindowManager not found – ei voida navigoida ClanMainiin.");
                return;
            }
            //windowManager.PopCurrentWindow();

            // Check if navigation target window is already in window stack and we area actually going back to it via button.
            int windowCount = windowManager.WindowCount;
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

    private bool CheckClanInputsValidity(
        string clanName,
        bool isOpen,
        string password,
        Language language,
        ClanAge age,
        ClanValues[] values)
    {
        bool validInputs = true;

        if (clanName == string.Empty)
        {
            _nameWarningOutline.SetActive(true);
            _warningPopup.ActivatePopUp("Lisää klaanin nimi");
            validInputs = false;
        }
        else if (clanName.Trim().Length < 3)
        {
            _nameWarningOutline.SetActive(true);
            _warningPopup.ActivatePopUp("Klaanin nimen pitää olla vähintään 3 merkkiä pitkä.");
            validInputs = false;
        }
        else if (clanName.Trim().Length > 30)
        {
            _nameWarningOutline.SetActive(true);
            _warningPopup.ActivatePopUp("Klaanin nimi saa olla maksimissaan 30 merkkiä pitkä.");
            validInputs = false;
        }
        else _nameWarningOutline.SetActive(false);

        if (!isOpen && password == string.Empty)
        {
            //_passwordWarningOutline.SetActive(true);
            _warningPopup.ActivatePopUp("Lukituilla klaaneilla tulee olla salasana");
            validInputs = false;
        }
        //else _passwordWarningOutline.SetActive(false);

        if (language == Language.None)
        {
            _languageWarningOutline.SetActive(true);
            _warningPopup.ActivatePopUp("Valitse klaanin kieli");
            validInputs = false;
        }
        else _languageWarningOutline.SetActive(false);

        if (age == ClanAge.None)
        {
            _ageWarningOutline.SetActive(true);
            _warningPopup.ActivatePopUp("Valitse klaanin ikäraja");
            validInputs = false;
        }
        else _ageWarningOutline.SetActive(false);

        if (values.Length < 3)
        {
            _valuesWarningOutline.SetActive(true);
            _warningPopup.ActivatePopUp("Klaanille tulee olla valittuna 3 arvoa");
            validInputs = false;
        }
        else
        {
            _valuesWarningOutline.SetActive(false);
        }

        return validInputs;
    }

    private void ConfirmLockClan()
    {
        HidePopup(_lockClanPopup);

        _confirmedLockedState = true;
        SetToggleWithoutCallback(true);
        ApplyOpenClanState(true);

        ShowPopup(_createClanPasswordPopup);
    }

    private void ConfirmOpenClan()
    {
        HidePopup(_openClanPopup);

        _confirmedLockedState = false;
        SetToggleWithoutCallback(false);
        ApplyOpenClanState(false);

        if (_clanPasswordField != null)
            _clanPasswordField.text = string.Empty;
    }

    private void ConfirmPasswordPopup()
    {
        if (_clanPasswordField != null && string.IsNullOrWhiteSpace(_clanPasswordField.text))
        {
            if (_warningPopup != null)
                _warningPopup.ActivatePopUp("Lisää klaanin salasana");

            /*if (_passwordWarningOutline != null)
                _passwordWarningOutline.SetActive(true);*/

            return;
        }

        /*if (_passwordWarningOutline != null)
            _passwordWarningOutline.SetActive(false);*/

        _confirmedLockedState = true;
        SetToggleWithoutCallback(true);
        ApplyOpenClanState(true);

        HidePopup(_createClanPasswordPopup);
    }

    private void CancelPasswordPopup()
    {
        if (_clanPasswordField != null)
            _clanPasswordField.text = string.Empty;

        _confirmedLockedState = false;
        SetToggleWithoutCallback(false);
        ApplyOpenClanState(false);

        HidePopup(_createClanPasswordPopup);
    }

    private void CancelOpenLockChange()
    {
        HidePopup(_lockClanPopup);
        HidePopup(_openClanPopup);

        RevertToggleToConfirmedState();
        ApplyOpenClanState(_confirmedLockedState);
    }

    private void RevertToggleToConfirmedState()
    {
        SetToggleWithoutCallback(_confirmedLockedState);
    }

    private void SetToggleWithoutCallback(bool isLocked)
    {
        if (_openClanToggle == null)
            return;

        _ignoreOpenClanToggleCallback = true;
        _openClanToggle.isOn = isLocked;
        _ignoreOpenClanToggleCallback = false;

        ToggleImage toggleImage = _openClanToggle.GetComponent<ToggleImage>();
        if (toggleImage != null)
            toggleImage.RefreshImage();
    }

    private void ApplyOpenClanState(bool isLocked)
    {
        /*if (_clanPasswordRoot != null)
            _clanPasswordRoot.SetActive(isLocked);*/

        if (_openClanToggleBackground != null)
            _openClanToggleBackground.color = isLocked ? _lockedClanBackgroundColor : _openClanBackgroundColor;
    }
}
