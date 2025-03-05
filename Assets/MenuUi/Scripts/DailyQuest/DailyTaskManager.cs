using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts;
using UnityEngine;
using UnityEngine.UI;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Model.Poco.Player;

public class DailyTaskManager : AltMonoBehaviour
{
    [Tooltip("Maximum time until a get or save data operation is forced to quit.")]
    [SerializeField] private float _timeoutSeconds = 10;
    [SerializeField] private TabButtonsVisualController _tabButtonsVisualController;

    private PlayerData _currentPlayerData;

    [Header("TabButtons")]
    [SerializeField] private Button _dailyTasksTabButton;
    [SerializeField] private Button _ownTaskTabButton;
    [SerializeField] private Button _clanTaskTabButton;

    private List<GameObject> _dailyTaskCardSlots = new List<GameObject>();

    [Header("DailyTaskCard prefabs")]
    [SerializeField] private GameObject _dailyTaskCard500Prefab;
    [SerializeField] private GameObject _dailyTaskCard1000Prefab;
    [SerializeField] private GameObject _dailyTaskCard1500Prefab;

    [Header("DailyTasksPage")]
    [SerializeField] private GameObject _dailyTasksView;
    [SerializeField] private Transform _dailyCategory500;
    [SerializeField] private Transform _dailyCategory1000;
    [SerializeField] private Transform _dailyCategory1500;
    [SerializeField] private RectTransform _tasksVerticalLayout;

    [Header("OwnTaskPage")]
    [SerializeField] private GameObject _ownTaskView;
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

    private string _ownTaskId;
    public string OwnTaskId { get { return _ownTaskId; } }

    [Header("ClanTaskPage")]
    [SerializeField] private GameObject _clanTaskView;
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

    void Start()
    {
        //DailyTask page setup
        StartCoroutine(DataSetup());

        _tabButtonsVisualController.UpdateButton(_dailyTasksTabButton);

        //Buttons
        _dailyTasksTabButton.onClick.AddListener(() => SwitchTab(SelectedTab.Tasks));
        _ownTaskTabButton.onClick.AddListener(() => SwitchTab(SelectedTab.OwnTask));
        _clanTaskTabButton.onClick.AddListener(() => SwitchTab(SelectedTab.ClanTask));

        _cancelTaskButton.onClick.AddListener(() => StartCancelTask());

        _ownTaskTabButton.interactable = false;

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

        StartCoroutine(PopulateTasks());
        yield return new WaitUntil(() => _dailyTaskCardSlots.Count != 0);

        StartCoroutine(PlayerDataTransferer("get", null, data => timeout = data, data => _currentPlayerData = data));
        yield return new WaitUntil(() => (_currentPlayerData != null || timeout != null));

        if (_currentPlayerData == null)
        {
            Debug.LogError("Failed to fetch player data.");
            yield break;
        }

        StartCoroutine(GetSetExistingTask());

        //Get clan data.
        ClanData clanData = null;

        Storefront.Get().GetClanData(_currentPlayerData.ClanId, data => clanData = data);

        if (clanData == null)
        {
            StartCoroutine(ServerManager.Instance.GetClanFromServer(content =>
            {
                if (content != null)
                    clanData = new(content);
                else
                {
                    Debug.LogError("Could not connect to server and receive player");
                    return;
                }
            }));
        }

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

    private IEnumerator PopulateTasks()
    {
        List<PlayerTask> tasklist = null;
        Storefront.Get().GetPlayerTasks(content => tasklist = content);

        StartCoroutine(ServerManager.Instance.GetPlayerTasksFromServer(content =>
        {
            if (content != null)
                tasklist = content;
            else
            {
                Debug.LogError("Could not connect to server and receive quests.");
                //Offline testing
                tasklist = TESTGenerateTasks();
                Debug.LogWarning("Using locally generated tasks.");
            }
        }));

        yield return new WaitUntil(() => tasklist != null);

        for (int i = 0; i < tasklist.Count; i++)
        {
            GameObject taskObject = Instantiate(GetPrefabCategory(tasklist[i].Points), gameObject.transform);
            _dailyTaskCardSlots.Add(taskObject);

            DailyQuest task = taskObject.GetComponent<DailyQuest>();
            task.SetTaskData(tasklist[i]);
            task.dailyTaskManager = this;

            Transform parentCategory = GetParentCategory(tasklist[i].Points);
            taskObject.transform.SetParent(parentCategory, false);
            taskObject.SetActive(true);

            Debug.Log("Created Quest: " + tasklist[i].Id);
        }

        //Needed to update the instantiated DT cards spacing in HorizontalLayoutGroups.
        LayoutRebuilder.ForceRebuildLayoutImmediate(_dailyCategory500.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(_dailyCategory1000.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(_dailyCategory1500.GetComponent<RectTransform>());

        //Sets DT cards to left side.
        _dailyCategory500.GetComponent<RectTransform>().anchoredPosition = new Vector2(int.MaxValue, 0f);
        _dailyCategory1000.GetComponent<RectTransform>().anchoredPosition = new Vector2(int.MaxValue, 0f);
        _dailyCategory1500.GetComponent<RectTransform>().anchoredPosition = new Vector2(int.MaxValue, 0f);

        //Sets DT card category list to the top.
        _tasksVerticalLayout.anchoredPosition = new Vector2(0f, -int.MaxValue);
    }

    private List<PlayerTask> TESTGenerateTasks() //TODO: Remove when fetching tasks from server is stable.
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
        return (tasklist);
    }

    private Transform GetParentCategory(int points)
    {
        return points switch
        {
            <= 500 => _dailyCategory500,
            <= 1000 => _dailyCategory1000,
            _ => _dailyCategory1500,
        };
    }

    private GameObject GetPrefabCategory(int points)
    {
        return points switch
        {
            <= 500 => _dailyTaskCard500Prefab,
            <= 1000 => _dailyTaskCard1000Prefab,
            _ => _dailyTaskCard1500Prefab,
        };
    }

    /// <summary>
    /// Tries to get an existing task from <c>PlayerData</c><br/>
    /// and if successful, sets it to the owntask page.
    /// </summary>
    private IEnumerator GetSetExistingTask()
    {
        PlayerTask playerTask = null;
        bool? timeout = null;

        if (_currentPlayerData.Task == null)
        {
            Debug.Log($"No current task in player data.");
            yield break;
        }

        //Get task.
        StartCoroutine(GetTask(_currentPlayerData.Task.Id, data => playerTask = data));
        StartCoroutine(WaitUntilTimeout(_timeoutSeconds, data => timeout = data));
        yield return new WaitUntil(() => (playerTask != null || timeout != null));

        if (playerTask == null)
        {
            Debug.Log($"Could not find task id: {_currentPlayerData.Task.Id}");
            yield break;
        }

        _ownTaskTabButton.interactable = true;
        SetHandleOwnTask(playerTask);
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
        if (_currentPlayerData.Task.Type == TaskType.StartBattleDifferentCharacter)
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
        taskData.InvokeOnTaskUpdated();
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

    private IEnumerator CancelTask()
    {
        PlayerData playerData = null;
        PlayerData savePlayerData = null;
        bool? timeout = null;

        //Get player data.
        StartCoroutine(PlayerDataTransferer("get", null, tdata => timeout = tdata, pdata => playerData = pdata));
        yield return new WaitUntil(() => (playerData != null || timeout != null));

        if (playerData == null)
            yield break;

        //Save player data.
        playerData.Task.ClearProgress();
        playerData.Task.ClearPlayerId();
        playerData.Task.InvokeOnTaskDeselected();
        playerData.Task = null;
        timeout = null;

        StartCoroutine(PlayerDataTransferer("save", playerData, tdata => timeout = tdata, pdata => savePlayerData = pdata));
        yield return new WaitUntil(() => (savePlayerData != null || timeout != null));

        if (savePlayerData == null)
            yield break;

        _currentPlayerData = savePlayerData;
        DailyTaskProgressManager.Instance.ChangeCurrentTask(savePlayerData.Task);
        _ownTaskPageHandler.ClearCurrentTask();
        Debug.Log("Task id: " + _ownTaskId + ", has been canceled.");
        _ownTaskId = null;
    }

    public void ClearCurrentTask()
    {
        _tabButtonsVisualController.UpdateButton(_dailyTasksTabButton);
        UpdateAvatarMood();
        _currentPlayerData.Task.ClearProgress();
        _ownTaskPageHandler.ClearCurrentTask();
        _ownTaskTabButton.interactable = false;
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

        switch (data.Value.Type)
        {
            case PopupData.PopupDataType.OwnTask: windowType = Popup.PopupWindowType.Accept; break;
            case PopupData.PopupDataType.CancelTask: windowType = Popup.PopupWindowType.Cancel; break;
            case PopupData.PopupDataType.ClanMilestone: windowType = Popup.PopupWindowType.ClanMilestone; break;
            default: windowType = Popup.PopupWindowType.Accept; break;
        }

        yield return Popup.RequestPopup(Message, data.Value.ClanRewardData, windowType, data.Value.Location, result =>
        {
            if (result == true && data != null)
            {
                Debug.Log("Confirmed!");
                switch(data.Value.Type)
                {
                    case PopupData.PopupDataType.OwnTask:
                        {
                            if (_currentPlayerData != null && _currentPlayerData.Task != null)
                                StartCoroutine(CancelTask());

                            _tabButtonsVisualController.UpdateButton(_ownTaskTabButton);
                            StartCoroutine(GetSaveSetHandleOwnTask(data.Value.OwnPage));
                            SwitchTab(SelectedTab.OwnTask);
                            //_ownTaskTabButton.interactable = true;
                            break;
                        }
                    case PopupData.PopupDataType.CancelTask:
                        {
                            StartCoroutine(CancelTask());
                            _tabButtonsVisualController.UpdateButton(_dailyTasksTabButton);
                            SwitchTab(SelectedTab.Tasks);
                            _ownTaskTabButton.interactable = false;
                            break;
                        }
                    case PopupData.PopupDataType.ClanMilestone: break;
                }
            }
            else
            {
                Debug.Log("Cancelled Popup.");
                // Perform actions for cancellation
            }
        });
    }

    /// <summary>
    /// Save given <c>PlayerTask</c> to <c>PlayerData</c> and update owntask page.
    /// </summary>
    /// <param name="playerTask"><c>PlayerData</c> to be set and saved to server as current task.</param>
    private IEnumerator GetSaveSetHandleOwnTask(PlayerTask playerTask)
    {
        PlayerData playerData = null;
        PlayerData savePlayerData = null;
        bool? timeout = null;

        //Get player data.
        StartCoroutine(PlayerDataTransferer("get", null, tdata => timeout = tdata, pdata => playerData = pdata));
        yield return new WaitUntil(() => (playerData != null || timeout != null));

        if (playerData == null)
            yield break;

        //Save player data.
        playerData.Task = playerTask;
        playerData.Task.AddPlayerId(playerData.Id);
        timeout = null;

        StartCoroutine(PlayerDataTransferer("save", playerData, tdata => timeout = tdata, pdata => savePlayerData = pdata));
        yield return new WaitUntil(() => (savePlayerData != null || timeout != null));

        if (savePlayerData == null)
            yield break;

        _currentPlayerData = savePlayerData;
        playerTask.InvokeOnTaskSelected();
        SetHandleOwnTask(playerTask);
    }

    /// <summary>
    /// Set OwnTask page.
    /// </summary>
    private void SetHandleOwnTask(PlayerTask playerTask)
    {
        DailyTaskProgressManager.Instance.ChangeCurrentTask(playerTask);
        _ownTaskId = playerTask.Id;
        _ownTaskPageHandler.SetDailyTask(playerTask.Title, playerTask.Amount, playerTask.Points, playerTask.Coins);
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

        Debug.Log($"Switched to {_selectedTab}.");
    }

    private IEnumerator SavePlayerData(PlayerData playerData , System.Action<PlayerData> callback) //TODO: Remove when available in AltMonoBehaviour.
    {
        //Cant' save to server because server manager doesn't have functionality!
        //Storefront.Get().SavePlayerData(playerData, callback);

        //if (callback == null)
        //{
        //    StartCoroutine(ServerManager.Instance.UpdatePlayerToServer( playerData., content =>
        //    {
        //        if (content != null)
        //            callback(new(content));
        //        else
        //        {
        //            //offline testing random generator with id generator
        //            Debug.LogError("Could not connect to server and save player");
        //            return;
        //        }
        //    }));
        //}

        //yield return new WaitUntil(() => callback != null);

        //Testing code
        callback(playerData);

        yield return true;
    }

    /// <summary>
    /// Used to get and save player data to/from server.
    /// </summary>
    /// <param name="operationType">"get" or "save"</param>
    /// <param name="unsavedData">If saving: insert unsaved data.<br/> If getting: insert <c>null</c>.</param>
    /// <param name="timeoutCallback">Returns value if timeout with server.</param>
    /// <param name="dataCallback">Returns <c>PlayerData</c>.</param>
    private IEnumerator PlayerDataTransferer(string operationType, PlayerData unsavedData, System.Action<bool> timeoutCallback, System.Action<PlayerData> dataCallback)
    {
        PlayerData receivedData = null;
        bool? timeout = null;
        Coroutine playerCoroutine;

        switch (operationType.ToLower())
        {
            case "get":
                {
                    //Get player data.
                    playerCoroutine = StartCoroutine(CoroutineWithTimeout(GetPlayerData, receivedData, _timeoutSeconds, timeoutCallBack => timeout = timeoutCallBack, data => receivedData = data));
                    break;
                }
            case "save":
                {
                    //Save player data.
                    playerCoroutine = StartCoroutine(CoroutineWithTimeout(SavePlayerData, unsavedData, receivedData, _timeoutSeconds, timeoutCallBack => timeout = timeoutCallBack, data => receivedData = data));
                    break;
                }
            default: Debug.LogError($"Received: {operationType}, when expecting \"get\" or \"save\"."); yield break;
        }

        yield return new WaitUntil(() => (receivedData != null || timeout != null));

        if (receivedData == null)
        {
            timeoutCallback(true);
            Debug.LogError($"Player data operation: {operationType} timeout or null.");
            yield break; //TODO: Add error handling.
        }

        dataCallback(receivedData);
    }
}
