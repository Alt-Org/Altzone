using UnityEngine;
using TMPro;
using Altzone.Scripts.Model.Poco.Game;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Altzone.Scripts.ReferenceSheets;

public class DailyQuest : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    //Variables
    private int _selfIndex = -1;
    private PlayerTask _taskData;
    private bool _clickEnabled = true;
    public PlayerTask TaskData { get { return _taskData; } }

    public enum TaskWindowType
    {
        Available,
        Reserved
    }

    [SerializeField] private DailyTaskCardImageReference _cardImageReference;

    [Header("Universal")]
    [SerializeField] private GameObject _coinIndicator;
    [SerializeField] private Image _TaskImage;

    [Header("Windows")]
    [SerializeField] private GameObject _availableWindow;
    [SerializeField] private GameObject _reservedWindow;

    [Header("Available Window")]
    [SerializeField] private TMP_Text _taskShort;
    [SerializeField] private TMP_Text _taskDebugID;
    [SerializeField] private TMP_Text _taskPoints;
    [SerializeField] private TMP_Text _taskCoins;
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
    [SerializeField] private Image _playerImage;
    [SerializeField] private Image _progressImage;
    [SerializeField] private Image _progressImageDefault;
    [SerializeField] private TMP_Text _progressText;

    [HideInInspector] public DailyTaskManager dailyTaskManager;

    private void OnEnable()
    {
        _playerImage.gameObject.SetActive(false);
    }

    private void Start()
    {
        SettingsCarrier.OnLanguageChanged += UpdateLanguage;
    }

    private void OnDestroy()
    {
        SettingsCarrier.OnLanguageChanged -= UpdateLanguage;
        //    if (_taskData != null)
        //    {
        //        _taskData.OnTaskSelected -= TaskSelected;
        //        _taskData.OnTaskDeselected -= TaskDeselected;
        //        _taskData.OnTaskUpdated -= UpdateProgressBar;
        //    }
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
        TaskSelected();
    }

    public void SetTaskData(PlayerTask taskData, int index)
    {
        _taskData = taskData;
        _selfIndex = index;

        /*Bind this class to PlayerTask for later interactions
         *between DailyTaskManager and this class.*/
        //_taskData.OnTaskSelected += TaskSelected;
        //_taskData.OnTaskDeselected += TaskDeselected;
        //_taskData.OnTaskUpdated += UpdateProgressBar;

        PopulateData();
        SwitchWindow(TaskWindowType.Available);
    }

    public void DailyTaskInfo()
    {
        if (!_clickEnabled || _taskData.PlayerId != "")
            return;

        PopupData data = new(_taskData, GetCornerLocation(), _selfIndex);
        StartCoroutine(dailyTaskManager.ShowPopupAndHandleResponse(_taskData.Title, data));
    }

    public void DailyTaskAccept()
    {
        if (!_clickEnabled || _taskData.PlayerId != "")
            return;

        StartCoroutine(dailyTaskManager.AcceptTask(_taskData, null, _selfIndex));
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
        // Use the current language to pick the correct title
        if (SettingsCarrier.Instance.Language == SettingsCarrier.LanguageType.English)
        {
            _taskShort.text = _taskData.EnglishTitle;
            //_taskContent.text = _taskData.EnglishContent; // if you have a content field
        }
        else
        {
            _taskShort.text = _taskData.Title;
            //_taskContent.text = _taskData.Content;
        }

        _taskDebugID.text = _taskData.Id.ToString();
        _taskPoints.text = _taskData.Points.ToString();
        _taskCoins.text = _taskData.Coins.ToString();
        _taskAmount.text = _taskData.Amount.ToString();
        _coinIndicator.SetActive(_taskData.Coins >= 0);

        _TaskImage.sprite = _cardImageReference.GetTaskImage(_taskData);
    }


    private string GetShortDescription(TaskNormalType taskType)
    {
        switch (taskType)
        {
            case TaskNormalType.PlayBattle: return ("Taisteluja");
            case TaskNormalType.WinBattle: return ("Voittoja");
            case TaskNormalType.StartBattleDifferentCharacter: return ("Taistele Eri Hahmoilla");
            case TaskNormalType.WriteChatMessage: return ("Kirjoita viestej�");
            case TaskNormalType.Vote: return ("��nest�");
            case TaskNormalType.Undefined: return ("");
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
        _playerImage.gameObject.SetActive(true);
        //_playerImage.sprite = INSERT PLAYER IMAGE HERE;
        UpdateProgressBar();
    }

    public void UpdateProgressBar()
    {
        _progressText.text = $"{TaskData.TaskProgress}/{TaskData.Amount}";
        _progressImage.fillAmount = (float)TaskData.TaskProgress / (float)TaskData.Amount;

        // Update secondary progress (grey background) to always be full
        if (_progressImageDefault != null)
        {
            _progressImageDefault.enabled = true;
        }
    }

    public void TaskDeselected()
    {
        SwitchWindow(TaskWindowType.Available);
        _playerImage.gameObject.SetActive(false);
        _taskData.ClearPlayerId();
    }

    private void UpdateLanguage(SettingsCarrier.LanguageType language)
    {
        _taskShort.text = language switch
        {
            SettingsCarrier.LanguageType.Finnish => _taskData.Title,
            SettingsCarrier.LanguageType.English => _taskData.EnglishTitle,
            _ => _taskData.Title,
        };
    }
}
