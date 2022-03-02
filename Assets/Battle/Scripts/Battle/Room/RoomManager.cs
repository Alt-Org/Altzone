using System.Collections;
using Altzone.Scripts.Battle;
using Altzone.Scripts.Config;
using Battle.Scripts.Battle.Factory;
using Battle.Scripts.Battle.interfaces;
using Photon.Pun;
using Prg.Scripts.Common.PubSub;
using Prg.Scripts.Common.Unity.Window;
using Prg.Scripts.Common.Unity.Window.ScriptableObjects;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Scripts.Battle.Room
{
    /// <summary>
    /// Manages room gameplay state from start to game over.
    /// </summary>
    internal class RoomManager : MonoBehaviour
    {
        private class TeamInfo
        {
            public readonly int Team;
            public readonly int Score;
            public readonly string ScoreKey;

            public TeamInfo(int team, int score, string scoreKey)
            {
                Team = team;
                Score = score;
                ScoreKey = scoreKey;
            }

            public override string ToString()
            {
                return $"Team: {Team}, Score: {Score}";
            }
        }

        [Header("Live Data"), SerializeField] private int _requiredActorCount;
        [SerializeField] private int _currentActorCount;
        [SerializeField] private bool _isWaitForActors;
        [SerializeField] private bool _isWaitForCountdown;
        [SerializeField] private WindowDef _gameOverWindow;

        private IPlayerManager _playerManager;

        private void Awake()
        {
            _requiredActorCount = 1 + PhotonBattle.CountRealPlayers();
            _currentActorCount = 0;
            _isWaitForActors = true;
            _isWaitForCountdown = false;
            Debug.Log($"Awake required {_requiredActorCount} master {PhotonNetwork.IsMasterClient}");
            this.Subscribe<ActorReportEvent>(OnActorReportEvent);
            this.Subscribe<ScoreManager.GameScoreEvent>(OnGameScoreEvent);
        }

        private void OnDestroy()
        {
            this.Unsubscribe();
        }

        private void OnActorReportEvent(ActorReportEvent data)
        {
            _currentActorCount += 1;
            Debug.Log(
                $"OnActorReportEvent component {data.ComponentTypeId} required {_requiredActorCount} current {_currentActorCount} master {PhotonNetwork.IsMasterClient}");
            Assert.IsTrue(_currentActorCount <= _requiredActorCount);
            if (_currentActorCount == _requiredActorCount)
            {
                Assert.IsTrue(_isWaitForActors);
                _isWaitForActors = false;
                Assert.IsFalse(_isWaitForCountdown);
                _isWaitForCountdown = true;
                _playerManager = Context.GetPlayerManager;
                _playerManager.StartCountdown(OnCountdownFinished);
            }
        }

        private void OnGameScoreEvent(ScoreManager.GameScoreEvent data)
        {
            Debug.Log($"OnGameScoreEvent {data}");

            var variables = RuntimeGameConfig.Get().Variables;
            if (data.TeamBlueHeadScore >= variables._headScoreToWin ||
                data.TeamBlueWallScore >= variables._wallScoreToWin)
            {
                GameOver(PhotonBattle.TeamBlueValue, data);
                return;
            }
            if (data.TeamRedHeadScore >= variables._headScoreToWin ||
                data.TeamRedWallScore >= variables._wallScoreToWin)
            {
                GameOver(PhotonBattle.TeamRedValue, data);
            }
        }

        private void GameOver(int winningTeam, ScoreManager.GameScoreEvent data)
        {
            var blue = new TeamInfo(PhotonBattle.TeamBlueValue, data.TeamBlueHeadScore + data.TeamBlueWallScore, PhotonBattle.TeamBlueScoreKey);
            var red = new TeamInfo(PhotonBattle.TeamRedValue, data.TeamRedHeadScore + data.TeamRedWallScore, PhotonBattle.TeamRedScoreKey);
            Debug.Log($"GameOver win {winningTeam} : {blue} : {red}");
            var room = PhotonNetwork.CurrentRoom;
            var props = new ExitGames.Client.Photon.Hashtable
            {
                { PhotonBattle.TeamWinKey, winningTeam },
                { blue.ScoreKey, blue.Score },
                { red.ScoreKey, red.Score },
            };
            room.SetCustomProperties(props);
            // Unsubscribe now to be on the safe side
            this.Unsubscribe<ScoreManager.GameScoreEvent>(OnGameScoreEvent);

            StartCoroutine(LoadGameOverWindow());
        }

        private IEnumerator LoadGameOverWindow()
        {
            yield return null;
            Debug.Log($"LoadGameOverWindow {_gameOverWindow}");
            WindowManager.Get().ShowWindow(_gameOverWindow);
        }

        private void OnCountdownFinished()
        {
            Debug.Log($"OnCountdownFinished master {PhotonNetwork.IsMasterClient}");
            Assert.IsTrue(_isWaitForCountdown);
            _isWaitForCountdown = false;
            _playerManager.StartGameplay();
        }

        internal class ActorReportEvent
        {
            public readonly int ComponentTypeId;

            public ActorReportEvent(int componentTypeId)
            {
                ComponentTypeId = componentTypeId;
            }
        }
    }
}