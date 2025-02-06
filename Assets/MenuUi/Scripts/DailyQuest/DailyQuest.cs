using UnityEngine;
using TMPro;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DailyQuest : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    //Variables
    private PlayerTasks.PlayerTask _taskData;
    public PlayerTasks.PlayerTask TaskData {  get { return _taskData; } }
    private bool _clickEnabled = true;
    
    [SerializeField] private TMP_Text _taskShort;
    [SerializeField] private TMP_Text _taskDebugID;
    [SerializeField] private TMP_Text _taskPoints;
    [SerializeField] private TMP_Text _taskAmount;
    [SerializeField] private GameObject _coinIndicator;

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

        string message;

        if (dailyTaskManager.OwnTaskId == null)
            message = "Haluatko hyväksyä tehtävän? \nquest id: ";
        else
            message = "Sinulla on jo valittu tehtävä.\n Haluatko hyväksyä tehtävän? \nquest id: ";

        PopupData data = new PopupData(_taskData);
        StartCoroutine(dailyTaskManager.ShowPopupAndHandleResponse(message + _taskData.Id, data));
    }

    public void PopulateData()
    {
        _taskShort.text = GetShortDescription(_taskData.Type);
        _taskDebugID.text = _taskData.Id.ToString();
        _taskPoints.text = _taskData.Points.ToString();
        _taskAmount.text = _taskData.Amount.ToString();
        _coinIndicator.SetActive(_taskData.Coins >= 0);
    }

    private string GetShortDescription(TaskType taskType)
    {
        switch (taskType)
        {
            case TaskType.PlayBattle: return ("Taisteluja");
            case TaskType.WinBattle: return ("Voittoja");
            case TaskType.StartBattleDifferentCharacter: return ("Taistele Eri Hahmoilla");
            case TaskType.WriteChatMessage: return ("Kirjoita viestejä");
            case TaskType.Vote: return ("Äänestä");
            case TaskType.Undefined: return ("");
            default: Debug.LogError($"No short descrition available for: {taskType.ToString()}"); return ("Error");
        }
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
