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

    public delegate void TaskChange(PlayerTask task);
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

    public delegate IEnumerator ClanMilestoneProgressed();
    /// <summary>
    /// Used to show <c>DailyTaskProgressPopup</c> window when clan milestone reward has been reached.
    /// </summary>
    public static event ClanMilestoneProgressed OnClanMilestoneProgressed;

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
        StartCoroutine(PlayerDataTransferer("get", null, _timeoutSeconds, tdata => timeout = tdata, pdata => playerData = pdata));
        yield return new WaitUntil(() => (playerData != null || timeout != null));

        if (playerData == null)
            yield break;

        CurrentPlayerTask = playerData.Task;
    }

    #region Task Processing

    // This is called from DailyTaskProgressListener.cs to update normal task.
    public void UpdateTaskProgress(TaskNormalType taskType, string value)
    {
        if ((taskType != CurrentPlayerTask.Type) && (taskType != TaskNormalType.Test))
        {
            Debug.LogError($"Current task type is: {CurrentPlayerTask.Type}, but type: {taskType}, was received.");
            return;
        }

        switch (CurrentPlayerTask.Type)
        {
            case TaskNormalType.PlayBattle: HandleSimpleTask(value); break;
            case TaskNormalType.WinBattle: HandleSimpleTask(value); break;
            case TaskNormalType.StartBattleDifferentCharacter: HandleNoRepetitionTask(value); break;
            case TaskNormalType.Vote: HandleSimpleTask(value); break;
            case TaskNormalType.WriteChatMessage: HandleSimpleTask(value); break;
            default: break;
        }
    }

    // This is called from DailyTaskProgressListener.cs to update education action task.
    public void UpdateTaskProgress(TaskEducationActionType taskType, string value)
    {
        if ((taskType != CurrentPlayerTask.EducationActionType) && (value != "test"))
        {
            Debug.LogError($"Current task type is: {CurrentPlayerTask.EducationActionType}, but type: {taskType}, was received.");
            return;
        }

        switch (CurrentPlayerTask.EducationActionType)
        {
            case TaskEducationActionType.BlowUpYourCharacter: HandleSimpleTask(value); break;
            case TaskEducationActionType.EditCharacterStats: HandleSimpleTask(value); break;
            case TaskEducationActionType.PlayBattle: HandleSimpleTask(value); break;
            case TaskEducationActionType.SwitchSoulhomeMusic: HandleSimpleTask(value); break;
            case TaskEducationActionType.WinBattle: HandleSimpleTask(value); break;
            case TaskEducationActionType.MakeMusicWithButtons: HandleSimpleTask(value); break;
            case TaskEducationActionType.MakeCharacterFast: HandleSimpleTask(value); break;
            case TaskEducationActionType.MakeCharacterDurable: HandleSimpleTask(value); break;
            case TaskEducationActionType.MakeCharacterStrong: HandleSimpleTask(value); break;
            case TaskEducationActionType.MakeCharacterBig: HandleSimpleTask(value); break;
            case TaskEducationActionType.ChangeAvatarClothes: HandleSimpleTask(value); break;
            case TaskEducationActionType.ChangeItemsPosition: HandleSimpleTask(value); break;
            case TaskEducationActionType.UseAllItemsSoulhome: HandleSimpleTask(value); break;
            case TaskEducationActionType.FindVariableValueInGame: HandleSimpleTask(value); break;
            case TaskEducationActionType.Find3ImportantButtons: HandleSimpleTask(value); break;
            case TaskEducationActionType.FindBug: HandleSimpleTask(value); break;
            default: break;
        }
    }

    // This is called from DailyTaskProgressListener.cs to update education social task.
    public void UpdateTaskProgress(TaskEducationSocialType taskType, string value)
    {
        if ((taskType != CurrentPlayerTask.EducationSocialType) && (value != "test"))
        {
            Debug.LogError($"Current task type is: {CurrentPlayerTask.EducationSocialType}, but type: {taskType}, was received.");
            return;
        }

        switch (CurrentPlayerTask.EducationSocialType)
        {
            case TaskEducationSocialType.AddNewFriend: HandleSimpleTask(value); break;
            case TaskEducationSocialType.CreateNewVote: HandleSimpleTask(value); break;
            case TaskEducationSocialType.EditCharacterAvatar: HandleSimpleTask(value); break;
            case TaskEducationSocialType.EmoteDuringBattle: HandleSimpleTask(value); break;
            case TaskEducationSocialType.WriteChatMessageClan: HandleSimpleTask(value); break;
            case TaskEducationSocialType.ChatAddReaction: HandleSimpleTask(value); break;
            case TaskEducationSocialType.FindAllChatOptions: HandleSimpleTask(value); break;
            case TaskEducationSocialType.UseAllChatFeelings: HandleSimpleTask(value); break;
            case TaskEducationSocialType.DefinePlayerStyle: HandleSimpleTask(value); break;
            case TaskEducationSocialType.WriteChatMessageGlobal: HandleSimpleTask(value); break;
            case TaskEducationSocialType.ClanVote: HandleSimpleTask(value); break;
            case TaskEducationSocialType.SuggestItemFleaMarket: HandleSimpleTask(value); break;
            case TaskEducationSocialType.AddItemFleaMarket: HandleSimpleTask(value); break;
            case TaskEducationSocialType.ChangeClanMotto: HandleSimpleTask(value); break;
            default: break;
        }
    }

    // This is called from DailyTaskProgressListener.cs to update education story task.
    public void UpdateTaskProgress(TaskEducationStoryType taskType, string value)
    {
        if ((taskType != CurrentPlayerTask.EducationStoryType) && (value != "test"))
        {
            Debug.LogError($"Current task type is: {CurrentPlayerTask.EducationStoryType}, but type: {taskType}, was received.");
            return;
        }

        switch (CurrentPlayerTask.EducationStoryType)
        {
            case TaskEducationStoryType.ClickCharacterDescription: HandleSimpleTask(value); break;
            case TaskEducationStoryType.ContinueClanStory: HandleSimpleTask(value); break;
            case TaskEducationStoryType.FindSybolicalFurniture: HandleSimpleTask(value); break;
            case TaskEducationStoryType.FindSymbolicalGraphics: HandleSimpleTask(value); break;
            case TaskEducationStoryType.RecognizeSoundClue: HandleSimpleTask(value); break;
            case TaskEducationStoryType.CreateUnifiedInterior: HandleSimpleTask(value); break;
            case TaskEducationStoryType.RecognizeCharacterMechanic: HandleSimpleTask(value); break;
            case TaskEducationStoryType.WhereGameHappens: HandleSimpleTask(value); break;
            default: break;
        }
    }

    // This is called from DailyTaskProgressListener.cs to update education culture task.
    public void UpdateTaskProgress(TaskEducationCultureType taskType, string value)
    {
        if ((taskType != CurrentPlayerTask.EducationCultureType) && (value != "test"))
        {
            Debug.LogError($"Current task type is: {CurrentPlayerTask.EducationCultureType}, but type: {taskType}, was received.");
            return;
        }

        switch (CurrentPlayerTask.EducationCultureType)
        {
            case TaskEducationCultureType.ClickKnownArtIdeaPerson: HandleSimpleTask(value); break;
            case TaskEducationCultureType.ClickKnownCharacter: HandleSimpleTask(value); break;
            case TaskEducationCultureType.GamesGenreTypes: HandleSimpleTask(value); break;
            case TaskEducationCultureType.SetProfilePlayerType: HandleSimpleTask(value); break;
            case TaskEducationCultureType.SimiliarToAGame: HandleSimpleTask(value); break;
            case TaskEducationCultureType.FindPowerOrEqualityWindow: HandleSimpleTask(value); break;
            default: break;
        }
    }

    // This is called from DailyTaskProgressListener.cs to update education ethical task.
    public void UpdateTaskProgress(TaskEducationEthicalType taskType, string value)
    {
        if ((taskType != CurrentPlayerTask.EducationEthicalType) && (value != "test"))
        {
            Debug.LogError($"Current task type is: {CurrentPlayerTask.EducationEthicalType}, but type: {taskType}, was received.");
            return;
        }

        switch (CurrentPlayerTask.EducationEthicalType)
        {
            case TaskEducationEthicalType.ClickBuyable: HandleSimpleTask(value); break;
            case TaskEducationEthicalType.ClickEthical: HandleSimpleTask(value); break;
            case TaskEducationEthicalType.ClickQuestionable: HandleSimpleTask(value); break;
            case TaskEducationEthicalType.UseOnlyNegativeEmotes: HandleSimpleTask(value); break;
            case TaskEducationEthicalType.UseOnlyPositiveEmotes: HandleSimpleTask(value); break;
            case TaskEducationEthicalType.PressSustainableConsumptionObjects: HandleSimpleTask(value); break;
            case TaskEducationEthicalType.PressValuesObjects: HandleSimpleTask(value); break;
            default: break;
        }
    }

    /// <summary>
    /// This will call all <c>DailyTaskProgressListener</c>s<br/>
    /// to update their <c>_on</c> state depending on the task type.
    /// </summary>
    public void ChangeCurrentTask(PlayerTask task)
    {
        if (CurrentPlayerTask != task)
        {
            _previousTaskStrings.Clear();
        }

        CurrentPlayerTask = task;

        if (OnTaskChange != null)
                OnTaskChange.Invoke(task);
    }

    public bool SameTask(TaskNormalType taskType)
    {
        if (CurrentPlayerTask == null)
            return false;

        if (taskType == TaskNormalType.Test)
            return (true);

        return (taskType == CurrentPlayerTask.Type);
    }

    /// <summary>
    /// Handles integer progression based tasks.
    /// </summary>
    private void HandleSimpleTask(string value)
    {
        try
        {
            if (value == "test")
                StartCoroutine(AddPlayerTaskProgress(1));
            else
                StartCoroutine(AddPlayerTaskProgress(int.Parse(value)));
        }
        catch
        {
            Debug.LogError($"Value: {value}, could not be parsed in to integer.");
        }
    }

    /// <summary>
    /// Handles string progression based tasks.
    /// </summary>
    private void HandleNoRepetitionTask(string value)
    {
        if (!_previousTaskStrings.Contains(value))
        {
            _previousTaskStrings.Add(value);
            StartCoroutine(AddPlayerTaskProgress(1));
        }
    }

    private IEnumerator AddPlayerTaskProgress(int value)
    {
        PlayerData playerData = null;
        PlayerData savePlayerData = null;
        bool? timeout = null;

        //Get player data.
        StartCoroutine(PlayerDataTransferer("get", null, _timeoutSeconds, tdata => timeout = tdata, pdata => playerData = pdata));
        yield return new WaitUntil(() => (playerData != null || timeout != null));

        if (playerData == null)
            yield break;

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
                yield break;
            }
            else if (done == false)
            {
                Debug.LogError($"Distribute clan rewards failed.");
                yield break;
            }

            playerData.points += playerData.Task.Points;

            //Clean up.
            _previousTaskStrings.Clear();
            CurrentPlayerTask = null;
            if (OnTaskDone != null)
                OnTaskDone.Invoke(); //Clear DailyTaskManagers OwnTask page.

            OnTaskChange.Invoke(null);
        }

        //Save player data
        playerData.Task = CurrentPlayerTask;
        timeout = null;

        StartCoroutine(PlayerDataTransferer("save", playerData, _timeoutSeconds, tdata => timeout = tdata, pdata => savePlayerData = pdata));
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

    #endregion

    public void InvokeOnClanMilestoneReached()
    {
        StartCoroutine(OnClanMilestoneProgressed.Invoke());
    }
}
