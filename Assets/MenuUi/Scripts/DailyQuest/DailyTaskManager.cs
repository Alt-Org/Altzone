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

    public GameObject dailyTaskPrefab;
    public GameObject weeklyTaskPrefab;
    public GameObject monthlyTaskPrefab;

    public Transform dailyCategory500;
    public Transform dailyCategory1000;
    public Transform dailyCategory1500;
    public Transform ActiveQuestWindow;
    public GameObject clanRewards;

    public Popup popup;

    public enum SelectedTab
    {
        Daily,
        Weekly,
        Monthly
    }

    private SelectedTab _selectedTab = SelectedTab.Daily;
    private SelectedTab? _activeTab = null;
    private int _activeTaskIndex = -1;

    [Header("Tab Panels")]
    public GameObject[] tabPanels; // Drag your panels here in the Inspector

    // Start of Code
    // First 3 functions are for quest slot population and fetching them from server
    void Start()
    {
        QuestGenerator();
    }
    public void QuestGenerator()
    {
        StartCoroutine(PopulateQuests(SelectedTab.Daily, _dailyQuestSlots, _questSlots, dailyTaskPrefab));
        StartCoroutine(PopulateQuests(SelectedTab.Weekly, _weeklyQuestSlots, _questSlots, weeklyTaskPrefab));
        StartCoroutine(PopulateQuests(SelectedTab.Monthly, _monthlyQuestSlots, _questSlots, monthlyTaskPrefab));
        Debug.Log("Task Slots populated!");
    }
    private IEnumerator PopulateQuests(SelectedTab tab, GameObject[] questSlots, int questAmount, GameObject questPrefab)
    {
        PlayerTasks tasks = null;
        Storefront.Get().GetPlayerTasks(content => tasks = content);
        List<PlayerTask> tasklist = null;
        Debug.Log(tasks);
        if (tasks == null)
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
            quest.GetQuestData(tasklist[i].Id, tasklist[i].Amount, tasklist[i].Coins, tasklist[i].Points, tasklist[i].Title, tasklist[i].Content);

            Transform parentCategory = GetParentCategory(tab, tasklist[i].Points);
            taskObject.transform.SetParent(parentCategory, false);

            quest.dailyTaskManager = this;

            taskObject.SetActive(tab == _selectedTab);
            Debug.Log("Created Quest: " +  tasklist[i].Id);

        }
    }
    private Transform GetParentCategory(SelectedTab tab, int points)
    {
        return tab switch
        {
            SelectedTab.Daily => points switch
            {
                <= 500 => dailyCategory500,
                <= 1000 => dailyCategory1000,
                _ => dailyCategory1500,
            },
            SelectedTab.Weekly => points switch
            {
                <= 500 => dailyCategory500,
                <= 1000 => dailyCategory1000,
                _ => dailyCategory1500,
            },
            SelectedTab.Monthly => points switch
            {
                <= 500 => dailyCategory500,
                <= 1000 => dailyCategory1000,
                _ => dailyCategory1500,
            },
            _ => null
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
                        clanRewards.SetActive(false);
                        ActiveQuestWindow.gameObject.SetActive(true);
                        break;
                    case 2:
                        Debug.Log("Cancel case happened " + popupId);
                        ShowCategories();
                        clanRewards.SetActive(true);
                        ActiveQuestWindow.gameObject.SetActive(false);
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
        dailyCategory500.transform.parent.gameObject.SetActive(true);
        dailyCategory1000.transform.parent.gameObject.SetActive(true);
        dailyCategory1500.transform.parent.gameObject.SetActive(true);
        Debug.Log("Categories shown.");
    }
    private void HideCategories()
    {
        dailyCategory500.transform.parent.gameObject.SetActive(false);
        dailyCategory1000.transform.parent.gameObject.SetActive(false);
        dailyCategory1500.transform.parent.gameObject.SetActive(false);
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
