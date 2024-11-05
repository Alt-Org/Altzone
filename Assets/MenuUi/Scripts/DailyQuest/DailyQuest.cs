using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Player;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyQuest : MonoBehaviour
{

    public int taskId = 0;

    public GameObject unActiveTask;
    public GameObject activeTask;

    public DailyTaskManager dailyTaskManager;

    public GameObject popUpScreen;

   
    public TMP_Text titleText;
    public TMP_Text pointText;
    public TMP_Text playerNameText;
    public TMP_Text idText;
    // active texts
    public TMP_Text titleActiveText;
    public TMP_Text pointActiveText;
    public TMP_Text idActiveText;

    public Button questSelectButton;
    public Slider taskGoalSlider;

    public void Start()
    {

        questSelectButton.onClick.AddListener(() =>
        {
        PlayerData playerData = null;
        Storefront.Get().GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, p => playerData = p);
            if (playerData.dailyTaskId < 1)
            {
                popUpScreen.GetComponent<PopupManager>().EnableDailyPopup(taskId, this);
            }
            else
            {
                Debug.Log("You have already chosen a Mission! " + playerData.dailyTaskId);
            }

        });
    }

    public void getMissionData(string taskTitle, int taskPoints, int taskGoal)
    {
        titleText.text = taskTitle;
        pointText.text = taskPoints.ToString();
        idText.text = taskId.ToString();
        taskGoalSlider.maxValue = taskGoal;

        titleActiveText.text = taskTitle;
        pointActiveText.text = taskPoints.ToString();
        idActiveText.text = taskId.ToString();
    }

    public void SetQuestActive()
    {
       
        dailyTaskManager.CancelTask();

        unActiveTask.SetActive(false);
        activeTask.SetActive(true);
        Debug.Log("Quest " + taskId + " is now active.");

        dailyTaskManager.TakeTask(taskId);

        Storefront.Get().GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, p => p.dailyTaskId = taskId);
    }


    public void CancelQuest()
    {
        PlayerData playerData = null;
        Storefront.Get().GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, p => playerData = p);
        playerData.dailyTaskId = -1;

        unActiveTask.SetActive(true);
        activeTask.SetActive(false);
        dailyTaskManager.CancelTask();

        Debug.Log("Quest has been Canceled");

    }

}
