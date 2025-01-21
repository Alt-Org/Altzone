using UnityEngine;
using TMPro;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DailyQuest : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    //Variables
    private PlayerTasks.PlayerTask _taskData;
    private bool _clickEnabled = true;
    
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
        if (!_clickEnabled)
            return;

        PopupData data = new PopupData(_taskData);
        StartCoroutine(dailyTaskManager.ShowPopupAndHandleResponse("Haluatko Hyv�ksy�! quest id: " + _taskData.Id, data));
    }

    public void PopulateData()
    {
        questTitle.text = _taskData.Title;
        questDebugID.text = _taskData.Id.ToString();
        questCoins.text = _taskData.Coins.ToString();
    }

    public virtual void OnBeginDrag(PointerEventData eventData)
    {
        _clickEnabled = false;
    }

    public virtual void OnEndDrag(PointerEventData eventData)
    {
        _clickEnabled = true;
    }
}
