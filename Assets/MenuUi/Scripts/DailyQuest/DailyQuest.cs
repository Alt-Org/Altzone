using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DailyQuest : MonoBehaviour
{
    //Variables
    public int id;
    private int _amount;
    private int _coins;
    private int _points;

    private string _title;
    private string _content;

    [Header("DailyQuest Texts")]
    public TMP_Text questTitle;
    public TMP_Text questDebugID;
    public TMP_Text questCoins;

    public DailyTaskManager dailyTaskManager;

    public void GetQuestData(int ids, int amount, int coins, int points, string title, string content)
    {
        id = ids;
        _amount = amount;
        _coins = coins;
        _points = points;
        _title = title;
        _content = content;
        PopulateData();
    }

    public void QuestAccept()
    {
        StartCoroutine(dailyTaskManager.ShowPopupAndHandleResponse("Haluatko Hyväksyä! quest id: " + id.ToString(),1));
    }

    public void PopulateData()
    {
        questTitle.text = _title;
        questDebugID.text = id.ToString();
        questCoins.text = _coins.ToString();
    }
}
