using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Model.Poco.Game;
using Altzone.Scripts.Model.Poco.Player;
using UnityEngine;

public class DailyTaskProgressManager : MonoBehaviour
{
    public static DailyTaskProgressManager Instance { get; private set; }

    public PlayerTasks.PlayerTask CurrentPlayerTask { get; private set; }

    private List<DailyTaskProgressListener> _progressListeners = new List<DailyTaskProgressListener>();

    #region Delegates & Events

    public delegate void DTSendChatMessages();
    public static event DTSendChatMessages OnChatMessageSent;

    #endregion

    private void Awake()
    {
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

    public void AddListener(DailyTaskProgressListener listener)
    {
        _progressListeners.Add(listener);
    }

    public void UpdateTaskProgress()
    {
        
    }
}
