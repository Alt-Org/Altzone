using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using MenuUi.Scripts.Window;
using MenuUi.Scripts.Window.ScriptableObjects;
using UnityEngine.SceneManagement;

public class PrivacyPolicyChecker : MonoBehaviour
{
    [SerializeField] Button _playButton;
    [SerializeField] string _privacyPolicyAddress;
    [SerializeField] TextMeshProUGUI _privacyPolicyText;
    [SerializeField] TextMeshProUGUI _privacyPolicyAgreementText;
    [SerializeField] WindowDef _mainMenuWindow;
    [SerializeField] WindowDef _introSceneWindow;
    [SerializeField] int _introScene;
    private int _privacyPolicyIndex = 0;            // 0 if not accepted, 1 if has

    private void Start()
    {
        TogglePlayButton(false);
        _privacyPolicyIndex = PlayerPrefs.GetInt("PrivacyPolicy", 0);
   
        _privacyPolicyText.text = _privacyPolicyText.text.Replace("link=1", "link= " + _privacyPolicyAddress);
        _privacyPolicyAgreementText.text = _privacyPolicyAgreementText.text.Replace("link=1", "link= " + _privacyPolicyAddress);
    }

    public void TogglePlayButton(bool value)
    {
        _playButton.interactable = value;
    }

    public void OnPlayButtonPressed()
    {
        PlayerPrefs.SetInt("PrivacyPolicy", 1);
        var windowManager = WindowManager.Get();
        if (PlayerPrefs.GetInt("hasSelectedCharacter", 0) == 0)
            if (_introSceneWindow != null) windowManager.ShowWindow(_introSceneWindow);
            else SceneManager.LoadScene(_introScene);
        else windowManager.ShowWindow(_mainMenuWindow);
    }
}
