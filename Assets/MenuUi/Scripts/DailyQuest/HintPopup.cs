using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Language;
using Altzone.Scripts.Model.Poco.Game;
using TMPro;
using UnityEngine;

public class HintPopup : MonoBehaviour
{

    [SerializeField]
    private GameObject _confirmScreen;

    [SerializeField]
    private GameObject _hintScreen;

    [SerializeField]
    private TextLanguageSelectorCaller _hintTitle;

    [SerializeField]
    private TextMeshProUGUI _hintDescription;

    private PlayerTask _currentTask = null;


    void Start()
    {
        DailyTaskOwnTask.OnTaskHintNeeded += RequestHint;
    }

    private void OnDestroy()
    {
        DailyTaskOwnTask.OnTaskHintNeeded -= RequestHint;
    }

    public void RequestHint(PlayerTask task)
    {
        _currentTask = task;
        ShowConfirmScreen();
    }

    private void ShowConfirmScreen()
    {
        if (_currentTask == null) return;
        _confirmScreen.SetActive(true);
    }

    public void HideConfirmScreen()
    {
        _confirmScreen.SetActive(false);
    }

    public void ShowHint()
    {
        if (_currentTask == null) return;
        HideConfirmScreen();
        _hintScreen.SetActive(true);

        _hintTitle.SetText(SettingsCarrier.Instance.Language, new[] { _currentTask.Title });

        _hintDescription.text = _currentTask.Instruction;
        
    }

    public void CloseHint()
    {
        _currentTask = null;
        _hintScreen.SetActive(false);
    }
}
