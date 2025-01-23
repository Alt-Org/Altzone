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

public class DailyTaskManager : MonoBehaviour
{
    //Variables
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

    //Local Testing
    private int _ownTaksProgress = 0;

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
        StartCoroutine(PopulateClanPlayers());

        //Tab bar
        _dailyTasksTabButton.onClick.AddListener(() => SwitchTab(SelectedTab.Tasks));
        _ownTaskTabButton.onClick.AddListener(() => SwitchTab(SelectedTab.OwnTask));
        _clanTaskTabButton.onClick.AddListener(() => SwitchTab(SelectedTab.ClanTask));

        //OwnTask cancel button
        _cancelTaskButton.onClick.AddListener(() => StartCancelTask());

        _ownTaskTabButton.interactable = false;
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

        //yield return new WaitUntil(() => clanId != null);

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

        //yield return new WaitUntil(() => clan != null);

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

        //yield return new WaitUntil(() => clan != null);

        #endregion

        //Testing code
        for (int i = 0; i < 30; i++)
        {
            GameObject player = Instantiate(_clanPlayerPrefab, _clanPlayersList);
            player.GetComponent<DailyTaskClanPlayer>().Set(i, null, null);

            _clanPlayers.Add(player);
            Debug.Log("Created clan player: " + i);
        }
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
                            if (_ownTaskId != null)
                                CancelTask();

                            PopupDataHandler(data.Value);
                            SwitchTab(SelectedTab.OwnTask);
                            _ownTaskTabButton.interactable = true;
                            break;
                        }
                    case PopupData.PopupDataType.CancelTask:
                        {
                            CancelTask();
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

    private void PopupDataHandler(PopupData data)
    {
        switch (data.Type)
        {
            case PopupData.PopupDataType.OwnTask: HandleOwnTask(data.OwnPage.Value); break;
            default: break;
        }
    }

    private void HandleOwnTask(PopupData.OwnPageData data)
    {
        //TODO: Add task accept code when server side has functionality.
        CalculateOwnTaskProgressBar(data.TaskAmount);
        StartCoroutine(_ownTaskPageHandler.SetDailyTask(data.TaskDescription, data.TaskAmount, data.TaskPoints, data.TaskCoins));
        _ownTaskId = data.TaskId;
        Debug.Log("Task id: " + _ownTaskId + ", has been accepted.");
    }

    public void TESTAddTaskProgress()
    {
        _ownTaksProgress++;

        foreach (GameObject obj in _dailyTaskCardSlots)
        {
            DailyQuest quest = obj.GetComponent<DailyQuest>();

            if (quest.TaskData.Id == _ownTaskId)
            {
                CalculateOwnTaskProgressBar(quest.TaskData.Amount);
                return;
            }
        }
    }

    private void CalculateOwnTaskProgressBar(int taskAmount)
    {
        float progress = (float)_ownTaksProgress / (float)taskAmount;
        StartCoroutine(_ownTaskPageHandler.SetTaskProgress(progress));
        Debug.Log("Task id: " + _ownTaskId + ", current progress: " + progress);
        if (progress >= 1f)
        {
            Debug.Log("Task id:" + _ownTaskId + ", is done");
        }
    }

    // Calling popup for canceling task.
    public void StartCancelTask()
    {
        PopupData data = new(PopupData.GetType("cancel_task"));
        StartCoroutine(ShowPopupAndHandleResponse("Haluatko Peruuttaa Nykyisen Tehtävän?", data));
    }

    private void CancelTask()
    {
        //TODO: Add task cancellation code when server side has functionality.
        _ownTaksProgress = 0;
        StartCoroutine(_ownTaskPageHandler.ClearCurrentTask());
        Debug.Log("Task id: " + _ownTaskId + ", has been canceled.");
        _ownTaskId = null;
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
}
