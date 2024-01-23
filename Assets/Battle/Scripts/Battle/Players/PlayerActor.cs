using System.Collections;
using Altzone.Scripts.Battle;
using Altzone.Scripts.Config;
using UnityConstants;
using UnityEngine;
using UnityEngine.Assertions;
using Prg.Scripts.Common.PubSub;
using System.Collections.Generic;

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
        // Serialized Fields
        [SerializeField] private Transform _geometryRoot;
        [SerializeField] private float _movementSpeed;
        [SerializeField] private float _shieldActivationDistance;
        [SerializeField] private bool _useNewRotationSysten;
        [SerializeField] private Sprite[] _playerCharacterSpriteSheet;
        [SerializeField] public string SeePlayerName;

        // Public Static Fields
        public static string PlayerName;

        // Public Properties
        public bool IsBusy => _isMoving || _hasTarget;
        public float MovementSpeed => _movementSpeed;
        public bool IsUsingNewRotionSysten => _useNewRotationSysten;

        #region Public Methods

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

        #region Public Methods - Setters

        public void SetPlayerDriver(PlayerDriverPhoton playerDriver)
        {
            _playerDriver = playerDriver;
        }

        public void SetRotation(float angle)
        {
            var multiplier = Mathf.Round(angle / _angleLimit);
            var newAngle = _angleLimit * multiplier;
            _geometryRoot.eulerAngles = new Vector3(0, 0, newAngle);
        }

        public void SetCharacterRotation(float angle)
        {
            var multiplier = Mathf.Round(angle / _angleLimit);
            var newAngle = _angleLimit * multiplier;
            _playerCharacter.Transform.eulerAngles = new Vector3(0, 0, newAngle);
        }

        public void SetShieldRotation(float angle)
        {
            var multiplier = Mathf.Round(angle / _angleLimit);
            var newAngle = _angleLimit * multiplier;
            _shieldPoseManager.SetShieldSpriteRotation(newAngle);
        }

        public void SetCharacterPose(int poseIndex)
        {
            StartCoroutine(ShieldDeformDelay(poseIndex));
        }

        #endregion Public Methods - Setters

        public void MoveTo(Vector2 targetPosition, int teleportUpdateNumber)
        {
            _isMoving = true;
            _playerCharacter.SpriteRenderer.sprite = _playerCharacterSpriteSheet[0];
            _playerCharacter.Transform.position = transform.position;

            float targetDistance = (targetPosition - new Vector2(_transform.position.x, _transform.position.y)).magnitude;
            float movementTimeS = (float)_syncedFixedUpdateClock.ToSeconds(Mathf.Max(teleportUpdateNumber - _syncedFixedUpdateClock.UpdateCount, 1));
            float movementSpeed = targetDistance / movementTimeS;

            Coroutine move = StartCoroutine(MoveCoroutine(targetPosition, movementSpeed));
            _syncedFixedUpdateClock.ExecuteOnUpdate(teleportUpdateNumber, 1, () =>
            {
                StopCoroutine(move);
                _hasTarget = false;
                _transform.position = targetPosition;
                _playerCharacter.Transform.position = targetPosition;
                _isMoving = false;
            });
        }

        public void ShieldHit(int damage)
        {
            if (!_allowShieldHit)
            {
                return;
            }
            _allowShieldHit = false;
            if (_shieldPoseManager == null)
            {
                return;
            }
            if (_currentPoseIndex < _maxPoseIndex)
            {
                StartCoroutine(ShieldHitDelay(damage));
            }
        }

        #endregion Public Methods

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

        private class PlayerCharacter
        {
            public Transform Transform;
            public SpriteRenderer SpriteRenderer;

            public PlayerCharacter(Transform transform)
            {
                Transform = transform;
                SpriteRenderer = Transform.GetComponentInChildren<SpriteRenderer>();
            }
        }
        private PlayerCharacter _playerCharacter;

        private Vector3 _tempPosition;

        // Drivers
        private PlayerDriverPhoton _playerDriver;
        private List<IDriver> _otherDrivers = new();

        // Components
        private Transform _transform;
        private ShieldPoseManager _shieldPoseManager;
        private AudioSource _audioSource;

        private SyncedFixedUpdateClockTest _syncedFixedUpdateClock;

        private void Awake()
        {
            var variables = GameConfig.Get().Variables;

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

            _playerCharacter = new(_geometryRoot.Find("PLayerCharacter"));

            // get components
            _transform = GetComponent<Transform>();
            _shieldPoseManager = GetComponentInChildren<ShieldPoseManager>();
            _audioSource = GetComponent<AudioSource>();

            _syncedFixedUpdateClock = Context.GetSyncedFixedUpdateClock;

            // subscribe to messages
            this.Subscribe<TeamsAreReadyForGameplay>(OnTeamsReadyForGameplay);


            if (_shieldPoseManager != null)
            {
                StartCoroutine(ResetPose());
            }

            _playerCharacter.SpriteRenderer.sprite = _playerCharacterSpriteSheet[3];


            if (_startBool == true)
            {
                SeePlayerName = PlayerName;
                _startBool = false;
            }
            Debug.Log($"{gameObject.name} {SeePlayerName}");
        }

        #region Message Listeners
        void OnTeamsReadyForGameplay(TeamsAreReadyForGameplay data)
        {
            foreach (IDriver driver in data.AllDrivers)
            {
                if (driver.ActorTransform == _transform) continue;
                _otherDrivers.Add(driver);
            }
        }
        #endregion Message Listeners

        #region Coroutines

        private IEnumerator ResetPose()
        {
            yield return new WaitUntil(() => _shieldPoseManager.MaxPoseIndex > 0);
            _currentPoseIndex = 0;
            _shieldPoseManager.SetPose(_currentPoseIndex);
            _shieldPoseManager.SetHitboxActive(true);
            _shieldPoseManager.SetShow(true);
            _maxPoseIndex = _shieldPoseManager.MaxPoseIndex;
        }

        private IEnumerator MoveCoroutine(Vector2 position, float movementSpeed)
        {
            Vector3 targetPosition = position;
            _hasTarget = true;
            while (_hasTarget)
            {
                yield return null;
                float maxDistanceDelta = movementSpeed * Time.deltaTime;
                _tempPosition = Vector3.MoveTowards(_playerCharacter.Transform.position, targetPosition, maxDistanceDelta);
                _playerCharacter.Transform.position = _tempPosition;
                _hasTarget = !(Mathf.Approximately(_tempPosition.x, targetPosition.x) && Mathf.Approximately(_tempPosition.y, targetPosition.y));
            }
            _playerCharacter.SpriteRenderer.sprite = _playerCharacterSpriteSheet[_isUsingShield ? 3 : 1];
        }

        private IEnumerator ShieldDeformDelay(int poseIndex)
        {
            yield return new WaitForSeconds(_shieldDeformDelay);
            _shieldPoseManager.SetPose(poseIndex);
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

        #endregion Coroutines

        private void FixedUpdate()
        {
            bool useShield = true;
            foreach (IDriver driver in _otherDrivers)
            {
                if ((_transform.position - driver.ActorTransform.position).sqrMagnitude < _shieldActivationDistance * _shieldActivationDistance)
                {
                    useShield = false;
                    break;
                }
            }

            if (_isUsingShield == useShield) return;
            _isUsingShield = useShield;

            if (useShield)
            {
                _playerCharacter.SpriteRenderer.sprite = _playerCharacterSpriteSheet[3];
                _shieldPoseManager.SetHitboxActive(true);
                _shieldPoseManager.SetShow(true);
            }
            else
            {
                if (!_isMoving)
                {
                    _playerCharacter.SpriteRenderer.sprite = _playerCharacterSpriteSheet[1];
                }
                _shieldPoseManager.SetShow(false);
                _shieldPoseManager.SetHitboxActive(false);
            }

        }
    }
}
