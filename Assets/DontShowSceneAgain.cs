using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using MenuUi.Scripts.Window;
using MenuUi.Scripts.Window.ScriptableObjects;

public class DontShowSceneAgain : MonoBehaviour
{
    [SerializeField] WindowDef _mainMenuWindow;
    [SerializeField] Button _SelectButton;
    private int _HasSelectedCharacter = 0;            // 0 if not accepted, 1 if has

    private void Start()
    {
        _HasSelectedCharacter = PlayerPrefs.GetInt("hasSelectedCharacter", 0);
    }

    public void OnSelectButtonPressed()
    {
        PlayerPrefs.SetInt("hasSelectedCharacter", 1);
        var windowManager = WindowManager.Get();
        windowManager.ShowWindow(_mainMenuWindow);
    }
}
