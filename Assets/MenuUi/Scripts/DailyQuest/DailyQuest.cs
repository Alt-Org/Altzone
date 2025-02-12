using UnityEngine;
using TMPro;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DailyQuest : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    //Variables
    private PlayerTask _taskData;
    public PlayerTask TaskData {  get { return _taskData; } }
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
    [Space]
    [SerializeField] private float _centerPullSignificance = 0.25f;

    [Header("Reserved Window")]
    [SerializeField] private Image _reservedImage;
    [SerializeField] private Image _playerImage;
    [SerializeField] private Image _progressImage;
    [SerializeField] private TMP_Text _progressText;

    [HideInInspector] public DailyTaskManager dailyTaskManager;

    public void SetTaskData(PlayerTask taskData)
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
        //message = "Haluatko hyv�ksy� teht�v�n? \nquest id: ";
        else
            message = $"{_taskData.Title}\n Korvataanko nykyinen teht�v�?";

        PopupData data = new PopupData(_taskData, GetCornerLocation());
        StartCoroutine(dailyTaskManager.ShowPopupAndHandleResponse(message, data));
    }

    private Vector3 GetCornerLocation()
    {
        var selfPosition = this.GetComponent<RectTransform>().position;
        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
        float xDistanceFromCenter = (screenCenter.x - this.GetComponent<RectTransform>().position.x);
        float yDistanceFromCenter = (screenCenter.y - this.GetComponent<RectTransform>().position.y);
        Vector3 centerDiff = new Vector3(xDistanceFromCenter, yDistanceFromCenter) * _centerPullSignificance;

        if (selfPosition.y > Screen.height / 2)
        {
            if (selfPosition.x > Screen.width / 2)
                return (_bottomLeftCorner.position + centerDiff);
            else
                return (_bottomRightCorner.position + centerDiff);
        }
        else
        {
            if (selfPosition.x > Screen.width / 2)
                return (_topLeftCorner.position + centerDiff);
            else
                return (_topRightCorner.position + centerDiff);
        }
    }

    public void PopulateData()
    {
        _taskShort.text = GetShortDescription(_taskData.Type);
        _taskDebugID.text = _taskData.Id.ToString();
        _taskPoints.text = _taskData.Points.ToString() + " p";
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
            case TaskType.WriteChatMessage: return ("Kirjoita viestej�");
            case TaskType.Vote: return ("��nest�");
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
