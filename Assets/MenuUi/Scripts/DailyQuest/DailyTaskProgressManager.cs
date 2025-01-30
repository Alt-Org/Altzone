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
    [HideInInspector] public static DailyTaskProgressManager Instance { get; private set; }

    [HideInInspector] public PlayerTasks.PlayerTask CurrentPlayerTask { get; private set; }
    [HideInInspector] public PlayerData CurrentPlayerData { get; private set; }

    [SerializeField] private int _timeoutMilliseconds = 100;

    private List<string> _previousTaskStrings = new List<string>();

    #region Delegates & Events

    public delegate void TaskChange(TaskType taskType);
    public event TaskChange OnTaskChange;

    public delegate IEnumerator TaskDone();
    public event TaskDone OnTaskDone;

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
        StartCoroutine(GetPlayerData(data => playerData = data));

        yield return new WaitUntil(() => (System.Threading.SpinWait.SpinUntil(() => playerData != null, _timeoutMilliseconds)));

        if (playerData == null)
        {
            Debug.LogError($"Save player data timeout or null.");
            yield break; //TODO: Add error handling.
        }

        CurrentPlayerData = playerData;
        CurrentPlayerTask = playerData.Task;
    }

    private IEnumerator GetPlayerData(System.Action<PlayerData> callback)
    {
        //Testing code------------//
        if (CurrentPlayerData != null)
        {
            callback(CurrentPlayerData);
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

    //For testing until server is functional. DailyTaskManager will give the current task.
    public void TESTSetPlayerData(PlayerData playerData)
    {
        CurrentPlayerData = playerData;
        CurrentPlayerTask = playerData.Task;
    }

    public void UpdateCurrentTask(PlayerData playerData)
    {
        if (CurrentPlayerTask != playerData.Task)
            _previousTaskStrings.Clear();

        CurrentPlayerData = playerData;
        CurrentPlayerTask = playerData.Task;
        OnTaskChange.Invoke(CurrentPlayerData.Task.Type);
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
        StartCoroutine(GetPlayerData(data => playerData = data));

        yield return new WaitUntil(() => (System.Threading.SpinWait.SpinUntil(() => playerData != null, _timeoutMilliseconds)));

        if (playerData == null)
        {
            Debug.LogError($"Get player data timeout or null.");
            yield break; //TODO: Add error handling.
        }

        playerData.TaskProgress += value;

        if (playerData.TaskProgress >= playerData.Task.Amount) //Is task done check.
        {
            //Distribute rewards.
            bool? done = null;
            StartCoroutine(DistributeRewardsForClan(data => done = data));

            yield return new WaitUntil(() => (System.Threading.SpinWait.SpinUntil(() => done != null, _timeoutMilliseconds)));

            playerData.points += playerData.Task.Points;

            //Clean up.
            _previousTaskStrings.Clear();
            CurrentPlayerTask = null;
            playerData.Task = null;
            playerData.TaskProgress = 0;
            StartCoroutine(OnTaskDone.Invoke()); //Clear DailyTaskManager OwnTask page & get fresh PlayerData.
        }

        StartCoroutine(SavePlayerData(playerData, data => savePlayerData = data));

        yield return new WaitUntil(() => (System.Threading.SpinWait.SpinUntil(() => savePlayerData != null, _timeoutMilliseconds)));

        if (savePlayerData == null)
        {
            Debug.LogError($"Save player data timeout or null.");
            yield break; //TODO: Add error handling.
        }

        CurrentPlayerData = savePlayerData;
    }

    private IEnumerator DistributeRewardsForClan(System.Action<bool> exitCallback)
    {
        ClanData clanData = null;
        ClanData saveClanData = null;
        Storefront.Get().GetClanData(CurrentPlayerData.ClanId, data => clanData = data);

        if (clanData == null)
        {
            StartCoroutine(ServerManager.Instance.GetClanFromServer(content =>
            {
                if (content != null)
                    clanData = new(content);
                else
                {
                    //offline testing random generator with id generator
                    Debug.LogError("Could not connect to server and receive clan");
                    return;
                }
            }));
        }

        yield return new WaitUntil(() => clanData != null);

        if (clanData == null)
        {
            Debug.LogError($"Get player data timeout or null.");
            yield break; //TODO: Add error handling.
        }



        //Storefront.Get().SaveClanData(CurrentPlayerData.ClanId, data => clanData = data);

        //if (clanData == null)
        //{
        //    StartCoroutine(ServerManager.Instance.SaveClanFromServerToDataStorage(content =>
        //    {
        //        if (content != null)
        //            clanData = new(content);
        //        else
        //        {
        //            //offline testing random generator with id generator
        //            Debug.LogError("Could not connect to server and receive clan");
        //            return;
        //        }
        //    }));
        //}

        //yield return new WaitUntil(() => clanData != null);

        if (clanData == null)
        {
            Debug.LogError($"Get player data timeout or null.");
            yield break; //TODO: Add error handling.
        }
    }
}
