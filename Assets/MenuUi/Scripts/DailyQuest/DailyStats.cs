
using System;
using UnityEngine;

public class DailyStats : MonoBehaviour
{

    public static DailyStats Instance { get; private set; }


    private int _battlesPlayed = 0;
    private int _tasksDone = 0;


    private void Awake()
    {
        // Only one DailyStats Instance can exist at a time
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        UpdateDailyStatsFromServer();

        DailyTaskProgressManager.OnTaskDone += AddTask;
    }

    private void OnDestroy()
    {
        DailyTaskProgressManager.OnTaskDone -= AddTask;
    }


    

    /// <summary>
    /// Gets the Daily Stats from server
    /// </summary>
    [Obsolete("TODO: Make the method ACTUALLY get the stats from the SERVER")]
    private void UpdateDailyStatsFromServer()
    {

        DateTime dateFromServer = DateTime.Today; // Get this from server

        // If dateFromServer is today
        if (dateFromServer == DateTime.Today)
        {
            // Get variable values from server
            // _battlesPlayed =
            // _tasksDone =
        }
        // If dateFromServer is some other date
        else
        {
            // Set daily stats to 0
            _battlesPlayed = 0;
            _tasksDone = 0;

            UpdateDailyStatsToServer();
        }

    }

    /// <summary>
    /// Updates the Daily Stats to the server
    /// </summary>
    [Obsolete("TODO: Make the method ACTUALLY update the stats to the SERVER")]
    private void UpdateDailyStatsToServer()
    {
        // Code here
        // _battlesPlayed
        // _tasksDone
    }

    public void AddBattle()
    {
        _battlesPlayed += 1;
        UpdateDailyStatsToServer();
    }

    public void AddTask()
    {
        _tasksDone += 1;
        UpdateDailyStatsToServer();
    }

    public int GetBattlesPlayed()
    {
        return _battlesPlayed;
    }

    public int GetTasksDone()
    {
        return _tasksDone;
    }
}
