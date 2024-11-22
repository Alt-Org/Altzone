using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopupManager : MonoBehaviour
{
    public Button acceptButton;
    public Button cancelButton;

    private DailyQuest _dailyQuest;
    private int _currentSelectedID = 0;

    public TMP_Text debugText;
    public TMP_Text description;
    public GameObject popUpUiElement;



    private void Start()
    {
        PlayerData playerData = null;
        Storefront.Get().GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, p => playerData = p);
        _currentSelectedID = playerData.dailyTaskId;
        Debug.Log("start id" + _currentSelectedID);

        acceptButton.onClick.AddListener(() =>
        {

            Debug.Log("Player Data ID: " + playerData.dailyTaskId);
            Debug.Log("accept id" + _currentSelectedID);

            if (playerData != null && _currentSelectedID <= 0)
            {
                _dailyQuest.SetQuestActive();
                popUpUiElement.SetActive(false);
                Debug.Log("Accept Button with dailytask Pressed");
                Debug.Log("Player Data ID: " + playerData?.dailyTaskId);
                _currentSelectedID = playerData.dailyTaskId;
                Debug.Log(_currentSelectedID);
            }

            else if (_currentSelectedID >= 1)
            {
                _dailyQuest.CallCancel();
                popUpUiElement.SetActive(false);
                Debug.Log("Accept with something Else was pressed");
                Debug.Log("Player Data ID: " + playerData?.dailyTaskId);
                _currentSelectedID = 0;
            }

        });

        cancelButton.onClick.AddListener(() =>
        {
            popUpUiElement.SetActive(false);
        });
    }

    public void EnableDailyPopup(int taskID, DailyQuest selectedDaily)
    {
        popUpUiElement.SetActive(true);
        description.text = "Haluatko valita tämän daily taskin?";
        debugText.text = "Selected Quest Id: " + taskID.ToString();
        _dailyQuest = selectedDaily;
    }
    public void EnableCancelPopup(int taskID, DailyQuest selectedDaily)
    {
        popUpUiElement.SetActive(true);
        description.text = "Haluatko varmasti peruuttaa tehtäväsi?";
        debugText.text = "Selected Quest Id: " + taskID.ToString();
        _dailyQuest = selectedDaily;


    }

}
