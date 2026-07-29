using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameTimeManager : MonoBehaviour
{
    public static GameTimeManager Instance { get; private set; }

    private float maxPlayTime; // Max hours allowed per day
    private float playedTimeToday; // Hours played today
    private DateTime lastPlayDate; // Last recorded play date
    private bool timeExpired = false; // Has the time limit been reached?

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        LoadTimeData();
        ResetTimeIfNeeded();
    }

    void Update()
    {
        if (!timeExpired)
        {
            playedTimeToday += Time.deltaTime / 3600f; // Convert seconds to hours
            PlayerPrefs.SetFloat("PlayedTimeToday", playedTimeToday);
            PlayerPrefs.Save();

            if (playedTimeToday >= maxPlayTime)
            {
                timeExpired = true;
                BattleManager.Instance.ShowTimeWarning(); // Notify battle system
            }
        }
    }

    private void LoadTimeData()
    {
        maxPlayTime = PlayerPrefs.GetFloat("MaxPlayTime", 2f); // Default: 2 hours
        playedTimeToday = PlayerPrefs.GetFloat("PlayedTimeToday", 0);
        lastPlayDate = DateTime.Parse(PlayerPrefs.GetString("LastPlayDate", DateTime.Now.ToString()));
    }

    private void ResetTimeIfNeeded()
    {
        if (lastPlayDate.Date != DateTime.Now.Date)
        {
            playedTimeToday = 0;
            PlayerPrefs.SetFloat("PlayedTimeToday", playedTimeToday);
            PlayerPrefs.SetString("LastPlayDate", DateTime.Now.ToString());
            PlayerPrefs.Save();
            timeExpired = false;
        }
    }

    public bool IsTimeExpired()
    {
        return timeExpired;
    }

    public void ReturnToMainMenu()
    {
        Debug.Log("Returning to main menu...");
        SceneManager.LoadScene("MainMenu"); // Load the main menu scene
    }
}
