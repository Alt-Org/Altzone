using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameAnalyticsSDK;
using UnityEngine.InputSystem;
using Photon.Realtime;


namespace Altzone.Scripts.GA
{
    public class GameAnalyticsManager : MonoBehaviour
    {
        private static GameAnalyticsManager s_instance;

        public static GameAnalyticsManager Instance
        {
            get { return s_instance; }
        }

        private int _battlesStartedThisSession = 0;
        private int _shieldHits = 0;
        private int _wallHits = 0;
        private int _moveCommandsCount = 0;
        private int _clanChanges = 0;
        private Dictionary<string, int> _playerShieldHitsPerMatch = new Dictionary<string, int>();
        private Dictionary<string, int> _teamWallHitsPerMatch = new Dictionary<string, int>();
        private Dictionary<string, float> _sectionStartTimes = new Dictionary<string, float>();

        private void Awake()
        {
            if (s_instance != null && s_instance != this)
            {
                Destroy(gameObject);
                return;
            }
            else
            {
                s_instance = this;
                DontDestroyOnLoad(gameObject);
            }

            Initialize(out string customerUserId, (success) =>
            {
                Debug.Log($"GameAnalytics initialization success: {success}");
            });
        }

        public virtual void Initialize(out string customUserId, Action<bool> OnInitialized)
        {
            string playerPrefsKey = $"{nameof(GameAnalyticsManager)}.customUserId";

            customUserId = PlayerPrefs.GetString(playerPrefsKey, null);
            if (string.IsNullOrEmpty(customUserId))
            {
                customUserId = Guid.NewGuid().ToString();
                PlayerPrefs.SetString(playerPrefsKey, customUserId);
            }
            Debug.Log($"GA user ID is {customUserId}");
            GameAnalytics.SetCustomId(customUserId);
            GameAnalytics.SetExternalUserId(customUserId);

            GameAnalytics.Initialize();
            OnInitialized?.Invoke(true);

        }

        public void BattleLaunch() //Milloin battle käynnistetään
        {
            _battlesStartedThisSession++;
            GameAnalytics.NewDesignEvent("battle:launched");
            Debug.Log("battle launched event");
        }

        public void OpenSoulHome() //Milloin sielunkotiin mennään
        {
            GameAnalytics.NewDesignEvent("location:soulhome:open");
            Debug.Log("SoulHome has been opened");
        }

        public void CharacterSelection(string characterName) //Mikä hahmo valitaan
        {
            GameAnalytics.NewDesignEvent("character:selected:" + characterName);
            Debug.Log($"Character selected: {characterName}");
        }

        public void CharacterWin(string characterName) //Mikä hahmo voittaa
        {
            GameAnalytics.NewDesignEvent("character:win:" + characterName);
            Debug.Log($"{characterName} won");
        }

        public void CharacterLoss(string characterName) //Mikä hahmo häviää
        {
            GameAnalytics.NewDesignEvent("character:loss:" + characterName);
            Debug.Log($"{characterName} lost");
        }

        public void EnterSection(string sectionName) //Aloittaa ajan kun pelin osaan mennään
        {
            float currentTime = Time.time;
            if (!_sectionStartTimes.ContainsKey(sectionName))
            {
                _sectionStartTimes.Add(sectionName, currentTime);
            }
            else
            {
                _sectionStartTimes[sectionName] = currentTime;
            }
        }

        public void ExitSection(string sectionName) //paljonko pelin osissa vietetään aikaa
        {
            if (_sectionStartTimes.ContainsKey(sectionName))
            {
                float startTime = _sectionStartTimes[sectionName];
                float currentTime = Time.time;
                float timeSpent = currentTime - startTime;

                Dictionary<string, object> eventData = new Dictionary<string, object>();
                eventData["section"] = sectionName;
                eventData["event"] = "exit";
                eventData["Time_spent"] = timeSpent;

                GameAnalytics.NewDesignEvent($"section_event:{sectionName}:exit", timeSpent);
                Debug.Log($"Time spend: {timeSpent} seconds.");

                _sectionStartTimes.Remove(sectionName);
            }
        }

        public void OnShieldHit(string player) //laskee kilpiosumat
        {
            _shieldHits++;

            if (_playerShieldHitsPerMatch.ContainsKey(player))
            {
                _playerShieldHitsPerMatch[player]++;
            }
            else
            {
                _playerShieldHitsPerMatch[player] = 1;
            }

            Debug.Log($"Shield hit count: {_shieldHits}");
        }

        public void OnWallHit(string team) //laskee muuriosumat
        {
            _wallHits++;

            if (_teamWallHitsPerMatch.ContainsKey(team))
            {
                _teamWallHitsPerMatch[team]++;
            }
            else
            {
                _teamWallHitsPerMatch[team] = 1;
            }

            Debug.Log($"Wall hit count: {_wallHits}");
            BattleShieldHitsBetweenWallHits();

            _shieldHits = 0;
        }
        public void MoveCommand(Vector3 targetPosition) //Seuraa missä hahmo liikkuu ja laskee liikkumiskäskyt
        {
            _moveCommandsCount++;
            var eventParams = new Dictionary<string, object>
            {
                { "x", targetPosition.x },
                { "y", targetPosition.y },
                { "z", targetPosition.z }
            };
            GameAnalytics.NewDesignEvent("move:command", eventParams);
            Debug.Log($"Move command to position {targetPosition}. total moves: {_moveCommandsCount}");
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

        public void DistanceToPlayer(float distance) //etäisyys kanssapelaajaan
        {
            //float distance = Vector3.Distance(playerPosition, otherPlayerPosition);

            var eventParams = new Dictionary<string, object>
            {
                {"distance", distance}
            };

            GameAnalytics.NewDesignEvent("battle:distance:player", distance);
            Debug.Log($"Distance to other player: {distance}");
        }

        public void DistanceToWall(float distance) //etäisyys muuriin
        {
            //float distance = Vector3.Distance(playerPosition, wallPosition);

            var eventParams = new Dictionary<string, object>
            {
                {"distance", distance}
            };

            GameAnalytics.NewDesignEvent("battle:distance:wall", distance);
            Debug.Log($"Distance to wall: {distance}");
        }

        public void ClanChange(string newClan) //laskee klaanien vaihdot
        {
            _clanChanges++;
            var eventParams = new Dictionary<string, object>
            {
                {"clan", newClan},
                {"totalClanChanges", _clanChanges }
            };

            GameAnalytics.NewDesignEvent("clan:change", eventParams);
            Debug.Log($"Clan changed to {newClan}. Total clan changes: {_clanChanges}");
        }

        // privaatti funktiot joka GameAnalyticsManager käsittelee

        private void BattleShieldHitsBetweenWallHits()
        {
            GameAnalytics.NewDesignEvent("battle:shield_hits_between_wall_hits", _shieldHits);
            Debug.Log($"Event sent: battle:shield_hits_between_wall_hits with value {_shieldHits}");
        }

        private void BattleWallHits()
        {
            GameAnalytics.NewDesignEvent("battle:wall_hits", _wallHits);
            Debug.Log($"Event sent: battle:wall_hits with value {_wallHits}");

            _wallHits = 0;
        }

        private void BattlesStarted()
        {
            GameAnalytics.NewDesignEvent("session:battles_started", _battlesStartedThisSession);
            Debug.Log($"Battles started this session: {_battlesStartedThisSession}");
        }

        private void MoveCommandSummary()
        {
            GameAnalytics.NewDesignEvent("battle:command:count", _moveCommandsCount);
            Debug.Log($"Total move commands: {_moveCommandsCount}");

            _moveCommandsCount = 0;
        }

        private void ShieldHitsPerMatchPerPlayer()
        {
            foreach (string key in _playerShieldHitsPerMatch.Keys)
            {
                var eventParams = new Dictionary<string, object>
                {
                    { "ShieldHitsPerMatch", _playerShieldHitsPerMatch[key] }
                };
                GameAnalytics.NewDesignEvent($"battle:shield_hits_per_match:{key}", _playerShieldHitsPerMatch[key]);
                Debug.Log($"Player {key} shield hits this match: {_playerShieldHitsPerMatch[key]}");
            }

            _playerShieldHitsPerMatch = new Dictionary<string, int>(); ;
        }

        private void WallHitsPerMatchPerTeam()
        {
            foreach (string key in _teamWallHitsPerMatch.Keys)
            {
                var eventParams = new Dictionary<string, object>
                {
                    { "WallHitsPerMatch", _teamWallHitsPerMatch[key] }
                };
                GameAnalytics.NewDesignEvent($"battle:wall_hits_per_match:{key}", _teamWallHitsPerMatch[key]);
                Debug.Log($"Team {key} wall hits this match: {_teamWallHitsPerMatch[key]}");
            }

            _teamWallHitsPerMatch = new Dictionary<string, int>(); ;
        }
    }
}
