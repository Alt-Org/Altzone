using System.Collections;
using Battle.Test.Scripts.Battle.Ball;
using Photon.Pun;
using UnityEngine;

namespace Battle.Test.Scripts.Test
{
    internal class BallManagerTest : MonoBehaviour
    {
        [Header("Test Settings")] public Vector2 _position;
        public Vector2 _velocity;
        public BallState _state;
        
        [Header("Debug Actions")] public bool _isBallPosition;
        public bool _isBallVelocity;
        public bool _isSetBallState;

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
        }

        private void Update()
        {
            if (_isBallPosition)
            {
                _isBallPosition = false;
                _ball.SetBallPosition(_position);
                return;
            }
            if (_isBallVelocity)
            {
                _isBallVelocity = false;
                _ball.SetBallVelocity(_velocity);
                return;
            }
            if (_isSetBallState)
            {
                _isSetBallState = false;
                _ball.SetBallState(_state);
            }
        }
    }
}