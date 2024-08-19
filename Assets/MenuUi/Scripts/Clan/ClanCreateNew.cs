using System.Collections;
using System.Collections.Generic;
using MenuUi.Scripts.Window;
using MenuUi.Scripts.Window.ScriptableObjects;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ClanCreateNew : MonoBehaviour
{
    [SerializeField] private TMP_InputField _clanNameInputField;
    //[SerializeField] private TMP_InputField _clanTagInputField;
    //[SerializeField] private TMP_InputField _gameCoinsInputField;
    [SerializeField] private Toggle _openClanButton;
    //[SerializeField] private Button _returnToMainClanViewButton;

    [SerializeField] protected WindowDef _naviTarget;

    private void Reset()
    {
        StopAllCoroutines();
        _clanNameInputField.text = "";
        //_clanTagInputField.text = "";
        //_gameCoinsInputField.text = "";
        _openClanButton.isOn = false;
    }

    public void PostClanToServer()
    {
        string clanName = _clanNameInputField.text;
        //string clanTag = _clanTagInputField.text;
        //int gameCoins = int.Parse(_gameCoinsInputField.text);
        bool isOpen = !_openClanButton.isOn;

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
