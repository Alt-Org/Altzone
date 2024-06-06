using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAnalyticsSDK;


namespace AltZone.Scripts.GA
{
    public class GameAnalyticsManager : MonoBehaviour 
    {
        private static GameAnalyticsManager instance;

        public static GameAnalyticsManager Instance
        {
            get { return instance; }
        }


        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            else
            {
                instance = this;
                DontDestroyOnLoad(gameObject);
            }

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

        public void BattleLaunch()
        {
            GameAnalytics.NewDesignEvent("battle:launched");
            Debug.Log("battle launced event");
        }

        public void OpenSoulHome()
        {
            GameAnalytics.NewDesignEvent("location:soulhome:open");
            Debug.Log("SoulHome has been opened");
        }
        }
    }
