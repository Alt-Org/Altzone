using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAnalyticsSDK;


namespace AltZone.Scripts.GA
{
    public class GameAnalyticsManager
    {
        private static bool isInitialized = false;

        public static void Initialize()
        {
            if (!isInitialized)
            {
                var customUserId = PlayerPrefs.GetString("customUserId", null);
                if (string.IsNullOrEmpty(customUserId))
                {
                    customUserId = Guid.NewGuid().ToString();
                    PlayerPrefs.SetString("customUserId", customUserId);
                }
                Debug.Log($"GA user ID is {customUserId}");

                GameAnalytics.SetCustomId(customUserId);
                GameAnalytics.Initialize();
                isInitialized = true;
            }
        }

        public static void BattleLaunch()
        {
            if (!isInitialized) Initialize();
            GameAnalytics.NewDesignEvent("battle:launched");
            Debug.Log("Battle launch event logged");
        }

        public static void OpenSoulHome()
        {
            if (!isInitialized) Initialize();
            GameAnalytics.NewDesignEvent("location:soulhome:open");
            Debug.Log("SoulHome opened event logged");
        }
    }
}
