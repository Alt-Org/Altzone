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

    public GameObject popUpScreen;

    public TMP_Text titleText;
    public TMP_Text pointText;
    public TMP_Text playerNameText;

    public Button questSelectButton;
    public Slider taskGoalSlider;

    public void Start()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(1080, 540);

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
                Debug.Log("You have already chosen a Mission!");
            }

        });
    }

    public void getMissionData(string taskTitle, int taskPoints, int taskGoal)
    {
        titleText.text = taskTitle;
        pointText.text = taskPoints.ToString();
        taskGoalSlider.maxValue = taskGoal;
    }

    public void SetQuestActive()
    {
        unActiveTask.SetActive(false);
        activeTask.SetActive(true);
        Storefront.Get().GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, p => p.dailyTaskId = taskId);
    }

}
