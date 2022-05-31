using System;
using System.Collections;
using System.Diagnostics;
using Altzone.Scripts.Config;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Test.Scripts.Battle.Ball
{
    internal class BallManager : MonoBehaviourPunCallbacks, IBallManager, IPunObservable
    {
        [Serializable]
        internal class DebugSettings
        {
            public bool _isShowBallText;
            public TextMeshPro _ballText;
            public bool _isShowTrailRenderer;
            public TrailRenderer _trailRenderer;
        }

        private static readonly BallState[] BallStates =
            { BallState.Stopped, BallState.NoTeam, BallState.RedTeam, BallState.BlueTeam, BallState.Ghosted, BallState.Hidden };

        // ColliderStates controls when ball collider is active based on ball state.
        private static readonly bool[] ColliderStates = { false, true, true, true, false, false };

        // StopStates control when ball is stopped implicitly when state changes - in practice state without active collider => stop the ball!
        private static readonly bool[] StopStates = { true, false, false, false, true, true };

        public static IBallManager Get() => FindObjectOfType<BallManager>();

        [Header("Settings"), SerializeField] private GameObject _ballCollider;
        [SerializeField] private GameObject _spriteStopped;
        [SerializeField] private GameObject _spriteNoTeam;
        [SerializeField] private GameObject _spriteRedTeam;
        [SerializeField] private GameObject _spriteBlueTeam;
        [SerializeField] private GameObject _spriteGhosted;
        [SerializeField] private GameObject _spriteHidden;

        [Header("Live Data"), SerializeField] private BallState _ballState;
        [SerializeField] private float _ballRequiredMoveSpeed;

        [Header("Photon Networking"), SerializeField] private Vector2 _networkPosition;
        [SerializeField] private float _networkLag;

        [Header("Debug Settings"), SerializeField] private DebugSettings _debug;

        private PhotonView _photonView;
        private Rigidbody2D _rigidbody;
        private GameObject[] _sprites;

        private float _ballMoveSpeedMultiplier;
        private float _ballMinMoveSpeed;
        private float _ballMaxMoveSpeed;
        private float _ballLerpSmoothingFactor;
        private float _ballTeleportDistance;

        private float _rigidbodyVelocitySqrMagnitude;

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
            _sprites = new[] { _spriteStopped, _spriteNoTeam, _spriteRedTeam, _spriteBlueTeam, _spriteGhosted, _spriteHidden };
            SetDebug();
            _SetBallState(BallState.Stopped);
            UpdateBallText();
        }

        private void SetDebug()
        {
            if (_debug._ballText == null)
            {
                _debug._isShowBallText = false;
            }
            else if (!_debug._isShowBallText)
            {
                _debug._ballText.gameObject.SetActive(false);
            }
            if (_debug._trailRenderer == null)
            {
                _debug._isShowTrailRenderer = false;
            }
            else if (!_debug._isShowTrailRenderer)
            {
                _debug._trailRenderer.gameObject.SetActive(false);
            }
        }

        public override void OnEnable()
        {
            base.OnEnable();
            if (!_photonView.ObservedComponents.Contains(this))
            {
                // If not set in Editor
                // - and this helps to avoid unnecessary warnings when view starts to serialize itself "too early" for other views not yet ready.
                _photonView.ObservedComponents.Add(this);
            }
            StartCoroutine(StartBallCoroutinesAndLogic());
            UpdateBallText();
        }

        public override void OnDisable()
        {
            base.OnDisable();
            StopAllCoroutines();
            _ballVelocityTracker = null;
        }

        private void MasterClientSwitched()
        {
            Debug.Log($"{name}");
            StopAllCoroutines();
            _ballVelocityTracker = null;
            StartCoroutine(StartBallCoroutinesAndLogic());
            UpdateBallText();
        }

        private IEnumerator StartBallCoroutinesAndLogic()
        {
            yield return new WaitUntil(() => PhotonNetwork.InRoom);
            Debug.Log($"{name}");
            _SetBallState(_ballState);
            if (_rigidbody.isKinematic != !PhotonNetwork.IsMasterClient)
            {
                Debug.Log($"{name} SET isKinematic {_rigidbody.isKinematic} <- {!PhotonNetwork.IsMasterClient}");
                _rigidbody.isKinematic = !PhotonNetwork.IsMasterClient;
            }
            if (PhotonNetwork.IsMasterClient)
            {
                StartCoroutine(OnMasterClientFixedUpdate());
            }
            else
            {
                StartCoroutine(OnRemoteBallNetworkUpdate());
                _ballCollider.SetActive(false);
            }
        }

        private IEnumerator OnMasterClientFixedUpdate()
        {
            Debug.Log($"{name}");
            var delay = new WaitForFixedUpdate();
            for (;;)
            {
                var velocity = _rigidbody.velocity;
                var sqrMagnitude = velocity.sqrMagnitude;
                if (!Mathf.Approximately(_rigidbodyVelocitySqrMagnitude, sqrMagnitude))
                {
                    if (_ballState == BallState.Stopped)
                    {
                        yield return delay;
                        continue;
                    }
                    if (velocity == Vector2.zero && _ballRequiredMoveSpeed > 0)
                    {
                        // We are badly stuck and can not move :-(
                        ((IBallManager)this).SetBallState(BallState.Stopped);
                        ((IBallManager)this).SetBallSpeed(0);
                        yield return delay;
                        continue;
                    }
                    if (_ballRequiredMoveSpeed > 0)
                    {
                        Debug.Log($"fix {_rigidbody.velocity} : {_rigidbodyVelocitySqrMagnitude} vs {sqrMagnitude}");
                        _rigidbody.velocity = velocity.normalized * _ballRequiredMoveSpeed;
                    }
                }
                yield return delay;
            }
        }

        private IEnumerator OnRemoteBallNetworkUpdate()
        {
            Debug.Log($"{name}");
            for (;;)
            {
                var position = _rigidbody.position;
                var isTeleport = Mathf.Abs(position.x - _networkPosition.x) > _ballTeleportDistance ||
                                 Mathf.Abs(position.y - _networkPosition.y) > _ballTeleportDistance;
                _rigidbody.position = isTeleport
                    ? _networkPosition
                    : Vector2.MoveTowards(position, _networkPosition, Time.deltaTime * _ballLerpSmoothingFactor);
                yield return null;
            }
        }

        private void _SetBallState(BallState ballState)
        {
            _ballState = ballState;
            var stateIndex = (int)ballState;
            _ballCollider.SetActive(ColliderStates[stateIndex]);
            if (StopStates[stateIndex])
            {
                InternalSetRigidbodyVelocity(0, Vector2.zero);
            }
            for (var i = 0; i < BallStates.Length; ++i)
            {
                _sprites[i].SetActive(BallStates[i] == ballState);
            }
            var isDebugVisible = _ballState != BallState.Hidden;
            if (_debug._isShowBallText)
            {
                _debug._ballText.gameObject.SetActive(isDebugVisible);
            }
            if (_debug._isShowTrailRenderer)
            {
                _debug._trailRenderer.gameObject.SetActive(isDebugVisible);
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
            _debug._ballText.text = $"{_rigidbody.velocity.magnitude:0.00}";
        }

        private Coroutine _ballVelocityTracker;
        private Vector2 _currentDebugVelocity;

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private void StartBallVelocityTracker()
        {
            if (_ballVelocityTracker == null)
            {
                _ballVelocityTracker = StartCoroutine(BallVelocityTracker());
            }
        }

        private IEnumerator BallVelocityTracker()
        {
            Debug.Log($"{name} speed {_ballRequiredMoveSpeed:0.00} velocity {_currentDebugVelocity} <- {_rigidbody.velocity}");
            _currentDebugVelocity = _rigidbody.velocity;
            for (;;)
            {
                yield return null;
                var velocity = _rigidbody.velocity;
                if (velocity != _currentDebugVelocity)
                {
                    var prevSqr = velocity.sqrMagnitude;
                    var curSqr = _currentDebugVelocity.sqrMagnitude;
                    if (!Mathf.Approximately(prevSqr, curSqr))
                    {
                        var velocityChange = -(1 - prevSqr / curSqr) * 100;
                        Debug.Log(
                            $"{name} speed {_ballRequiredMoveSpeed:0.00} velocity {_currentDebugVelocity} <- {velocity} sqr {prevSqr:0.00} <- {curSqr:0.00} = {velocityChange:0.00}%");
                    }
                    _currentDebugVelocity = velocity;
                }
                UpdateBallText();
                if (velocity == Vector2.zero)
                {
                    _ballVelocityTracker = null;
                    yield break;
                }
            }
        }

        #endregion

        #region IBallManager

        void IBallManager.FixCameraRotation(Camera gameCamera)
        {
            if (_debug._isShowBallText)
            {
                _debug._ballText.GetComponent<Transform>().rotation = gameCamera.GetComponent<Transform>().rotation;
            }
        }

        void IBallManager.SetBallPosition(Vector2 position)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Assert.IsTrue(PhotonNetwork.InRoom, "PhotonNetwork.InRoom");
                return;
            }
            _rigidbody.position = position;
            UpdateBallText();
            _photonView.RPC(nameof(TestBallPositionRpc), RpcTarget.Others, position);
        }

        void IBallManager.SetBallSpeed(float speed)
        {
            ((IBallManager)this).SetBallSpeed(speed, Vector2.zero);
        }

        void IBallManager.SetBallSpeed(float speed, Vector2 direction)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Assert.IsTrue(PhotonNetwork.InRoom, "PhotonNetwork.InRoom");
                return;
            }
            var actualVelocity = InternalSetRigidbodyVelocity(speed, direction);
            UpdateBallText();
            _photonView.RPC(nameof(TestBallVelocityRpc), RpcTarget.Others, actualVelocity);
            StartBallVelocityTracker();
        }

        private Vector2 InternalSetRigidbodyVelocity(float speed, Vector2 direction)
        {
            if (speed == 0)
            {
                _ballRequiredMoveSpeed = 0;
                _rigidbody.velocity = Vector2.zero;
                _rigidbodyVelocitySqrMagnitude = 0;
                return Vector2.zero;
            }
            _ballRequiredMoveSpeed = Mathf.Clamp(speed, _ballMinMoveSpeed, _ballMaxMoveSpeed) * _ballMoveSpeedMultiplier;
            var rigidbodyVelocity = direction != Vector2.zero
                ? direction.normalized * _ballRequiredMoveSpeed
                : _rigidbody.velocity.normalized * _ballRequiredMoveSpeed;
            _rigidbody.velocity = rigidbodyVelocity;
            _rigidbodyVelocitySqrMagnitude = rigidbodyVelocity.sqrMagnitude;
            return rigidbodyVelocity;
        }

        void IBallManager.SetBallState(BallState ballState)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Assert.IsTrue(PhotonNetwork.InRoom, "PhotonNetwork.InRoom");
                return;
            }
            _SetBallState(ballState);
            UpdateBallText();
            _photonView.RPC(nameof(TestSetBallStateRpc), RpcTarget.Others, ballState);
        }

        #endregion

        #region IPunObservable

        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(_rigidbody.position);
                stream.SendNext(_rigidbody.velocity);
                return;
            }
            _networkPosition = (Vector2)stream.ReceiveNext();
            var networkVelocity = (Vector2)stream.ReceiveNext();
            if (_rigidbody.velocity != networkVelocity)
            {
                _rigidbody.velocity = networkVelocity;
                UpdateBallText();
            }
            _networkLag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
            _networkPosition += networkVelocity * _networkLag;
        }

        #endregion

        #region Photon

        public override void OnMasterClientSwitched(Player newMasterClient)
        {
            if (newMasterClient.Equals(PhotonNetwork.LocalPlayer))
            {
                MasterClientSwitched();
            }
        }

        public override void OnPlayerEnteredRoom(Player newPlayer)
        {
            // This is purely optional for late coming players to make them "catch up" current ball state.
            // - position and velocity will be updated automatically!
            if (!PhotonNetwork.IsMasterClient)
            {
                return;
            }
            _photonView.RPC(nameof(TestSetBallStateRpc), RpcTarget.Others, _ballState);
        }

        #endregion

        #region Photon RPC

        // NOTE! When adding new RPC method check that the name is unique in PhotonServerSettings Rpc List!

        [PunRPC]
        private void TestBallPositionRpc(Vector2 position)
        {
            _rigidbody.position = position;
            UpdateBallText();
        }

        [PunRPC]
        private void TestBallVelocityRpc(Vector2 velocity)
        {
            _rigidbody.velocity = velocity;
            UpdateBallText();
        }

        [PunRPC]
        private void TestSetBallStateRpc(BallState ballState)
        {
            _SetBallState(ballState);
            UpdateBallText();
        }

        #endregion

        public override string ToString()
        {
            return $"{name}";
        }
    }
}