using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts;
using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Player;
using Altzone.Scripts.Config;
using MenuUi.Scripts.TabLine;

public class DailyTaskManager : AltMonoBehaviour
{
    #region Variables

    [Tooltip("Maximum time until a get or save data operation is forced to quit.")]
    [SerializeField] private float _timeoutSeconds = 10;
    [SerializeField] private TabLine _tabline;

    private PlayerData _currentPlayerData;

    [Header("Views")]
    [SerializeField] private GameObject _dailyTasksView;
    [SerializeField] private GameObject _ownTaskView;
    [SerializeField] private GameObject _clanTaskView;

    [Header("TabButtons")]
    [SerializeField] private Button _dailyTasksTabButton;
    [SerializeField] private Button _ownTaskTabButton;
    [SerializeField] private Button _clanTaskTabButton;

    private List<GameObject> _dailyTaskCardSlots = new List<GameObject>();
    private int _currentTaskCardIndex = -1;

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
    [Space]
    [SerializeField] private Button _showMultipleChoiceTaskButton;

    [System.Serializable]
    public struct MoodThreshold
    {
        public string Name;
        public DailyTaskOwnTask.MoodType MoodType;
        public int PointsThreshold;
    }

    private string _ownTaskId;
    public string OwnTaskId { get { return _ownTaskId; } }

    [Header("ClanTaskPage")]
    [SerializeField] private GameObject _clanPlayerPrefab;
    [SerializeField] private RectTransform _clanPlayersList;

    private List<GameObject> _clanPlayers = new List<GameObject>();

    [Header("ClanTaskProgressBar")]
    [SerializeField] private Slider _clanProgressBarSlider;
    [SerializeField] private RectTransform _clanProgressBarMarkersBase;
    [SerializeField] private GameObject _clanProgressBarMarkerPrefab;

    private List<GameObject> _clanProgressBarMarkers = new List<GameObject>();
    private int _clanMilestoneLatestRewardIndex = -1;

    //Local Testing, remove later
    private int _clanProgressBarGoal = 10000;
    private int _clanProgressBarCurrentPoints = 0;

    public enum SelectedTab
    {
        Tasks,
        OwnTask,
        ClanTask
    }
    private SelectedTab _selectedTab = SelectedTab.Tasks;

    #endregion

    void Start()
    {
        //DailyTask page setup
        StartCoroutine(DataSetup());

        //Buttons
        _dailyTasksTabButton.onClick.AddListener(() => SwitchTab(SelectedTab.Tasks));
        _ownTaskTabButton.onClick.AddListener(() => SwitchTab(SelectedTab.OwnTask));
        _clanTaskTabButton.onClick.AddListener(() => SwitchTab(SelectedTab.ClanTask));

        _cancelTaskButton.onClick.AddListener(() => StartCancelTask());
        _showMultipleChoiceTaskButton.onClick.AddListener(() => ShowMultipleChoiceTask());

        //_ownTaskTabButton.interactable = false;

        //Register to events
        try
        {
            DailyTaskProgressManager.OnTaskDone += ClearCurrentTask;
            DailyTaskProgressManager.OnTaskProgressed += UpdateOwnTaskProgress;
        }
        catch
        {
            Debug.LogError("DailyTaskProgressManager instance missing!");
        }
    }

    private IEnumerator DataSetup()
    {
        bool? timeout = null;
        bool? dtCardsReady = null;

        StartCoroutine(PlayerDataTransferer("get", null, _timeoutSeconds, data => timeout = data, data => _currentPlayerData = data));
        yield return new WaitUntil(() => (_currentPlayerData != null || timeout != null));

        if (_currentPlayerData == null)
        {
            Debug.LogError("Failed to fetch player data.");
            yield break;
        }

        StartCoroutine(PopulateTasks(data => dtCardsReady = data));
        yield return new WaitUntil(() => dtCardsReady != null);

        StartCoroutine(GetSetExistingTask());

        //Get clan data.
        ClanData clanData = null;
        if (_currentPlayerData?.ClanId != null)
        {
            StartCoroutine(GetClanData(_currentPlayerData.ClanId, data => clanData = data));

            StartCoroutine(WaitUntilTimeout(_timeoutSeconds, data => timeout = data));
            yield return new WaitUntil(() => (clanData != null || timeout != null));
            if (clanData == null)
            {
                Debug.LogError("Failed to fetch clan data.");
            }
            else
            {
                PopulateClanPlayers(clanData);
                SetClanProgressBar(clanData);
            }
        }

        CreateClanProgressBar(); //TODO: Move to inside the brackets when server is ready.
    }

    private void OnDestroy()
    {
        try
        {
            DailyTaskProgressManager.OnTaskDone -= ClearCurrentTask;
            DailyTaskProgressManager.OnTaskProgressed -= UpdateOwnTaskProgress;
        }
        catch
        {
            Debug.LogError("DailyTaskProgressManager instance missing!");
        }
    }

    #region Tasks

    private IEnumerator PopulateTasks(System.Action<bool> callback)
    {
        var gameVersion = GameConfig.Get().GameVersionType;

        if (gameVersion == VersionType.Education)
        {
            _dailyTasksEducationView.gameObject.SetActive(true);
            _dailyTasksNormalView.gameObject.SetActive(false);
        }
        else
        {
            _dailyTasksEducationView.gameObject.SetActive(false);
            _dailyTasksNormalView.gameObject.SetActive(true);
        }

        ClanTasks clanTasks = null;
        PlayerData playerData = null;
        Storefront.Get().GetPlayerTasks(content => clanTasks = content);
        StartCoroutine(GetPlayerData(content => playerData = content)); //MQTT message tells if we need to fetch the data again.
        if (playerData == null || !playerData.HasClanId)
        {
            if (gameVersion == VersionType.Education)
                clanTasks = GenerateEducationTasks();
            else
                clanTasks = TESTGenerateNormalTasks();
        }
        else
        StartCoroutine(ServerManager.Instance.GetPlayerTasksFromServer(content =>
        {
            if (content != null)
            {
                //clanTasks = content;
            }
            else
            {
                Debug.LogError("Could not connect to server and receive quests.");
                //Offline testing
                if (gameVersion == VersionType.Education)
                    clanTasks = GenerateEducationTasks();
                else
                    clanTasks = TESTGenerateNormalTasks();
                Debug.LogWarning("Using locally generated tasks.");
            }
        }));

        yield return new WaitUntil(() => clanTasks != null);

        ClanTasks referenceTasks = gameVersion == VersionType.Education ? GenerateEducationTasks() : GenerateNormalTasks();
        ClanTasks validatedTasks = gameVersion == VersionType.Education ? new(TaskVersionType.Education, new()) : new(TaskVersionType.Normal, new());
        if (referenceTasks.TaskVersionType == clanTasks.TaskVersionType)
            for (int i = 0; i < clanTasks.Tasks.Count; i++)
            {
                for (int j = 0; j < referenceTasks.Tasks.Count; j++)
                {
                    if (clanTasks.TaskVersionType == TaskVersionType.Education && clanTasks.Tasks[i].EducationCategory == referenceTasks.Tasks[j].EducationCategory)
                    {
                        switch (clanTasks.Tasks[i].EducationCategory)
                        {
                            case EducationCategoryType.Action:
                                {
                                    if (clanTasks.Tasks[i].EducationActionType == referenceTasks.Tasks[j].EducationActionType)
                                        validatedTasks.Tasks.Add(clanTasks.Tasks[i]);
                                    break;
                                }
                            case EducationCategoryType.Social:
                                {
                                    if (clanTasks.Tasks[i].EducationSocialType == referenceTasks.Tasks[j].EducationSocialType)
                                        validatedTasks.Tasks.Add(clanTasks.Tasks[i]);
                                    break;
                                }
                            case EducationCategoryType.Story:
                                {
                                    if (clanTasks.Tasks[i].EducationStoryType == referenceTasks.Tasks[j].EducationStoryType)
                                        validatedTasks.Tasks.Add(clanTasks.Tasks[i]);
                                    break;
                                }
                            case EducationCategoryType.Culture:
                                {
                                    if (clanTasks.Tasks[i].EducationCultureType == referenceTasks.Tasks[j].EducationCultureType)
                                        validatedTasks.Tasks.Add(clanTasks.Tasks[i]);
                                    break;
                                }
                            case EducationCategoryType.Ethical:
                                {
                                    if (clanTasks.Tasks[i].EducationEthicalType == referenceTasks.Tasks[j].EducationEthicalType)
                                        validatedTasks.Tasks.Add(clanTasks.Tasks[i]);
                                    break;
                                }
                        }
                    }
                    else if (clanTasks.TaskVersionType == TaskVersionType.Normal)
                    {
                        if (clanTasks.Tasks[i].Type == referenceTasks.Tasks[j].Type)
                            validatedTasks.Tasks.Add(clanTasks.Tasks[i]);
                    }
                }
            }
        // Generate a list of working tasks if none of the tasks from server were validated
        if (validatedTasks.Tasks.Count == 0)
        {
            switch (gameVersion)
            {
                case VersionType.Education: validatedTasks = GenerateEducationTasks(); break;
                case VersionType.Standard: validatedTasks = GenerateNormalTasks(); break;
            }
        }

        for (int i = 0; i < validatedTasks.Tasks.Count; i++)
        {
            GameObject prefabToInstantiate = (
                gameVersion == VersionType.Education ?
                GetEducationPrefabCategory(validatedTasks.Tasks[i].EducationCategory) :
                GetNormalPrefabCategory(validatedTasks.Tasks[i].Points)
                );

            GameObject taskObject = Instantiate(prefabToInstantiate, gameObject.transform);
            _dailyTaskCardSlots.Add(taskObject);

            DailyQuest task = taskObject.GetComponent<DailyQuest>();
            task.SetTaskData(validatedTasks.Tasks[i], i);
            if (playerData.Task != null && playerData.Task.Id == validatedTasks.Tasks[i].Id) _currentTaskCardIndex = i;
            task.dailyTaskManager = this;

            if (validatedTasks.Tasks[i].PlayerId != "")
                _dailyTaskCardSlots[i].GetComponent<DailyQuest>().TaskSelected();

            if (_currentPlayerData.Id == validatedTasks.Tasks[i].PlayerId)
            {
                _currentPlayerData.Task = validatedTasks.Tasks[i]; //TODO: Remove when fetching task data works.
                _currentTaskCardIndex = i;
            }

            Transform parentCategory = (
                gameVersion == VersionType.Education ?
                GetEducationParentCategory(validatedTasks.Tasks[i].EducationCategory) :
                GetNormalParentCategory(validatedTasks.Tasks[i].Points)
                );

            taskObject.transform.SetParent(parentCategory, false);
            taskObject.SetActive(true);

            Debug.Log("Created Task: " + validatedTasks.Tasks[i].Id);
        }

        if (gameVersion == VersionType.Education)
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

        callback(true);
    }

    private ClanTasks TESTGenerateNormalTasks() //TODO: Remove when fetching normal tasks from server is stable.
    {
        ServerPlayerTasks serverTasks = new ServerPlayerTasks();

        serverTasks.daily = new List<ServerPlayerTask>();
        serverTasks.weekly = new List<ServerPlayerTask>();
        serverTasks.monthly = new List<ServerPlayerTask>();
        for (int i = 0; i < 15; i++)
        {
            ServerPlayerTask serverTask = new ServerPlayerTask();
            serverTask._id = i.ToString();
            serverTask.amount = (i + 1) * 5;
            serverTask.amountLeft = serverTask.amount;
            serverTask.title = new ServerPlayerTask.TaskTitle();
            serverTask.title.fi = $"Lähetä {serverTask.amount} viestiä.";
            serverTask.points = (i + 1) * 100;
            serverTask.coins = (i + 1) * 100;
            serverTask.type = "write_chat_message";

            serverTasks.daily.Add(serverTask);
        }

        PlayerTasks tasks = new PlayerTasks(serverTasks);
        List<PlayerTask> tasklist = null;
        tasklist = tasks.Daily;
        tasklist.AddRange(tasks.Week);
        tasklist.AddRange(tasks.Month);
        return new(TaskVersionType.Normal, tasklist);
    }

    private ClanTasks GenerateNormalTasks()
    {
        List<PlayerTask> tasklist = new();
        List<NormalDailyTaskData> normalTasks = DailyTaskConfig.Instance.GetNormalTasks();

        for (int i = 0; i < normalTasks.Count; i++)
        {
            ServerPlayerTask serverTask = new();
            serverTask._id = i.ToString();
            serverTask.amount = normalTasks[i].amount;
            serverTask.amountLeft = serverTask.amount;
            serverTask.title = new ServerPlayerTask.TaskTitle();
            serverTask.title.fi = normalTasks[i].title;
            serverTask.content = new ServerPlayerTask.TaskContent();
            serverTask.content.fi = normalTasks[i].description;
            serverTask.points = normalTasks[i].points;
            serverTask.coins = normalTasks[i].coins;
            serverTask.type = normalTasks[i].type;
            serverTask.educationCategoryType = "";
            serverTask.educationCategoryTaskType = "";

            tasklist.Add(new(serverTask));
        }

        return new(TaskVersionType.Normal, tasklist);
    }

    private ClanTasks GenerateEducationTasks()
    {
        List<PlayerTask> tasklist = new();
        List<EducationDailyTaskData> educationTasks = DailyTaskConfig.Instance.GetEducationTasks();

        for (int i = 0; i < educationTasks.Count; i++)
        {
            ServerPlayerTask serverTask = new();
            serverTask._id = i.ToString();
            serverTask.amount = educationTasks[i].amount;
            serverTask.amountLeft = serverTask.amount;
            serverTask.title = new ServerPlayerTask.TaskTitle();
            serverTask.title.fi = educationTasks[i].title;
            serverTask.content = new ServerPlayerTask.TaskContent();
            serverTask.content.fi = educationTasks[i].description;
            serverTask.title.en = educationTasks[i].englishTitle;
            serverTask.content.en = educationTasks[i].englishDescription;
            serverTask.points = educationTasks[i].points;
            serverTask.coins = educationTasks[i].coins;
            serverTask.type = "";
            serverTask.educationCategoryType = educationTasks[i].educationCategoryType;
            serverTask.educationCategoryTaskType = educationTasks[i].educationCategoryTaskType;
            serverTask.isPlaceHolder = true;

            tasklist.Add(new(serverTask));
        }

        return new(TaskVersionType.Education, tasklist);
    }

    private Transform GetNormalParentCategory(int points)
    {
        if (points < _dailyCategoryNormalRow1PointsLimit)
            return (_taskListNormalRow1);

        if (points < _dailyCategoryNormalRow2PointsLimit)
            return (_taskListNormalRow2);

        return (_taskListNormalRow3);
    }

    private GameObject GetNormalPrefabCategory(int points)
    {
        if (points < _dailyCategoryNormalRow1PointsLimit)
            return (_dailyTaskCardNormalRow1Prefab);

        if (points < _dailyCategoryNormalRow2PointsLimit)
            return (_dailyTaskCardNormalRow2Prefab);

        return (_dailyTaskCardNormalRow3Prefab);
    }

    private Transform GetEducationParentCategory(EducationCategoryType type)
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

    private GameObject GetEducationPrefabCategory(EducationCategoryType type)
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

    /// <summary>
    /// Tries to get an existing task from <c>PlayerData</c><br/>
    /// and if successful, sets it to the owntask page.
    /// </summary>
    private IEnumerator GetSetExistingTask()
    {
        if (_currentPlayerData.Task == null)
        {
            Debug.Log($"No current task in player data.");
            yield break;
        }

        SetHandleOwnTask(_currentPlayerData.Task);
        SwitchTab(SelectedTab.OwnTask);
    }

    /// <summary>
    /// Get existing task from task card object.
    /// </summary>
    private IEnumerator GetTask(string id, System.Action<PlayerTask> callback)
    {
        foreach (GameObject taskObj in _dailyTaskCardSlots)
        {
            if (taskObj == null)
                continue;

            DailyQuest dailyQuest = taskObj.GetComponent<DailyQuest>();
            if (dailyQuest.TaskData.Id == id)
            {
                callback(dailyQuest.TaskData);
                yield return true;
            }
        }

        yield return true;
    }

    public void TESTAddTaskProgress()
    {
        if (_currentPlayerData.Task.Type == TaskNormalType.StartBattleDifferentCharacter)
            this.GetComponent<DailyTaskProgressListener>().UpdateProgress($"{System.DateTime.Now}");
        else
            this.GetComponent<DailyTaskProgressListener>().UpdateProgress("1");
    }

    private void UpdateOwnTaskProgress()
    {
        var taskData = DailyTaskProgressManager.Instance.CurrentPlayerTask;

        float progress = (float)taskData.TaskProgress / (float)taskData.Amount;
        _ownTaskPageHandler.SetTaskProgress(progress);
        _ownTaskPageHandler.TESTSetTaskValue(taskData.TaskProgress);
        //taskData.InvokeOnTaskUpdated();
        _dailyTaskCardSlots[_currentTaskCardIndex].GetComponent<DailyQuest>().UpdateProgressBar();
        Debug.Log("Task id: " + _ownTaskId + ", current progress: " + progress);
        if (progress >= 1f)
        {
            Debug.Log("Task id:" + _ownTaskId + ", is done");
        }
    }

    public void StartCancelTask()
    {
        PopupData data = new(PopupData.GetType("cancel_task"));
        StartCoroutine(ShowPopupAndHandleResponse("Haluatko Peruuttaa Nykyisen Tehtävän?", data));
    }

    private IEnumerator CancelTask(System.Action<bool> done)
    {
        PlayerData playerData = null;
        PlayerData savePlayerData = null;
        bool? unreserveResult = null;
        bool? timeout = null;
        Coroutine coroutineTimeout;

        //Get player data.
        StartCoroutine(PlayerDataTransferer("get", null, _timeoutSeconds, tdata => timeout = tdata, pdata => playerData = pdata));
        yield return new WaitUntil(() => (playerData != null || timeout != null));

        if (playerData == null || playerData.Task == null)
        {
            done(false);
            yield break;
        }

        if (!playerData.Task.Offline)
        {
            StartCoroutine(ServerManager.Instance.UnreservePlayerTaskFromServer(data => unreserveResult = data));
            coroutineTimeout = StartCoroutine(WaitUntilTimeout(_timeoutSeconds, data => timeout = data));
            yield return new WaitUntil(() => (unreserveResult != null || timeout != null));

            if (unreserveResult == null)
                Debug.LogError($"Failed to unreserve task id: {playerData.Task.Id}");

            StopCoroutine(coroutineTimeout);
        }
        //Save player data.
        playerData.Task.ClearProgress();
        playerData.Task.ClearPlayerId();
        //playerData.Task.InvokeOnTaskDeselected();
        _dailyTaskCardSlots[_currentTaskCardIndex].GetComponent<DailyQuest>().TaskDeselected();
        _currentTaskCardIndex = -1;
        playerData.Task = null;
        timeout = null;

        StartCoroutine(PlayerDataTransferer("save", playerData, _timeoutSeconds, tdata => timeout = tdata, pdata => savePlayerData = pdata));
        yield return new WaitUntil(() => (savePlayerData != null || timeout != null));

        if (savePlayerData == null)
        {
            done(false);
            yield break;
        }

        _currentPlayerData = savePlayerData;
        DailyTaskProgressManager.Instance.ChangeCurrentTask(savePlayerData.Task);
        _ownTaskPageHandler.ClearCurrentTask();
        Debug.Log("Task id: " + _ownTaskId + ", has been canceled.");
        _ownTaskId = null;
        done(true);
    }

    public void ClearCurrentTask()
    {
        UpdateAvatarMood();
        _currentPlayerData.Task.ClearProgress();
        _ownTaskPageHandler.ClearCurrentTask();
        //_ownTaskTabButton.interactable = false;
        SwitchTab(SelectedTab.Tasks);
        Debug.Log("Task id: " + _ownTaskId + ", has been cleard.");
        _ownTaskId = null;
    }

    private void UpdateAvatarMood()
    {
        int playerPoints = _currentPlayerData.points;

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

    public IEnumerator AcceptTask(PlayerTask playerTask, System.Action<bool> callback, int index)
    {
        bool? done = null;

        if (_currentPlayerData != null && _currentPlayerData.Task != null)
        {
            StartCoroutine(CancelTask(data => done = data));
            yield return new WaitUntil(() => done != null);
            done = null;
        }

        StartCoroutine(GetSaveSetHandleOwnTask(playerTask, data => done = data, index));
        yield return new WaitUntil(() => done != null);

        if (done.Value)
            SwitchTab(SelectedTab.OwnTask);

        if (callback != null)
            callback(done.Value);
    }

    private void ShowMultipleChoiceTask()
    {
        if (_currentPlayerData.Task == null || !MultipleChoiceOptions.Instance.IsMultipleChoice(_currentPlayerData.Task)) return;
        PopupData data = new(_currentPlayerData.Task);
        StartCoroutine(ShowPopupAndHandleResponse(_currentPlayerData.Task.Content, data));
    }

    #endregion

    #region Clan

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

    private void SetClanProgressBar(ClanData clanData)
    {
        //TODO: Get clan milestone reward data and fill the clan progress bar based on that data.
        _clanProgressBarSlider.value = (float)clanData.Points / (float)_clanProgressBarGoal;
    }

    private List<DailyTaskClanReward.ClanRewardData> TESTGenerateClanRewardsBar() //TODO: Remove when server is ready.
    {
        var clanRewardDatas = new List<DailyTaskClanReward.ClanRewardData>()
        {
            new DailyTaskClanReward.ClanRewardData(false, DailyTaskClanReward.ClanRewardType.Box, 500, null, Random.Range(0,1000)),
            new DailyTaskClanReward.ClanRewardData(false, DailyTaskClanReward.ClanRewardType.Box, 1000, null, Random.Range(0,1000)),
            new DailyTaskClanReward.ClanRewardData(false, DailyTaskClanReward.ClanRewardType.Box, 5000, null, Random.Range(0,1000)),
            new DailyTaskClanReward.ClanRewardData(false, DailyTaskClanReward.ClanRewardType.Chest, 10000, null, Random.Range(0,1000))
        };
        return clanRewardDatas;
    }

    public void TESTAddClanRewardBarPoints(int value)
    {
        _clanProgressBarCurrentPoints += value;

        StartCoroutine(CalculateClanRewardBarProgress());
    }

    //TODO: Needs to be moved or overhauled when server is ready.
    private IEnumerator CalculateClanRewardBarProgress()
    {
        float sectionLenghts = (1f / (float)_clanProgressBarMarkers.Count);

        for (int i = 0; i < _clanProgressBarMarkers.Count; i++)
        {
            int startPoints = (
                (i) <= 0 ?
                0 :
                _clanProgressBarMarkers[i - 1].GetComponent<DailyTaskClanReward>().Data.Threshold
                );

            int endPoints = _clanProgressBarMarkers[i].GetComponent<DailyTaskClanReward>().Data.Threshold;

            if ((_clanProgressBarCurrentPoints < endPoints) || (i >= _clanProgressBarMarkers.Count - 1))
            {
                float startPosition = sectionLenghts * i;
                float endPosition = ((i + 1) >= _clanProgressBarMarkers.Count ? 1f : (sectionLenghts * (float)(i + 1)));

                float chunkProgress = (float)(_clanProgressBarCurrentPoints - startPoints) / (float)(endPoints - startPoints);
                Debug.Log("ClanRewardsProgressBar: chunk progress: " + chunkProgress + ", start points: " + startPoints + ", end points: " + endPoints);

                //All but final reward.
                for (int j = 0; j < i; j++)
                {
                    if (j <= _clanMilestoneLatestRewardIndex)
                        continue;

                    _clanMilestoneLatestRewardIndex = j;
                    _clanProgressBarMarkers[j].GetComponent<DailyTaskClanReward>().UpdateState(true);
                    DailyTaskProgressManager.Instance.InvokeOnClanMilestoneReached(); //TODO: Remove when server ready.
                }

                //Final reward
                if ((i >= _clanProgressBarMarkers.Count - 1) && chunkProgress == 1)
                {
                    _clanProgressBarMarkers[_clanProgressBarMarkers.Count - 1].GetComponent<DailyTaskClanReward>().UpdateState(true);
                    DailyTaskProgressManager.Instance.InvokeOnClanMilestoneReached(); //TODO: Remove when server ready.
                }

                _clanProgressBarSlider.value = Mathf.Lerp(startPosition, endPosition, chunkProgress);
                break;
            }
        }

        yield return true;
    }

    private void CreateClanProgressBar()
    {
        var datas = TESTGenerateClanRewardsBar(); //TODO: Replace with data from server.

        foreach (var data in datas)
        {
            GameObject rewardMarker = Instantiate(_clanProgressBarMarkerPrefab, _clanProgressBarMarkersBase);
            rewardMarker.GetComponent<DailyTaskClanReward>().Set(data, this);
            _clanProgressBarMarkers.Add(rewardMarker);
        }
    }

    #endregion

    /// <summary>
    /// Shows <c>Popup</c> window and handles it's response.
    /// </summary>
    /// <param name="Message">Message to be shown in <c>Popup</c> window.</param>
    public IEnumerator ShowPopupAndHandleResponse(string Message, PopupData? data)
    {
        Popup.PopupWindowType windowType;
        bool? result = null;

        switch (data.Value.Type)
        {
            case PopupData.PopupDataType.OwnTask: windowType = Popup.PopupWindowType.Accept; break;
            case PopupData.PopupDataType.CancelTask: windowType = Popup.PopupWindowType.Cancel; break;
            case PopupData.PopupDataType.ClanMilestone: windowType = Popup.PopupWindowType.ClanMilestone; break;
            case PopupData.PopupDataType.MultipleChoice: windowType = Popup.PopupWindowType.MultipleChoice; break;
            default: windowType = Popup.PopupWindowType.Accept; break;
        }

        StartCoroutine(Popup.RequestPopup(Message, data.Value, OwnTaskId, windowType, data => result = data));

        yield return new WaitUntil(() => result != null);

        if (result == true && data != null)
        {
            bool? done = null;

            Debug.Log("Confirmed!");
            switch (data.Value.Type)
            {
                case PopupData.PopupDataType.OwnTask:
                    {
                        if (_currentPlayerData != null && _currentPlayerData.Task != null)
                        {
                            StartCoroutine(CancelTask(data => done = data));
                            yield return new WaitUntil(() => done != null);
                            done = null;
                        }

                        StartCoroutine(GetSaveSetHandleOwnTask(data.Value.OwnPage, data => done = data, data.Value.DailyTaskCardIndex.Value));
                        yield return new WaitUntil(() => (_currentPlayerData.Task != null || done != null));

                        if (_currentPlayerData.Task == null)
                            break;

                        SwitchTab(SelectedTab.OwnTask);
                        ShowMultipleChoiceTask();
                        break;
                    }
                case PopupData.PopupDataType.CancelTask:
                    {
                        StartCoroutine(CancelTask(data => done = data));
                        yield return new WaitUntil(() => done != null);

                        if (!done.Value)
                        {
                            Debug.LogError("No task to be cancelled.");
                            break;
                        }

                        SwitchTab(SelectedTab.Tasks);
                        //_ownTaskTabButton.interactable = false;
                        break;
                    }
                case PopupData.PopupDataType.ClanMilestone: break;
                case PopupData.PopupDataType.MultipleChoice:
                    {
                        gameObject.GetComponent<MultipleChoiceProgressListener>().UpdateProgressMultipleChoice(_currentPlayerData.Task);
                        break;
                    }
            }
        }
        else
        {
            Debug.Log("Cancelled Popup.");
            // Perform actions for cancellation
        }
    }

    /// <summary>
    /// Save given <c>PlayerTask</c> to <c>PlayerData</c> and update owntask page.
    /// </summary>
    /// <param name="playerTask"><c>PlayerData</c> to be set and saved to server as current task.</param>
    private IEnumerator GetSaveSetHandleOwnTask(PlayerTask playerTask, System.Action<bool> callback, int index)
    {
        PlayerData playerData = null;
        //PlayerData savePlayerData = null;
        PlayerTask reserveResult = null;
        bool? timeout = null;
        Coroutine coroutineTimeout;

        //Get player data.
        StartCoroutine(PlayerDataTransferer("get", null, _timeoutSeconds, tdata => timeout = tdata, pdata => playerData = pdata));
        yield return new WaitUntil(() => (playerData != null || timeout != null));

        if (playerData == null)
        {
            callback(false);
            yield break;
        }
        if (!playerTask.Offline)
        {
            StartCoroutine(ServerManager.Instance.ReservePlayerTaskFromServer(playerTask.Id, data => reserveResult = data));
            coroutineTimeout = StartCoroutine(WaitUntilTimeout(_timeoutSeconds, data => timeout = data));
            yield return new WaitUntil(() => (reserveResult != null || timeout != null));

            if (reserveResult == null)
            {
                Debug.LogError($"Failed to reserve task id: {playerTask.Id}");
                callback(false);
                yield break;
            }

            StopCoroutine(coroutineTimeout);
        }
        else reserveResult = playerTask;

        playerData.Task = reserveResult;
        _currentPlayerData = playerData;
        _dailyTaskCardSlots[index].GetComponent<DailyQuest>().SetTaskData(reserveResult);
        _currentTaskCardIndex = index;
        SetHandleOwnTask(reserveResult);
        callback(true);
    }

    /// <summary>
    /// Set OwnTask page.
    /// </summary>
    private void SetHandleOwnTask(PlayerTask playerTask)
    {
        DailyTaskProgressManager.Instance.ChangeCurrentTask(playerTask);
        _dailyTaskCardSlots[_currentTaskCardIndex].GetComponent<DailyQuest>().SetTaskData(playerTask);
        _ownTaskId = playerTask.Id;
        StartCoroutine(_ownTaskPageHandler.SetDailyTask(playerTask));
        _ownTaskPageHandler.SetTaskProgress((float)playerTask.TaskProgress / (float)playerTask.Amount);
        _ownTaskPageHandler.TESTSetTaskValue(playerTask.TaskProgress);
        Debug.Log("Task id: " + _ownTaskId + ", has been accepted.");
    }

    public void SwitchTab(SelectedTab tab)
    {
        //Hide old tab
        switch (_selectedTab)
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
            case SelectedTab.OwnTask: _ownTaskView.SetActive(true); break;
            default: _clanTaskView.SetActive(true); break;
        }
        _tabline.ActivateTabButton((int)_selectedTab);

        Debug.Log($"Switched to {_selectedTab}.");
    }
}
