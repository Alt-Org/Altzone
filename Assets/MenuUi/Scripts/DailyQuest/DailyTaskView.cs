using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using MenuUi.Scripts.TabLine;
using UnityEngine;
using UnityEngine.UI;
using static DailyQuest;

public class DailyTaskView : AltMonoBehaviour
{

    [SerializeField] private TabLine _tabline;

    [Header("Views")]
    [SerializeField] private GameObject _dailyTasksView;
    [SerializeField] private GameObject _ownTaskView;
    [SerializeField] private GameObject _clanTaskView;

    [Header("TabButtons")]
    /*[SerializeField] private Button _dailyTasksTabButton;
    [SerializeField] private Button _ownTaskTabButton;
    [SerializeField] private Button _clanTaskTabButton;*/

    [Header("DailyTaskCard Education Prefabs")]
    [SerializeField] private GameObject _dailyTaskCardEducationSocialPrefab;
    [SerializeField] private GameObject _dailyTaskCardEducationStoryPrefab;
    [SerializeField] private GameObject _dailyTaskCardEducationCulturePrefab;
    [SerializeField] private GameObject _dailyTaskCardEducationEthicalPrefab;
    [SerializeField] private GameObject _dailyTaskCardEducationActionPrefab;

    [Header("DailyTaskCard Normal Prefabs")]
    [SerializeField] private GameObject _dailyTaskCardNormalRow1Prefab;
    [SerializeField] private GameObject _dailyTaskCardNormalRow2Prefab;
    [SerializeField] private GameObject _dailyTaskCardNormalRow3Prefab;


    [Header("DailyTasksEducationPage")]
    [SerializeField] private GameObject _dailyTasksEducationView;
    [SerializeField] private RectTransform _tasksEducationVerticalLayout;
    [Space]
    [SerializeField] private Transform _taskListEducationSocial;
    [SerializeField] private Transform _taskListEducationStory;
    [SerializeField] private Transform _taskListEducationCulture;
    [SerializeField] private Transform _taskListEducationEthical;
    [SerializeField] private Transform _taskListEducationAction;

    [Header("DailyTasksNormalPage")]
    [Space]
    [SerializeField] private GameObject _dailyTasksNormalView;
    [SerializeField] private RectTransform _tasksNormalVerticalLayout;
    [Space]
    [SerializeField] private Transform _taskListNormalRow1;
    [SerializeField] private int _dailyCategoryNormalRow1PointsLimit = 100;
    [Space]
    [SerializeField] private Transform _taskListNormalRow2;
    [SerializeField] private int _dailyCategoryNormalRow2PointsLimit = 500;
    [Space]
    [SerializeField] private Transform _taskListNormalRow3;

    [Header("OwnTaskPage")]
    [SerializeField] private Button _cancelTaskButton;
    [SerializeField] private DailyTaskOwnTask _ownTaskPageHandler;
    [Space]
    [SerializeField] private List<MoodThreshold> _moodThresholds;
    

    [System.Serializable]
    public struct MoodThreshold
    {
        public string Name;
        public DailyTaskOwnTask.MoodType MoodType;
        public int PointsThreshold;
    }

    [Header("ClanTaskPage")]
    [SerializeField] private GameObject _clanPlayerPrefab;
    [SerializeField] private RectTransform _clanPlayersList;

    [SerializeField] private RewardBar _clanRewardBar;


    private List<GameObject> _clanPlayers = new List<GameObject>();

    private List<GameObject> _dailyTaskCardSlots = new List<GameObject>();
    [HideInInspector] public List<GameObject> DailyTaskCardSlots { get { return _dailyTaskCardSlots; } }


    private VersionType _gameVersion;

    private bool _viewSetupDone = false;

    public enum SelectedTab
    {
        Tasks,
        OwnTask,
        ClanTask
    }
    private SelectedTab _selectedTab = SelectedTab.Tasks;


    IEnumerator Initialize()
    {
        yield return new WaitUntil(() => DailyTaskManager.Instance.DataReady);

        StartCoroutine(ViewSetup());

        // Don't allow canceling task on turboeducation
        if (_gameVersion == VersionType.TurboEducation)
        {
            _cancelTaskButton.gameObject.SetActive(false);
        }
        else
        {
            _cancelTaskButton.onClick.AddListener(() => StartCancelTask());
        }
        

        //Buttons
        //_dailyTasksTabButton.onClick.AddListener(() => SwitchTab(SelectedTab.Tasks));
        //_ownTaskTabButton.onClick.AddListener(() => SwitchTab(SelectedTab.OwnTask));
        //_clanTaskTabButton.onClick.AddListener(() => SwitchTab(SelectedTab.ClanTask));

        //Register to events
        try
        {
            DailyTaskProgressManager.OnTaskDone += ClearCurrentTask;
            DailyTaskProgressManager.OnTaskProgressed += UpdateOwnTaskProgress;
            DailyTaskManager.OnAcceptTask += OnTaskAccept;
            DailyTaskManager.OnCancelTask += OnTaskCancel;
            DailyTaskManager.OnMultipleChoiceProgress += OnMultipleChoiceProgress;
            DailyTaskOwnTask.OnCurrentTaskInfoNeeded += ShowCurrentTaskInfo;
        }
        catch
        {
            Debug.LogError("DailyTaskProgressManager instance missing!");
        }
    }


    private void OnEnable()
    {
        UpdateOwnTaskProgress();
    }


    private void Start()
    {
        StartCoroutine(Initialize());
    }

    private void OnDestroy()
    {
        try
        {
            DailyTaskProgressManager.OnTaskDone -= ClearCurrentTask;
            DailyTaskProgressManager.OnTaskProgressed -= UpdateOwnTaskProgress;
            DailyTaskManager.OnAcceptTask -= OnTaskAccept;
            DailyTaskManager.OnCancelTask -= OnTaskCancel;
            DailyTaskManager.OnMultipleChoiceProgress -= OnMultipleChoiceProgress;
            DailyTaskOwnTask.OnCurrentTaskInfoNeeded -= ShowCurrentTaskInfo;


        }
        catch
        {
            Debug.LogError("DailyTaskProgressManager instance missing!");
        }
    }

    void OnMultipleChoiceProgress()
    {
        gameObject.GetComponent<MultipleChoiceProgressListener>().UpdateProgressMultipleChoice(DailyTaskManager.Instance.GetCurrentTask());
    }

    void OnTaskAccept()
    {
        SwitchTab(SelectedTab.OwnTask);
    }

    void OnTaskCancel()
    {

        PlayerTask currentTask = DailyTaskManager.Instance.GetCurrentTask();

        if (currentTask != null)
        {
            if (FindDailyQuestForTask(currentTask) != null)
                FindDailyQuestForTask(currentTask).SetTaskAvailability(true); // Set task back to available (visually)
        }

        _ownTaskPageHandler.ClearCurrentTask();

        SwitchTab(SelectedTab.Tasks);

    }

    /// <summary>
    /// Switch tabs on the quest page
    /// </summary>
    /// <param name="tab">Tab to switch to</param>
    public void SwitchTab(SelectedTab tab)
    {
        //Hide old tab
        /*switch (_selectedTab)
        {
            case SelectedTab.Tasks: _dailyTasksView.SetActive(false); break;
            case SelectedTab.OwnTask: _ownTaskView.SetActive(false); break;
            default: _clanTaskView.SetActive(false); break;
        }

        // Set new selected tab
        _selectedTab = tab;

        //Show new tab
        switch (tab)
        {
            case SelectedTab.Tasks: _dailyTasksView.SetActive(true); break;
            case SelectedTab.OwnTask:
                _ownTaskView.SetActive(true);
                UpdateOwnTaskProgress();
                break;
            default: _clanTaskView.SetActive(true); break;
        }*/
        _tabline.ActivateTabButton((int)_selectedTab);

        Debug.Log($"Switched to {_selectedTab}.");
    }


    /// <summary>
    /// DailyTask page setup
    /// </summary>
    private IEnumerator ViewSetup()
    {
        _gameVersion = GameConfig.Get().GameVersionType;

        
        if (_gameVersion is VersionType.Education or VersionType.TurboEducation)
        {
            _dailyTasksEducationView.gameObject.SetActive(true);
            _dailyTasksNormalView.gameObject.SetActive(false);
        }
        else
        {
            _dailyTasksEducationView.gameObject.SetActive(false);
            _dailyTasksNormalView.gameObject.SetActive(true);
        }

        bool? dtCardsReady = null;

        PopulateTasks(data => dtCardsReady = data);
        yield return new WaitUntil(() => dtCardsReady != null);

        // Wait until GetSetExistingTask is done
        yield return StartCoroutine(GetSetExistingTask());

        //Get clan data.
        ClanData clanData = null;
        PlayerData playerData = DailyTaskManager.Instance.GetCurrentPlayerData();

        if (playerData.ClanId != null)
        {
            StartCoroutine(GetClanData(playerData.ClanId, data => clanData = data));

            bool? timeout = null;

            StartCoroutine(WaitUntilTimeout(DailyTaskManager.Instance.TimeoutSeconds, data => timeout = data));
            yield return new WaitUntil(() => (clanData != null || timeout != null));
            if (clanData == null)
            {
                Debug.LogError("Failed to fetch clan data.");
            }
            else
            {
                PopulateClanPlayers(clanData);
                _clanRewardBar.Initialize(clanData);
                //SetClanProgressBar(clanData);
            }
        }

        //CreateClanProgressBar(); //TODO: Move to inside the brackets when server is ready.
        _viewSetupDone = true;


    }

    /// <summary>
    /// Creates a task card for the given task and adds it to the view
    /// </summary>
    /// <param name="task">The task to create a task card for</param>
    /// <returns>DailyQuest from the given PlayerTask</returns>
    public DailyQuest AddTaskCardToView(PlayerTask task)
    {

        GameObject prefabToInstantiate = (
                _gameVersion is VersionType.Education or VersionType.TurboEducation ?
                GetEducationPrefabCategory(task.EducationCategory) :
                GetNormalPrefabCategory(task.Points)
                );

        Transform parentCategory = (
            _gameVersion is VersionType.Education or VersionType.TurboEducation ?
            GetEducationParentCategory(task.EducationCategory) :
            GetNormalParentCategory(task.Points)
            );

        DailyQuest quest = CreateTaskCard(task, parentCategory);

        return quest;
    }

    private void PopulateTasks(System.Action<bool> callback)
    {

        foreach (PlayerTask task in DailyTaskManager.Instance.ValidTasks.Tasks)
        {

            DailyQuest quest = AddTaskCardToView(task);

            // Update visually
            quest.SetTaskAvailability(task != DailyTaskManager.Instance.GetCurrentTask());

        }

        UpdateDailyTaskCards();

        callback(true);
    }


    public void StartCancelTask()
    {
        PopupData data = new(PopupData.PopupDataType.CancelTask);
        DailyTaskManager.Instance.ShowPopupAndHandleResponse("Haluatko Peruuttaa Nykyisen Teht�v�n?", data);
    }

    public void ClearCurrentTask()
    {
        UpdateAvatarMood();
        _ownTaskPageHandler.ClearCurrentTask();
        SwitchTab(DailyTaskView.SelectedTab.Tasks);
        DailyTaskManager.Instance.ClearCurrentTask();
    }

    private void UpdateAvatarMood()
    {
        int playerPoints = DailyTaskManager.Instance.GetCurrentPlayerData().points;

        for (int i = 0; i < _moodThresholds.Count; i++)
        {
            if ((_moodThresholds[i].PointsThreshold <= playerPoints) &&
                (((i + 1) >= _moodThresholds.Count) || (_moodThresholds[i + 1].PointsThreshold > playerPoints)))
            {
                _ownTaskPageHandler.SetMood(_moodThresholds[i].MoodType);
                break;
            }
        }
    }

    private void UpdateOwnTaskProgress()
    {
        if (!_viewSetupDone) return;

        PlayerTask taskData = DailyTaskProgressManager.Instance.CurrentPlayerTask;

        if (taskData == null) return;

        // Update OwnTaskPage in case already on OwnTask page
        _ownTaskPageHandler.UpdateOwnTaskPage();

        DailyQuest currentQuest = FindDailyQuestForTask(taskData);
        if (currentQuest == null) return;
        currentQuest.ReserveTask(taskData);
        currentQuest.UpdateProgressBar();
    }

    public Transform GetNormalParentCategory(int points)
    {
        if (points < _dailyCategoryNormalRow1PointsLimit)
            return (_taskListNormalRow1);

        if (points < _dailyCategoryNormalRow2PointsLimit)
            return (_taskListNormalRow2);

        return (_taskListNormalRow3);
    }

    public Transform GetEducationParentCategory(EducationCategoryType type)
    {
        return type switch
        {
            <= EducationCategoryType.Social => _taskListEducationSocial,
            <= EducationCategoryType.Story => _taskListEducationStory,
            <= EducationCategoryType.Culture => _taskListEducationCulture,
            <= EducationCategoryType.Ethical => _taskListEducationEthical,
            <= EducationCategoryType.Action => _taskListEducationAction,
            _ => _taskListEducationSocial,
        };
    }

    public GameObject GetNormalPrefabCategory(int points)
    {
        if (points < _dailyCategoryNormalRow1PointsLimit)
            return (_dailyTaskCardNormalRow1Prefab);

        if (points < _dailyCategoryNormalRow2PointsLimit)
            return (_dailyTaskCardNormalRow2Prefab);

        return (_dailyTaskCardNormalRow3Prefab);
    }

    public GameObject GetEducationPrefabCategory(EducationCategoryType type)
    {
        return type switch
        {
            <= EducationCategoryType.Social => _dailyTaskCardEducationSocialPrefab,
            <= EducationCategoryType.Story => _dailyTaskCardEducationStoryPrefab,
            <= EducationCategoryType.Culture => _dailyTaskCardEducationCulturePrefab,
            <= EducationCategoryType.Ethical => _dailyTaskCardEducationEthicalPrefab,
            <= EducationCategoryType.Action => _dailyTaskCardEducationActionPrefab,
            _ => _dailyTaskCardEducationSocialPrefab,
        };
    }

    public void UpdateDailyTaskCards()
    {
        if (_gameVersion is VersionType.Education or VersionType.TurboEducation)
        {
            //Needed to update the instantiated DT cards spacing in HorizontalLayoutGroups.
            LayoutRebuilder.ForceRebuildLayoutImmediate(_taskListEducationSocial.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(_taskListEducationStory.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(_taskListEducationCulture.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(_taskListEducationEthical.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(_taskListEducationAction.GetComponent<RectTransform>());

            //Sets DT cards to left side.
            _taskListEducationSocial.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            _taskListEducationStory.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            _taskListEducationCulture.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            _taskListEducationEthical.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            _taskListEducationAction.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            //Sets DT card category list to the top.
            _tasksEducationVerticalLayout.anchoredPosition = Vector2.zero;
        }
        else
        {
            //Needed to update the instantiated DT cards spacing in HorizontalLayoutGroups.
            LayoutRebuilder.ForceRebuildLayoutImmediate(_taskListNormalRow1.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(_taskListNormalRow2.GetComponent<RectTransform>());
            LayoutRebuilder.ForceRebuildLayoutImmediate(_taskListNormalRow3.GetComponent<RectTransform>());

            //Sets DT cards to left side.
            _taskListNormalRow1.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            _taskListNormalRow2.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            _taskListNormalRow3.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

            //Sets DT card category list to the top.
            _tasksNormalVerticalLayout.anchoredPosition = Vector2.zero;
        }
    }

    /// <summary>
    /// Creates a task card for the given task and adds it under the given parent
    /// </summary>
    /// <param name="task">The task to create a card for</param>
    /// <param name="parent">The parent for the task card to spawn in</param>
    /// <returns>The DailyQuest on the task card</returns>
    public DailyQuest CreateTaskCard(PlayerTask task, Transform parent)
    {
        // If given task is null, don't create task card
        if (task == null) return null;

        var gameVersion = GameConfig.Get().GameVersionType;

        GameObject prefabToInstantiate = (
                gameVersion == VersionType.Education || gameVersion == VersionType.TurboEducation ?
                GetEducationPrefabCategory(task.EducationCategory) :
                GetNormalPrefabCategory(task.Points)
                );

        GameObject taskCard = Instantiate(prefabToInstantiate, parent);

        DailyQuest quest = taskCard.GetComponent<DailyQuest>();
        quest.SetTaskData(task);
        quest.PopulateData();
        quest.ShowWindowWithType(TaskWindowType.Available);

        taskCard.SetActive(true);

        return quest;
    }

    /// <summary>
    /// Tries to get an existing task from <c>PlayerData</c><br/>
    /// and if successful, sets it to the owntask page.
    /// </summary>
    private IEnumerator GetSetExistingTask()
    {
        PlayerData playerData = DailyTaskManager.Instance.GetCurrentPlayerData();
        if (playerData.Task == null)
        {
            Debug.Log($"No current task in player data.");
            yield break;
        }

        DailyTaskManager.Instance.SetHandleOwnTask(playerData.Task);
    }

    public DailyQuest FindDailyQuestForTask(PlayerTask task)
    {
        foreach (GameObject taskCard in DailyTaskCardSlots)
        {
            DailyQuest taskQuest = taskCard.GetComponent<DailyQuest>();

            if (taskQuest.TaskData.Id == task.Id)
            {
                return taskQuest;
            }
        }

        return null;
    }

    private void PopulateClanPlayers(ClanData clanData)
    {
        for (int i = 0; i < clanData.Members.Count; i++)
        {
            GameObject player = Instantiate(_clanPlayerPrefab, _clanPlayersList);
            player.GetComponent<DailyTaskClanPlayer>().Set(i, clanData.Members[i].Name, 0);

            _clanPlayers.Add(player);
            Debug.Log("Created clan player: " + clanData.Members[i].Name);
        }

        //Needed to update the instantiated DT cards spacing in HorizontalLayoutGroups.
        //LayoutRebuilder.ForceRebuildLayoutImmediate(_clanPlayersList);

        //Sets DT cards to left side.
        _clanPlayersList.anchoredPosition = new Vector2(0f, -5000f);
    }

    private void ShowCurrentTaskInfo()
    {
        PlayerTask currentTask = DailyTaskManager.Instance.GetCurrentTask();

        if (currentTask == null) return;


        // If the current task is a multiple choice task, show the task
        if (MultipleChoiceOptions.Instance.IsMultipleChoice(currentTask))
        {

            DailyTaskManager.Instance.ShowMultipleChoiceTask();
            return;
        }

        // If the task is not a multiple choice, show the info
        Vector3 popupLocation = GetScreenCenter();
        PopupData data = new(currentTask, popupLocation);

        DailyTaskManager.Instance.ShowPopupAndHandleResponse(currentTask.Title, data);
    }

    private Vector3 GetScreenCenter()
    {
        Vector2 screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);

        return screenCenter;
    }



}
