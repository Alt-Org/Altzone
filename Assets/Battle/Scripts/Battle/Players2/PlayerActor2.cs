using System.Collections;
using Altzone.Scripts.Battle;
using Altzone.Scripts.Config;
using Altzone.Scripts.Model;
using Battle.Scripts.Battle.Ball;
using Battle.Scripts.Battle.Factory;
using Battle.Scripts.Battle.interfaces;
using Battle.Scripts.Battle.Room;
using Photon.Pun;
using Prg.Scripts.Common.PubSub;
using Prg.Scripts.Common.Unity.ToastMessages;
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
        private const float UnReachableDistance = 100f;

        [Header("Settings"), SerializeField] private BattlePlayMode _startPlayMode;
        [SerializeField] private SpriteRenderer _highlightSprite;
        [SerializeField] private SpriteRenderer _stateSprite;
        [SerializeField] private Collider2D _collider;
        [SerializeField] private Transform _playerShieldHead;
        [SerializeField] private Transform _playerShieldFoot;

        [Header("Live Data"), SerializeField] private Transform _playerShield;
        [SerializeField] private float _shieldDistance;

        [Header("Debug"), SerializeField] private TextMeshPro _playerInfo;
        [SerializeField] private bool _isShowPlayerDebugText;
        [SerializeField] private bool _isShowLocalDebugInfo;

        private PhotonView _photonView;
        private Transform _transform;
        private IPlayerMovement _playerMovement;
        private IPlayerShield _shield;
        private IPlayerDistanceMeter _distanceMeter;
        private PhotonPlayerRpc _rpc;
        private bool _isDestroyed;
        private bool _isStunned;
        private float _playerShieldHitStunDuration;
        private int _playerResistance;
        private int _playerResitanceOnStart;
        private int _playerAttack;

        public void SetPhotonView(PhotonView photonView) => _photonView = photonView;

        public string StateString =>
            $"{_state._currentMode} {((PlayerShield)_shield).StateString} d={_distanceMeter.SqrDistance:0}\r\n{_playerMovement.StateString}";

        private void Awake()
        {   
            var player = _photonView.Owner;
            Debug.Log($"prefab {name} {_photonView}");
            Debug.Log($"{player.GetDebugLabel()}");
            _transform = GetComponent<Transform>();
            _state.InitState(_transform, player);
            var prefix = $"{(player.IsLocal ? "L" : "R")}{PlayerPos}:{TeamNumber}";
            name = $"@{name.Replace("(Clone)", string.Empty)}:{prefix}>{player.NickName}";
            if (_isShowPlayerDebugText)
            {
                SetDebug();
            }
            var gameCamera = Context.GetGameCamera;
            Assert.IsNotNull(gameCamera, "gameCameraInstance != null");
            // Must detect player position from actual y coordinate!
            var isYCoordNegative = _transform.position.y < 0;
            var isLower = isYCoordNegative;
            var isCameraRotated = false;
            var runtimeGameConfig = RuntimeGameConfig.Get();
            var features = runtimeGameConfig.Features;
            if (features._isRotateGameCamera)
            {
                isCameraRotated = gameCamera.IsRotated;
                if (isCameraRotated)
                {
                    // We are upside down!
                    isLower = !isLower;
                    Debug.Log($"RotatePlayer {_transform.name}");
                    _transform.Rotate(true);
                }
            }

            var model = PhotonBattle.GetCharacterModelForRoom(player);
            var variables = runtimeGameConfig.Variables;

            _playerShieldHitStunDuration = variables._playerShieldHitStunDuration;
            _playerResistance = model.Resistance;
            _playerResitanceOnStart = model.Resistance;
            _playerAttack = model.Attack;



            // Shield
            _playerShield = isLower
                ? _playerShieldHead
                : _playerShieldFoot;
            var shieldConfig = GetPlayerShield(_playerShield, model.MainDefence);
            _shield = new PlayerShield(shieldConfig);
            var isShieldRotated = !isYCoordNegative;
            _shield.Setup(name, _startPlayMode, isShieldRotated, false, 0);
            var multiplier = variables._shieldDistanceMultiplier;
            _shieldDistance = model.Defence * multiplier;

            Debug.Log(
                $"Awake {name} pos {_transform.position} model {model.Name} speed {model.Speed} " +
                $"isLower={(isLower ? 1 : 0)} isRotated camera={(isCameraRotated ? 1 : 0)} shield={(isShieldRotated ? 1 : 0)}");

            // Player movement and play area.
            var playerArea = Context.GetPlayerPlayArea.GetPlayerPlayArea(PlayerPos);
            var gameInput = runtimeGameConfig.Input;
            _playerMovement = new PlayerMovement2(_transform, gameInput, Camera.main, _photonView)
            {
                PlayerArea = playerArea,
                UnReachableDistance = UnReachableDistance,
                Speed = model.Speed,
            };
            // Setup audio listener only for local player.
            if (player.IsLocal)
            {
                SetupAudio(this, gameCamera);
            }
            this.Subscribe<BallManager.ActiveTeamEvent>(OnActiveTeamEvent);

            Debug.Log($"Awake Done {name} shieldDistance {_shieldDistance} playerArea {playerArea}");
        }

        private static void SetupAudio(PlayerActor2 actor, GameCamera gameCamera)
        {
            var audioListener = actor.gameObject.GetOrAddComponent<AudioListener>();
            audioListener.enabled = true;
            gameCamera.DisableAudio();
        }

        private void SetDebug()
        {
            _playerInfo = GetComponentInChildren<TextMeshPro>();
            if (_playerInfo != null)
            {
                SetDebugText($"{PlayerPos:N0}");
            }
            else
            {
                _isShowPlayerDebugText = false;
            }
        }

        private void SetDebugText(string text)
        {
            if (_isShowPlayerDebugText)
            {
                _playerInfo.text = text;
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
            if (_isShowLocalDebugInfo && _photonView.IsMine)
            {
                var debugInfoPrefab = Resources.Load<PlayerDebugInfo>(nameof(PlayerDebugInfo));
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
            Debug.Log($"{name}");
            this.Unsubscribe();
            _playerMovement.OnDestroy();
            _playerMovement = null;
            _isDestroyed = true;
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

        private static ShieldConfig GetPlayerShield(Transform transform, Defence defence)
        {
            var runtimeGameConfig = RuntimeGameConfig.Get();
            var shieldPrefab = runtimeGameConfig.Prefabs.GetShieldPrefab(defence);
            Assert.IsNotNull(shieldPrefab, "shieldPrefab != null");
            var instance = Instantiate(shieldPrefab, transform);
            instance.name = instance.name.Replace("(Clone)", string.Empty);
            var shieldConfig = instance.GetComponent<ShieldConfig>();
            return shieldConfig;
        }

        #region External events

        public void OnNetworkLost()
        {
            if (_isDestroyed)
            {
                return;
            }
            Debug.Log($"{name}");
            // Just stop doing anything if we loose our controlling player instance.
            this.Unsubscribe();
            OnSetPlayMode(BattlePlayMode.Ghosted);
        }

        private void OnActiveTeamEvent(BallManager.ActiveTeamEvent data)
        {
            if (data.TeamIndex == PhotonBattle.NoTeamValue)
            {
                // Ball is moving from one side to an other, everybody is frozen (except ghosts)!
                if (_state._currentMode != BattlePlayMode.Ghosted)
                {
                    ((IPlayerActor)this).SetFrozenMode();
                }
                return;
            }
            if (data.TeamIndex == _state._teamNumber)
            {
                // NOP
            }
            else
            {
                // Ball has moved to opposite team, we can move now
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
                if (_playerShieldHitStunDuration > 0)
                {
                    ScoreFlash.Push($"{PhotonNetwork.NickName} stun for {_playerShieldHitStunDuration}");
                }
            }
        }

        void IPlayerActor.ShieldCollision()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                var ball = Context.GetBall;
                ball.SetSpeed(_playerAttack);
                var rotationIndex = _shield.RotationIndex + 1;
                _rpc.SendShieldRotation(OnSetShieldRotation, rotationIndex);
            }
        }

        void IPlayerActor.SetNormalMode()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                SetDebugText($"{PlayerPos:N0}N");
                _rpc.SendPlayMode(OnSetPlayMode, BattlePlayMode.Normal);
            }
        }

        void IPlayerActor.SetFrozenMode()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                SetDebugText($"{PlayerPos:N0}F");
                _rpc.SendPlayMode(OnSetPlayMode, BattlePlayMode.Frozen);
            }
        }

        void IPlayerActor.SetGhostedMode()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                SetDebugText($"{PlayerPos:N0}G");
                _rpc.SendPlayMode(OnSetPlayMode, BattlePlayMode.Ghosted);
            }
        }

        private void OnSetShieldVisibility(bool isVisible)
        {
            _shield.SetVisibility(isVisible);
        }

        private void OnSetPlayMode(BattlePlayMode battlePlayMode)
        {
            Debug.Log($"OnSetPlayMode {name} {battlePlayMode}");
            Assert.IsTrue(battlePlayMode >= BattlePlayMode.Normal && battlePlayMode <= BattlePlayMode.Ghosted,
                "playMode >= PlayModeNormal && playMode <= PlayModeGhosted");
            _state._currentMode = battlePlayMode;
            switch (battlePlayMode)
            {
                case BattlePlayMode.Normal:
                    SetDebugText($"{PlayerPos:N0}N");
                    _collider.enabled = true;
                    // This is only setup to work if the player doesn't become ghosted
                    // Ghosted is treated as a higher priority than the other states
                    if (!_isStunned)
                    {
                        _playerMovement.SetMovementAllowed();
                    }
                    break;
                case BattlePlayMode.Frozen:
                    SetDebugText($"{PlayerPos:N0}F");
                    _collider.enabled = true;
                    _playerMovement.SetStopped();
                    break;
                case BattlePlayMode.Ghosted:
                    SetDebugText($"{PlayerPos:N0}G");
                    _collider.enabled = false;
                    _playerMovement.SetMovementAllowed();
                    break;
            }
            _shield.SetPlayMode(battlePlayMode);
        }

        private void OnSetShieldRotation(int rotationIndex)
        {
            // Note that this is for local player only!
            _shield.PlayHitEffects();
            if (_shield.CanRotate)
            {
                // Check _playerResistance here!
                if (_playerResistance >= 1)
                {
                   --_playerResistance;
                }
                if (_playerResistance < 1)
                {
                    _shield.SetRotation(rotationIndex);
                    _playerResistance = _playerResitanceOnStart;
                }
                OnSetPlayMode(BattlePlayMode.Ghosted);
            }
            else
            {
                // This will be changed, Since this only allows the OnParalysis to work once
                StartCoroutine(OnParalysis(_playerShieldHitStunDuration));
                Debug.Log($"Player is set to Paralyzed {_playerShieldHitStunDuration}");
            }

        }

        #endregion

        // Currently setup so that it only messes with variables in the script
        // without introducing anything new apart from a bool
        // Works independently from the SetPlayMode.
        // The stun is instantaneous and doesn't wait until the player is unfrozen
        private IEnumerator OnParalysis(float playerHeadHitStunDuration)
        {
            _isStunned = true;
            _playerMovement.SetStopped();
            yield return new WaitForSeconds(playerHeadHitStunDuration);
            _isStunned = false;
            // If not for this if statement, the player would be able to move
            // when the stun timer expires while the ball is on their side
            // TLDR; Prevents a backdoor that allows the player to move when frozen
            if (_state._currentMode != BattlePlayMode.Frozen)
            {
                _playerMovement.SetMovementAllowed();
            }
            Debug.Log($"{name} expired");
        }
    }
}
