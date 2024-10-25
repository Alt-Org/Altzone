using System.Threading.Tasks;
using Altzone.Scripts;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Player;
using UnityEngine;

public class DailyTaskManager : MonoBehaviour
{
    private GameObject[] _dailyQuestSlots = new GameObject[31];

    private string _taskTitle;
    private int _taskPoints;
    private int _taskgoal;
    private int _questAmount = 31;

    public int currentTaskIndex = 1;

    public Leaderboard leaderboard;
    public GameObject popupScreenPrefab;
    public GameObject dailyTaskPrefab;


    private void Start()
    {
        QuestGenerator();
        RestoreActiveQuest();
    }

    public void QuestGenerator()
    {
        for (int i = 1; i < _questAmount; i++)
        {
            GameObject taskObject = Instantiate(dailyTaskPrefab.gameObject, gameObject.transform);
            _dailyQuestSlots[i] = taskObject;
            (string title, int points, int goals) = QuestRandomizer();

            DailyQuest quest = taskObject.GetComponent<DailyQuest>();
            quest.taskId = i;
            quest.getMissionData(title, points, goals);
            quest.popUpScreen = popupScreenPrefab;
            quest.dailyTaskManager = this;
        }
        Debug.Log("Task Slots populated!");
    }

    private void RestoreActiveQuest()
    {
        PlayerData playerData = null;
        Storefront.Get().GetPlayerData(GameConfig.Get().PlayerSettings.PlayerGuid, p => playerData = p);

        if (playerData != null && playerData.dailyTaskId >= 1 && playerData.dailyTaskId < _dailyQuestSlots.Length)
        {
            int taskIndex = playerData.dailyTaskId;
            if (_dailyQuestSlots[taskIndex] != null) // Check to avoid null reference
            {
                TakeTask(taskIndex);

                DailyQuest activeQuest = _dailyQuestSlots[taskIndex].GetComponent<DailyQuest>();
                activeQuest.unActiveTask.SetActive(false);
                activeQuest.activeTask.SetActive(true);

                Debug.Log("Restored active quest: " + taskIndex);
            }
        }
        else
        {
            CancelTask();
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

    // Method to hide all tasks except the one currently taken
    public void TakeTask(int taskIndex)
    {
        currentTaskIndex = taskIndex;

        for (int i = 1; i < _dailyQuestSlots.Length; i++)
        {
            if (i != taskIndex)
            {
                _dailyQuestSlots[i].SetActive(false); // Hide all other tasks
            }
        }
    }

    // Method to cancel task and show all hidden tasks again
    public void CancelTask()
    {
        currentTaskIndex = -1; // Reset current task

        foreach (GameObject taskObject in _dailyQuestSlots)
        {
            if (taskObject != null) // Ensure taskObject is not null
            {
                taskObject.SetActive(true); // Show all tasks
            }
        }
    }
}
