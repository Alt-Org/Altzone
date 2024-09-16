using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyQuest : MonoBehaviour
{

    public int taskId = 0;

    public GameObject unActiveTask;
    public GameObject activeTask;

    public TMP_Text titleText;
    public TMP_Text pointText;
    public TMP_Text playerNameText;


    public Slider taskGoalSlider;

    public void Start()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(1080, 540);
    }

    public void getMissionData(string taskTitle, int taskPoints, int taskGoal)
    {
        titleText.text = taskTitle;
        pointText.text = taskPoints.ToString();
        taskGoalSlider.maxValue = taskGoal;
    }
}
