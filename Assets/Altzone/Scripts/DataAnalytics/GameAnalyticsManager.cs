using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAnalyticsSDK;
using UnityEngine.InputSystem;
using Photon.Realtime;


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
        private int moveCommandsCount = 0;
        private Dictionary<string, int> playerShieldHitsPerMatch = new Dictionary<string, int>();
        private Dictionary<string, int> teamWallHitsPerMatch = new Dictionary<string, int>();
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

        public void OnShieldHit(string player) //laskee kilpiosumat
        {
            shieldHits++;

            if (playerShieldHitsPerMatch.ContainsKey(player))
            {
                playerShieldHitsPerMatch[player]++;
            }
            else
            {
                playerShieldHitsPerMatch[player] = 1;
            }

            Debug.Log($"Shield hit count: {shieldHits}");
        }

        public void OnWallHit(string team) //laskee muuriosumat
        {
            wallHits++;

            if (teamWallHitsPerMatch.ContainsKey(team))
            {
                teamWallHitsPerMatch[team]++;
            }
            else
            {
                teamWallHitsPerMatch[team] = 1;
            }

            Debug.Log($"Wall hit count: {wallHits}");
            BattleShieldHitsBetweenWallHits();

            shieldHits = 0;
        }
        public void MoveCommand(Vector3 targetPosition) //Seuraa miss‰ hahmo liikkuu ja laskee liikkumisk‰skyt
        {
            moveCommandsCount++;
            var eventParams = new Dictionary<string, object>
            {
                { "x", targetPosition.x },
                { "y", targetPosition.y },
                { "z", targetPosition.z }
            };
            GameAnalytics.NewDesignEvent("move:command", eventParams);
            Debug.Log($"Move command to position {targetPosition}. total moves: {moveCommandsCount}");
        }

        public void OnSessionEnd() //kutsutaan sovelluksen sulkemisessa
        {
            BattlesStarted();
        }

        public void OnBattleEnd() //kutsutaan battlen lopetuksessa
        {
            MoveCommandSummary();
            BattleWallHits();
            ShieldHitsPerMatchPerPlayer();
            WallHitsPerMatchPerTeam();
        }

        public void DistanceToPlayer(Vector3 playerPosition, Vector3 otherPlayerPosition) //et‰isyys kanssapelaajaan
        {
            float distance = Vector3.Distance(playerPosition, otherPlayerPosition);

            var eventParams = new Dictionary<string, object>
            {
                {"distance", distance}
            };

            GameAnalytics.NewDesignEvent("battle:distance:player", eventParams);
            Debug.Log($"Distance to other player: {distance}");
        }

        public void DistanceToWall(Vector3 playerPosition, Vector3 wallPosition) //et‰isyys muuriin
        {
            float distance = Vector3.Distance(playerPosition, wallPosition);

            var eventParams = new Dictionary<string, object>
            {
                {"distance", distance}
            };

            GameAnalytics.NewDesignEvent("battle:distance:wall", eventParams);
            Debug.Log($"Distance to wall: {distance}");
        }

        // privaatti funktiot joka GameAnalyticsManager k‰sittelee

        private void BattleShieldHitsBetweenWallHits()
        {
            GameAnalytics.NewDesignEvent("battle:shield_hits_between_wall_hits", shieldHits);
            Debug.Log($"Event sent: battle:shield_hits_between_wall_hits with value {shieldHits}");
        }

        private void BattleWallHits()
        {
            GameAnalytics.NewDesignEvent("battle:wall_hits", wallHits);
            Debug.Log($"Event sent: battle:wall_hits with value {wallHits}");

            wallHits = 0;
        }

        private void BattlesStarted()
        {
            GameAnalytics.NewDesignEvent("session:battles_started", battlesStartedThisSession);
            Debug.Log($"Battles started this session: {battlesStartedThisSession}");
        }

        private void MoveCommandSummary()
        {
            GameAnalytics.NewDesignEvent("battle:command:count", moveCommandsCount);
            Debug.Log($"Total move commands: {moveCommandsCount}");

            moveCommandsCount = 0;
        }

        private void ShieldHitsPerMatchPerPlayer()
        {
            foreach (string key in playerShieldHitsPerMatch.Keys)
            {
                var eventParams = new Dictionary<string, object>
                {
                    { "ShieldHitsPerMatch", playerShieldHitsPerMatch[key] }
                };
                GameAnalytics.NewDesignEvent($"battle:shield_hits_per_match:{key}", eventParams);
                Debug.Log($"Player {key} shield hits this match: {playerShieldHitsPerMatch[key]}");
            }

            playerShieldHitsPerMatch = new Dictionary<string, int>(); ;
        }

        private void WallHitsPerMatchPerTeam()
        {
            foreach (string key in teamWallHitsPerMatch.Keys)
            {
                var eventParams = new Dictionary<string, object>
                {
                    { "WallHitsPerMatch", teamWallHitsPerMatch[key] }
                };
                GameAnalytics.NewDesignEvent($"battle:wall_hits_per_match:{key}", eventParams);
                Debug.Log($"Team {key} wall hits this match: {teamWallHitsPerMatch[key]}");
            }

            teamWallHitsPerMatch = new Dictionary<string, int>(); ;
        }
    }
}
