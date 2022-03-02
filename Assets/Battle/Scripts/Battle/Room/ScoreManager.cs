using Battle.Scripts.Battle.interfaces;
using Photon.Pun;
using Prg.Scripts.Common.Photon;
using Prg.Scripts.Common.PubSub;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Scripts.Battle.Room
{
    /// <summary>
    /// Collects local scores and synchronizes them over network.
    /// </summary>
    /// <remarks>
    /// Scores are transformed from input <c>ScoreEvent</c> to output <c>GameScoreEvent</c> during the process.
    /// </remarks>
    internal class ScoreManager : MonoBehaviour
    {
        private readonly LocalScore _localScore = new LocalScore();
        private readonly NetworkScore _networkScore = new NetworkScore();

        private void Awake()
        {
            Debug.Log($"Awake");
            _localScore.Initialize();
            _networkScore.Initialize();
            _networkScore.LocalScore = _localScore;
            this.Subscribe<ScoreEvent>(OnScoreEvent);
        }

        private void OnDestroy()
        {
            this.Unsubscribe();
            _localScore.UnInitialize();
            _networkScore.UnInitialize();
        }

        private void OnScoreEvent(ScoreEvent data)
        {
            Debug.Log($"OnScoreEvent {data}");
            AddScore(data.ScoreType, data.ScoreAmount);
        }

        private void AddScore(ScoreType scoreType, int scoreAmount)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            Debug.Log($"AddScore {scoreType} {scoreAmount}");
            Assert.IsTrue(scoreAmount > 0, "scoreAmount > 0");
            _networkScore.AddScore(scoreType, scoreAmount);
        }

        /// <summary>
        /// Single score "point" for one team.
        /// </summary>
        internal class ScoreEvent
        {
            public readonly ScoreType ScoreType;
            public readonly int ScoreAmount;

            public ScoreEvent(ScoreType scoreType, int scoreAmount = 1)
            {
                ScoreType = scoreType;
                ScoreAmount = scoreAmount;
            }

            public override string ToString()
            {
                return $"Score: {ScoreType} : {ScoreAmount}";
            }
        }

        /// <summary>
        /// Current game score state for all teams.
        /// </summary>
        internal class GameScoreEvent
        {
            public int TeamBlueHeadScore => _teamBlueHeadScore;
            public int TeamBlueWallScore => _teamBlueWallScore;
            public int TeamRedHeadScore => _teamRedHeadScore;
            public int TeamRedWallScore => _teamRedWallScore;

            private int _teamBlueHeadScore;
            private int _teamBlueWallScore;
            private int _teamRedHeadScore;
            private int _teamRedWallScore;

            internal GameScoreEvent(int teamBlueHeadScore, int teamBlueWallScore, int teamRedHeadScore, int teamRedWallScore)
            {
                _teamBlueHeadScore = teamBlueHeadScore;
                _teamBlueWallScore = teamBlueWallScore;
                _teamRedHeadScore = teamRedHeadScore;
                _teamRedWallScore = teamRedWallScore;
            }

            internal void AddScore(ScoreType scoreType, int scoreAmount)
            {
                switch (scoreType)
                {
                    case ScoreType.BlueWall:
                        _teamBlueWallScore += scoreAmount;
                        break;
                    case ScoreType.BlueHead:
                        _teamBlueHeadScore += scoreAmount;
                        break;
                    case ScoreType.RedWall:
                        _teamRedWallScore += scoreAmount;
                        break;
                    case ScoreType.RedHead:
                        _teamRedHeadScore += scoreAmount;
                        break;
                }
            }

            public override string ToString()
            {
                return $"BH: {TeamBlueHeadScore}, BW: {TeamBlueWallScore}, RH: {TeamRedHeadScore}, RW: {TeamRedWallScore}";
            }
        }

        /// <summary>
        /// Manages local game score and publishes scores to local listeners.
        /// </summary>
        private class LocalScore
        {
            private readonly GameScoreEvent _currentScore =
                new GameScoreEvent(0, 0, 0, 0);

            public void Initialize()
            {
                // NOP.
            }

            public void UnInitialize()
            {
                // NOP.
            }

            public void AddScore(ScoreType scoreType, int scoreAmount)
            {
                _currentScore.AddScore(scoreType, scoreAmount);
                this.Publish(_currentScore);
            }
        }

        /// <summary>
        /// Manages scores over network
        /// </summary>
        private class NetworkScore
        {
            private const int MsgSendScore = PhotonEventDispatcher.EventCodeBase + 4;

            public LocalScore LocalScore { get; set; }

            private PhotonEventDispatcher _photonEventDispatcher;

            public void Initialize()
            {
                if (_photonEventDispatcher == null)
                {
                    _photonEventDispatcher = PhotonEventDispatcher.Get();
                    _photonEventDispatcher.RegisterEventListener(MsgSendScore, data => { OnSendScore(data.CustomData); });
                }
            }

            public void UnInitialize()
            {
                // No need to unregister anything.
            }

            #region Photon Events

            private void OnSendScore(object data)
            {
                var payload = (byte[])data;
                Assert.AreEqual(3, payload.Length, "Invalid message length");
                var index = 0;
                Assert.AreEqual((byte)MsgSendScore, payload[index], "Invalid message id");
                var scoreType = (ScoreType)payload[++index];
                var scoreAmount = (int)payload[++index];
                LocalScore.AddScore(scoreType, scoreAmount);
            }

            public void AddScore(ScoreType scoreType, int scoreAmount)
            {
                var payload = new[] { (byte)MsgSendScore, (byte)scoreType, (byte)scoreAmount };
                _photonEventDispatcher.RaiseEvent(MsgSendScore, payload);
            }

            #endregion
        }
    }
}