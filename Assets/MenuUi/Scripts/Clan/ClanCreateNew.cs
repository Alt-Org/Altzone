using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using MenuUi.Scripts.Window;
using MenuUi.Scripts.Window.ScriptableObjects;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClanCreateNew : MonoBehaviour
{
    public enum Language
    {
        None,
        Finnish,
        Swedish,
        English
    }
    //List
    [SerializeField] private List <string> _valuesList;

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

   [SerializeField] private Toggle _openClanButton;

   //Buttons

   [SerializeField] private Button _returnToMainClanViewButton;
   [SerializeField] private Button _clanValues;
   [SerializeField] private Button _buttonLogo;

   // Dropdowns Or TMP Dropdowns

   [SerializeField] private TMP_Dropdown _ClanLanguageDropdown;
   [SerializeField] private TMP_Dropdown _ClanGoals;

   [SerializeField] protected WindowDef _naviTarget;

    private void Reset()
    {
        StopAllCoroutines();
        _clanNameInputField.text = "";
        //_clanTagInputField.text = "";
        //_gameCoinsInputField.text = "";
        _openClanButton.isOn = false;


       SetLanguageDropdown();
       SetValuesPanel();
    }
    private void OnEnable()
    {
        Reset();
    }
    private void SetLanguageDropdown()
    {
     _ClanLanguageDropdown.options.Clear();
     foreach(Language language in Enum.GetValues(typeof (Language) ) )
        {
            String Text;

            switch (language)
            {
                case Language.None:
                    Text = "Kieli / Språk / Language";
                    break;
                case Language.Finnish:
                    Text = "Suomi";
                    break;
                case Language.Swedish:
                    Text = "Svenska";
                    break;
                case Language.English:
                    Text = "English";
                    break;
                default:
                    Text = "";
                    break;
            }
            _ClanLanguageDropdown.options.Add(new TMP_Dropdown.OptionData(Text));
        }
    }

    private void SetValuesPanel()
    {
        for (int i = _valuesContainer.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(_valuesContainer.transform.GetChild(i));
        }
        foreach(string Text in _valuesList)
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
        String Phrase = _clanPhrase.text;
        Debug.Log($"language: {language}, Phrase: {Phrase} ");

        if (clanName == string.Empty /*|| clanTag == string.Empty || _gameCoinsInputField.text == string.Empty*/)
            return;

        StartCoroutine(ServerManager.Instance.PostClanToServer(clanName, clanName.Trim().Substring(0, 4), 0, isOpen, clan =>
        {
            if (clan == null)
            {
                return;
            }

            Debug.Log($"naviTarget {_naviTarget} isCurrentPopOutWindow {true}", _naviTarget);
            var windowManager = WindowManager.Get();
            windowManager.PopCurrentWindow();
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
