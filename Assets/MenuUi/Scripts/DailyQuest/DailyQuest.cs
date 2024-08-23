using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyQuest : MonoBehaviour
{

    public int taskId = 0;
    private string taskTitle;
    private int taskPoints;
    private int taskgoal;

    public TMP_Text titleText;
    public TMP_Text pointText;

    public void Start()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(1080, 540);
        InitializeTask(taskId);
    }

    public void InitializeTask(int taskId)
    {
        Debug.Log("taskInit Runned");
        this.taskId = taskId;

        int rnd = UnityEngine.Random.Range(1, 12);
        Debug.Log(rnd);

        switch (rnd)
        {
            case 1:
                taskTitle = "Pelaa 3 taistelua";
                taskgoal = 3;
                taskPoints = 70;
                UpdateTexts(taskTitle, taskPoints);
                break;
            case 2:
                taskTitle = "Pelaa 10 taistelua";
                taskgoal = 10;
                taskPoints = 500;
                UpdateTexts(taskTitle, taskPoints);
                break;
            case 3:
                taskTitle = "Ker‰‰ timantteja Taistelussa";
                taskgoal = 50;
                taskPoints = 50;
                UpdateTexts(taskTitle, taskPoints);
                break;
            case 4:
                taskTitle = "Ker‰‰ timantteja Taistelussa";
                taskgoal = 100;
                taskPoints = 120;
                UpdateTexts(taskTitle, taskPoints);
                break;
            case 5:
                taskTitle = "Kehit‰ pelihahmoa";
                taskgoal = 2;
                taskPoints = 100;
                UpdateTexts(taskTitle, taskPoints);
                break;
            case 6:
                taskTitle = "Voita 3 taistelu";
                taskgoal = 3;
                taskPoints = 100;
                UpdateTexts(taskTitle, taskPoints);
                break;
            case 7:
                taskTitle = "Voita 5 taistelu";
                taskgoal = 5;
                taskPoints = 200;
                UpdateTexts(taskTitle, taskPoints);
                break;
            case 8:
                taskTitle = "Muokkaa avatarisi ulkon‰kˆ‰";
                taskgoal = 2;
                taskPoints = 500;
                UpdateTexts(taskTitle, taskPoints);
                break;
            case 9:
                taskTitle = "Moikkaa avatareja";
                taskgoal = 5;
                taskPoints = 250;
                UpdateTexts(taskTitle, taskPoints);
                break;
            case 10:
                taskTitle = "Kirjoita viesti chattiin";
                taskgoal = 10;
                taskPoints = 100;
                UpdateTexts(taskTitle, taskPoints);
                break;
            case 11:
                taskTitle = "Sijoita pommi sielunkotiin";
                taskgoal = 3;
                taskPoints = 30;
                UpdateTexts(taskTitle, taskPoints);
                break;
            default:
                taskTitle = "Unknown Task";
                taskgoal = 100;
                taskPoints = 1;
                Debug.Log("Jokin Meni pieleen DailyTaskiss‰");
                break;
        }

    }

    public void UpdateTexts(string title,int points)
    {
        titleText.text = title;
        pointText.text = points.ToString();
    }

}
