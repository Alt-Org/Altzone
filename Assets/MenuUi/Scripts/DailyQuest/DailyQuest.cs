using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyQuest : MonoBehaviour
{

    public int taskId = 0;
    public string taskTitle;
    public int taskPoints;

    public TMP_Text titleText;

    public void Start()
    {
        RectTransform rectTransform = GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(1080, 540);
    }

    public void InitializeTask(int taskId)
    {
        this.taskId = taskId;

        int rnd = Random.Range(1, 3);

        switch (rnd)
        {
            case 1:
                taskTitle = "Collect 10 Apples";
                taskPoints = 50;
                break;
            case 2:
                taskTitle = "Defeat 5 Enemies";
                taskPoints = 75;
                break;
            case 3:
                taskTitle = "Find the Lost Artifact";
                taskPoints = 100;
                break;
            default:
                taskTitle = "Unknown Task";
                taskPoints = 0;
                break;
        }


    }

}
