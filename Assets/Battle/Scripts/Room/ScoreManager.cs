using System;
using System.Collections;
using Altzone.Scripts.Battle;
using Altzone.Scripts.Config;
using Battle.Scripts.Player;
using Photon.Pun;
using Prg.Scripts.Common.Photon;
using Prg.Scripts.Common.PubSub;
using Prg.Scripts.Common.Unity.Window;
using Prg.Scripts.Common.Unity.Window.ScriptableObjects;
using UnityConstants;
using UnityEngine;

namespace Battle.Scripts.Room
{
    /// <summary>
    /// Data holder class for team score.
    /// </summary>
    [Serializable]
    public class TeamScore
    {
        public int _teamNumber;
        public int _headCollisionCount;
        public int _wallCollisionCount;

        public byte[] ToBytes()
        {
            return new[] { (byte)_teamNumber, (byte)_headCollisionCount, (byte)_wallCollisionCount };
        }

        public static void FromBytes(object data, out int teamNumber, out int headCollisionCount, out int wallCollisionCount)
        {
            var payload = (byte[])data;
            teamNumber = payload[0];
            headCollisionCount = payload[1];
            wallCollisionCount = payload[2];
        }

        public static TeamScore[] AllocateTeamScores()
        {
            var scores = new TeamScore[2];
            var teamIndex = PhotonBattle.GetTeamIndex(PhotonBattle.TeamBlueValue);
            scores[teamIndex] = new TeamScore
            {
                _teamNumber = PhotonBattle.TeamBlueValue
            };
            teamIndex = PhotonBattle.GetTeamIndex(PhotonBattle.TeamRedValue);
            scores[teamIndex] = new TeamScore
            {
                _teamNumber = PhotonBattle.TeamRedValue
            };
            return scores;
        }

        public override string ToString()
        {
            return $"team: {_teamNumber}, headCollision: {_headCollisionCount}, wallCollision: {_wallCollisionCount}";
        }
    }

    /// <summary>
    /// Optional score manager for the room that synchronizes game score over network.
    /// </summary>
    public class ScoreManager : MonoBehaviour
    {
        private const int MsgSetTeamScore = PhotonEventDispatcher.eventCodeBase + 6;

        [SerializeField] private WindowDef _gameOverWindow;

        private static ScoreManager Get()
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<ScoreManager>();
            }
            return _instance;
        }

        private static ScoreManager _instance;

        [SerializeField] private TeamScore[] _scores;

        private PhotonEventDispatcher _photonEventDispatcher;

        private void Awake()
        {
            _scores = TeamScore.AllocateTeamScores();
            _photonEventDispatcher = PhotonEventDispatcher.Get();
            _photonEventDispatcher.registerEventListener(MsgSetTeamScore, data => { OnSetTeamScore(data.CustomData); });
        }

        private void OnDestroy()
        {
            _instance = null;
        }

        private void OnEnable()
        {
            this.Subscribe<TeamScoreEvent>(OnTeamScoreEvent);
            // Set initial state for scores
            SendTeamNames();
            this.Publish(new TeamScoreEvent(_scores[0]));
            this.Publish(new TeamScoreEvent(_scores[1]));
        }

        private void OnDisable()
        {
            this.Unsubscribe();
        }

        private void SendTeamNames()
        {
            var room = PhotonNetwork.CurrentRoom;
            string homeTeamName;
            string visitorTeamName;
            if (PlayerActivator.HomeTeamNumber == PhotonBattle.TeamBlueValue)
            {
                homeTeamName = room.GetCustomProperty<string>(PhotonBattle.TeamBlueNameKey);
                visitorTeamName = room.GetCustomProperty<string>(PhotonBattle.TeamRedNameKey);
            }
            else
            {
                homeTeamName = room.GetCustomProperty<string>(PhotonBattle.TeamRedNameKey);
                visitorTeamName = room.GetCustomProperty<string>(PhotonBattle.TeamBlueNameKey);
            }
            this.Publish(new TeamNameEvent(homeTeamName, visitorTeamName));
        }

        private void SendSetTeamScore(TeamScore score)
        {
            _photonEventDispatcher.RaiseEvent(MsgSetTeamScore, score.ToBytes());
        }

        private void OnSetTeamScore(object data)
        {
            TeamScore.FromBytes(data, out var teamNumber, out var headCollisionCount, out var wallCollisionCount);
            var teamIndex = PhotonBattle.GetTeamIndex(teamNumber);
            var score = _scores[teamIndex];
            // Update and publish new score
            score._headCollisionCount = headCollisionCount;
            score._wallCollisionCount = wallCollisionCount;
            this.Publish(new TeamScoreEvent(score));
        }

        private void OnTeamScoreEvent(TeamScoreEvent data)
        {
            var scoreNew = data.Score;
            var teamIndex = PhotonBattle.GetTeamIndex(scoreNew._teamNumber);
            var score = _scores[teamIndex];
            score._headCollisionCount = scoreNew._headCollisionCount;
            score._wallCollisionCount = scoreNew._wallCollisionCount;
            // Check for winning condition
            if (!PhotonNetwork.InRoom)
            {
                return;
            }
            var variables = RuntimeGameConfig.Get().Variables;
            foreach (var teamScore in _scores)
            {
                if (teamScore._headCollisionCount >= variables._headScoreToWin
                || teamScore._wallCollisionCount >= variables._wallScoreToWin)
                {
                    var room = PhotonNetwork.CurrentRoom;
                    var scoreKey = teamScore._teamNumber == PhotonBattle.TeamBlueValue
                        ? PhotonBattle.TeamBlueScoreKey
                        : PhotonBattle.TeamRedScoreKey;
                    room.SetCustomProperty(scoreKey, 1);
                    scoreKey = teamScore._teamNumber != PhotonBattle.TeamBlueValue
                        ? PhotonBattle.TeamBlueScoreKey
                        : PhotonBattle.TeamRedScoreKey;
                    room.SetCustomProperty(scoreKey, 0);
                    StartCoroutine(LoadGameOverWindow(_gameOverWindow));
                    break;
                }
            }
        }

        private static IEnumerator LoadGameOverWindow(WindowDef window)
        {
            yield return null;
            WindowManager.Get().ShowWindow(window);
        }

        private void _addHeadScore(int teamIndex)
        {
            var score = _scores[teamIndex];
            score._headCollisionCount += 1;
            // Send updated score to everybody
            SendSetTeamScore(score);
        }

        private void _addWallScore(int teamIndex)
        {
            var score = _scores[teamIndex];
            score._wallCollisionCount += 1;
            // Send updated score to everybody
            SendSetTeamScore(score);
        }

        public static void AddHeadScore(int teamNumber)
        {
            var manager = Get();
            if (PhotonNetwork.IsMasterClient && manager != null)
            {
                var teamIndex = PhotonBattle.GetTeamIndex(teamNumber);
                manager._addHeadScore(teamIndex);
            }
        }

        public static void AddWallScore(GameObject gameObject)
        {
            var manager = Get();
            if (PhotonNetwork.IsMasterClient && manager != null)
            {
                if (gameObject.CompareTag(Tags.BotSide))
                {
                    manager._addWallScore(PhotonBattle.GetTeamIndex(PhotonBattle.TeamBlueValue));
                }
                else if (gameObject.CompareTag(Tags.TopSide))
                {
                    manager._addWallScore(PhotonBattle.GetTeamIndex(PhotonBattle.TeamRedValue));
                }
            }
        }

        internal class TeamNameEvent
        {
            public readonly string TeamBlueName;
            public readonly string TeamRedName;

            public TeamNameEvent(string teamBlueName, string teamRedName)
            {
                TeamBlueName = teamBlueName;
                TeamRedName = teamRedName;
            }
        }

        internal class TeamScoreEvent
        {
            public readonly TeamScore Score;

            public TeamScoreEvent(TeamScore score)
            {
                this.Score = score;
            }

            public override string ToString()
            {
                return $"{nameof(Score)}: {Score}";
            }
        }
    }
}