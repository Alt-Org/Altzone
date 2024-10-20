using System.Threading.Tasks;
using UnityEngine;

public class DailyTaskManager : MonoBehaviour
{
    private GameObject[] dailyQuestSlots = new GameObject[30];

    private string _taskTitle;
    private int _taskPoints;
    private int _taskgoal;
    private int _guestMultiplier;
    private int _questAmount = 30;

    public Leaderboard leaderboard;
    public GameObject popupScreenPrefab;
    public GameObject dailyTaskPrefab;


    private void Start()
    {
        QuestGenerator();

    }

    public void QuestGenerator()
    {
        for (int i = 0; i < _questAmount; i++)
        {
            GameObject taskObject = Instantiate(dailyTaskPrefab.gameObject, gameObject.transform);
            dailyQuestSlots[i] = taskObject;
            (string title, int points, int goals) = QuestRandomizer();
            taskObject.GetComponent<DailyQuest>().getMissionData(title, points, goals);
            taskObject.GetComponent<DailyQuest>().taskId = i + 1;
            taskObject.GetComponent<DailyQuest>().popUpScreen = popupScreenPrefab;
        }
        Debug.Log("Task Slots populated!");
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


}
