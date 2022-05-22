using System.Collections;
using Altzone.Scripts.Battle;
using Battle.Test.Scripts.Battle.Ball;
using Photon.Pun;
using UnityEngine;

namespace Battle.Test.Scripts.Test
{
    [RequireComponent(typeof(BallManager))]
    internal class BallManagerTest : MonoBehaviour
    {
        [Header("Test Actions")] public bool _setBallState;
        public bool _setBallPosition;
        public bool _setBallVelocity;
        public bool _stopBall;

        [Header("Test Settings")] public Vector2 _position;
        public Vector2 _velocity = Vector2.one;
        public BallState _state;

        [Header("Photon Master Client")] public bool _isAutoStart;
        public int _requiredPlayerCount;
        public int _realPlayerCount;

        private IBallManager _ball;

        private void Awake()
        {
            Debug.Log($"IsMasterClient {PhotonNetwork.IsMasterClient} OfflineMode {PhotonNetwork.OfflineMode}");
        }

        private IEnumerator Start()
        {
            for (;;)
            {
                _ball = BallManager.Get();
                if (_ball != null)
                {
                    break;
                }
                yield return null;
            }
            if (!_isAutoStart)
            {
                yield break;
            }
            while (!PhotonNetwork.InRoom)
            {
                yield return null;
            }
            Debug.Log(
                $"IsMasterClient {PhotonNetwork.IsMasterClient} OfflineMode {PhotonNetwork.OfflineMode} _requiredPlayerCount {_requiredPlayerCount}");
            if (PhotonNetwork.OfflineMode)
            {
                _realPlayerCount = 1;
            }
            else
            {
                while (PhotonNetwork.InRoom && PhotonNetwork.IsMasterClient)
                {
                    _realPlayerCount = PhotonBattle.CountRealPlayers();
                    if (_realPlayerCount >= _requiredPlayerCount)
                    {
                        break;
                    }
                    yield return null;
                }
            }
            // Auto start!
            _setBallState = true;
            _setBallPosition = true;
            _setBallVelocity = true;
        }

        private void Update()
        {
            if (_setBallState)
            {
                _setBallState = false;
                _ball.SetBallState(_state);
            }
            if (_setBallPosition)
            {
                _setBallPosition = false;
                _ball.SetBallPosition(_position);
            }
            if (_setBallVelocity)
            {
                _setBallVelocity = false;
                _ball.SetBallVelocity(_velocity);
            }
            else if (_stopBall)
            {
                _stopBall = false;
                _ball.SetBallVelocity(Vector2.zero);
            }
        }
    }
}