using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Config;
using Altzone.Scripts;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using UnityEngine;
using Altzone.Scripts.Model.Poco.Clan;

public class DailyTaskProgressManager : MonoBehaviour
{
    public static DailyTaskProgressManager Instance { get; private set; }

    [HideInInspector] public PlayerTasks.PlayerTask CurrentPlayerTask { get; private set; }

    [SerializeField] private float _timeoutSeconds = 10;

    private List<string> _previousTaskStrings = new List<string>();

    #region Delegates & Events

    public delegate void TaskChange(TaskType taskType);
    public static event TaskChange OnTaskChange;

    public delegate void TaskProgressed();
    public static event TaskProgressed OnTaskProgressed;

    public delegate void TaskDone();
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

    private IEnumerator GetPlayerData(System.Action<PlayerData> callback)
    {
        Storefront.Get().GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, callback);

        if (callback == null)
        {
            StartCoroutine(ServerManager.Instance.GetPlayerFromServer(content =>
            {
                if (content != null)
                    callback(new(content));
                else
                {
                    Debug.LogError("Could not connect to server and receive player");
                    return;
                }
            }));
        }

        yield return new WaitUntil(() => callback != null);
    }

    //TODO: Uncomment, remove testing code and fix bugs when server side ready!
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

    public void UpdateTaskProgress(TaskType taskType, string value)
    {
        if (taskType != CurrentPlayerTask.Type)
        {
            Debug.LogError($"Current task type is: {CurrentPlayerTask.Type}, but type: {taskType}, was received.");
            return;
        }

        switch (taskType)
        {
            case TaskType.PlayBattle: HandleSimpleTask(value); break;
            case TaskType.WinBattle: HandleSimpleTask(value); break;
            case TaskType.StartBattleDifferentCharacter: HandleNoRepetitionTask(value); break;
            case TaskType.Vote: HandleSimpleTask(value); break;
            case TaskType.WriteChatMessage: HandleSimpleTask(value); break;
            default: break;
        }
    }

    public void UpdateCurrentTask(PlayerTasks.PlayerTask task)
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

            var clanCoroutine = StartCoroutine(DistributeRewardsForClan(playerData.ClanId, data => done = data));

            var timeoutCoroutine = StartCoroutine(WaitUntilTimeout(_timeoutSeconds, data => timeout = data));
            yield return new WaitUntil(() => (done != null || timeout != null));

            if (done == null)
            {
                StopCoroutine(clanCoroutine);
                Debug.LogError($"Distribute clan rewards timeout or null.");
                yield break; //TODO: Add error handling.
            }
            else
                StopCoroutine(timeoutCoroutine);

            if (done == false)
            {
                Debug.LogError($"Distribute clan rewards failed.");
                yield break; //TODO: Add error handling.
            }

            playerData.points += playerData.Task.Points;

            //Clean up.
            _previousTaskStrings.Clear();
            CurrentPlayerTask = null;
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

    private IEnumerator DistributeRewardsForClan(string clanId, System.Action<bool> exitCallback)
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

        timeout = null;
        Storefront.Get().SaveClanData(clanData, data => clanData = data);

        if (clanData == null)
        {
            clanCoroutine = StartCoroutine(ServerManager.Instance.UpdateClanToServer(clanData, content =>
            {
                if (content)
                    Debug.Log("Rewards distributed successfully to clan.");
                else
                {
                    Debug.LogError("Failed to distribute rewards to clan.");
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
            Debug.LogError($"Save clan data timeout or null.");
            yield break; //TODO: Add error handling.
        }
        else
            StopCoroutine(timeoutCoroutine);

        exitCallback(true);
    }

    private IEnumerator PlayerDataTransferer(string operationType, PlayerData unsavedData, System.Action<bool> timeoutCallback, System.Action<PlayerData> dataCallback)
    {
        PlayerData receivedData = null;
        bool? timeout = null;
        Coroutine playerCoroutine , timeoutCoroutine;

        switch (operationType.ToLower())
        {
            case "get":
                {
                    //Get player data.
                    playerCoroutine = StartCoroutine(GetPlayerData(data => receivedData = data));
                    break;
                }
            case "save":
                {
                    //Save player data.
                    playerCoroutine = StartCoroutine(SavePlayerData(unsavedData, data => receivedData = data));
                    break;
                }
            default: Debug.LogError($"Received: {operationType}, when expecting \"get\" or \"save\"."); yield break;
        }

        timeoutCoroutine = StartCoroutine(WaitUntilTimeout(_timeoutSeconds, data => timeout = data));

        yield return new WaitUntil(() => (receivedData != null || timeout != null));

        if (receivedData == null)
        {
            timeoutCallback(true);
            StopCoroutine(playerCoroutine);
            Debug.LogError($"Player data operation: {operationType} timeout or null.");
            yield break; //TODO: Add error handling.
        }
        else
            StopCoroutine(timeoutCoroutine);

        dataCallback(receivedData);
    }

    private IEnumerator WaitUntilTimeout(float timeoutSeconds, System.Action<bool> callback)
    {
        yield return new WaitForSeconds(timeoutSeconds);
        callback(true);
    }
}
