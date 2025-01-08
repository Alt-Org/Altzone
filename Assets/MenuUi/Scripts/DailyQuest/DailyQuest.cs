using UnityEngine;
using TMPro;
using Altzone.Scripts.Model.Poco.Game;

public class DailyQuest : MonoBehaviour
{
    //Variables
    private PlayerTasks.PlayerTask _taskData;

    [Header("DailyQuest Texts")]
    [SerializeField] private TMP_Text questTitle;
    [SerializeField] private TMP_Text questDebugID;
    [SerializeField] private TMP_Text questCoins;

    [HideInInspector] public DailyTaskManager dailyTaskManager;

    public void GetQuestData(PlayerTasks.PlayerTask taskData)
    {
        _taskData = taskData;
        PopulateData();
    }

    public void QuestAccept()
    {
        StartCoroutine(dailyTaskManager.ShowPopupAndHandleResponse("Haluatko Hyväksyä! quest id: " + _taskData.Id.ToString(),1));
    }

    public void PopulateData()
    {
        questTitle.text = _taskData.Title;
        questDebugID.text = _taskData.Id.ToString();
        questCoins.text = _taskData.Coins.ToString();
    }
}
