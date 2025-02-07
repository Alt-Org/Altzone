using UnityEngine;
using TMPro;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEditor.Experimental.GraphView;

public class DailyQuest : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    //Variables
    private PlayerTasks.PlayerTask _taskData;
    public PlayerTasks.PlayerTask TaskData {  get { return _taskData; } }
    private bool _clickEnabled = true;

    public enum TaskWindowType
    {
        Available,
        Reserved
    }

    [Header("Universal")]
    [SerializeField] private GameObject _coinIndicator;

    [Header("Windows")]
    [SerializeField] private GameObject _availableWindow;
    [SerializeField] private GameObject _reservedWindow;

    [Header("Available Window")]
    [SerializeField] private Image _AvailableImage;
    [SerializeField] private TMP_Text _taskShort;
    [SerializeField] private TMP_Text _taskDebugID;
    [SerializeField] private TMP_Text _taskPoints;
    [SerializeField] private TMP_Text _taskAmount;
    [Space]
    [SerializeField] private RectTransform _topLeftCorner;
    [SerializeField] private RectTransform _topRightCorner;
    [SerializeField] private RectTransform _bottomLeftCorner;
    [SerializeField] private RectTransform _bottomRightCorner;

    [Header("Reserved Window")]
    [SerializeField] private Image _reservedImage;
    [SerializeField] private Image _playerImage;
    [SerializeField] private Image _progressImage;
    [SerializeField] private TMP_Text _progressText;

    [HideInInspector] public DailyTaskManager dailyTaskManager;

    public void SetTaskData(PlayerTasks.PlayerTask taskData)
    {
        _taskData = taskData;
        _taskData.OnTaskSelected += TaskSelected;
        _taskData.OnTaskDeselected += TaskDeselected;
        _taskData.OnTaskUpdated += UpdateProgressBar;
        PopulateData();
        //if (available)
        SwitchWindow(TaskWindowType.Available);
        //else
        //{
        //SwitchWindow(TaskWindowType.Reserved);
        //SetTaskProgress();
        //}
    }

    private void OnDestroy()
    {
        if (_taskData != null)
        {
            _taskData.OnTaskSelected -= TaskSelected;
            _taskData.OnTaskDeselected -= TaskDeselected;
            _taskData.OnTaskUpdated -= UpdateProgressBar;
        }
    }

    public void QuestAccept()
    {
        if (!_clickEnabled || TaskData.PlayerId != "")
            return;

        string message;

        if (dailyTaskManager.OwnTaskId == null)
            message = _taskData.Title;
        //message = "Haluatko hyväksyä tehtävän? \nquest id: ";
        else
            message = $"Sinulla on jo valittu tehtävä.\n Haluatko hyväksyä tehtävän?";

        PopupData data = new PopupData(_taskData, GetCornerLocation());
        StartCoroutine(dailyTaskManager.ShowPopupAndHandleResponse(message, data));
    }

    private Vector3 GetCornerLocation()
    {
        var selfPosition = this.GetComponent<RectTransform>().position;

        if (selfPosition.y > Screen.height / 2)
        {
            if (selfPosition.x > Screen.width / 2)
                return (_bottomLeftCorner.position + new Vector3(0f, Screen.height * 0.01f));
            else
                return (_bottomRightCorner.position + new Vector3(0f, Screen.height * 0.01f));
        }
        else
        {
            if (selfPosition.x > Screen.width / 2)
                return (_topLeftCorner.position + new Vector3(0f, Screen.height * 0.05f));
            else
                return (_topRightCorner.position + new Vector3(0f, Screen.height * 0.05f));
        }
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

    private void SwitchWindow(TaskWindowType type)
    {
        _availableWindow.SetActive(type == TaskWindowType.Available);
        _reservedWindow.SetActive(type == TaskWindowType.Reserved);
    }

    public void TaskSelected()
    {
        SwitchWindow(TaskWindowType.Reserved);
        //_playerImage.sprite = INSERT PLAYER IMAGE HERE;
        UpdateProgressBar();
    }

    public void UpdateProgressBar()
    {
        _progressText.text = $"{TaskData.TaskProgress}/{TaskData.Amount}";
        _progressImage.fillAmount = (float)TaskData.TaskProgress / (float)TaskData.Amount;
    }

    public void TaskDeselected()
    {
        SwitchWindow(TaskWindowType.Available);
        _taskData.ClearPlayerId();
    }
}
