using System;
using Battle.Scripts.Battle.interfaces;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityConstants;
using UnityEngine;

namespace Battle.Scripts.Battle.Ball
{
    [Serializable]
    internal class BallSettings
    {
        [Header("Ball Setup")] public GameObject _ballCollider;
        public GameObject _colorNoTeam;
        public GameObject _colorRedTeam;
        public GameObject _colorBlueTeam;
        public GameObject _colorGhosted;
        public GameObject _colorHidden;

        [Header("Layers")] public LayerMask _teamAreaMask;
        public LayerMask _headMask;
        public LayerMask _shieldMask;
        public LayerMask _brickMask;
        public LayerMask _wallMask;

        [Header("Ball Constraints")] public float _minBallSpeed;
        public float _maxBallSpeed;
    }

    [Serializable]
    internal class BallState
    {
        public BallColor _ballColor;
        public bool _isMoving;
    }

    [RequireComponent(typeof(PhotonView))]
    internal class BallActor : MonoBehaviourPunCallbacks, IPunObservable, IBall, IBallCollision
    {
        private const float BallTeleportDistance = 1f;
        private const float CheckVelocityDelay = 0.5f;

        [SerializeField] private BallSettings _settings;
        [SerializeField] private BallState _state;

        [Header("Photon"), SerializeField] private Vector2 _networkPosition;
        [SerializeField] private float _networkLag;

        private PhotonView _photonView;
        private Rigidbody2D _rigidbody;

        [Header("Live Data"), SerializeField] private float _currentSpeed;
        private bool _isCheckVelocityAfterCollision;
        private float _checkVelocityTime;

        [Header("Debug"), SerializeField] private bool _isDebugInfoText;
        [SerializeField] private TextMeshPro _debugInfoText;
        private GameObject _debugInfoParent;


        // This is indexed by BallColor!
        private GameObject[] _stateObjects;

        private int _teamAreaMaskValue;
        private int _headMaskValue;
        private int _shieldMaskValue;
        private int _brickMaskValue;
        private int _wallMaskValue;

        private Action<Collision2D> _onHeadCollision;
        private Action<Collision2D> _onShieldCollision;
        private Action<Collision2D> _onBrickCollision;
        private Action<Collision2D> _onWallCollision;
        private Action<GameObject> _onEnterTeamArea;
        private Action<GameObject> _onExitTeamArea;

        private void Awake()
        {
            Debug.Log("Awake");
            _photonView = PhotonView.Get(this);
            _rigidbody = GetComponent<Rigidbody2D>();
            _stateObjects = new[]
            {
                _settings._colorNoTeam,
                _settings._colorRedTeam,
                _settings._colorBlueTeam,
                _settings._colorGhosted,
                _settings._colorHidden
            };
            _teamAreaMaskValue = _settings._teamAreaMask.value;
            _headMaskValue = _settings._headMask.value;
            _shieldMaskValue = _settings._shieldMask.value;
            _brickMaskValue = _settings._brickMask.value;
            _wallMaskValue = _settings._wallMask.value;
            _debugInfoParent = _debugInfoText.gameObject;
            if (!_isDebugInfoText)
            {
                _debugInfoParent.SetActive(false);
            }
            PhotonSetup();
        }

        private void PhotonSetup()
        {
            Debug.Log($"PhotonSetup mine {_photonView.IsMine} room {_photonView.IsRoomView} master {PhotonNetwork.IsMasterClient}");
            _rigidbody.isKinematic = !PhotonNetwork.IsMasterClient;
        }

        private void MasterClientSwitched()
        {
            Debug.Log($"MasterClientSwitched mine {_photonView.IsMine} room {_photonView.IsRoomView} master {PhotonNetwork.IsMasterClient}");
            var velocity = _rigidbody.velocity;
            Debug.Log($"_rigidbody position {_rigidbody.position} velocity {velocity} isKinematic {_rigidbody.isKinematic}");
            _rigidbody.isKinematic = !PhotonNetwork.IsMasterClient;
            if (velocity.x != 0f || velocity.y != 0f)
            {
                ((IBall)this).StartMoving(_rigidbody.position, velocity);
            }
            else
            {
                ((IBall)this).StopMoving();
            }
        }

        public override void OnEnable()
        {
            base.OnEnable();
            Debug.Log($"OnEnable IsMine {_photonView.IsMine} add observed {!_photonView.ObservedComponents.Contains(this)}");
            if (!_photonView.ObservedComponents.Contains(this))
            {
                // If not set in Editor
                // - and this helps to avoid unnecessary warnings when view starts to serialize itself "too early" for other views not yet ready.
                _photonView.ObservedComponents.Add(this);
            }
        }

        #region Movement

        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                stream.SendNext(_rigidbody.position);
                stream.SendNext(_rigidbody.velocity);
            }
            else
            {
                _networkPosition = (Vector2)stream.ReceiveNext();
                _rigidbody.velocity = (Vector2)stream.ReceiveNext();

                _networkLag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));
                _networkPosition += _rigidbody.velocity * _networkLag;

                // Just for testing - this is expensive call!
                if (_isDebugInfoText)
                {
                    _debugInfoText.text = _rigidbody.velocity.magnitude.ToString("F1");
                }
            }
        }

        private void Update()
        {
            if (!PhotonNetwork.IsMasterClient)
            {
                var position = _rigidbody.position;
                var isTeleport = Mathf.Abs(position.x - _networkPosition.x) > BallTeleportDistance ||
                                 Mathf.Abs(position.y - _networkPosition.y) > BallTeleportDistance;
                _rigidbody.position = isTeleport
                    ? _networkPosition
                    : Vector2.MoveTowards(position, _networkPosition, Time.deltaTime);
                return;
            }
            if (_isCheckVelocityAfterCollision && _checkVelocityTime > Time.time)
            {
                _isCheckVelocityAfterCollision = false;
                if (!Mathf.Approximately(_currentSpeed * _currentSpeed, _rigidbody.velocity.sqrMagnitude))
                {
                    Debug.Log($"fix velocity {_rigidbody.velocity} <- {_currentSpeed:0.0}");
                    _rigidbody.velocity = _rigidbody.velocity.normalized * _currentSpeed;
                }
            }
            // Just for testing - this is expensive call!
            if (_isDebugInfoText)
            {
                _debugInfoText.text = _rigidbody.velocity.magnitude.ToString("F1");
            }
        }

        #endregion

        #region Collisions

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!enabled)
            {
                return; // Collision events will be sent to disabled MonoBehaviours, to allow enabling Behaviours in response to collisions.
            }
            var otherGameObject = other.gameObject;
            var layer = otherGameObject.layer;
            if (layer == 0)
            {
                Debug.Log($"IGNORE trigger_enter {otherGameObject.name} layer {otherGameObject.layer}");
                return;
            }
            if (!otherGameObject.CompareTag(Tags.Untagged))
            {
                var colliderMask = 1 << layer;
                if (CallbackEvent(_teamAreaMaskValue, colliderMask, otherGameObject, _onEnterTeamArea))
                {
                    return;
                }
            }
            Debug.Log($"UNHANDLED trigger_enter {otherGameObject.name} layer {layer} {LayerMask.LayerToName(layer)}");
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (!enabled)
            {
                return; // Collision events will be sent to disabled MonoBehaviours, to allow enabling Behaviours in response to collisions.
            }
            var otherGameObject = other.gameObject;
            var layer = otherGameObject.layer;
            if (layer == 0)
            {
                Debug.Log($"IGNORE trigger_exit {otherGameObject.name} layer {otherGameObject.layer}");
                return;
            }
            if (!otherGameObject.CompareTag(Tags.Untagged))
            {
                var colliderMask = 1 << layer;
                if (CallbackEvent(_teamAreaMaskValue, colliderMask, otherGameObject, _onExitTeamArea))
                {
                    return;
                }
            }
            Debug.Log($"UNHANDLED trigger_exit {otherGameObject.name} layer {layer} {LayerMask.LayerToName(layer)}");
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!enabled)
            {
                return; // Collision events will be sent to disabled MonoBehaviours, to allow enabling Behaviours in response to collisions.
            }
            _isCheckVelocityAfterCollision = true;
            _checkVelocityTime = Time.time + CheckVelocityDelay;
            var otherGameObject = collision.gameObject;
            var layer = otherGameObject.layer;
            if (layer == 0)
            {
                Debug.Log($"IGNORE collision_enter {otherGameObject.name} layer {otherGameObject.layer}");
                return;
            }
            var colliderMask = 1 << layer;
            if (CallbackEvent(_headMaskValue, colliderMask, collision, _onHeadCollision))
            {
                return;
            }
            if (CallbackEvent(_shieldMaskValue, colliderMask, collision, _onShieldCollision))
            {
                return;
            }
            if (CallbackEvent(_brickMaskValue, colliderMask, collision, _onBrickCollision))
            {
                return;
            }
            if (IsCallbackEvent(_wallMaskValue, colliderMask))
            {
                if (!otherGameObject.CompareTag(Tags.Untagged))
                {
                    _onWallCollision?.Invoke(collision);
                }
                return;
            }
            Debug.Log($"UNHANDLED collision_enter {otherGameObject.name} layer {layer} {LayerMask.LayerToName(layer)}");
        }

        private static bool IsCallbackEvent(int maskValue, int colliderMask)
        {
            return maskValue == (maskValue | colliderMask);
        }

        private static bool CallbackEvent(int maskValue, int colliderMask, Collision2D collision, Action<Collision2D> callback)
        {
            if (maskValue == (maskValue | colliderMask))
            {
                callback?.Invoke(collision);
                return true;
            }
            return false;
        }

        private static bool CallbackEvent(int maskValue, int colliderMask, GameObject gameObject, Action<GameObject> callback)
        {
            if (maskValue == (maskValue | colliderMask))
            {
                callback?.Invoke(gameObject);
                return true;
            }
            return false;
        }

        #endregion

        #region IBall

        IBallCollision IBall.BallCollision => this;

        void IBall.StopMoving()
        {
            Debug.Log($"StopMoving {_state._isMoving} <- {false}");
            _state._isMoving = false;
            if (PhotonNetwork.IsMasterClient)
            {
                _settings._ballCollider.SetActive(false);
            }
            _currentSpeed = 0f;
            _rigidbody.velocity = Vector2.zero;
        }

        void IBall.StartMoving(Vector2 position, Vector2 velocity)
        {
            Debug.Log($"StartMoving {_state._isMoving} <- {true} position {position} velocity {velocity}");
            _state._isMoving = true;
            if (PhotonNetwork.IsMasterClient)
            {
                _settings._ballCollider.SetActive(true);

                _rigidbody.position = position;
                var speed = Mathf.Clamp(Mathf.Abs(velocity.magnitude), _settings._minBallSpeed, _settings._maxBallSpeed);
                _rigidbody.velocity = velocity.normalized * speed;
                _currentSpeed = _rigidbody.velocity.magnitude;
            }
        }

        void IBall.SetColor(BallColor ballColor)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                _photonView.RPC(nameof(SetBallColorRpc), RpcTarget.All, (byte)ballColor);
            }
        }

        [PunRPC]
        private void SetBallColorRpc(byte ballColor)
        {
            _setBallColorLocal((BallColor)ballColor);
        }

        private void _setBallColorLocal(BallColor ballColor)
        {
            //Debug.Log($"_setBallColorLocal {state.ballColor} <- {ballColor}");
            _stateObjects[(int)_state._ballColor].SetActive(false);
            _state._ballColor = ballColor;
            _stateObjects[(int)_state._ballColor].SetActive(true);
            if (_isDebugInfoText)
            {
                _debugInfoParent.SetActive(ballColor != BallColor.Hidden);
            }
        }

        #endregion

        #region IBallCollision

        Action<Collision2D> IBallCollision.OnHeadCollision
        {
            get => _onHeadCollision;
            set => _onHeadCollision = value;
        }

        Action<Collision2D> IBallCollision.OnShieldCollision
        {
            get => _onShieldCollision;
            set => _onShieldCollision = value;
        }

        Action<Collision2D> IBallCollision.OnWallCollision
        {
            get => _onWallCollision;
            set => _onWallCollision = value;
        }

        Action<Collision2D> IBallCollision.OnBrickCollision
        {
            get => _onBrickCollision;
            set => _onBrickCollision = value;
        }

        Action<GameObject> IBallCollision.OnEnterTeamArea
        {
            get => _onEnterTeamArea;
            set => _onEnterTeamArea = value;
        }

        Action<GameObject> IBallCollision.OnExitTeamArea
        {
            get => _onExitTeamArea;
            set => _onExitTeamArea = value;
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

        #endregion
    }
}