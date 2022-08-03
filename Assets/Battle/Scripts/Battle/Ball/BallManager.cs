using System;
using System.Collections;
using System.Diagnostics;
using Altzone.Scripts.Config;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Scripts.Battle.Ball
{
    /// <summary>
    /// Manages <c>Ball</c> local and remote physical and visual state.
    /// </summary>
    /// <remarks>
    /// <c>Ball</c> rigidbody and collider setup and behaviour if different on local and remote clients.
    /// </remarks>
    [RequireComponent(typeof(PhotonView))]
    internal class BallManager : MonoBehaviourPunCallbacks, IBallManager, IBallCollision, IPunObservable
    {
        [Serializable]
        internal class DebugSettings
        {
            public bool _isApiCalls;
            public bool _isSpeedTracking;
            public bool _isShowBallText;
            public TextMeshPro _ballText;
            public bool _isShowTrailRenderer;
            public TrailRenderer _trailRenderer;
        }

        [Serializable]
        internal class ColorSettings
        {
            public Color _colorNoTeam;
            public Color _colorBlueTeam;
            public Color _colorRedTeam;
            public Color _colorTwoTeam;
        }

        private static readonly BallState[] BallStates =
            { BallState.Stopped, BallState.Moving, BallState.Ghosted, BallState.Hidden };

        // ColliderStates controls when ball collider is active based on ball state.
        private static readonly bool[] ColliderStates = { false, true, false, false };

        // StopStates control when ball is stopped implicitly when state changes - in practice state without active collider => stop the ball!
        private static readonly bool[] StopStates = { true, false, true, true };

        private const float BallMinMoveDistance = 0.01f;

        [Header("Settings"), SerializeField] private GameObject _ballColliderParent;
        [SerializeField] private GameObject _spriteStopped;
        [SerializeField] private GameObject _spriteMoving;
        [SerializeField] private GameObject _spriteGhosted;
        [SerializeField] private GameObject _spriteHidden;

        [Header("Live Data"), SerializeField] private BallState _ballState;
        [SerializeField] private float _ballLastExternalMoveSpeed;
        [SerializeField] private float _ballRequiredMoveSpeed;
        private float _rigidbodyRequiredVelocitySqrMagnitude;

        [Header("Photon Networking"), SerializeField] private Vector2 _networkPosition;
        [SerializeField] private float _networkLag;
        private int _networkUpdateDebugCount;

        [Header("Debug Settings"), SerializeField] private DebugSettings _debug;

        [Header("Color Settings"), SerializeField] private ColorSettings _colors;

        private PhotonView _photonView;
        private Rigidbody2D _rigidbody;
        private GameObject[] _sprites;
        private Collider2D _ballCollider;
        private Color[] _teamColors;

        private float _ballMoveSpeedMultiplier;
        private float _ballMinMoveSpeed;
        private float _ballMaxMoveSpeed;
        private float _ballLerpSmoothingFactor;
        private float _ballTeleportDistance;
        private float _ballIdleAccelerationStartDelay;
        private float _ballIdleAccelerationInterval;
        private float _ballIdleAccelerationMultiplier;
        private bool _isBallIdle;
        private float _ballIdleAccelerationUpdateTime;

        private void Awake()
        {
            Debug.Log($"{name}");
            _photonView = PhotonView.Get(this);
            _rigidbody = GetComponent<Rigidbody2D>();
            _ballCollider = _ballColliderParent.GetComponent<Collider2D>();
            var variables = RuntimeGameConfig.Get().Variables;
            _ballMoveSpeedMultiplier = variables._ballMoveSpeedMultiplier;
            _ballMinMoveSpeed = variables._ballMinMoveSpeed;
            _ballMaxMoveSpeed = variables._ballMaxMoveSpeed;
            _ballLerpSmoothingFactor = variables._ballLerpSmoothingFactor;
            _ballTeleportDistance = variables._ballTeleportDistance;
            _ballIdleAccelerationStartDelay = variables._ballIdleAccelerationStartDelay;
            _ballIdleAccelerationInterval = variables._ballIdleAccelerationInterval;
            _ballIdleAccelerationMultiplier = variables._ballIdleAccelerationMultiplier;
            _sprites = new[] { _spriteStopped, _spriteMoving, _spriteGhosted, _spriteHidden };
            _teamColors = new[] { _colors._colorNoTeam, _colors._colorBlueTeam, _colors._colorRedTeam, _colors._colorTwoTeam };
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
            Debug.Log($"{name} IsMasterClient {PhotonNetwork.IsMasterClient}");
            if (!_photonView.ObservedComponents.Contains(this))
            {
                // If not set in Editor
                // - and this helps to avoid unnecessary warnings when view starts to serialize itself "too early" for other views not yet ready.
                _photonView.ObservedComponents.Add(this);
            }
            StartCoroutine(StartBallCoroutinesAndLogic());
            UpdateBallText();
            if (_debug._isShowBallText && !PhotonNetwork.InRoom)
            {
                _debug._ballText.text = string.Empty;
            }
        }

        public override void OnDisable()
        {
            base.OnDisable();
            StopAllCoroutines();
            _ballVelocityTracker = null;
        }

        private void MasterClientSwitched()
        {
            Debug.Log($"{name} IsMasterClient {PhotonNetwork.IsMasterClient}");
            StopAllCoroutines();
            _ballVelocityTracker = null;
            StartCoroutine(StartBallCoroutinesAndLogic());
            UpdateBallText();
        }

        private IEnumerator StartBallCoroutinesAndLogic()
        {
            yield return new WaitUntil(() => PhotonNetwork.InRoom);
            Debug.Log($"{name} IsMasterClient {PhotonNetwork.IsMasterClient} state {_ballState} collider {_ballCollider.enabled}");
            _SetBallState(_ballState);
            if (_rigidbody.isKinematic != !PhotonNetwork.IsMasterClient)
            {
                Debug.Log($"{name} SET isKinematic {_rigidbody.isKinematic} <- {!PhotonNetwork.IsMasterClient}");
                _rigidbody.isKinematic = !PhotonNetwork.IsMasterClient;
            }
            var stateIndex = (int)_ballState;
            SetBallCollider(ColliderStates[stateIndex]);
            if (PhotonNetwork.IsMasterClient)
            {
                StartCoroutine(OnMasterClientFixedUpdateKeepBallVelocity());
                _ballCollider.isTrigger = false;
            }
            else
            {
                StartCoroutine(LagCompensationForNonPhysicObjects());
                _ballCollider.isTrigger = true;
            }
            UpdateBallText();
        }

        private IEnumerator OnMasterClientFixedUpdateKeepBallVelocity()
        {
            Debug.Log($"{name}");
            Assert.IsFalse(_rigidbody.isKinematic, "_rigidbody.isKinematic");
            var delay = new WaitForFixedUpdate();
            for (; enabled;)
            {
                var velocity = _rigidbody.velocity;
                var sqrMagnitude = velocity.sqrMagnitude;
                if (!Mathf.Approximately(_rigidbodyRequiredVelocitySqrMagnitude, sqrMagnitude))
                {
                    if (_ballState == BallState.Stopped)
                    {
                        yield return delay;
                        continue;
                    }
                    if (_ballRequiredMoveSpeed > 0)
                    {
                        if (velocity == Vector2.zero)
                        {
                            // We are badly stuck and can not move :-(
                            ((IBallManager)this).SetBallState(BallState.Stopped);
                            ((IBallManager)this).SetBallSpeed(0);
                            yield return delay;
                            continue;
                        }
                        Debug.Log($"fix {velocity} : {_rigidbodyRequiredVelocitySqrMagnitude} vs {sqrMagnitude}");
                        _rigidbody.velocity = velocity.normalized * _ballRequiredMoveSpeed;
                        yield return delay;
                        continue;
                    }
                }
                if (_isBallIdle)
                {
                    if (Time.time > _ballIdleAccelerationUpdateTime && _ballRequiredMoveSpeed > 0)
                    {
                        _ballIdleAccelerationUpdateTime = Time.time + _ballIdleAccelerationInterval;
                        var speed = _rigidbody.velocity.magnitude * _ballIdleAccelerationMultiplier;
                        InternalSetRigidbodyVelocity(speed, velocity);
                    }
                }
                else if (_ballRequiredMoveSpeed > 0 && _ballIdleAccelerationMultiplier > 0)
                {
                    _isBallIdle = true;
                    _ballIdleAccelerationUpdateTime = Time.time + _ballIdleAccelerationStartDelay;
                }
                yield return delay;
            }
        }

        private IEnumerator LagCompensationForNonPhysicObjects()
        {
            // https://doc.photonengine.com/en-us/pun/current/gameplay/lagcompensation
            Debug.Log($"{name}");
            Assert.IsTrue(_rigidbody.isKinematic, "_rigidbody.isKinematic");
            _currentDebugVelocity = _rigidbody.velocity;
            _networkUpdateDebugCount = 0;
            for (; enabled;)
            {
                var rigidbodyPosition = _rigidbody.position;
                var deltaX = Mathf.Abs(rigidbodyPosition.x - _networkPosition.x);
                var deltaY = Mathf.Abs(rigidbodyPosition.y - _networkPosition.y);
                var isTeleport = deltaX > _ballTeleportDistance || deltaY > _ballTeleportDistance;
                if (isTeleport)
                {
                    _rigidbody.position = _networkPosition;
                    _networkUpdateDebugCount = 0;
                }
                else
                {
                    var isMoving = deltaX > BallMinMoveDistance || deltaY > BallMinMoveDistance;
                    if (isMoving)
                    {
                        _rigidbody.position = Vector2.MoveTowards(rigidbodyPosition, _networkPosition, Time.deltaTime * _ballLerpSmoothingFactor);
                        if (_currentDebugVelocity == _rigidbody.velocity)
                        {
                            _networkUpdateDebugCount += 1;
                        }
                        else
                        {
                            _networkUpdateDebugCount = 0;
                        }
                    }
                    else if (_networkUpdateDebugCount > 0)
                    {
                        _networkUpdateDebugCount = 0;
                    }
                }
                yield return null;
            }
        }

        private void SetBallCollider(bool state)
        {
            if (_debug._isApiCalls && _ballCollider.enabled != state)
            {
                Debug.Log($"{_ballCollider.enabled} <- {state}");
            }
            _ballColliderParent.SetActive(state);
            _ballCollider.enabled = state;
        }

        private void _SetBallState(BallState ballState)
        {
            if (_debug._isApiCalls && _ballState != ballState)
            {
                Debug.Log($"{_ballState} <- {ballState}");
            }
            _ballState = ballState;
            var stateIndex = (int)ballState;
            SetBallCollider(ColliderStates[stateIndex]);
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
            if (PhotonNetwork.IsMasterClient)
            {
                _debug._ballText.text = $"{_rigidbody.velocity.magnitude:0.00}";
            }
            else
            {
                _debug._ballText.text = $"{_networkUpdateDebugCount}:{_networkLag * 100:000}";
            }
        }

        private Coroutine _ballVelocityTracker;
        private Vector2 _currentDebugVelocity;

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        private void StartBallVelocityTracker()
        {
            if (!_debug._isSpeedTracking)
            {
                return;
            }
            _ballVelocityTracker ??= StartCoroutine(BallVelocityTracker());
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
                        Debug.Log($"{name} speed {_ballRequiredMoveSpeed:0.00} velocity {_currentDebugVelocity} <- {velocity} " +
                                  $"sqr {prevSqr:0.00} <- {curSqr:0.00} = {velocityChange:0.00}%");
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

        IBallCollision IBallManager.BallCollision => this;

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
            if (_debug._isApiCalls)
            {
                Debug.Log($"{_rigidbody.position} <- {position}");
            }
            _rigidbody.position = position;
            UpdateBallText();
            _photonView.RPC(nameof(SetBallPositionRpc), RpcTarget.Others, position);
        }

        void IBallManager.SetBallSpeed(float speed)
        {
            ((IBallManager)this).SetBallSpeed(speed, _rigidbody.velocity);
        }

        void IBallManager.SetBallSpeed(float speed, Vector2 direction)
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                Assert.IsTrue(PhotonNetwork.InRoom, "PhotonNetwork.InRoom");
                return;
            }
            _ballLastExternalMoveSpeed = speed;
            var actualVelocity = InternalSetRigidbodyVelocity(speed, direction);
            UpdateBallText();
            _photonView.RPC(nameof(SetBallVelocityRpc), RpcTarget.Others, actualVelocity);
            StartBallVelocityTracker();
        }

        private Vector2 InternalSetRigidbodyVelocity(float speed, Vector2 direction)
        {
            if (_debug._isApiCalls)
            {
                Debug.Log($"{_ballRequiredMoveSpeed} <- {speed} : {_rigidbody.velocity.normalized} <- {direction.normalized}");
            }
            if (speed == 0)
            {
                // When ball is stopped, it will "forget" its current direction and can not start moving without new direction!
                _ballRequiredMoveSpeed = 0;
                _rigidbody.velocity = Vector2.zero;
                _rigidbodyRequiredVelocitySqrMagnitude = 0;
                return Vector2.zero;
            }
            _ballRequiredMoveSpeed = Mathf.Clamp(speed, _ballMinMoveSpeed, _ballMaxMoveSpeed) * _ballMoveSpeedMultiplier;
            var velocity = direction.normalized * _ballRequiredMoveSpeed;
            if (direction != Vector2.zero && velocity == Vector2.zero)
            {
                // When ball bounces from shield its velocity can be changed, but it should not stop moving.
                Debug.LogError($"ZERO VELOCITY: speed {speed} direction {direction} : " +
                               $"ballRequiredMoveSpeed {_ballRequiredMoveSpeed} cur velocity {_rigidbody.velocity}");
            }
            _rigidbody.velocity = velocity;
            _rigidbodyRequiredVelocitySqrMagnitude = velocity.sqrMagnitude;
            return velocity;
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
            _photonView.RPC(nameof(SetBallStateRpc), RpcTarget.Others, ballState);
        }

        void IBallManager.SetBallLocalTeamColor(int colorIndex)
        {
            Assert.IsTrue(colorIndex >= 0 && colorIndex < _teamColors.Length, "colorIndex >= 0 && colorIndex < _teamColors.Length");
            _spriteMoving.GetComponent<SpriteRenderer>().color = _teamColors[colorIndex];
        }

        #endregion

        #region IBallCollision

        void IBallCollision.OnBrickCollision(Collision2D collision)
        {
            _isBallIdle = false;
            ((IBallManager)this).SetBallSpeed(_ballLastExternalMoveSpeed, _rigidbody.velocity);
        }

        void IBallCollision.OnHeadCollision(Collision2D collision)
        {
            _isBallIdle = false;
        }

        #endregion

        #region IPunObservable

        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            // https://doc.photonengine.com/en-us/pun/current/gameplay/lagcompensation
            // - rigidbody.position is set every frame (Update) using coroutine
            // - rigidbody.velocity is set here
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
            _photonView.RPC(nameof(SetBallStateRpc), RpcTarget.Others, _ballState);
        }

        #endregion

        #region Photon RPC

        // NOTE! When adding new RPC method check that the name is unique in PhotonServerSettings Rpc List!

        [PunRPC]
        private void SetBallPositionRpc(Vector2 position)
        {
            _rigidbody.position = position;
            UpdateBallText();
        }

        [PunRPC]
        private void SetBallVelocityRpc(Vector2 velocity)
        {
            _rigidbody.velocity = velocity;
            UpdateBallText();
        }

        [PunRPC]
        private void SetBallStateRpc(BallState ballState)
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