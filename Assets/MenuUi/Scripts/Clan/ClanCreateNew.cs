using System;
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
    [Header("Panels")]
    [SerializeField] private GameObject _mainCreatePanel;
    [SerializeField] private GameObject _languagePanel;

    [Header("Setting fields")]
    [SerializeField] private TMP_InputField _clanNameField;
    [SerializeField] private Toggle _openClanToggle;
    [SerializeField] private TMP_InputField _clanPasswordField;
    [SerializeField] private GameObject _clanPasswordRoot;
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

    [Header("Warnings")]
    [SerializeField] private GameObject _nameWarningOutline;
    [SerializeField] private GameObject _passwordWarningOutline;
    [SerializeField] private GameObject _ageWarningOutline;
    [SerializeField] private GameObject _languageWarningOutline;
    [SerializeField] private GameObject _valuesWarningOutline;
    [SerializeField] private PopupController _warningPopup;

    [Header("Buttons")]
    [SerializeField] private Button _closeLanguageSelect;
    [SerializeField] private Button _createClanOK;

    [Header("Navigation")]
    [SerializeField] protected WindowDef _naviTarget;

    [Header("Popups")]
    [SerializeField] private GameObject _raycastBlocker;
    [SerializeField] private AgreementController _agreementController;

    //private Color _selectedHeartColor;

    private Color _defaultHeartColor;
    private List<HeartPieceData> _heartPieces;

    private readonly ClanRoleRights[] _defaultRights = new ClanRoleRights[3]  {
        ClanRoleRights.None,
        ClanRoleRights.EditSoulHome,
        ClanRoleRights.EditClanSettings | ClanRoleRights.EditSoulHome | ClanRoleRights.EditMemberRights
    };

    private void Start()
    {
        //_createClanOK.onClick.RemoveAllListeners();
        //_createClanOK.onClick.AddListener(PostClanToServer);

        _openClanToggle.onValueChanged.AddListener(OnOpenClanToggleChanged);
        OnOpenClanToggleChanged(_openClanToggle.isOn);
    }

    private void OnOpenClanToggleChanged(bool isOn)
    {
        bool isOpen = !isOn;
        if(_clanPasswordRoot != null)
        {
            _clanPasswordRoot.SetActive(!isOpen);
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
            bool fillWholeHeart = !isOn;          
            _heartColorChanger.SetFillWholeHeart(fillWholeHeart);
        }
    }

    private void OnEnable() => Reset();

    private void Reset()
    {
        _mainCreatePanel.SetActive(true);
        _languagePanel.SetActive(false);

        StopAllCoroutines();

        _clanNameField.text = "";
        _openClanToggle.isOn = false;
        OnOpenClanToggleChanged(_openClanToggle.isOn);

        _nameWarningOutline.SetActive(false);
        _passwordWarningOutline.SetActive(false);
        _ageWarningOutline.SetActive(false);
        _languageWarningOutline.SetActive(false);
        _valuesWarningOutline.SetActive(false);

        if(_valueSelection != null)
        {
            _valueSelection.ResetSelection();
        }

        _ageSelection.Initialize(ClanAge.None);
        UpdateAgeDisplay();

        _flagImage.SetFlag(Language.None);
        _languageSelection.Initialize(Language.None);
        _closeLanguageSelect.onClick.RemoveAllListeners();
        _closeLanguageSelect.onClick.AddListener(() => _flagImage.SetFlag(_languageSelection.SelectedLanguage));

        if(_colorButtons != null && _colorButtons.Length > 0)
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
            _heartColorChanger.SetFillWholeHeart(true);
        }

        if (_agreementController != null)
        {
            _agreementController.ResetState();
        }
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

    /*private void SetHeartColor(Color color)
    {
        _selectedHeartColor = color;
        _heartColorSetter.SetHeartColor(_selectedHeartColor);
        _heartColorSetterPopup.SetHeartColor(_selectedHeartColor);
    }

    public void CancelHeartEdit()
    {
        SetHeartColor(_defaultHeartColor);
    }*/

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
            _passwordWarningOutline.SetActive(true);
            _warningPopup.ActivatePopUp("Lukituilla klaaneilla tulee olla salasana");
            validInputs = false;
        }
        else _passwordWarningOutline.SetActive(false);

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
}
