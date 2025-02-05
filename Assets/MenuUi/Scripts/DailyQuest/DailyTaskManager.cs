using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts;
using UnityEngine;
using static Altzone.Scripts.Model.Poco.Game.PlayerTasks;
using UnityEngine.UI;
using Altzone.Scripts.Model.Poco.Clan;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Player;

public class DailyTaskManager : AltMonoBehaviour
{
    //Variables
    [SerializeField] private float _timeoutSeconds = 10;
    [Header("TabButtons")]
    [SerializeField] private Button _dailyTasksTabButton;
    [SerializeField] private Button _ownTaskTabButton;
    [SerializeField] private Button _clanTaskTabButton;

    private const int CardSlots = 100;
    private GameObject[] _dailyTaskCardSlots = new GameObject[CardSlots];

    [Header("DailyTaskCard prefabs")]
    [SerializeField] private GameObject _dailyTaskCard500Prefab;
    [SerializeField] private GameObject _dailyTaskCard1000Prefab;
    [SerializeField] private GameObject _dailyTaskCard1500Prefab;

    [Header("DailyTasksPage")]
    [SerializeField] private GameObject _dailyTasksView;
    [SerializeField] private Transform _dailyCategory500;
    [SerializeField] private Transform _dailyCategory1000;
    [SerializeField] private Transform _dailyCategory1500;

    [Header("OwnTaskPage")]
    [SerializeField] private GameObject _ownTaskView;
    [SerializeField] private Button _cancelTaskButton;
    [SerializeField] private DailyTaskOwnTask _ownTaskPageHandler;

    private int? _ownTaskId;
    public int? OwnTaskId { get { return _ownTaskId; } }

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

    //Local Testing
    private int _clanProgressBarGoal = 10000;
    private int _clanProgressBarCurrentPoints = 0;
    private PlayerData _currentPlayerData;

    public enum SelectedTab
    {
        Tasks,
        OwnTask,
        ClanTask
    }
    private SelectedTab _selectedTab = SelectedTab.Tasks;

    // Start of Code
    void Start()
    {
        TaskGenerator();
        StartCoroutine(GetSetExistingTask());
        StartCoroutine(PopulateClanPlayers());
        StartCoroutine(SetClanProgressBar());
        StartCoroutine(CreateClanProgressBar());

        //Tab bar
        _dailyTasksTabButton.onClick.AddListener(() => SwitchTab(SelectedTab.Tasks));
        _ownTaskTabButton.onClick.AddListener(() => SwitchTab(SelectedTab.OwnTask));
        _clanTaskTabButton.onClick.AddListener(() => SwitchTab(SelectedTab.ClanTask));

        //OwnTask cancel button
        _cancelTaskButton.onClick.AddListener(() => StartCancelTask());

        _ownTaskTabButton.interactable = false;

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

    private IEnumerator GetSetExistingTask()
    {
        //Get existing player task.
        PlayerData playerData = null;
        PlayerTask playerTask = null;
        bool? timeout = null;
        Coroutine timeoutCoroutine;

        //Get player data.
        StartCoroutine(PlayerDataTransferer("get", null, tdata => timeout = tdata, pdata => playerData = pdata));
        yield return new WaitUntil(() => (playerData != null || timeout != null));

        if (playerData == null)
            yield break; //TODO: Add error handling.

        //Check when daily task cards loaded.
        timeoutCoroutine = StartCoroutine(WaitUntilTimeout(_timeoutSeconds, data => timeout = data));
        yield return new WaitUntil(() => (_dailyTaskCardSlots[0] != null || timeout != null));

        if (playerData.Task == null)
        {
            Debug.Log("No existing task available in PlayerData.");
            yield break;
        }

        //Get task.
        StartCoroutine(GetTask(playerData.Task.Id, data => playerTask = data));

        yield return new WaitUntil(() => playerTask != null);

        if (playerTask == null)
        {
            Debug.Log($"Could not find task id: {playerData.Task.Id}");
            yield break;
        }

        _ownTaskTabButton.interactable = true;
        StartCoroutine(SetHandleOwnTask(playerTask));
        SwitchTab(SelectedTab.OwnTask);
    }

    // First 4 functions are for task slot population and fetching them from server
    public void TaskGenerator()
    {
        StartCoroutine(PopulateTasks(_dailyTaskCardSlots));

        Debug.Log("Task Slots populated!");
    }

    private IEnumerator PopulateTasks(GameObject[] taskSlots)
    {
        PlayerTasks tasks = null;
        Storefront.Get().GetPlayerTasks(content => tasks = content);
        List<PlayerTask> tasklist = null;
        Debug.Log(tasks);

        if (tasks == null)
        {
            StartCoroutine(ServerManager.Instance.GetPlayerTasksFromServer(content =>
            {
                if (content != null)
                    tasks = content;
                else
                {
                    //offline testing random generator with id generator
                    Debug.LogError("Could not connect to server and receive quests");
                    return;
                }
            }));
        }

        yield return new WaitUntil(() => tasks != null);

        //!Temporary until the PlayerTask is modified to have one Task list/array!
        tasklist = tasks.Daily;
        tasklist.AddRange(tasks.Week);
        tasklist.AddRange(tasks.Month);
        //-----------------------------------------------------------------------|

        for (int i = 0; i < tasklist.Count; i++)
        {
            GameObject taskObject = Instantiate(GetPrefabCategory(tasklist[i].Points), gameObject.transform);
            taskSlots[i] = taskObject;

            DailyQuest task = taskObject.GetComponent<DailyQuest>();
            task.GetQuestData(tasklist[i]);
            task.dailyTaskManager = this;

            Transform parentCategory = GetParentCategory(tasklist[i].Points);
            taskObject.transform.SetParent(parentCategory, false);
            taskObject.SetActive(true);

            Debug.Log("Created Quest: " +  tasklist[i].Id);
        }

        //Needed to update the instantiated DT cards spacing in HorizontalLayoutGroups.
        LayoutRebuilder.ForceRebuildLayoutImmediate(_dailyCategory500.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(_dailyCategory1000.GetComponent<RectTransform>());
        LayoutRebuilder.ForceRebuildLayoutImmediate(_dailyCategory1500.GetComponent<RectTransform>());

        //Sets DT cards to left side.
        _dailyCategory500.GetComponent<RectTransform>().anchoredPosition = new Vector2( int.MaxValue ,0f );
        _dailyCategory1000.GetComponent<RectTransform>().anchoredPosition = new Vector2( int.MaxValue ,0f );
        _dailyCategory1500.GetComponent<RectTransform>().anchoredPosition = new Vector2( int.MaxValue ,0f );
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

    private IEnumerator GetTask(int id, System.Action<PlayerTask> callback)
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

    private IEnumerator PopulateClanPlayers()
    {
        /*Commented code refering to getting PlayerData's from ClanData but
         *there is missing or wrong format data. (Waiting for server side update)*/

        #region

        //ClanData clan = null;
        //string clanId = null;
        //Storefront.Get().GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, p => clanId = p.ClanId);

        //if (clanId == null)
        //{
        //    StartCoroutine(ServerManager.Instance.GetPlayerFromServer(content =>
        //    {
        //        if (content != null)
        //            clanId = content.clan_id;
        //        else
        //        {
        //            Debug.LogError("Could not connect to server and receive PlayerData");
        //            return;
        //        }
        //    }));
        //}

        //timeoutCoroutine = StartCoroutine(WaitUntilTimeout(_timeoutSeconds, data => timeout = data));
        //yield return new WaitUntil(() => (playerData != null || timeout != null));

        //if (playerData == null)
        //{
        //    StopCoroutine(playerCoroutine);
        //    Debug.LogError($"Get player data timeout or null.");
        //    yield break; //TODO: Add error handling.
        //}
        //else
        //    StopCoroutine(timeoutCoroutine);

        //Storefront.Get().GetClanData(clanId, content => clan = content);

        //if (clan == null)
        //{
        //    StartCoroutine(ServerManager.Instance.GetClanFromServer(content =>
        //    {
        //        if (content != null)
        //            clan = content;
        //        else
        //        {
        //            //offline testing random generator with id generator
        //            Debug.LogError("Could not connect to server and receive quests");
        //            return;
        //        }
        //    }));
        //}

        //timeoutCoroutine = StartCoroutine(WaitUntilTimeout(_timeoutSeconds, data => timeout = data));
        //yield return new WaitUntil(() => (playerData != null || timeout != null));

        //if (playerData == null)
        //{
        //    StopCoroutine(playerCoroutine);
        //    Debug.LogError($"Get player data timeout or null.");
        //    yield break; //TODO: Add error handling.
        //}
        //else
        //    StopCoroutine(timeoutCoroutine);

        //PlayerData clanPlayer = null;
        //Storefront.Get().GetPlayerData(clan.Members[].PlayerDataId, cp => clanPlayer = cp);


        //if (clan == null)
        //{
        //    StartCoroutine(ServerManager.Instance.GetPlayerLeaderboardFromServer(content =>
        //    {
        //        if (content != null)
        //            clan = content;
        //        else
        //        {
        //            //offline testing random generator with id generator
        //            Debug.LogError("Could not connect to server and receive quests");
        //            return;
        //        }
        //    }));
        //}

        //timeoutCoroutine = StartCoroutine(WaitUntilTimeout(_timeoutSeconds, data => timeout = data));
        //yield return new WaitUntil(() => (playerData != null || timeout != null));

        //if (playerData == null)
        //{
        //    StopCoroutine(playerCoroutine);
        //    Debug.LogError($"Get player data timeout or null.");
        //    yield break; //TODO: Add error handling.
        //}
        //else
        //    StopCoroutine(timeoutCoroutine);

        #endregion

        //Testing code
        for (int i = 0; i < 30; i++)
        {
            GameObject player = Instantiate(_clanPlayerPrefab, _clanPlayersList);
            player.GetComponent<DailyTaskClanPlayer>().Set(i, null, null);

            _clanPlayers.Add(player);
            Debug.Log("Created clan player: " + i);
        }

        //Needed to update the instantiated DT cards spacing in HorizontalLayoutGroups.
        //LayoutRebuilder.ForceRebuildLayoutImmediate(_clanPlayersList);

        //Sets DT cards to left side.
        _clanPlayersList.anchoredPosition = new Vector2(0f, -500f);

        yield return true;
    }

    // Function for popup calling
    public IEnumerator ShowPopupAndHandleResponse(string Message, PopupData? data)
    {
        yield return Popup.RequestPopup(Message, result =>
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

                            PopupDataHandler(data.Value);
                            SwitchTab(SelectedTab.OwnTask);
                            _ownTaskTabButton.interactable = true;
                            break;
                        }
                    case PopupData.PopupDataType.CancelTask:
                        {
                            StartCoroutine(CancelTask());
                            SwitchTab(SelectedTab.Tasks);
                            _ownTaskTabButton.interactable = false;
                            break;
                        }
                }
            }
            else
            {
                Debug.Log("Cancelled Popup.");
                // Perform actions for cancellation
            }
        });
    }

    //Handle popup data.
    private void PopupDataHandler(PopupData data)
    {
        switch (data.Type)
        {
            case PopupData.PopupDataType.OwnTask: StartCoroutine(GetSaveSetHandleOwnTask(data.OwnPage)); break;
            default: break;
        }
    }

    //Save taskid & set OwnTask page.
    private IEnumerator GetSaveSetHandleOwnTask(PlayerTask playerTask)
    {
        PlayerData playerData = null;
        PlayerData savePlayerData = null;
        bool? timeout = null;

        //Get player data.
        StartCoroutine(PlayerDataTransferer("get", null, tdata => timeout = tdata, pdata => playerData = pdata));
        yield return new WaitUntil(() => (playerData != null || timeout != null));

        if (playerData == null)
            yield break; //TODO: Add error handling.

        //Save player data.
        playerData.Task = playerTask;
        timeout = null;

        StartCoroutine(PlayerDataTransferer("save", playerData, tdata => timeout = tdata, pdata => savePlayerData = pdata));
        yield return new WaitUntil(() => (savePlayerData != null || timeout != null));

        if (savePlayerData == null)
            yield break; //TODO: Add error handling.

        _currentPlayerData = savePlayerData;
        StartCoroutine(SetHandleOwnTask(playerTask));
    }

    //Set OwnTask page.
    private IEnumerator SetHandleOwnTask(PlayerTask playerTask)
    {
        DailyTaskProgressManager.Instance.UpdateCurrentTask(playerTask);
        _ownTaskId = playerTask.Id;
        _ownTaskPageHandler.SetDailyTask(playerTask.Content, playerTask.Amount, playerTask.Points, playerTask.Coins);
        _ownTaskPageHandler.SetTaskProgress(playerTask.TaskProgress);
        Debug.Log("Task id: " + _ownTaskId + ", has been accepted.");

        yield return true;
    }

    private IEnumerator GetPlayerData(System.Action<PlayerData> callback)
    {
        //Testing code------------//
        if (_currentPlayerData != null)
        {
            callback(_currentPlayerData);
            yield break;
        }
        //------------------------//

        Storefront.Get().GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, callback);

        if (callback == null)
        {
            StartCoroutine(ServerManager.Instance.GetPlayerFromServer(content =>
            {
                if (content != null)
                    callback(new(content));
                else
                {
                    //offline testing random generator with id generator
                    Debug.LogError("Could not connect to server and receive player");
                    return;
                }
            }));
        }

        yield return new WaitUntil(() => callback != null);
    }

    //TODO: Uncomment, remove testing code and fix bugs when server side ready!
    private IEnumerator SavePlayerData(PlayerData playerData , System.Action<PlayerData> callback)
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

    public void TESTAddTaskProgress()
    {
        //_ownTaskProgress++;
        _currentPlayerData.Task.AddProgress(1);

        foreach (GameObject obj in _dailyTaskCardSlots)
        {
            if (obj == null)
                continue;

            DailyQuest quest = obj.GetComponent<DailyQuest>();

            if (quest.TaskData.Id == _ownTaskId)
            {
                UpdateOwnTaskProgress();
                return;
            }
        }

        Debug.LogError($"Could not find task with id: {_ownTaskId}");
    }

    private void UpdateOwnTaskProgress()
    {
        //TODO: Replace with fetch from server when possible.
        var taskData = DailyTaskProgressManager.Instance.CurrentPlayerTask;
        
        float progress = CalculateProgressBar(_currentPlayerData.Task.Amount, taskData.TaskProgress);
        _ownTaskPageHandler.SetTaskProgress(progress);
        Debug.Log("Task id: " + _ownTaskId + ", current progress: " + progress);
        if (progress >= 1f)
        {
            Debug.Log("Task id:" + _ownTaskId + ", is done");
        }
    }

    private float CalculateProgressBar(int targetPoints, int currentPoints)
    {
        return ((float)currentPoints / (float)targetPoints);
    }

    // Calling popup for canceling task.
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
            yield break; //TODO: Add error handling.

        //Save player data.
        playerData.Task.ClearProgress();
        playerData.Task = null;
        timeout = null;

        StartCoroutine(PlayerDataTransferer("save", playerData, tdata => timeout = tdata, pdata => savePlayerData = pdata));
        yield return new WaitUntil(() => (savePlayerData != null || timeout != null));

        if (savePlayerData == null)
            yield break; //TODO: Add error handling.

        _currentPlayerData = savePlayerData;
        DailyTaskProgressManager.Instance.UpdateCurrentTask(savePlayerData.Task);
        _ownTaskPageHandler.ClearCurrentTask();
        Debug.Log("Task id: " + _ownTaskId + ", has been canceled.");
        _ownTaskId = null;
    }

    public void ClearCurrentTask()
    {
        _currentPlayerData.Task.ClearProgress();
        _ownTaskPageHandler.ClearCurrentTask();
        _ownTaskTabButton.interactable = false;
        SwitchTab(SelectedTab.Tasks);
        Debug.Log("Task id: " + _ownTaskId + ", has been cleard.");
        _ownTaskId = null;
    }

    //Switch tab.
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

    //Clan progress bar functions.
    private IEnumerator SetClanProgressBar()
    {
        //TODO: Get clan task data and fill the clan progress bar based on that data.
        string clanId = null;
        ClanData clan = null;
        Storefront.Get().GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, p => clanId = p.ClanId);

        if (clanId == null)
            StartCoroutine(ServerManager.Instance.GetPlayerFromServer(content =>
            {
                if (content != null)
                    clanId = content.clan_id;
                else
                {
                    Debug.LogError("Could not connect to server and receive player data");
                    return;
                }
            }));

        yield return new WaitUntil(() => clanId != null);

        Storefront.Get().GetClanData(clanId, c => clan = c);

        if (clan == null)
            StartCoroutine(ServerManager.Instance.GetClanFromServer(content =>
            {
                if (content != null)
                    clan = new(content);
                else
                {
                    Debug.LogError("Could not connect to server and receive player data");
                    return;
                }
            }));

        yield return new WaitUntil(() => clan != null);

        _clanProgressBarSlider.value = CalculateProgressBar(_clanProgressBarGoal, clan.Points);
    }

    private List<DailyTaskClanReward.ClanRewardData> TESTGenerateClanRewardsBar()
    {
        var clanRewardDatas = new List<DailyTaskClanReward.ClanRewardData>()
        {
            new DailyTaskClanReward.ClanRewardData(false, DailyTaskClanReward.ClanRewardType.Box, 500),
            new DailyTaskClanReward.ClanRewardData(false, DailyTaskClanReward.ClanRewardType.Box, 1000),
            new DailyTaskClanReward.ClanRewardData(false, DailyTaskClanReward.ClanRewardType.Box, 5000),
            new DailyTaskClanReward.ClanRewardData(false, DailyTaskClanReward.ClanRewardType.Chest, 10000)
        };
        return clanRewardDatas;
    }

    public void TESTAddClanRewardBarPoints(int value)
    {
        _clanProgressBarCurrentPoints += value;

        StartCoroutine(CalculateClanRewardBarProgress());
    }

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
                    _clanProgressBarMarkers[j].GetComponent<DailyTaskClanReward>().UpdateState(true);
                }

                //Final reward
                if ((i >= _clanProgressBarMarkers.Count - 1) && chunkProgress == 1)
                {
                    _clanProgressBarMarkers[_clanProgressBarMarkers.Count - 1].GetComponent<DailyTaskClanReward>().UpdateState(true);
                }

                _clanProgressBarSlider.value = Mathf.Lerp(startPosition, endPosition, chunkProgress);
                break;
            }
        }

        yield return true;
    }

    private IEnumerator CreateClanProgressBar()
    {
        //TODO: Replace with data from server.
        var datas = TESTGenerateClanRewardsBar();

        foreach (var data in datas)
        {
            GameObject rewardMarker = Instantiate(_clanProgressBarMarkerPrefab, _clanProgressBarMarkersBase);
            rewardMarker.GetComponent<DailyTaskClanReward>().Set(data);
            _clanProgressBarMarkers.Add(rewardMarker);
        }
        yield return true;
    }

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
