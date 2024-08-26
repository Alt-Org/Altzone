using UnityEngine;

public class PopupWindowScript : MonoBehaviour
{
    // Muuttuja sen taskin id:n tallentamiseksi, jota t�m� popup-ikkuna hallinnoi
    private int[] associatedTaskIds = new int[] {1,2,3,4,5,6,7,8,9,10,11,12 };
    public Leaderboard leaderboard;
    public GameObject popupScreen;
    public GameObject dailyTaskPrefab;

    private void Start()
    {
        CreateTaskFromId();
    }
    public void CreateTaskFromId()
    {
        Debug.Log("Luodaan teht�vi� id:ist�");

        foreach (int taskID in associatedTaskIds)
        {
            // Ensure the prefab is assigned
            if (dailyTaskPrefab == null)
            {
                Debug.LogError("daily Prefab Puuttuu!");
                return;
            }

            // Instantiate the task object
            GameObject taskObject = Instantiate(dailyTaskPrefab.gameObject, gameObject.transform);

            // Ensure the DailyQuest component exists on the prefab
            DailyQuest dailyQuestComponent = taskObject.GetComponent<DailyQuest>();
            Debug.Log(dailyQuestComponent);
            if (dailyQuestComponent == null)
            {
                Debug.LogError("dailytaskista puuttuu componentti!");
                return;
            }

            // Initialize the task with its ID
            dailyQuestComponent.InitializeTask(taskID);
            Debug.Log("Task created with ID: " + taskID);
        }
    }


    // Metodi avaa popup-ikkunan
    public void OpenPopupWindow()
    {
        // Tarkista, onko taskin id asetettu
        //if (associatedTaskIds <= 0)
        //{
        //    Debug.LogWarning("Popup window opening failed: Task id is not set.");
        //    return;
        //}

        // T�ss� on koodi popup-ikkunan avaamiseksi joka on m��ritetty muuttujissa
        //Debug.Log("Popup window opened for task: " + associatedTaskId);
        popupScreen.SetActive(true);

    }

    // Metodi sulkee popup-ikkunan
    public void ClosePopupWindow()
    {
        // T�ss� voisi olla koodi popup-ikkunan sulkemiseksi
        Debug.Log("Popup window closed");
        popupScreen.SetActive(false);
    }

    // Metodi kun pelaaja hyv�ksyy popup ikkunan
    public void AcceptPopupWindow()
    {
        Debug.Log("Daily mission accepted, Popup window now closes");
        popupScreen.SetActive(false);
    }

    // Metodi asettaa sen taskin id:n, jota t�m� popup-ikkuna hallinnoi
    public void SetAssociatedTaskId(int taskId)
    {
        // Tarkista, ett� taskin id on kelvollinen
        if (taskId <= 0)
        {
            Debug.LogWarning("Error: Invalid task id. Task id must be a positive integer.");
            return;
        }

        // Aseta taskin id
        //associatedTaskId = taskId;
        //Debug.Log("Popup window set for task id: " + associatedTaskId);
    }

    // Metodi nollaa sen taskin id:n, jota t�m� popup-ikkuna hallinnoi
    public void ResetAssociatedTaskId()
    {
        // Nollaa taskin id
        //associatedTaskId = 0;
        Debug.Log("Popup window task id reset");
    }

    private void TaskCompleted()
    {
        if (leaderboard != null)
        {
            //int pointsForTask = CalculatePointsForTask(associatedTaskId);
            //leaderboard.AddScore(pointsForTask);
        }
    }

    private int CalculatePointsForTask(int taskId)
    {
        // t�st� saa vaihdettua eri taskien pisteiden m��r��
        switch (taskId)
        {
            // esim
            // case 1: 50
            default:
                return 100;
        }
    }
}
