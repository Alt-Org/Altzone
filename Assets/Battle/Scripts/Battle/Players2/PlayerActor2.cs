using Altzone.Scripts.Battle;
using Altzone.Scripts.Config;
using Battle.Scripts.Battle.Ball;
using Battle.Scripts.Battle.Factory;
using Battle.Scripts.Battle.interfaces;
using Photon.Pun;
using Prg.Scripts.Common.PubSub;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;

namespace Battle.Scripts.Battle.Players2
{
    /// <summary>
    /// Game Actor for player, manages player and shield state. Movement is handled separately.
    /// </summary>
    internal class PlayerActor2 : PlayerActor, IPlayerActor
    {
        private static readonly string[] StateNames = { "Norm", "Frozen", "Ghost" };

        private const string Tooltip1 = @"0=""Normal"", 1=""Frozen"", 2=""Ghosted""";

        [Header("Settings"), SerializeField, Tooltip(Tooltip1), Range(0, 2)] private int _startPlayMode;
        [SerializeField] private SpriteRenderer _highlightSprite;
        [SerializeField] private SpriteRenderer _stateSprite;
        [SerializeField] private Collider2D _collider;
        [SerializeField] private Transform _playerShieldHead;
        [SerializeField] private Transform _playerShieldFoot;

        [Header("Play Area"), SerializeField] private Rect _upperPlayArea;
        [SerializeField] private Rect _lowerPlayArea;

        [Header("Live Data"), SerializeField] private Transform _playerShield;
        [SerializeField] private float _shieldDistance;

        [Header("Debug"), SerializeField] private TextMeshPro _playerInfo;
        [SerializeField] private bool _isShowDebugCanvas;
        [SerializeField] private bool _isStateSpriteColorTint;

        private PhotonView _photonView;
        private Transform _transform;
        private IPlayerMovement _playerMovement;
        private IPlayerShield2 _shield;
        private IPlayerDistanceMeter _distanceMeter;
        private PhotonPlayerRpc _rpc;

        public void SetPhotonView(PhotonView photonView) => _photonView = photonView;

        public string StateString => $"{StateNames[_state._currentMode]} {((PlayerShield2)_shield).StateString} d={_distanceMeter.SqrDistance:0}\r\n{_playerMovement.StateString}";

        private void Awake()
        {
            Debug.Log($"Awake {_photonView}");
            var player = _photonView.Owner;
            _transform = GetComponent<Transform>();
            _state.InitState(_transform, player);
            var prefix = $"{(player.IsLocal ? "L" : "R")}{PlayerPos}:{TeamNumber}";
            name = $"@{prefix}>{player.NickName}";
            SetDebug();
            var gameCameraInstance = Context.GetGameCamera;
            Assert.IsNotNull(gameCameraInstance, "gameCameraInstance != null");
            // Must detect player position from actual y coordinate!
            var isYCoordNegative = _transform.position.y < 0;
            var isLower = isYCoordNegative;
            var isCameraRotated = false;
            var features = RuntimeGameConfig.Get().Features;
            if (features._isRotateGameCamera)
            {
                isCameraRotated = gameCameraInstance.IsRotated;
                if (isCameraRotated)
                {
                    // We are upside down!
                    isLower = !isLower;
                    Debug.Log($"RotatePlayer {_transform.name}");
                    _transform.Rotate(true);
                }
            }

            var runtimeGameConfig = RuntimeGameConfig.Get();

            // Shield
            _playerShield = isLower
                ? _playerShieldHead
                : _playerShieldFoot;
            var model = PhotonBattle.GetPlayerCharacterModel(player);
            var shieldConfig = GetLocalPlayerShield(_playerShield);
            _shield = new PlayerShield2(shieldConfig);
            var isShieldRotated = !isYCoordNegative;
            _shield.Setup(name, isShieldRotated, false, _startPlayMode, 0);
            var multiplier = runtimeGameConfig.Variables._shieldDistanceMultiplier;
            _shieldDistance = model.Defence * multiplier;

            Debug.Log(
                $"Awake {name} pos {_transform.position} isLower {(isLower ? 1 : 0)} isCameraRotated {(isCameraRotated ? 1 : 0)} isShieldRotated {(isShieldRotated ? 1 : 0)}");

            // Player movement
            var playerArea = isYCoordNegative ? _lowerPlayArea : _upperPlayArea;
            var gameInput = runtimeGameConfig.Input;
            _playerMovement = new PlayerMovement2(_transform, gameInput, Camera.main, _photonView)
            {
                PlayerArea = playerArea,
                UnReachableDistance = 100,
                Speed = 10f,
            };
            // Setup audio listener for the player.
            gameObject.AddComponent<AudioListener>();
            var cameraAudioListener = gameCameraInstance.GetComponent<AudioListener>();
            if (cameraAudioListener != null)
            {
                cameraAudioListener.enabled = false;
            }
            this.Subscribe<BallManager.ActiveTeamEvent>(OnActiveTeamEvent);

            Debug.Log($"Awake Done {name} shieldDistance {_shieldDistance} playerArea {playerArea}");
        }

        private void SetDebug()
        {
            var playerData = RuntimeGameConfig.Get().PlayerDataCache;
            var isDebugFlag = playerData.IsDebugFlag;
            _isShowDebugCanvas = isDebugFlag && _isShowDebugCanvas;
            _playerInfo = GetComponentInChildren<TextMeshPro>();
            if (_playerInfo != null)
            {
                if (isDebugFlag)
                {
                    _playerInfo.text = PlayerPos.ToString("N0");
                }
                else
                {
                    _playerInfo.enabled = false;
                }
            }
        }

        private void CreateDistanceMeter()
        {
            var otherTransform = TeamMate.GetComponent<Transform>();
            _distanceMeter = new PlayerDistanceMeter(_transform, otherTransform, _shieldDistance);
        }

        private void OnEnable()
        {
            // Finalize player setup when we have both team members available.
            _state.FindTeamMember();
            var isShieldVisible = true;
            if (TeamMate != null)
            {
                TeamMate.ConnectWith(this);
                // TODO: refactor when PlayerActor and PlayerActor2 is merged and PlayerActor1 removed
                CreateDistanceMeter();
                ((PlayerActor2)TeamMate).CreateDistanceMeter();
            }
            else
            {
                var features = RuntimeGameConfig.Get().Features;
                isShieldVisible = features._isSinglePlayerShieldOn;
                _distanceMeter = new PlayerDistanceMeterFixed(isShieldVisible);
            }
            Debug.Log($"OnEnable {name} IsMine {_photonView.IsMine} IsMaster {_photonView.Owner.IsMasterClient} teamMate {TeamMate}");
            OnSetPlayMode(_startPlayMode);
            OnSetShieldVisibility(isShieldVisible);

            if (_photonView.IsMine && _highlightSprite.enabled)
            {
                _highlightSprite.color = Color.yellow;
            }
            if (_isShowDebugCanvas && _photonView.IsMine)
            {
                var debugInfoPrefab = Resources.Load<PlayerDebugInfo>($"PlayerDebugInfo");
                var debugInfo = Instantiate(debugInfoPrefab, _transform);
                debugInfo.PlayerActor = this;
            }
            _rpc = _photonView.gameObject.GetOrAddComponent<PhotonPlayerRpc>();
            _rpc.SendPlayMode(OnSetPlayMode);
            _rpc.SendShieldVisibility(OnSetShieldVisibility);
            _rpc.SendShieldRotation(OnSetShieldRotation);
        }

        private void OnDestroy()
        {
            Debug.Log($"OnDestroy {name}");
            this.Unsubscribe();
            _playerMovement.OnDestroy();
            _playerMovement = null;
        }

        private void Update()
        {
            _playerMovement.Update();
            if (_photonView.IsMine)
            {
                _distanceMeter.CheckShieldVisibility(
                    () => _rpc.SendShieldVisibility(OnSetShieldVisibility, true),
                    () => _rpc.SendShieldVisibility(OnSetShieldVisibility, false));
            }
        }

        private static ShieldConfig GetLocalPlayerShield(Transform transform)
        {
            var runtimeGameConfig = RuntimeGameConfig.Get();
            var playerDataCache = runtimeGameConfig.PlayerDataCache;
            var defence = playerDataCache.CharacterModel.MainDefence;
            var  shieldPrefab = runtimeGameConfig.Prefabs.GetShieldPrefab(defence);
            Assert.IsNotNull(shieldPrefab, "shieldPrefab != null");
            var instance = Instantiate(shieldPrefab, transform);
            instance.name = instance.name.Replace("(Clone)", string.Empty);
            var shieldConfig = instance.GetComponent<ShieldConfig>();
            return shieldConfig;
        }

        #region External events

        private void OnActiveTeamEvent(BallManager.ActiveTeamEvent data)
        {
            if (data.TeamIndex == _state._teamNumber)
            {
                // Ghosted -> Frozen is not allowed
                if (_state._currentMode != PlayModeNormal)
                {
                    return;
                }
                ((IPlayerActor)this).SetFrozenMode();
            }
            else
            {
                ((IPlayerActor)this).SetNormalMode();
            }
        }

        #endregion

        #region IPlayerActor

        Transform IPlayerActor.Transform => _state._transform;

        int IPlayerActor.PlayerPos => _state._playerPos;

        int IPlayerActor.TeamNumber => _state._teamNumber;

        IPlayerActor IPlayerActor.TeamMate => (IPlayerActor)_state._teamMate;

        void IPlayerActor.HeadCollision()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                ((IPlayerActor)this).SetGhostedMode();
            }
        }

        void IPlayerActor.ShieldCollision()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                var rotationIndex = _shield.RotationIndex + 1;
                _rpc.SendShieldRotation(OnSetShieldRotation, rotationIndex);
            }
        }

        void IPlayerActor.SetNormalMode()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                _rpc.SendPlayMode(OnSetPlayMode, PlayModeNormal);
            }
        }

        void IPlayerActor.SetFrozenMode()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                _rpc.SendPlayMode(OnSetPlayMode, PlayModeFrozen);
            }
        }

        void IPlayerActor.SetGhostedMode()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                _rpc.SendPlayMode(OnSetPlayMode, PlayModeGhosted);
            }
        }

        private void OnSetShieldVisibility(bool isVisible)
        {
            _shield.SetVisibility(isVisible);
        }

        private void OnSetPlayMode(int playMode)
        {
            Debug.Log($"OnSetPlayMode {name} {StateNames[playMode]}");
            Assert.IsTrue(playMode >= PlayModeNormal && playMode <= PlayModeGhosted,
                "playMode >= PlayModeNormal && playMode <= PlayModeGhosted");
            _state._currentMode = playMode;
            switch (playMode)
            {
                case PlayModeNormal:
                    _collider.enabled = true;
                    _playerMovement.SetMovementAllowed();
                    if (_isStateSpriteColorTint)
                    {
                        _stateSprite.color = Color.blue;
                    }
                    break;
                case PlayModeFrozen:
                    _collider.enabled = true;
                    _playerMovement.SetStopped();
                    if (_isStateSpriteColorTint)
                    {
                        _stateSprite.color = Color.magenta;
                    }
                    break;
                case PlayModeGhosted:
                    _collider.enabled = false;
                    _playerMovement.SetMovementAllowed();
                    if (_isStateSpriteColorTint)
                    {
                        _stateSprite.color = Color.grey;
                    }
                    break;
            }
            _shield.SetPlayMode(playMode);
        }

        private void OnSetShieldRotation(int rotationIndex)
        {
            _shield.SetRotation(rotationIndex);
            _shield.PlayHitEffects();
        }

        #endregion
    }
}
