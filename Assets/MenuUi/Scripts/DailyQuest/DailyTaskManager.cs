using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts;
using UnityEngine;
using static Altzone.Scripts.Model.Poco.Game.PlayerTasks;

public class DailyTaskManager : MonoBehaviour
{
    //Variables
    private const int _questSlots = 100;
    private GameObject[] _dailyQuestSlots = new GameObject[_questSlots];
    private GameObject[] _weeklyQuestSlots = new GameObject[_questSlots];
    private GameObject[] _monthlyQuestSlots = new GameObject[_questSlots];

    [SerializeField] private GameObject _dailyTaskPrefab;
    [SerializeField] private GameObject _weeklyTaskPrefab;
    [SerializeField] private GameObject _monthlyTaskPrefab;

    [SerializeField] private Transform _dailyCategory500;
    [SerializeField] private Transform _dailyCategory1000;
    [SerializeField] private Transform _dailyCategory1500;
    [SerializeField] private Transform _activeQuestWindow;
    [SerializeField] private GameObject _clanRewards;

    public enum SelectedTab
    {
        Daily,
        Weekly,
        Monthly
    }

    private SelectedTab _selectedTab = SelectedTab.Daily;

    [Header("Tab Panels")]
    [SerializeField] private GameObject[] _tabPanels; // Drag your panels here in the Inspector

    // Start of Code
    // First 3 functions are for quest slot population and fetching them from server
    void Start()
    {
        QuestGenerator();
    }

    public void QuestGenerator()
    {
        StartCoroutine(PopulateQuests(SelectedTab.Daily, _dailyQuestSlots, _dailyTaskPrefab));
        StartCoroutine(PopulateQuests(SelectedTab.Weekly, _weeklyQuestSlots, _weeklyTaskPrefab));
        StartCoroutine(PopulateQuests(SelectedTab.Monthly, _monthlyQuestSlots, _monthlyTaskPrefab));

        Debug.Log("Task Slots populated!");
    }

    private IEnumerator PopulateQuests(SelectedTab tab, GameObject[] questSlots, GameObject questPrefab)
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
                }
            }));
        }

        yield return new WaitUntil(() => tasks != null);

        switch (tab)
        {
            case SelectedTab.Daily:
                tasklist = tasks.Daily;
                break;
            case SelectedTab.Weekly:
                tasklist = tasks.Week;
                break;
            case SelectedTab.Monthly:
                tasklist = tasks.Month;
                break;
        }

        for (int i = 0; i < tasklist.Count; i++)
        {
            GameObject taskObject = Instantiate(questPrefab, gameObject.transform);
            questSlots[i] = taskObject;

            DailyQuest quest = taskObject.GetComponent<DailyQuest>();
            quest.GetQuestData(tasklist[i]);
            quest.dailyTaskManager = this;

            Transform parentCategory = GetParentCategory(tasklist[i].Points);
            taskObject.transform.SetParent(parentCategory, false);
            taskObject.SetActive(tab == _selectedTab);

            Debug.Log("Created Quest: " +  tasklist[i].Id);
        }
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

    // Function for popup calling
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
                        HideCategories();
                        _clanRewards.SetActive(false);
                        _activeQuestWindow.gameObject.SetActive(true);
                        break;
                    case 2:
                        Debug.Log("Cancel case happened " + popupId);
                        ShowCategories();
                        _clanRewards.SetActive(true);
                        _activeQuestWindow.gameObject.SetActive(false);
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

    // calling popup for canceling quest
    public void CancelActiveQuest()
    {
        StartCoroutine(ShowPopupAndHandleResponse("Haluatko Peruuttaa Nykyisen Tehtävän?", 2));
    }

    // show/hide works for quest selection to hide and show quest selection
    private void ShowCategories()
    {
        _dailyCategory500.transform.parent.gameObject.SetActive(true);
        _dailyCategory1000.transform.parent.gameObject.SetActive(true);
        _dailyCategory1500.transform.parent.gameObject.SetActive(true);

        Debug.Log("Categories shown.");
    }

    private void HideCategories()
    {
        _dailyCategory500.transform.parent.gameObject.SetActive(false);
        _dailyCategory1000.transform.parent.gameObject.SetActive(false);
        _dailyCategory1500.transform.parent.gameObject.SetActive(false);

        Debug.Log("Categories' parents hidden.");
    }

    // next functions are for tab switching system
    public void SwitchTab(SelectedTab tab)
    {
        // Update the selected tab
        _selectedTab = tab;

        // Toggle visibility for each category based on the selected tab
        ToggleQuestVisibility(_dailyQuestSlots, _selectedTab == SelectedTab.Daily);
        ToggleQuestVisibility(_weeklyQuestSlots, _selectedTab == SelectedTab.Weekly);
        ToggleQuestVisibility(_monthlyQuestSlots, _selectedTab == SelectedTab.Monthly);

        Debug.Log($"Switched to {_selectedTab} tasks.");
    }

    private void ToggleQuestVisibility(GameObject[] questSlots, bool isVisible)
    {
        foreach (GameObject quest in questSlots)
        {
            if (quest != null)
            {
                quest.SetActive(isVisible);
            }
        }
    }

    public void OnDailyTabSelected()
    {
        SwitchTab(SelectedTab.Daily);
    }

    public void OnWeeklyTabSelected()
    {
        SwitchTab(SelectedTab.Weekly);
    }

    public void OnMonthlyTabSelected()
    {
        SwitchTab(SelectedTab.Monthly);
    }
}
