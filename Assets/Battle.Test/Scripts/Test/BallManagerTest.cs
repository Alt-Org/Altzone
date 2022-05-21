using System.Collections;
using Battle.Test.Scripts.Battle.Ball;
using Photon.Pun;
using UnityEngine;

namespace Battle.Test.Scripts.Test
{
    [RequireComponent(typeof(BallManager))]
    internal class BallManagerTest : MonoBehaviour
    {
        [Header("Test Settings")] public Vector2 _position;
        public Vector2 _velocity = Vector2.one;
        public BallState _state;
        
        [Header("Debug Actions")] public bool _setBallPosition;
        public bool _setBallVelocity;
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
            if (_setBallPosition)
            {
                _setBallPosition = false;
                _ball.SetBallPosition(_position);
                return;
            }
            if (_setBallVelocity)
            {
                _setBallVelocity = false;
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