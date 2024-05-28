using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAnalyticsSDK;

public class GameAnalyticsManager : MonoBehaviour
{
    private static GameAnalyticsManager instance;

    private void Awake()
    {
        instance = this;

        DontDestroyOnLoad(this.gameObject);

        Initialize(out string customerUserId, (success) =>
        {
            Debug.Log($"GameAnalytics initialization success: {success}");
        });
    }

    public virtual void Initialize(out string customUserId, Action<bool> OnInitialized)
    {
        var playerPrefsKey = $"{nameof(GameAnalyticsManager)}.customUserId";

        customUserId = PlayerPrefs.GetString(playerPrefsKey, null);
        if (string.IsNullOrEmpty(customUserId))
        {
            customUserId = Guid.NewGuid().ToString();
            PlayerPrefs.SetString(playerPrefsKey, customUserId);
        }
        Debug.Log($"GA user ID is {customUserId}");

        GameAnalytics.SetCustomId(customUserId);
        GameAnalytics.Initialize();
        OnInitialized?.Invoke(true);
    }
   
    public void LogProgressionEvent(GAProgressionStatus status, string biomeName)
    {
        Debug.Log($"Logging progression event: {status} - {biomeName}");
        GameAnalytics.NewProgressionEvent(status, biomeName);
    }
}
