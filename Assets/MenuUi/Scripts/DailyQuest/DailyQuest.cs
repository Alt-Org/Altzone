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

    private DailyTaskManager dailyTaskManager;

    

    private void Start()
    {
        SettingsCarrier.OnLanguageChanged += UpdateLanguage;

        dailyTaskManager = GameObject.Find("DailyTaskView").GetComponent<DailyTaskManager>();


        /*Bind this class to PlayerTask for later interactions
         *between DailyTaskManager and this class.*/
        //_taskData.OnTaskSelected += SetTaskAvailability(false);
        //_taskData.OnTaskDeselected += SetTaskAvailability(true);
        //_taskData.OnTaskUpdated += UpdateProgressBar;
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

    public void ReserveTask(PlayerTask taskData)
    {
        SetTaskData(taskData); // Set task data to make sure the task matches
        SetTaskAvailability(false);
    }

    public void SetTaskData(PlayerTask taskData)
    {
        _taskData = taskData;
    }

    public void SetTaskDataAndPopulate(PlayerTask taskData, int index)
    {
        SetTaskData(taskData);
        _selfIndex = index;
        PopulateData();
    }


    /// <summary>
    /// Opens the window where you see information about the task and can select it
    /// ("AcceptWindow" under pop.out.container -> UiElements).
    /// This method is called from the DailyTaskCard OnClick()
    /// </summary>
    public void DailyTaskInfo()
    {
        if (!_clickEnabled || _taskData.PlayerId != "") return;

        Vector3 popupLocation = GetScreenCenter(); //GetCornerLocation();
        PopupData data = new(_taskData, popupLocation, this);
        StartCoroutine(dailyTaskManager.ShowPopupAndHandleResponse(_taskData.Title, data));
    }

    /// <summary>
    /// Start the task
    /// </summary>
    public void DailyTaskAccept()
    {
        if (!_clickEnabled || _taskData.PlayerId != "")
            return;

        StartCoroutine(dailyTaskManager.AcceptTask(_taskData, null, this));
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

    private Vector3 GetScreenCenter()
    {
        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);

        return screenCenter;
    }

    /// <summary>
    /// Show the task data on the task card
    /// </summary>
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


        ShowWindowWithType(TaskWindowType.Available);
    }


    /// <summary>
    /// Change the appearance of the task card depending if it's available or not
    /// </summary>
    /// <param name="type"></param>
    public void ShowWindowWithType(TaskWindowType type)
    {
        _availableWindow.SetActive(type == TaskWindowType.Available);
        _reservedWindow.SetActive(type == TaskWindowType.Reserved);
    }

    

    /// <summary>
    /// Update progress bar shown on task card
    /// </summary>
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

    public void SetTaskAvailability(bool available)
    {
        if (available)
        {
            ShowWindowWithType(TaskWindowType.Available);
            _playerImage.gameObject.SetActive(false);
            _taskData.ClearPlayerId();
            
        }
        else
        {
            ShowWindowWithType(TaskWindowType.Reserved);
            _playerImage.gameObject.SetActive(true);
            //_playerImage.sprite = INSERT PLAYER IMAGE HERE;
            UpdateProgressBar();
        }
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
