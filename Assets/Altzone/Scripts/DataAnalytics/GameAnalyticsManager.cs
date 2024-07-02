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

        private int battlesStartedThisSession = 0;
        private int shieldHits = 0;
        private int wallHits = 0;

        private Dictionary<string, float> sectionStartTimes = new Dictionary<string, float>();

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
            GameAnalytics.SetEnabledManualSessionHandling(true);

            GameAnalytics.Initialize();
            OnInitialized?.Invoke(true);

        }

        public void BattleLaunch() //Milloin battle k‰ynnistet‰‰n
        {
            battlesStartedThisSession++;
            GameAnalytics.NewDesignEvent("battle:launched");
            Debug.Log("battle launched event");
        }

        public void OpenSoulHome() //Milloin sielunkotiin menn‰‰n
        {
            GameAnalytics.NewDesignEvent("location:soulhome:open");
            Debug.Log("SoulHome has been opened");
        }

        public void CharacterSelection(string characterName) //Mik‰ hahmo valitaan
        {
            GameAnalytics.NewDesignEvent("character:selected:" + characterName);
            Debug.Log($"Character selected: {characterName}");
        }

        public void CharacterWin(string characterName) //Mik‰ hahmo voittaa
        {
            GameAnalytics.NewDesignEvent("character:win:" + characterName);
            Debug.Log($"{characterName} won");
        }

        public void CharacterLoss(string characterName) //Mik‰ hahmo h‰vi‰‰
        {
            GameAnalytics.NewDesignEvent("character:loss:" + characterName);
            Debug.Log($"{characterName} lost");
        }

        public void EnterSection(string sectionName) //Aloittaa ajan kun pelin osaan menn‰‰n
        {
            float currentTime = Time.time;
            if (!sectionStartTimes.ContainsKey(sectionName))
            {
                sectionStartTimes.Add(sectionName, currentTime);
            }
            else
            {
                sectionStartTimes[sectionName] = currentTime;
            }
        }

        public void ExitSection(string sectionName) //paljonko pelin osissa vietet‰‰n aikaa
        {
            if (sectionStartTimes.ContainsKey(sectionName))
            {
                float startTime = sectionStartTimes[sectionName];
                float currentTime = Time.time;
                float timeSpent = currentTime - startTime;

                Dictionary<string, object> eventData = new Dictionary<string, object>();
                eventData["section"] = sectionName;
                eventData["event"] = "exit";
                eventData["Time_spent"] = timeSpent;

                GameAnalytics.NewDesignEvent($"section_event:{sectionName}:exit", eventData);
                Debug.Log($"Time spend: {timeSpent} seconds.");

                sectionStartTimes.Remove(sectionName);
            }
        }

        public void OnShieldHit() //laskee kilpiosumat
        {
            shieldHits++;
            Debug.Log($"Shield hit count: {shieldHits}");
        }

        public void OnWallHit() //laskee muuriosumat
        {
            wallHits++;
            Debug.Log($"Wall hit count: {wallHits}");
            SessionShieldHitsBetweenWallHits();

            shieldHits = 0;
        }

        public void OnSessionEnd() //kutsutaan sovelluksen sulkemisessa
        {
            // sending battles_started
            BattlesStarted();

            // sending wall_hits
            SessionWallHits();
        }

        
        // privaatti funktiot joka GameAnalyticsManager k‰sittelee

        private void SessionShieldHitsBetweenWallHits()
        {
            GameAnalytics.NewDesignEvent("session:shield_hits_between_wall_hits", shieldHits);
            Debug.Log($"Event sent: session:shield_hits_between_wall_hits with value {shieldHits}");
        }

        private void SessionWallHits()
        {
            GameAnalytics.NewDesignEvent("session:wall_hits", wallHits);
            Debug.Log($"Event sent: session:wall_hits with value {wallHits}");
        }

        private void BattlesStarted()
        {
            GameAnalytics.NewDesignEvent("session:battles_started", battlesStartedThisSession);
            Debug.Log($"Battles started this session: {battlesStartedThisSession}");
        }
    }
}
