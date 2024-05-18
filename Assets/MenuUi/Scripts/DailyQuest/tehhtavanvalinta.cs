using UnityEngine;

public class PopupWindowScript : MonoBehaviour
{
    // Muuttuja sen taskin id:n tallentamiseksi, jota tämä popup-ikkuna hallinnoi
    public int associatedTaskId;
    public Leaderboard leaderboard;

    // Metodi avaa popup-ikkunan
    public void OpenPopupWindow()
    {
        // Tarkista, onko taskin id asetettu
        if (associatedTaskId <= 0)
        {
            Debug.LogWarning("Popup window opening failed: Task id is not set.");
            return;
        }

        // Tässä voisi olla koodi popup-ikkunan avaamiseksi
        Debug.Log("Popup window opened for task: " + associatedTaskId);

        TaskCompleted();
    }

    // Metodi sulkee popup-ikkunan
    public void ClosePopupWindow()
    {
        // Tössä voisi olla koodi popup-ikkunan sulkemiseksi
        Debug.Log("Popup window closed");
    }

    // Metodi asettaa sen taskin id:n, jota tämä popup-ikkuna hallinnoi
    public void SetAssociatedTaskId(int taskId)
    {
        // Tarkista, että taskin id on kelvollinen
        if (taskId <= 0)
        {
            Debug.LogWarning("Error: Invalid task id. Task id must be a positive integer.");
            return;
        }

        // Aseta taskin id
        associatedTaskId = taskId;
        Debug.Log("Popup window set for task id: " + associatedTaskId);
    }

    // Metodi nollaa sen taskin id:n, jota tämä popup-ikkuna hallinnoi
    public void ResetAssociatedTaskId()
    {
        // Nollaa taskin id
        associatedTaskId = 0;
        Debug.Log("Popup window task id reset");
    }

    private void TaskCompleted()
    {
        if (leaderboard != null)
        {
            int pointsForTask = CalculatePointsForTask(associatedTaskId);
            leaderboard.AddScore(pointsForTask);
        }
    }

    private int CalculatePointsForTask(int taskId)
    {
        // tästä saa vaihdettua eri taskien pisteiden määrää
        switch (taskId)
        {
            // esim
            // case 1: 50
            default:
                return 100;
        }
    }
}
