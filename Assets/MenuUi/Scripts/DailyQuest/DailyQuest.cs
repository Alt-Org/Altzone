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
        var data = new DailyTaskManager.PopupData();
        data.Type = DailyTaskManager.PopupData.PopupDataType.OwnTask;

        var temp = new DailyTaskManager.PopupData.OwnPageData();
        temp.TaskId = _taskData.Id;
        temp.TaskDescription = _taskData.Title;
        temp.TaskAmount = _taskData.Amount;
        temp.TaskPoints = _taskData.Points;
        temp.TaskCoins = _taskData.Coins;

        data.OwnPage = temp;

        StartCoroutine(dailyTaskManager.ShowPopupAndHandleResponse("Haluatko Hyväksyä! quest id: " + _taskData.Id.ToString(), 1, data));
    }

    public void PopulateData()
    {
        questTitle.text = _taskData.Title;
        questDebugID.text = _taskData.Id.ToString();
        questCoins.text = _taskData.Coins.ToString();
    }
}
