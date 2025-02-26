using UnityEngine;
using TMPro;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DailyQuest : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    //Variables
    private PlayerTask _taskData;
    public PlayerTask TaskData { get { return _taskData; } }
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
    [Tooltip("Determines how close to the center of the screen the \"Popup\" window will be.")]
    [SerializeField] private float _centerPullSignificance = 0.25f;

    [Header("Reserved Window")]
    [SerializeField] private Image _reservedImage;
    [SerializeField] private Image _playerImage;
    [SerializeField] private Image _progressImage;
    [SerializeField] private TMP_Text _progressText;

    [HideInInspector] public DailyTaskManager dailyTaskManager;

    private void OnDestroy()
    {
        if (_taskData != null)
        {
            _taskData.OnTaskSelected -= TaskSelected;
            _taskData.OnTaskDeselected -= TaskDeselected;
            _taskData.OnTaskUpdated -= UpdateProgressBar;
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

    public void SetTaskData(PlayerTask taskData)
    {
        _taskData = taskData;

        /*Bind this class to PlayerTask for later interactions
         *between DailyTaskManager and this class.*/
        _taskData.OnTaskSelected += TaskSelected;
        _taskData.OnTaskDeselected += TaskDeselected;
        _taskData.OnTaskUpdated += UpdateProgressBar;

        PopulateData();
        SwitchWindow(TaskWindowType.Available);
    }

    public void QuestAccept() //TODO: Rename to: DailyTaskAccept.
    {
        if (!_clickEnabled || TaskData.PlayerId != "")
            return;

        string message;

        if (dailyTaskManager.OwnTaskId == null)
            message = _taskData.Title;
        else
            message = $"{_taskData.Title}\n Korvataanko nykyinen teht�v�?";

        PopupData data = new(_taskData, GetCornerLocation());
        StartCoroutine(dailyTaskManager.ShowPopupAndHandleResponse(message, data));
    }

    /// <summary>
    /// Returns the best location for the <c>Popup.cs</c> window depending <br/>
    /// where this DailyQuest card is located on the screen.
    /// </summary>
    private Vector3 GetCornerLocation()
    {
        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
        float xDistanceFromCenter = (screenCenter.x - transform.position.x);
        float yDistanceFromCenter = (screenCenter.y - transform.position.y);
        Vector3 centerDiff = new Vector3(xDistanceFromCenter, yDistanceFromCenter) * _centerPullSignificance;

        if (transform.position.y > Screen.height / 2)
        {
            if (transform.position.x > Screen.width / 2)
                return (_bottomLeftCorner.position + centerDiff);
            else
                return (_bottomRightCorner.position + centerDiff);
        }
        else
        {
            if (transform.position.x > Screen.width / 2)
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
