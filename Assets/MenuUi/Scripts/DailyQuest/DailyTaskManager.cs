using System.Threading.Tasks;
using UnityEditor.VersionControl;
using UnityEngine;

public class DailyTaskManager : MonoBehaviour
{
    private GameObject[] dailyQuestSlots = new GameObject[12];

    private string _taskTitle;
    private int _taskPoints;
    private int _taskgoal;

    private int _questAmount = 12;

    public Leaderboard leaderboard;
    public GameObject popupScreenPrefab;
    public GameObject dailyTaskPrefab;

    private void Start()
    {
        QuestGenerator();
    }

    // Generoi 12 tyhj‰‰ quest slottia arreihin dailytask prefabista
    public void QuestGenerator()
    {
        for (int i = 0; i < _questAmount; i++)
        {
            Debug.Log("This is action #" + (i + 1));
            // kopioi prefabin
            GameObject taskObject = Instantiate(dailyTaskPrefab.gameObject, gameObject.transform);
            // laittaa kopion arrayhin
            dailyQuestSlots[i] = taskObject;
            // pyyt‰‰ QuestRandomizerilta teht‰nv‰n ja laittaa local variableihin
            (string title, int points, int goals) = QuestRandomizer();
            // l‰hett‰‰ questin tiedot tyhj‰lle kopiolle
            taskObject.GetComponent<DailyQuest>().getMissionData(title, points, goals);
        }
        Debug.Log("Taski slotit tehty!");
    }
    // Generoi satunnaisen teht‰v‰n pyyt‰ess‰ HUOM v‰liaikainen!
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
                _taskTitle = "Ker‰‰ timantteja Taistelussa";
                _taskgoal = 50;
                _taskPoints = 50;
                return (_taskTitle, _taskPoints, _taskgoal);
            case 4:
                _taskTitle = "Ker‰‰ timantteja Taistelussa";
                _taskgoal = 100;
                _taskPoints = 120;
                return (_taskTitle, _taskPoints, _taskgoal);
            case 5:
                _taskTitle = "Kehit‰ pelihahmoa";
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
                _taskTitle = "Muokkaa avatarisi ulkon‰kˆ‰";
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
                Debug.Log("Jokin Meni pieleen DailyTaskiss‰");
                return (_taskTitle, _taskPoints, _taskgoal);
        }
    }
}
