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
    [SerializeField] private ClanGoalSelection _clanGoalSelection;
    [SerializeField] private ClanAgeSelection _ageSelection;
    [SerializeField] private ClanLanguageList _languageSelection;
    [SerializeField] private ValueSelectionController _valueSelection;
    [SerializeField] private LanguageFlagImage _flagImage;

    [Header("Clan Heart")]
    [SerializeField] private ClanHeartColorSetter _heartColorSetter;
    [SerializeField] private ColorButton[] _colorButtons;

    [Header("Warnings")]
    [SerializeField] private GameObject _nameWarningOutline;
    [SerializeField] private GameObject _passwordWarningOutline;
    [SerializeField] private GameObject _goalWarningOutline;
    [SerializeField] private GameObject _languageWarningOutline;
    [SerializeField] private GameObject _valuesWarningOutline;
    [SerializeField] private PopupController _warningPopup;

    [Header("Buttons")]
    [SerializeField] private Button _closeLanguageSelect;
    [SerializeField] private Button _createClanOK;

    [Header("Navigation")]
    [SerializeField] protected WindowDef _naviTarget;

    private Color _selectedHeartColor;
    private readonly ClanRoleRights[] _defaultRights = new ClanRoleRights[3]  {
        ClanRoleRights.None,
        ClanRoleRights.EditSoulHome,
        ClanRoleRights.EditClanSettings | ClanRoleRights.EditSoulHome | ClanRoleRights.EditMemberRights
    };

    private void Start()
    {
        _createClanOK.onClick.RemoveAllListeners();
        _createClanOK.onClick.AddListener(PostClanToServer);
    }

    private void OnEnable() => Reset();

    private void Reset()
    {
        _mainCreatePanel.SetActive(true);
        _languagePanel.SetActive(false);

        StopAllCoroutines();

        _clanNameField.text = "";
        _openClanToggle.isOn = false;

        _nameWarningOutline.SetActive(false);
        _passwordWarningOutline.SetActive(false);
        _goalWarningOutline.SetActive(false);
        _passwordWarningOutline.SetActive(false);
        _languageWarningOutline.SetActive(false);
        _valuesWarningOutline.SetActive(false);

        _ageSelection.Initialize(ClanAge.None);
        _clanGoalSelection.Initialize(Goals.Fiilistely);

        _flagImage.SetFlag(Language.None);
        _languageSelection.Initialize(Language.None);
        _closeLanguageSelect.onClick.AddListener(() => _flagImage.SetFlag(_languageSelection.SelectedLanguage));

        SetHeartColor(ColorConstants.GetColorConstant(_colorButtons[0].color));
        foreach (ColorButton colorButton in _colorButtons)
        {
            Color color = ColorConstants.GetColorConstant(colorButton.color);
            colorButton.button.onClick.AddListener(() => SetHeartColor(color));
        }
    }

    private void SetHeartColor(Color color)
    {
        _selectedHeartColor = color;
        _heartColorSetter.SetHeartColor(_selectedHeartColor);
    }

    public void PostClanToServer()
    {
        string clanName = _clanNameField.text;
        string phrase = " ";
        bool isOpen = !_openClanToggle.isOn;
        string password = _clanPasswordField.text;
        Language language = _languageSelection.SelectedLanguage;
        Goals goal = _clanGoalSelection.GoalsRange;
        ClanAge age = _ageSelection.ClanAgeRange;
        ClanRoleRights[] clanRights = _defaultRights;
        ClanValues[] values = _valueSelection.SelectedValues.ToArray();
        List<HeartPieceData> clanHeartPieces = new();
        for (int i = 0; i < 50; i++) clanHeartPieces.Add(new HeartPieceData(i, _selectedHeartColor));

        if (!CheckClanInputsValidity(clanName, isOpen, password, language, goal, values))
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
            goal = goal,
            ageRange = age,
            labels = serverValues,
            clanLogo = logo
        };

        StartCoroutine(ServerManager.Instance.PostClanToServer(serverClan, clan =>
        {
            if (clan == null)
            {
                return;
            }

            Debug.Log($"naviTarget {_naviTarget} isCurrentPopOutWindow {true}", _naviTarget);
            IWindowManager windowManager = WindowManager.Get();
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

    private bool CheckClanInputsValidity(string clanName, bool isOpen, string password, Language language, Goals goal, ClanValues[] values)
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

        if (goal == Goals.None)
        {
            _goalWarningOutline.SetActive(true);
            _warningPopup.ActivatePopUp("Valitse klaanin tavoite");
            validInputs = false;
        }
        else _goalWarningOutline.SetActive(false);

        if (values.Length < 3)
        {
            _valuesWarningOutline.SetActive(true);
            _warningPopup.ActivatePopUp("Klaanille tulee olla valittuna vähintää 3 arvoa");
            validInputs = false;
        }
        else
        {
            _valuesWarningOutline.SetActive(false);
        }

        return validInputs;
    }
}
