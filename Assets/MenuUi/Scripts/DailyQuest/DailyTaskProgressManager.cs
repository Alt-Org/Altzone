using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using UnityEngine;
using Altzone.Scripts.Model.Poco.Clan;

/// <summary>
/// Controlls and monitors players progress on a selected daily task.
/// </summary>
public class DailyTaskProgressManager : AltMonoBehaviour
{
    public static DailyTaskProgressManager Instance { get; private set; }

    [HideInInspector] public PlayerTask CurrentPlayerTask { get; private set; }

    [Tooltip("Maximum time until a get or save data operation is forced to quit.")]
    [SerializeField] private float _timeoutSeconds = 10;

    private List<string> _previousTaskStrings = new List<string>();

    #region Delegates & Events

    public delegate void TaskChange(TaskType taskType);
    /// <summary>
    /// Used to update existing <c>DailyTaskProgressListener</c>'s on/off states.
    /// </summary>
    public static event TaskChange OnTaskChange;

    public delegate void TaskProgressed();
    /// <summary>
    /// Used to notify <c>DailyTaskManager</c> when task has progressed.
    /// </summary>
    public static event TaskProgressed OnTaskProgressed;

    public delegate void TaskDone();
    /// <summary>
    /// Used to clear <c>DailyTaskManager</c> from a completed daily task.
    /// </summary>
    public static event TaskDone OnTaskDone;

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
            DontDestroyOnLoad(this);
        }
    }

    private void Start()
    {
        StartCoroutine(Setup());
    }

    private IEnumerator Setup()
    {
        PlayerData playerData = null;
        bool? timeout = null;
        StartCoroutine(PlayerDataTransferer("get", null, tdata => timeout = tdata, pdata => playerData = pdata));
        yield return new WaitUntil(() => (playerData != null || timeout != null));

        if (playerData == null)
            yield break; //TODO: Add error handling.

        CurrentPlayerTask = playerData.Task;
    }

    //TODO: Remove when available in AltMonoBehaviour.
    private IEnumerator SavePlayerData(PlayerData playerData, System.Action<PlayerData> callback)
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

    #region Task Processing

    public void UpdateTaskProgress(TaskType taskType, string value)
    {
        if ((taskType != CurrentPlayerTask.Type) && (taskType != TaskType.Test))
        {
            Debug.LogError($"Current task type is: {CurrentPlayerTask.Type}, but type: {taskType}, was received.");
            return;
        }

        switch (CurrentPlayerTask.Type)
        {
            case TaskType.PlayBattle: HandleSimpleTask(value); break;
            case TaskType.WinBattle: HandleSimpleTask(value); break;
            case TaskType.StartBattleDifferentCharacter: HandleNoRepetitionTask(value); break;
            case TaskType.Vote: HandleSimpleTask(value); break;
            case TaskType.WriteChatMessage: HandleSimpleTask(value); break;
            default: break;
        }
    }

    public void ChangeCurrentTask(PlayerTask task)
    {
        if (CurrentPlayerTask != task)
        {
            _previousTaskStrings.Clear();
        }

        CurrentPlayerTask = task;

        if (OnTaskChange != null)
        {
            if (CurrentPlayerTask != null)
                OnTaskChange.Invoke(CurrentPlayerTask.Type);
            else
                OnTaskChange.Invoke(TaskType.Undefined);
        }
    }

    public bool SameTask(TaskType taskType)
    {
        if (CurrentPlayerTask == null)
            return false;

        if (taskType == TaskType.Test)
            return (true);

        return (taskType == CurrentPlayerTask.Type);
    }

    private void HandleSimpleTask(string value)
    {
        try
        {
            StartCoroutine(AddProgress(int.Parse(value)));
        }
        catch
        {
            Debug.LogError($"Value: {value}, could not be parsed in to integer.");
        }
    }

    private void HandleNoRepetitionTask(string value)
    {
        if (!_previousTaskStrings.Contains(value))
        {
            _previousTaskStrings.Add(value);
            StartCoroutine(AddProgress(1));
        }
    }

    private IEnumerator AddProgress(int value)
    {
        PlayerData playerData = null;
        PlayerData savePlayerData = null;
        bool? timeout = null;

        //Get player data.
        StartCoroutine(PlayerDataTransferer("get", null, tdata => timeout = tdata, pdata => playerData = pdata));
        yield return new WaitUntil(() => (playerData != null || timeout != null));

        if (playerData == null)
            yield break; //TODO: Add error handling.

        CurrentPlayerTask.AddProgress(value);

        if (OnTaskProgressed != null)
            OnTaskProgressed.Invoke();

        //Is task done check.
        if (CurrentPlayerTask.TaskProgress >= CurrentPlayerTask.Amount)
        {
            //Distribute rewards.
            bool? done = null;
            timeout = null;

            StartCoroutine(CoroutineWithTimeout(DistributeRewardsForClan, playerData.ClanId, done, _timeoutSeconds, timeoutCallBack => timeout = timeoutCallBack, data => done = data));

            yield return new WaitUntil(() => (done != null || timeout != null));

            if (done == null)
            {
                Debug.LogError($"Distribute clan rewards timeout or null.");
                yield break; //TODO: Add error handling.
            }
            else if (done == false)
            {
                Debug.LogError($"Distribute clan rewards failed.");
                yield break; //TODO: Add error handling.
            }

            playerData.points += playerData.Task.Points;

            //Clean up.
            _previousTaskStrings.Clear();
            CurrentPlayerTask = null;
            if (OnTaskDone != null)
                OnTaskDone.Invoke(); //Clear DailyTaskManager OwnTask page & get fresh PlayerData.
        }

        //Save player data
        playerData.Task = CurrentPlayerTask;
        timeout = null;

        StartCoroutine(PlayerDataTransferer("save", playerData, tdata => timeout = tdata, pdata => savePlayerData = pdata));
        //yield return new WaitUntil(() => (savePlayerData != null || timeout != null));

        //if (savePlayerData == null)
        //    yield break;
    }

    //TODO: WARNING! Clan data saving is disabled! Uncomment when saving is functional.
    private IEnumerator DistributeRewardsForClan(string clanId, System.Action<bool?> exitCallback)
    {
        ClanData clanData = null;
        bool? timeout = null;
        Coroutine clanCoroutine = null, timeoutCoroutine;

        //Get clan data.
        Storefront.Get().GetClanData(clanId, data => clanData = data);

        if (clanData == null)
        {
            clanCoroutine = StartCoroutine(ServerManager.Instance.GetClanFromServer(content =>
            {
                if (content != null)
                    clanData = new(content);
                else
                {
                    Debug.LogError("Could not connect to server and receive clan");
                    return;
                }
            }));
        }

        timeoutCoroutine = StartCoroutine(WaitUntilTimeout(_timeoutSeconds, data => timeout = data));
        yield return new WaitUntil(() => (clanData != null || timeout != null));

        if (clanData == null)
        {
            StopCoroutine(clanCoroutine);
            exitCallback(false);
            Debug.LogError($"Get clan data timeout or null.");
            yield break; //TODO: Add error handling.
        }
        else
            StopCoroutine(timeoutCoroutine);

        //Save clan data.
        clanData.GameCoins += CurrentPlayerTask.Coins;
        clanData.Points += CurrentPlayerTask.Points;

        //TODO: Uncomment when it works again.
        //timeout = null;
        //Storefront.Get().SaveClanData(clanData, data => clanData = data);

        //if (clanData == null)
        //{
        //    clanCoroutine = StartCoroutine(ServerManager.Instance.UpdateClanToServer(clanData, content =>
        //    {
        //        if (content)
        //            Debug.Log("Rewards distributed successfully to clan.");
        //        else
        //        {
        //            Debug.LogError("Failed to distribute rewards to clan.");
        //            return;
        //        }
        //    }));
        //}

        //timeoutCoroutine = StartCoroutine(WaitUntilTimeout(_timeoutSeconds, data => timeout = data));
        //yield return new WaitUntil(() => (clanData != null || timeout != null));

        //if (clanData == null)
        //{
        //    StopCoroutine(clanCoroutine);
        //    exitCallback(false);
        //    Debug.LogError($"Save clan data timeout or null.");
        //    yield break; //TODO: Add error handling.
        //}
        //else
        //    StopCoroutine(timeoutCoroutine);

        exitCallback(true);
    }

    /// <summary>
    /// Used to get or save <c>PlayerData</c>.
    /// </summary>
    /// <param name="operationType">Use "<c>get</c>" or "<c>save</c>" to select an operation.</param>
    /// <param name="unsavedData">Use <c>PlayerData</c> when saving and <c>null</c> when getting <c>PlayerData</c>.</param>
    /// <param name="timeoutCallback">Returns a value if selected operation has timeouted.</param>
    /// <param name="dataCallback">Returns always <c>PlayerData</c> unless timeouted.</param>
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

    #endregion
}
