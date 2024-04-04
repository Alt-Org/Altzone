using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Assertions;
using UnityConstants;

using Altzone.Scripts.Battle;
using Altzone.Scripts.Config;
using Altzone.Scripts.Config.ScriptableObjects;
using Prg.Scripts.Common.PubSub;

namespace Battle.Scripts.Battle.Players
{
    /// <summary>
    /// <c>PlayerActor</c> for local and remote instances.
    /// </summary>
    /// <remarks>
    /// Needs to derive from <c>PlayerActorBase</c> for type safe UNITY prefab instantiation.
    /// </remarks>
    internal class PlayerActor : PlayerActorBase
    {
        #region Serialized Fields
        [SerializeField] private Transform _geometryRoot;
        [SerializeField] private float _movementSpeed;
        [SerializeField] private float _shieldActivationDistance;
        [SerializeField] private float _impactForce;
        [SerializeField] private Sprite[] _playerCharacterSpriteSheet;
        [SerializeField] public string SeePlayerName;
        #endregion

        #region Public

        #region Public - Constants
        [Obsolete("SPRITE_VARIANT_A is deprecated, please use SpriteVariant.A instead.")]
        public const int SPRITE_VARIANT_A = 0;
        [Obsolete("SPRITE_VARIANT_B is deprecated, please use SpriteVariant.B instead.")]
        public const int SPRITE_VARIANT_B = 1;
        public const int SPRITE_VARIANT_COUNT = (int)SpriteVariant.B + 1;
        #endregion Public - Constants

        #region Public - Enums
        public enum SpriteVariant { A = 0, B = 1 }
        #endregion Public - Enums

        #region Public -  Static Fields
        public static string PlayerName;
        #endregion Public -  Static Fields

        #region Public - Properties
        public bool IsBusy => _isMoving || _hasTarget;
        public float MovementSpeed => _movementSpeed;
        public Transform ShieldTransform => _playerShield.Transform;
        public Transform CharacterTransform => _playerCharacter.transform;
        public Transform SoulTransform => _playerSoul.transform;
        public float ImpactForce => _impactForce;
        #endregion Public - Properties

        #region Public - Methods

        public static PlayerActor InstantiatePrefabFor(PlayerDriverPhoton playerDriver, int playerPos, PlayerActor playerPrefab, string gameObjectName, float scale)
        {
            PlayerName = gameObjectName;
            Debug.Log($"heoooo{gameObjectName}");
            var instantiationGridPosition = Context.GetBattlePlayArea.GetPlayerStartPosition(playerPos);
            var instantiationPosition = Context.GetGridManager.GridPositionToWorldPoint(instantiationGridPosition);
            var instance = Instantiate(playerPrefab, instantiationPosition, Quaternion.identity);
            Assert.IsNotNull(instance, $"bad prefab: {playerPrefab.name}");
            instance.name = instance.name.Replace("Clone", gameObjectName);
            switch (playerPos)
            {
                case PhotonBattle.PlayerPosition1:
                    instance.gameObject.layer = Layers.Player1;
                    break;
                case PhotonBattle.PlayerPosition2:
                    instance.gameObject.layer = Layers.Player2;
                    break;
                case PhotonBattle.PlayerPosition3:
                    instance.gameObject.layer = Layers.Player3;
                    break;
                case PhotonBattle.PlayerPosition4:
                    instance.gameObject.layer = Layers.Player4;
                    break;
                default:
                    throw new UnityException($"Invalid player position {playerPos}");
            }
            instance.transform.localScale = Vector3.one * scale;
            instance.SetPlayerDriver(playerDriver);
            return instance;
        }

        public void ResetSprite()
        {
            _playerCharacter.SpritIndex = PlayerCharacter.SpriteIndexEnum.IdleWithShield;
        }

        #region Public - Methods - Setters

        public void SetPlayerDriver(PlayerDriverPhoton playerDriver)
        {
            _playerDriver = playerDriver;
        }

        public void SetSpriteVariant(SpriteVariant variant)
        {
            _playerCharacter.SpriteVariant = variant;
            _playerShield.PoseManager.SetSpriteVariant((int)variant);
        }


        public void SetRotation(float angle)
        {
            float multiplier = Mathf.Round(angle / _angleLimit);
            float newAngle = _angleLimit * multiplier;
            //_geometryRoot.eulerAngles = new Vector3(0, 0, newAngle);
            _playerShield.Transform.eulerAngles = new Vector3(0, 0, newAngle + 180f);
            _playerCharacter.transform.eulerAngles = new Vector3(0, 0, newAngle);
            _playerSoul.transform.eulerAngles = new Vector3(0, 0, newAngle);
        }

        /* old
        public void SetShieldRotation(float angle)
        {
            float multiplier = Mathf.Round(angle / _angleLimit);
            float newAngle = _angleLimit * multiplier;
            _playerShield.PoseManager.SetShieldSpriteRotation(newAngle);
        }

        public void SetCharacterRotation(float angle)
        {
            float multiplier = Mathf.Round(angle / _angleLimit);
            float newAngle = _angleLimit * multiplier;
            _playerCharacter.Transform.eulerAngles = new Vector3(0, 0, newAngle);
        }
        */

        public void SetShieldPose(int poseIndex)
        {
            StartCoroutine(ShieldDeformDelay(poseIndex));
        }

        #endregion Public - Methods - Setters

        public void MoveTo(Vector2 targetPosition, int teleportUpdateNumber)
        {
            Debug.Log(string.Format(
                DEBUG_LOG_NAME_AND_TIME_AND_PLAYER_INFO + "Moving (current position: {3}, target position: {4})",
                _syncedFixedUpdateClock.UpdateCount,
                _playerDriver.TeamNumber,
                _playerDriver.PlayerPos,
                _playerShield.Transform.position,
                targetPosition
            ));

            _isMoving = true;
            _playerCharacter.transform.position = _playerShield.Transform.position;
            _playerSoul.Show = true;
            Debug.Log(string.Format(DEBUG_LOG_IS_MOVING, _syncedFixedUpdateClock.UpdateCount, _playerDriver.TeamNumber, _playerDriver.PlayerPos, _isMoving));

            float targetDistance = (targetPosition - new Vector2(_playerShield.Transform.position.x, _playerShield.Transform.position.y)).magnitude;
            float movementTimeS = (float)_syncedFixedUpdateClock.ToSeconds(Mathf.Max(teleportUpdateNumber - _syncedFixedUpdateClock.UpdateCount, 1));
            float movementSpeed = targetDistance / movementTimeS;

            Coroutine move = StartCoroutine(MoveCoroutine(targetPosition, movementSpeed));
            _syncedFixedUpdateClock.ExecuteOnUpdate(teleportUpdateNumber, 1, () =>
            {
                StopCoroutine(move);
                Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME_AND_PLAYER_INFO + "Move coroutine stopped", _syncedFixedUpdateClock.UpdateCount, _playerDriver.TeamNumber, _playerDriver.PlayerPos));

                _hasTarget = false;

                _playerShield.Transform.position = targetPosition;
                _playerShield.PoseManager.SetShieldSpriteOpacity(1f);

                _playerCharacter.transform.position = targetPosition;
                _playerCharacter.SpritIndex = _isUsingShield ? PlayerCharacter.SpriteIndexEnum.IdleWithShield : PlayerCharacter.SpriteIndexEnum.IdleWithoutShield;

                _playerSoul.Show = false;

                _isMoving = false;

                Debug.Log(string.Format(DEBUG_LOG_HAS_TARGET, _syncedFixedUpdateClock.UpdateCount, _playerDriver.TeamNumber, _playerDriver.PlayerPos, _hasTarget));
                Debug.Log(string.Format(DEBUG_LOG_IS_MOVING, _syncedFixedUpdateClock.UpdateCount, _playerDriver.TeamNumber, _playerDriver.PlayerPos, _isMoving));
                Debug.Log(string.Format(
                    DEBUG_LOG_NAME_AND_TIME_AND_PLAYER_INFO + "Shield teleported (current position: {3})",
                    _syncedFixedUpdateClock.UpdateCount,
                    _playerDriver.TeamNumber,
                    _playerDriver.PlayerPos,
                    _playerShield.Transform.position
                ));
            });
        }

        /* OLD
        public void ShieldHit(int damage)
        {
            if (!_allowShieldHit)
            {
                return;
            }
            _allowShieldHit = false;
            if (_playerShield.PoseManager == null)
            {
                return;
            }
            if (_currentPoseIndex < _maxPoseIndex)
            {
                StartCoroutine(ShieldHitDelay(damage));
            }
        }
        */

        public bool OnBallShieldCollision() => _playerClass.OnBallShieldCollision();
        public void OnBallShieldBounce() => _playerClass.OnBallShieldBounce();

        #endregion Public - Methods

        #endregion Public

        #region Private

        #region Private - Fields

        // Config
        private int _shieldResistance;
        private float _angleLimit;
        private int _maxPoseIndex;

        // Delays
        private float _shieldDeformDelay;
        private float _shieldHitDelay;

        // State
        private bool _startBool = true;
        private bool _hasTarget;
        private bool _isMoving;
        private bool _isUsingShield;
        private bool _allowShieldHit;
        private int _shieldHitPoints;
        private int _currentPoseIndex;

        private IPlayerClass _playerClass;

        private class PlayerShield
        {
            public Transform Transform;
            public ShieldPoseManager PoseManager;
            //public SpriteRenderer SpriteRenderer;

            public PlayerShield(Transform transform)
            {
                Transform = transform;
                PoseManager = Transform.GetComponent<ShieldPoseManager>();
            }
        }
        private PlayerShield _playerShield;

        private PlayerCharacter _playerCharacter;
        private PlayerSoul _playerSoul;

        private Vector3 _tempPosition;

        // Drivers
        private PlayerDriverPhoton _playerDriver;
        private readonly List<IDriver> _otherDrivers = new();

        // Components
        //private Transform _transform;
        //private ShieldPoseManager _shieldPoseManager;
        private AudioSource _audioSource;

        private SyncedFixedUpdateClock _syncedFixedUpdateClock;

        #endregion Private - Fields

        #region DEBUG
        private const string DEBUG_LOG_NAME = "[BATTLE] [PLAYER ACTOR] ";
        private const string DEBUG_LOG_NAME_AND_TIME = "[{0:000000}] " + DEBUG_LOG_NAME;
        private const string DEBUG_LOG_NAME_AND_TIME_AND_PLAYER_INFO = DEBUG_LOG_NAME_AND_TIME + "(team: {1}, pos: {2}) ";
        private const string DEBUG_LOG_IS_MOVING = DEBUG_LOG_NAME_AND_TIME_AND_PLAYER_INFO + "Is moving: {3}";
        private const string DEBUG_LOG_HAS_TARGET = DEBUG_LOG_NAME_AND_TIME_AND_PLAYER_INFO + "Has target: {3}";
        #endregion DEBUG

        #region Private - Methods

        private void Awake()
        {
            GameVariables variables = GameConfig.Get().Variables;

            // get config
            _shieldResistance = variables._shieldResistance;
            _angleLimit = variables._angleLimit;

            // get delays
            _shieldDeformDelay = variables._shieldDeformDelay;
            _shieldHitDelay = variables._shieldHitDelay;

            // set state
            _hasTarget = false;
            _isMoving = false;
            _isUsingShield = true;
            _allowShieldHit = true;
            _shieldHitPoints = _shieldResistance;

            _playerClass = _geometryRoot.GetComponentInChildren<IPlayerClass>();
            _playerShield = new(_geometryRoot.Find("BoxShield"));
            _playerCharacter = _geometryRoot.GetComponentInChildren<PlayerCharacter>();
            _playerSoul = _geometryRoot.GetComponentInChildren<PlayerSoul>();

            // get components
            //_transform = GetComponent<Transform>();
            //_shieldPoseManager = GetComponentInChildren<ShieldPoseManager>();
            _audioSource = GetComponent<AudioSource>();

            _syncedFixedUpdateClock = Context.GetSyncedFixedUpdateClock;

            // subscribe to messages
            this.Subscribe<TeamsAreReadyForGameplay>(OnTeamsReadyForGameplay);


            if (_playerShield.PoseManager != null)
            {
                StartCoroutine(ResetPose());
            }

            if (_startBool == true)
            {
                SeePlayerName = PlayerName;
                _startBool = false;
            }
            Debug.Log($"{gameObject.name} {SeePlayerName}");
        }

        #region Private - Methods - Message Listeners
        private void OnTeamsReadyForGameplay(TeamsAreReadyForGameplay data)
        {
            foreach (IDriver driver in data.AllDrivers)
            {
                if (driver.ActorShieldTransform == _playerShield.Transform) continue;
                _otherDrivers.Add(driver);
            }
        }
        #endregion Private - Methods - Message Listeners

        #region Private - Methods - Coroutines

        private IEnumerator ResetPose()
        {
            yield return new WaitUntil(() => _playerShield.PoseManager.MaxPoseIndex > -1);
            _currentPoseIndex = 0;
            _playerShield.PoseManager.SetPose(_currentPoseIndex);
            _playerShield.PoseManager.SetHitboxActive(true);
            _playerShield.PoseManager.SetShow(true);
            _maxPoseIndex = _playerShield.PoseManager.MaxPoseIndex;
        }

        private IEnumerator MoveCoroutine(Vector2 position, float movementSpeed)
        {
            Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME_AND_PLAYER_INFO + "Move coroutine started", _syncedFixedUpdateClock.UpdateCount, _playerDriver.TeamNumber, _playerDriver.PlayerPos));
            Vector3 targetPosition = position;
            _hasTarget = true;
            Debug.Log(string.Format(DEBUG_LOG_HAS_TARGET, _syncedFixedUpdateClock.UpdateCount, _playerDriver.TeamNumber, _playerDriver.PlayerPos, _hasTarget));
            while (_hasTarget)
            {
                yield return null;
                float maxDistanceDelta = movementSpeed * Time.deltaTime;
                _tempPosition = Vector3.MoveTowards(_playerSoul.transform.position, targetPosition, maxDistanceDelta);
                _playerSoul.transform.position = _tempPosition;
                _hasTarget = !(Mathf.Approximately(_tempPosition.x, targetPosition.x) && Mathf.Approximately(_tempPosition.y, targetPosition.y));
            }
            Debug.Log(string.Format(DEBUG_LOG_HAS_TARGET, _syncedFixedUpdateClock.UpdateCount, _playerDriver.TeamNumber, _playerDriver.PlayerPos, _hasTarget));
            Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME_AND_PLAYER_INFO + "Move coroutine finished", _syncedFixedUpdateClock.UpdateCount, _playerDriver.TeamNumber, _playerDriver.PlayerPos));
        }

        private IEnumerator ShieldDeformDelay(int poseIndex)
        {
            yield return new WaitForSeconds(_shieldDeformDelay);
            _playerShield.PoseManager.SetPose(poseIndex);
            //_audioSource.Play();
        }

        private IEnumerator ShieldHitDelay(int damage)
        {
            yield return new WaitForSeconds(_shieldHitDelay);
            _shieldHitPoints -= damage;
            if (_shieldHitPoints <= 0)
            {
                _currentPoseIndex++;
                _playerDriver.SetCharacterPose(_currentPoseIndex);
                _shieldHitPoints = _shieldResistance;
            }
            _allowShieldHit = true;
        }

        #endregion Private - Methods - Coroutines

        private void FixedUpdate()
        {
            bool useShield = true;
            foreach (IDriver driver in _otherDrivers)
            {
                if ((_playerShield.Transform.position - driver.ActorShieldTransform.position).sqrMagnitude < _shieldActivationDistance * _shieldActivationDistance)
                {
                    useShield = false;
                    break;
                }
            }

            if (_isUsingShield == useShield) return;
            _isUsingShield = useShield;

            if (useShield)
            {
                _playerCharacter.SpritIndex = PlayerCharacter.SpriteIndexEnum.IdleWithShield;
                _playerShield.PoseManager.SetHitboxActive(true);
                _playerShield.PoseManager.SetShow(true);
            }
            else
            {
                if (!_isMoving)
                {
                    _playerCharacter.SpritIndex = PlayerCharacter.SpriteIndexEnum.IdleWithoutShield;
                }
                _playerShield.PoseManager.SetShow(false);
                _playerShield.PoseManager.SetHitboxActive(false);
            }

        }

        #endregion Private - Methods

        #endregion Private
    }
}
