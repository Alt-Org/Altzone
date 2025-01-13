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

    private const int _cardSlots = 100;
    private GameObject[] _dailyTaskCardSlots = new GameObject[_cardSlots];

    [Header("DailyTaskCard prefabs")]
    [SerializeField] private GameObject _dailyTaskCardPrefab;

    [Header("DailyTasksPage")]
    [SerializeField] private GameObject _dailyTasksView;
    [SerializeField] private Transform _dailyCategory500;
    [SerializeField] private Transform _dailyCategory1000;
    [SerializeField] private Transform _dailyCategory1500;

    [Header("OwnTaskPage")]
    [SerializeField] private GameObject _ownTaskView;
    [SerializeField] private Button _cancelTaskButton;

    [Header("ClanTaskPage")]
    [SerializeField] private GameObject _clanTaskView;

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

    // First 3 functions are for task slot population and fetching them from server
    public void TaskGenerator()
    {
        StartCoroutine(PopulateTasks(_dailyTaskCardSlots, _dailyTaskCardPrefab));
        //StartCoroutine(PopulateQuests(_weeklyQuestSlots, _weeklyTaskPrefab));
        //StartCoroutine(PopulateQuests(_monthlyQuestSlots, _monthlyTaskPrefab));

        Debug.Log("Task Slots populated!");
    }

    private IEnumerator PopulateTasks(GameObject[] questSlots, GameObject questPrefab)
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
            GameObject taskObject = Instantiate(questPrefab, gameObject.transform);
            questSlots[i] = taskObject;

            DailyQuest quest = taskObject.GetComponent<DailyQuest>();
            quest.GetQuestData(tasklist[i]);
            quest.dailyTaskManager = this;

            Transform parentCategory = GetParentCategory(tasklist[i].Points);
            taskObject.transform.SetParent(parentCategory, false);
            taskObject.SetActive(true);

            Debug.Log("Created Quest: " +  tasklist[i].Id);
        }
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

    // Function for popup calling - TODO: Expand to handle and execute data sent from DailyQuest.cs about selected task.
    public IEnumerator ShowPopupAndHandleResponse(string Message, int popupId)
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
                        HideAvailableTasks();
                        SwitchTab(SelectedTab.OwnTask);
                        //TODO: Add functionality to set the "Omatyö" page.
                        break;
                    case 2:
                        Debug.Log("Cancel case happened " + popupId);
                        ShowAvailableTasks();
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

    // calling popup for canceling task
    public void CancelActiveTask()
    {
        StartCoroutine(ShowPopupAndHandleResponse("Haluatko Peruuttaa Nykyisen Tehtävän?", 2));
    }

    // show/hide works for task selection to hide and show task selection
    private void ShowAvailableTasks()
    {
        _dailyTasksView.SetActive(true);

        Debug.Log("Available tasks shown.");
    }

    private void HideAvailableTasks()
    {
        _dailyTasksView.SetActive(false);

        Debug.Log("Available tasks hidden.");
    }

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
