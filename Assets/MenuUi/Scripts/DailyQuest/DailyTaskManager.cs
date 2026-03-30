using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using UnityEngine;

public class DailyTaskManager : AltMonoBehaviour
{

    public static DailyTaskManager Instance { get; private set; }

    private bool _dataReady = false;
    public bool DataReady { get { return _dataReady; } }

    #region Variables

    [Tooltip("Maximum time until a get or save data operation is forced to quit.")]
    [SerializeField] private float _timeoutSeconds = 10;
    [HideInInspector] public float TimeoutSeconds { get { return _timeoutSeconds;  } }

    private PlayerData _currentPlayerData = null;
    

    private string _ownTaskId;
    public string OwnTaskId { get { return _ownTaskId; } set { _ownTaskId = value; } }

    private ClanTasks _validTasks;

    public ClanTasks ValidTasks { get { return _validTasks; } }

    private DailyTaskView _dailyTaskView;

    public delegate void StopTask();
    public static event StopTask OnCancelTask;

    public delegate void StartTask();
    public static event StartTask OnAcceptTask;

    public delegate void MultipleChoiceProgress();
    public static event MultipleChoiceProgress OnMultipleChoiceProgress;

    private PlayerTask _currentTask;


    #endregion


    private void Awake()
    {
        // Only one DailyTaskManager Instance can exist at a time
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }


    void Start()
    {

        StartCoroutine(DataSetup());
    }

    /// <summary>
    /// DailyTask data setup
    /// </summary>
    /// <returns></returns>
    private IEnumerator DataSetup()
    {
        bool? timeout = null;
        bool? dailyTasksReady = null;

        StartCoroutine(PlayerDataTransferer("get", null, _timeoutSeconds, data => timeout = data, data => _currentPlayerData = data));
        yield return new WaitUntil(() => (_currentPlayerData != null || timeout != null));

        StartCoroutine(PopulateTasks(data => dailyTasksReady = data));
        yield return new WaitUntil(() => dailyTasksReady != null);

        //Debug.LogWarning("DATA SETUP");
        if (_currentPlayerData == null)
        {
            Debug.LogError("Failed to fetch player data.");
            yield break;
        }

        _dataReady = true;

    }

    #region Tasks

    private IEnumerator PopulateTasks(System.Action<bool> callback)
    {
        var gameVersion = GameConfig.Get().GameVersionType;

        ClanTasks clanTasks = null;

        
        StartCoroutine(GetPlayerData(content => _currentPlayerData = content)); //MQTT message tells if we need to fetch the data again.

        if (_currentPlayerData == null || !_currentPlayerData.HasClanId)
        {
            if (gameVersion is VersionType.Education or VersionType.TurboEducation)
                clanTasks = GenerateEducationTasks();
            else
                clanTasks = TESTGenerateNormalTasks();
        }
        else
        {
            StartCoroutine(ServerManager.Instance.GetPlayerTasksFromServer(content =>
            {
                if (content != null)
                {
                    // This is used to get playertasks, because the tasks on the server are incorrect
                    Storefront.Get().GetPlayerTasks(content => clanTasks = content);

                    //clanTasks = content; // Uncomment when it's possible to get proper info from the server
                }
                else
                {
                    Debug.LogError("Could not connect to server and receive quests.");
                    //Offline testing
                    if (gameVersion is VersionType.Education or VersionType.TurboEducation)
                        clanTasks = GenerateEducationTasks();
                    else
                        clanTasks = TESTGenerateNormalTasks();
                    Debug.LogWarning("Using locally generated tasks.");
                }
            }));
        }

        yield return new WaitUntil(() => clanTasks != null);

        ClanTasks referenceTasks = gameVersion is VersionType.Education or VersionType.TurboEducation ? GenerateEducationTasks() : GenerateNormalTasks();
        ClanTasks validatedTasks = gameVersion is VersionType.Education or VersionType.TurboEducation ? new(TaskVersionType.Education, new()) : new(TaskVersionType.Normal, new());
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
                case VersionType.Education:
                case VersionType.TurboEducation:
                    validatedTasks = GenerateEducationTasks(); break;
                case VersionType.Standard:
                    validatedTasks = GenerateNormalTasks(); break;
            }
        }
        // Store validated tasks
        _validTasks = validatedTasks;

        // Find a matching task from playerdata with a validated task, if it exists
        for (int i = 0; i < validatedTasks.Tasks.Count; i++)
        {

            PlayerTask task = validatedTasks.Tasks[i];

            if (_currentPlayerData.Task != null && _currentPlayerData.Task.Id == task.Id) _currentTask = task;

            if (_currentPlayerData.Id == task.PlayerId)
            {
                _currentPlayerData.Task = task; //TODO: Remove when fetching task data works.
                _currentTask = task;
            }
            Debug.Log("Created Task: " + task.Id);
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

    public void TESTAddTaskProgress()
    {
        if (_currentPlayerData.Task.Type == TaskNormalType.StartBattleDifferentCharacter)
            this.GetComponent<DailyTaskProgressListener>().UpdateProgress($"{System.DateTime.Now}");
        else
            this.GetComponent<DailyTaskProgressListener>().UpdateProgress("1");
    }

    #endregion

    public void SetCurrentPlayerData(PlayerData playerData)
    {
        bool? timeout = null;
        StartCoroutine(PlayerDataTransferer("save", playerData, _timeoutSeconds, data => timeout = data, data => _currentPlayerData = data));
    }
    
    public PlayerData GetCurrentPlayerData()
    {
        return _currentPlayerData;
    }

    public IEnumerator GetNewPlayerData(System.Action<PlayerData> callback, System.Action<bool> failed)
    {
        PlayerData playerData = null;
        bool? timeout = null;

        //Get player data.
        StartCoroutine(PlayerDataTransferer("get", null, _timeoutSeconds, tdata => timeout = tdata, pdata => playerData = pdata));
        yield return new WaitUntil(() => (playerData != null || timeout != null));

        failed(playerData == null);
        callback(playerData);
        yield break;
    }

    public IEnumerator CancelTask(System.Action<bool> done)
    {
        PlayerData playerData = _currentPlayerData;
        PlayerData savePlayerData = null;
        bool? unreserveResult = null;
        bool? timeout = null;
        Coroutine coroutineTimeout;

        // This is commented out for now, because I don't see why get the playerdata again if we already have _currentPlayerData
        //Get player data.
        //StartCoroutine(PlayerDataTransferer("get", null, _timeoutSeconds, tdata => timeout = tdata, pdata => playerData = pdata));
        //yield return new WaitUntil(() => (playerData != null || timeout != null));

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

        playerData.Task = null;
        timeout = null;


        StartCoroutine(PlayerDataTransferer("save", playerData, _timeoutSeconds, tdata => timeout = tdata, pdata => savePlayerData = pdata));
        yield return new WaitUntil(() => (savePlayerData != null || timeout != null));

        if (savePlayerData == null)
        {
            done(false);
            yield break;
        }

        _currentTask = null;
        _currentPlayerData = savePlayerData;
        DailyTaskProgressManager.Instance.ChangeCurrentTask(savePlayerData.Task);
        Debug.Log("Task id: " + _ownTaskId + ", has been canceled.");
        _ownTaskId = null;
        done(true);
    }

    public void ClearCurrentTask()
    {
        _currentPlayerData.Task.ClearProgress();
        Debug.Log("Task id: " + _ownTaskId + ", has been cleard.");
        _ownTaskId = null;
        _currentTask = null;
    }

    /// <summary>
    /// Calls ShowPopupAndHandleResponseCoroutine which
    /// shows <c>Popup</c> window and handles it's response.
    /// </summary>
    /// <param name="Message">Message to be shown in <c>Popup</c> window.</param>
    public void ShowPopupAndHandleResponse(string Message, PopupData? data) => StartCoroutine(ShowPopupAndHandleResponseCoroutine(Message, data));

    /// <summary>
    /// Shows <c>Popup</c> window and handles it's response.
    /// </summary>
    /// <param name="Message">Message to be shown in <c>Popup</c> window.</param>
    public IEnumerator ShowPopupAndHandleResponseCoroutine(string Message, PopupData? data)
    {
        Popup.PopupWindowType windowType;
        Popup.ResultType result = Popup.ResultType.Null;

        switch (data.Value.Type)
        {
            case PopupData.PopupDataType.OwnTask: windowType = Popup.PopupWindowType.Accept; break;
            case PopupData.PopupDataType.CancelTask: windowType = Popup.PopupWindowType.Cancel; break;
            case PopupData.PopupDataType.ClanMilestone: windowType = Popup.PopupWindowType.ClanMilestone; break;
            case PopupData.PopupDataType.MultipleChoice: windowType = Popup.PopupWindowType.MultipleChoice; break;
            default: windowType = Popup.PopupWindowType.Accept; break;
        }

        StartCoroutine(Popup.RequestPopup(Message, data.Value, _ownTaskId, windowType, data => result = data));

        yield return new WaitUntil(() => result != Popup.ResultType.Null);

        if (result == Popup.ResultType.Accept && data != null)
        {
            bool? done = null;

            switch (data.Value.Type)
            {
                case PopupData.PopupDataType.OwnTask:
                    {
                        if (_currentPlayerData != null && _currentPlayerData.Task != null)
                        {
                            Debug.LogWarning("Found task!");
                            StartCoroutine(CancelTask(data2 => done = data2));
                            yield return new WaitUntil(() => done != null);
                            done = null;
                        }

                        StartCoroutine(GetSaveSetHandleOwnTask(data.Value.OwnPage, data2 => done = data2));
                        yield return new WaitUntil(() => (_currentPlayerData.Task != null || done != null));

                        if (_currentPlayerData.Task == null)
                        {
                            Debug.LogWarning("PlayerDataTask is null");
                            break;
                        }

                        OnAcceptTask?.Invoke();

                        ShowMultipleChoiceTask();

                        break;
                    }
                case PopupData.PopupDataType.CancelTask:
                    {
                        StartCoroutine(CancelTask(data2 => done = data2));
                        yield return new WaitUntil(() => done != null);   

                        if (!done.Value)
                        {
                            Debug.LogError("No task to be cancelled.");
                            break;
                        }

                        OnCancelTask?.Invoke();

                        break;
                    }
                case PopupData.PopupDataType.ClanMilestone: break;
                case PopupData.PopupDataType.MultipleChoice:
                    {
                        // This has to be changed to a better getter
                        //gameObject.GetComponentInParent<MultipleChoiceProgressListener>().UpdateProgressMultipleChoice(playerData.Task);

                        OnMultipleChoiceProgress?.Invoke();
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

    public void ShowMultipleChoiceTask()
    {
        PlayerData playerData = _currentPlayerData;

        if (playerData.Task == null || !MultipleChoiceOptions.Instance.IsMultipleChoice(playerData.Task)) return;
        PopupData data = new(playerData.Task);
        ShowPopupAndHandleResponse(playerData.Task.Content, data);
    }

    /// <summary>
    /// Save given <c>PlayerTask</c> to <c>PlayerData</c> and update owntask page.
    /// </summary>
    /// <param name="playerTask"><c>PlayerData</c> to be set and saved to server as current task.</param>
    public IEnumerator GetSaveSetHandleOwnTask(PlayerTask playerTask, System.Action<bool> callback)
    {
        PlayerData playerData = _currentPlayerData;
        PlayerTask reserveResult = null;
        bool failed = false;

        StartCoroutine(DailyTaskManager.Instance.GetNewPlayerData(pdata => playerData = pdata, faildata => failed = faildata));
        yield return new WaitUntil(() => (playerData != null || failed));

        if (playerData == null)
        {
            callback(false);
            yield break;
        }

        if (!playerTask.Offline)
        {
            bool? timeout = null;
            Coroutine coroutineTimeout;
            StartCoroutine(ServerManager.Instance.ReservePlayerTaskFromServer(playerTask.Id, data => reserveResult = data));
            coroutineTimeout = StartCoroutine(WaitUntilTimeout(DailyTaskManager.Instance.TimeoutSeconds, data => timeout = data));
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
        DailyTaskManager.Instance.SetCurrentPlayerData(playerData);
        SetHandleOwnTask(reserveResult);
        callback(true);
    }

    /// <summary>
    /// Update OwnTask
    /// </summary>
    public void SetHandleOwnTask(PlayerTask playerTask)
    {
        DailyTaskProgressManager.Instance.ChangeCurrentTask(playerTask);
        _ownTaskId = playerTask.Id;
        _currentTask = playerTask;
        
        Debug.Log("Task id: " + DailyTaskManager.Instance.OwnTaskId + ", has been accepted.");
        
    }

    public PlayerTask GetCurrentTask()
    {
        return _currentTask;
    }

    /*public IEnumerator AcceptTask(PlayerTask playerTask, System.Action<bool> callback)
    {
        bool? done = null;

        PlayerData playerData = DailyTaskManager.Instance.GetCurrentPlayerData();

        if (playerData != null && playerData.Task != null)
        {
            StartCoroutine(CancelTask(data => done = data));
            yield return new WaitUntil(() => done != null);
            done = null;
        }

        StartCoroutine(DailyTaskManager.Instance.GetSaveSetHandleOwnTask(playerTask, data => done = data));
        yield return new WaitUntil(() => done != null);

        Debug.LogWarning("DONE VALUE: " + done.Value + " | " + done);
        if (done.Value)
            OnAcceptTask?.Invoke();

        if (callback != null)
            callback(done.Value);
    }*/
}
