using System;
using System.Diagnostics;
using Altzone.Scripts.Config;
using Photon.Pun;
using TMPro;
using UnityEngine;

namespace Battle.Test.Scripts.Battle.Ball
{
    internal enum BallState : byte
    {
        NoTeam = 0,
        RedTeam = 1,
        BlueTeam = 2,
        Ghosted = 3,
        Hidden = 4,
    }

    internal interface IBallManager
    {
        void SetBallPosition(Vector2 position);

        void SetBallVelocity(Vector2 velocity);

        void SetBallState(BallState ballState);
    }

    internal class BallManager : MonoBehaviour, IBallManager
    {
        [Serializable]
        internal class DebugSettings
        {
            public bool _isShowBallText;
            public TextMeshPro _ballText;
        }

        private static readonly BallState[] BallStates =
            { BallState.NoTeam, BallState.RedTeam, BallState.BlueTeam, BallState.Ghosted, BallState.Hidden };

        private static readonly bool[] ColliderStates = { true, true, true, false, false };

        public static BallManager Get() => FindObjectOfType<BallManager>();

        [Header("Settings"), SerializeField] private GameObject _ballCollider;
        [SerializeField] private GameObject _spriteNoTeam;
        [SerializeField] private GameObject _spriteRedTeam;
        [SerializeField] private GameObject _spriteBlueTeam;
        [SerializeField] private GameObject _spriteGhosted;
        [SerializeField] private GameObject _spriteHidden;

        [Header("Live Data"), SerializeField] private BallState _ballState;

        [Header("Debug Settings"), SerializeField] private DebugSettings _debug;

        private PhotonView _photonView;
        private Rigidbody2D _rigidbody;
        private GameObject[] _sprites;

        private float _ballMoveSpeedMultiplier;
        private float _ballMinMoveSpeed;
        private float _ballMaxMoveSpeed;
        private float _ballLerpSmoothingFactor;
        private float _ballTeleportDistance;

        private void Awake()
        {
            Debug.Log($"{name}");
            _photonView = PhotonView.Get(this);
            _rigidbody = GetComponent<Rigidbody2D>();
            var variables = RuntimeGameConfig.Get().Variables;
            _ballMoveSpeedMultiplier = variables._ballMoveSpeedMultiplier;
            _ballMinMoveSpeed = variables._ballMinMoveSpeed;
            _ballMaxMoveSpeed = variables._ballMaxMoveSpeed;
            _ballLerpSmoothingFactor = variables._ballLerpSmoothingFactor;
            _ballTeleportDistance = variables._ballTeleportDistance;
            _sprites = new[] { _spriteNoTeam, _spriteRedTeam, _spriteBlueTeam, _spriteGhosted, _spriteHidden };
            if (_debug._ballText == null)
            {
                _debug._isShowBallText = false;
            }
            else if (!_debug._isShowBallText)
            {
                _debug._ballText.gameObject.SetActive(false);
            }
            _SetBallState(BallState.Ghosted);
            UpdateBallText();
        }

        private void OnEnable()
        {
            if (!_photonView.ObservedComponents.Contains(this))
            {
                // If not set in Editor
                // - and this helps to avoid unnecessary warnings when view starts to serialize itself "too early" for other views not yet ready.
                _photonView.ObservedComponents.Add(this);
            }
        }

        private void _SetBallState(BallState ballState)
        {
            _ballState = ballState;
            var stateIndex = (int)ballState;
            _ballCollider.SetActive(ColliderStates[stateIndex]);
            for (var i = 0; i < BallStates.Length; ++i)
            {
                _sprites[i].SetActive(BallStates[i] == ballState);
            }
            if (_debug._isShowBallText)
            {
                _debug._ballText.gameObject.SetActive(_ballState != BallState.Hidden);
            }
        }

        #region Debugging

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private void UpdateBallText()
        {
            if (!_debug._isShowBallText)
            {
                return;
            }
            _debug._ballText.text = $"{_rigidbody.velocity.magnitude:0.0}";
        }

        #endregion

        #region IBallManager

        void IBallManager.SetBallPosition(Vector2 position)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            _rigidbody.position = position;
            _photonView.RPC(nameof(TestBallPosition), RpcTarget.Others, position);
            UpdateBallText();
        }

        void IBallManager.SetBallVelocity(Vector2 velocity)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            var speed = Mathf.Clamp(Mathf.Abs(velocity.magnitude), _ballMinMoveSpeed, _ballMaxMoveSpeed);
            _rigidbody.velocity = velocity.normalized * speed * _ballMoveSpeedMultiplier;
            _photonView.RPC(nameof(TestBallVelocity), RpcTarget.Others, velocity);
            UpdateBallText();
        }

        void IBallManager.SetBallState(BallState ballState)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            _SetBallState(ballState);
            _photonView.RPC(nameof(TestSetBallState), RpcTarget.Others, ballState);
            UpdateBallText();
        }

        #endregion
        
        #region Photon RPC

        // NOTE! When adding new RPC method check that the name is unique in PhotonServerSettings Rpc List!

        [PunRPC]
        private void TestBallPosition(Vector2 position)
        {
            _rigidbody.position = position;
        }

        [PunRPC]
        private void TestBallVelocity(Vector2 velocity)
        {
            _rigidbody.velocity = velocity;
        }

        [PunRPC]
        private void TestSetBallState(BallState ballState)
        {
            _SetBallState(ballState);
        }

        #endregion
    }
}