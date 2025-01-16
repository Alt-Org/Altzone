using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts;
using UnityEngine;
using static Altzone.Scripts.Model.Poco.Game.PlayerTasks;
using UnityEngine.UI;
using UnityEditor.Overlays;

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

    [Header("ClanTaskPage")]
    [SerializeField] private GameObject _clanTaskView;

    public struct PopupData
    {
        public enum PopupDataType
        {
            OwnTask,
        }
        public PopupDataType Type;
        public struct OwnPageData
        {
            public int TaskId;
            public string TaskDescription;
            public int TaskAmount;
            public int TaskPoints;
            public int TaskCoins;
        }
        public OwnPageData? OwnPage;
    }

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

        //Tab bar
        _dailyTasksTabButton.onClick.AddListener(() => SwitchTab(SelectedTab.Tasks));
        _ownTaskTabButton.onClick.AddListener(() => SwitchTab(SelectedTab.OwnTask));
        _clanTaskTabButton.onClick.AddListener(() => SwitchTab(SelectedTab.ClanTask));

        //OwnTask cancel button
        _cancelTaskButton.onClick.AddListener(() => CancelActiveTask());
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

    // Function for popup calling - TODO: Expand to handle and execute data sent from DailyQuest.cs about selected task.
    public IEnumerator ShowPopupAndHandleResponse(string Message, int popupId, PopupData? data)
    {
        yield return Popup.RequestPopup(Message, result =>
        {
            if (result == true)
            {
                Debug.Log("Confirmed!");
                switch(popupId)
                {
                    case 1:
                        Debug.Log("Accept case happened " + popupId);
                        //HideAvailableTasks();
                        if (data != null)
                            PopupDataHandler(data ?? new PopupData());
                        //else
                        //    SwitchTab(SelectedTab.OwnTask);

                        break;
                    case 2:
                        Debug.Log("Cancel case happened " + popupId);
                        //ShowAvailableTasks();
                        SwitchTab(SelectedTab.Tasks);
                        break;
                }
            }
            else
            {
                Debug.Log("Cancelled Popup!");
                // Perform actions for cancellation
            }
        });
    }

    private void PopupDataHandler(PopupData data)
    {
        switch (data.Type)
        {
            case PopupData.PopupDataType.OwnTask: HandleOwnTask(data.OwnPage ?? new PopupData.OwnPageData()); break;
            default: break;
        }
    }

    private void HandleOwnTask(PopupData.OwnPageData data)
    {
        StartCoroutine(_ownTaskPageHandler.SetDailyTask(data.TaskDescription, data.TaskAmount, data.TaskPoints, data.TaskCoins));
        _ownTaskId = data.TaskId;
        SwitchTab(SelectedTab.OwnTask);
    }

    // calling popup for canceling task
    public void CancelActiveTask()
    {
        StartCoroutine(ShowPopupAndHandleResponse("Haluatko Peruuttaa Nykyisen Tehtävän?", 2, null));
    }

    //// show/hide works for task selection to hide and show task selection
    //private void ShowAvailableTasks()
    //{
    //    _dailyTasksView.SetActive(true);

    //    Debug.Log("Available tasks shown.");
    //}

    //private void HideAvailableTasks()
    //{
    //    _dailyTasksView.SetActive(false);

    //    Debug.Log("Available tasks hidden.");
    //}

    // next functions are for tab switching system
    public void SwitchTab(SelectedTab tab)
    {
        //Hide old tab
        switch (_selectedTab)
        {
            case SelectedTab.Tasks: _dailyTasksView.SetActive(false); break;
            case SelectedTab.OwnTask: _ownTaskView.SetActive(false); break;
            default: _clanTaskView.SetActive(false); break;
        }

        // Update the selected tab
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
