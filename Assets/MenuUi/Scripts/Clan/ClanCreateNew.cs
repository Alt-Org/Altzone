using System;
using System.Collections;
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
    //List
    [SerializeField] private List<string> _valuesList;

    //GameObject
    [SerializeField] private GameObject _valuePrefab;
    [SerializeField] private GameObject _valuesContainer;

    //InputFields Or TMP InputFields

    [SerializeField] private TMP_InputField _clanNameInputField;
    //[SerializeField] private TMP_InputField _clanTagInputField;
    [SerializeField] private TMP_InputField _clanPhrase;
    [SerializeField] private TMP_InputField _clanAge;
    [SerializeField] private TMP_InputField _clanMembers;
    [SerializeField] private TMP_InputField _clanPassword;

    //Toggles
    [SerializeField] private Toggle _AgeTeinit;
    [SerializeField] private Toggle _AgeTaaperot;
    [SerializeField] private Toggle _AgeAikuiset;
    [SerializeField] private Toggle _AgeKaikenIkaiset;
    [SerializeField] private Toggle _openClanButton;

    //Warnings

    [SerializeField] private Image _ClanNameWarningImage;
    [SerializeField] private PopupController _NameTakenPopup;

    //Buttons

    [SerializeField] private Button _returnToMainClanViewButton;
    [SerializeField] private Button _clanValues;
    [SerializeField] private Button _buttonLogo;

    // Dropdowns Or TMP Dropdowns

    [SerializeField] private TMP_Dropdown _ClanLanguageDropdown;
    [SerializeField] private TMP_Dropdown _ClanGoals;

    [SerializeField] protected WindowDef _naviTarget;

    private ClanAge _clanAgeEnum = ClanAge.None;

    private void Reset()
    {
        StopAllCoroutines();
        _clanNameInputField.text = "";
        //_clanTagInputField.text = "";
        //_gameCoinsInputField.text = "";
        _openClanButton.isOn = false;


        SetLanguageDropdown();
        SetValuesPanel();
        SetGoalsDropDown();
        SetToggleListeners();
    }

    private void OnEnable()
    {
        Reset();
    }

    private void SetLanguageDropdown()
    {
        _ClanLanguageDropdown.options.Clear();
        foreach (Language language in Enum.GetValues(typeof(Language)))
        {
            string text = ClanDataTypeConverter.GetLanguageText(language);
            _ClanLanguageDropdown.options.Add(new TMP_Dropdown.OptionData(text));
        }
    }

    private void SetGoalsDropDown()
    {
        _ClanGoals.options.Clear();
        foreach (Goals goal in Enum.GetValues(typeof(Goals)))
        {
            string text = ClanDataTypeConverter.GetGoalText(goal);
            _ClanGoals.options.Add(new TMP_Dropdown.OptionData(text));
        }
    }

    private void SetToggleListeners()
    {
        _AgeTaaperot.onValueChanged.RemoveAllListeners();
        _AgeAikuiset.onValueChanged.RemoveAllListeners();
        _AgeKaikenIkaiset.onValueChanged.RemoveAllListeners();
        _AgeTeinit.onValueChanged.RemoveAllListeners();

        _AgeTaaperot.onValueChanged.AddListener((Value) =>
        {
            if (Value)
            {
                SetClanAge(ClanAge.AgeToddlers);
                ColorBlock colors = _AgeTaaperot.colors;
                colors.normalColor = Color.yellow;
                _AgeTaaperot.colors = colors;
            }
            else
            {
                ColorBlock colors = _AgeTaaperot.colors;
                colors.normalColor = Color.white;
                _AgeTaaperot.colors = colors;
            }
        });
        _AgeAikuiset.onValueChanged.AddListener((Value) =>
        {
            if (Value)
            {
                SetClanAge(ClanAge.AgeAdults);
                ColorBlock colors = _AgeAikuiset.colors;
                colors.normalColor = Color.yellow;
                _AgeAikuiset.colors = colors;
            }
            else
            {
                ColorBlock colors = _AgeAikuiset.colors;
                colors.normalColor = Color.white;
                _AgeAikuiset.colors = colors;
            }
        });
        _AgeKaikenIkaiset.onValueChanged.AddListener((Value) =>
        {
            if (Value)
            {
                SetClanAge(ClanAge.AgeAllAges);
                ColorBlock colors = _AgeKaikenIkaiset.colors;
                colors.normalColor = Color.yellow;
                _AgeKaikenIkaiset.colors = colors;
            }
            else
            {
                ColorBlock colors = _AgeKaikenIkaiset.colors;
                colors.normalColor = Color.white;
                _AgeKaikenIkaiset.colors = colors;
            }
        });
        _AgeTeinit.onValueChanged.AddListener((Value) =>
        {
            if (Value)
            {
                SetClanAge(ClanAge.AgeTeenagers);
                ColorBlock colors = _AgeTeinit.colors;
                colors.normalColor = Color.yellow;
                _AgeTeinit.colors = colors;
            }
            else
            {
                ColorBlock colors = _AgeTeinit.colors;
                colors.normalColor = Color.white;
                _AgeTeinit.colors = colors;
            }
        });
    }

    private void SetClanAge(ClanAge age)
    {
        _clanAgeEnum = age;
    }

    private void SetValuesPanel()
    {
        for (int i = _valuesContainer.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(_valuesContainer.transform.GetChild(i));
        }
        foreach (string Text in _valuesList)
        {
            GameObject ValueObject = Instantiate(_valuePrefab, _valuesContainer.transform);
            ValueObject.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = Text;
        }
    }

    public void PostClanToServer()
    {
        string clanName = _clanNameInputField.text;
        //string clanTag = _clanTagInputField.text;
        //int gameCoins = int.Parse(_gameCoinsInputField.text);
        bool isOpen = !_openClanButton.isOn;

        Language language = (Language)_ClanLanguageDropdown.value;
        Goals goals = (Goals)_ClanGoals.value;
        string Phrase = _clanPhrase.text;
        Debug.Log($"language: {language}, Phrase: {Phrase}, Goals: {goals} clanAgeEnum {_clanAgeEnum},");

        if (clanName == string.Empty /*|| clanTag == string.Empty || _gameCoinsInputField.text == string.Empty*/)
        {
            _ClanNameWarningImage.gameObject.SetActive(true);
            _NameTakenPopup.ActivatePopUp("Lisää klaanin nimi");
            return;
        }

        /*if (_clanPhrase.text== string.Empty)
        {
           _ClanNameWarningImage.gameObject.SetActive(true);
            _NameTakenPopup.ActivatePopUp("Lis�� klaanin motto");
            return;
        }*/


        StartCoroutine(ServerManager.Instance.PostClanToServer(clanName, clanName.Trim().Substring(0, 4), 0, isOpen, clan =>
        {
            if (clan == null)
            {
                return;
            }

            Debug.Log($"naviTarget {_naviTarget} isCurrentPopOutWindow {true}", _naviTarget);
            var windowManager = WindowManager.Get();
            //windowManager.PopCurrentWindow();
            // Check if navigation target window is already in window stack and we area actually going back to it via button.
            var windowCount = windowManager.WindowCount;
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
            return;
        }));
    }
}
