using System.Threading.Tasks;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Player;
using UnityEngine;

public class DailyTaskManager : MonoBehaviour
{
    private GameObject[] _dailyQuestSlots = new GameObject[31];
    private GameObject[] _weeklyQuestSlots = new GameObject[31];
    private GameObject[] _monthlyQuestSlots = new GameObject[31];

    private int _dailyQuestAmount = 31;
    private int _weeklyQuestAmount = 30;
    private int _monthlyQuestAmount = 30;

    private string _taskTitle;
    private int _taskPoints;
    private int _taskgoal;

    public int currentTaskIndex = 1;

    public Leaderboard leaderboard;
    public GameObject popupScreenPrefab;
    public GameObject dailyTaskPrefab;
    public GameObject weeklyTaskPrefab;
    public GameObject monthlyTaskPrefab;

    private enum SelectedTab
    {
        Daily,
        Weekly,
        Monthly
    }

    private SelectedTab _selectedTab = SelectedTab.Daily;
    private SelectedTab? _activeTab = null; 
    private int _activeTaskIndex = -1;      


    private void Start()
    {
        QuestGenerator();
        RestoreActiveQuest();
    }

    public void QuestGenerator()
    {
        PopulateQuests(SelectedTab.Daily, _dailyQuestSlots, _dailyQuestAmount, dailyTaskPrefab);
        PopulateQuests(SelectedTab.Weekly, _weeklyQuestSlots, _weeklyQuestAmount, weeklyTaskPrefab);
        PopulateQuests(SelectedTab.Monthly, _monthlyQuestSlots, _monthlyQuestAmount, monthlyTaskPrefab);
        Debug.Log("Task Slots populated!");
    }

    private void PopulateQuests(SelectedTab tab, GameObject[] questSlots, int questAmount, GameObject questPrefab)
    {
        for (int i = 1; i <= questAmount; i++)
        {
            GameObject taskObject = Instantiate(questPrefab, gameObject.transform);
            questSlots[i - 1] = taskObject;
            (string title, int points, int goals) = QuestRandomizer();

            DailyQuest quest = taskObject.GetComponent<DailyQuest>();
            quest.taskId = i;
            quest.getMissionData(title, points, goals);
            quest.popUpScreen = popupScreenPrefab;
            quest.dailyTaskManager = this;

            taskObject.SetActive(tab == _selectedTab); 
        }
    }

    public void SwitchTab(int tabIndex)
    {
        if (_activeTab.HasValue)
        {
            Debug.Log("Cannot switch tabs while a quest is active.");
            return; 
        }

        _selectedTab = (SelectedTab)tabIndex;
        UpdateTabDisplay();
    }



    private void UpdateTabDisplay()
    {
        foreach (GameObject taskObject in _dailyQuestSlots) taskObject?.SetActive(_selectedTab == SelectedTab.Daily);
        foreach (GameObject taskObject in _weeklyQuestSlots) taskObject?.SetActive(_selectedTab == SelectedTab.Weekly);
        foreach (GameObject taskObject in _monthlyQuestSlots) taskObject?.SetActive(_selectedTab == SelectedTab.Monthly);

        
        if (_activeTab == _selectedTab && _activeTaskIndex >= 0)
        {
            GetQuestSlot(_selectedTab, _activeTaskIndex)?.SetActive(true);
        }
    }

    public (string _taskTitle, int _taskGoal, int _taskPoints) QuestRandomizer()
    {
        Debug.Log("taskInit Runned");
        int rnd = UnityEngine.Random.Range(1, 11);

        switch (rnd)
        {
            case 1:
                _taskTitle = "Pelaa 3 taistelua";
                _taskgoal = 3;
                _taskPoints = 70;
                return( _taskTitle, _taskPoints, _taskgoal ) ;
            case 2:
                _taskTitle = "Pelaa 10 taistelua";
                _taskgoal = 10;
                _taskPoints = 500;
                return (_taskTitle, _taskPoints, _taskgoal);
            case 3:
                _taskTitle = "Kerää timantteja Taistelussa";
                _taskgoal = 50;
                _taskPoints = 50;
                return (_taskTitle, _taskPoints, _taskgoal);
            case 4:
                _taskTitle = "Kerää timantteja Taistelussa";
                _taskgoal = 100;
                _taskPoints = 120;
                return (_taskTitle, _taskPoints, _taskgoal);
            case 5:
                _taskTitle = "Kehitä pelihahmoa";
                _taskgoal = 2;
                _taskPoints = 100;
                return (_taskTitle, _taskPoints, _taskgoal);
            case 6:
                _taskTitle = "Voita 3 taistelu";
                _taskgoal = 3;
                _taskPoints = 100;
                return (_taskTitle, _taskPoints, _taskgoal);
            case 7:
                _taskTitle = "Voita 5 taistelu";
                _taskgoal = 5;
                _taskPoints = 200;
                return (_taskTitle, _taskPoints, _taskgoal);
            case 8:
                _taskTitle = "Muokkaa avatarisi ulkonäköä";
                _taskgoal = 2;
                _taskPoints = 500;
                return (_taskTitle, _taskPoints, _taskgoal);
            case 9:
                _taskTitle = "Moikkaa avatareja";
                _taskgoal = 5;
                _taskPoints = 250;
                return (_taskTitle, _taskPoints, _taskgoal);
            case 10:
                _taskTitle = "Kirjoita viesti chattiin";
                _taskgoal = 10;
                _taskPoints = 100;
                return (_taskTitle, _taskPoints, _taskgoal);
            case 11:
                _taskTitle = "Sijoita pommi sielunkotiin";
                _taskgoal = 3;
                _taskPoints = 30;
                return (_taskTitle, _taskPoints, _taskgoal);
            default:
                _taskTitle = "Unknown Task";
                _taskgoal = 100;
                _taskPoints = 1;
                Debug.Log("Jokin Meni pieleen DailyTaskissä");
                return (_taskTitle, _taskPoints, _taskgoal);
        }
    }

    
    public void TakeTask(int taskIndex)
    {
        _activeTab = _selectedTab;
        _activeTaskIndex = taskIndex;
        HideAllOtherTasks();
    }

    private void HideAllOtherTasks()
    {
        if (!_activeTab.HasValue) return;

       
        GameObject[] questSlots = GetQuestSlots(_activeTab.Value);
        for (int i = 0; i < questSlots.Length; i++)
        {
            questSlots[i]?.SetActive(i == _activeTaskIndex);
        }
    }
    private void RestoreActiveQuest()
    {
        PlayerData playerData = null;
        Storefront.Get().GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, p => playerData = p);

        if (playerData != null && playerData.dailyTaskId >= 1)
        {
            _activeTab = SelectedTab.Daily; 
            _activeTaskIndex = playerData.dailyTaskId - 1;
            HideAllOtherTasks();
        }
        else
        {
            CancelTask();
        }
    }

    public void CancelTask()
    {
        currentTaskIndex = -1;
        UpdateTabDisplay();
    }

    private GameObject GetQuestSlot(SelectedTab tab, int index) =>
    GetQuestSlots(tab)[index];

    private GameObject[] GetQuestSlots(SelectedTab tab) =>
        tab switch
        {
            SelectedTab.Daily => _dailyQuestSlots,
            SelectedTab.Weekly => _weeklyQuestSlots,
            SelectedTab.Monthly => _monthlyQuestSlots,
            _ => null
        };
}
