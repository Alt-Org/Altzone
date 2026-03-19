using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
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

    private PlayerData _currentPlayerData;
    

    private string _ownTaskId;
    public string OwnTaskId { get { return _ownTaskId; } set { _ownTaskId = value; } }

    private ClanTasks _validTasks;

    public ClanTasks ValidTasks { get { return _validTasks; } }


    #endregion


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            //DontDestroyOnLoad(this);
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

        PlayerData playerData = null;
        
        StartCoroutine(GetPlayerData(content => playerData = content)); //MQTT message tells if we need to fetch the data again.

        if (playerData == null || !playerData.HasClanId)
        //if (_currentPlayerData == null || !_currentPlayerData.HasClanId)
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
        Debug.Log("Task id: " + _ownTaskId + ", has been canceled.");
        _ownTaskId = null;
        done(true);
    }

    public void ClearCurrentTask()
    {
        _currentPlayerData.Task.ClearProgress();
        Debug.Log("Task id: " + _ownTaskId + ", has been cleard.");
        _ownTaskId = null;
    }
}
