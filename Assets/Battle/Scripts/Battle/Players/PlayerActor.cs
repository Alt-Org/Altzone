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
        [SerializeField] private Transform _geometryRoot;
        [SerializeField] private float _movementSpeed;
        [SerializeField] private float _shieldActivationDistance;
        public string SeePlayerName;
        public static string PlayerName;
        private bool StartBool = true;

        [SerializeField] private Sprite[] _playerCharacterSpriteSheet;

        // public const double PLAYER_SHIELD_ANIMATION_LENGTH_SECONDS

        private SyncedFixedUpdateClockTest _syncedFixedUpdateClock;
        private PlayerDriverPhoton _playerDriver;
        private Transform _playerCharacterTransform;
        // private Animator _playerCharacterAnimator;
        private SpriteRenderer _playerCharacterSpriteRenderer;
        private ShieldPoseManager _shieldPoseManager;
        // private float _playerMoveSpeedMultiplier;
        private Transform _transform;
        private Vector3 _tempPosition;
        private bool _hasTarget;
        private bool _isMoving;
        private bool _isUsingShield;
        private int _shieldResistance;
        private int _shieldHitPoints;
        private float _shieldDeformDelay;
        private float _shieldHitDelay;
        private float _angleLimit;
        private int _maxPoseIndex;
        private int _currentPoseIndex;
        private bool _allowShieldHit;

        private List<IDriver> _otherDrivers = new();

        private void Awake()
        {
            _allowShieldHit = true;
            _syncedFixedUpdateClock = Context.GetSyncedFixedUpdateClock;
            _transform = GetComponent<Transform>();
            _playerCharacterTransform = _geometryRoot.Find("PLayerCharacter");
            //_playerCharacterAnimator = _playerCharacterTransform.GetComponent<Animator>();
            _playerCharacterSpriteRenderer = _playerCharacterTransform.GetComponent<SpriteRenderer>();
            var variables = GameConfig.Get().Variables;
            // _playerMoveSpeedMultiplier = variables._playerMoveSpeedMultiplier;
            _shieldResistance = variables._shieldResistance;
            _shieldHitPoints = _shieldResistance;
            _shieldDeformDelay = variables._shieldDeformDelay;
            _shieldHitDelay = variables._shieldHitDelay;
            _angleLimit = variables._angleLimit;
            _shieldPoseManager = GetComponentInChildren<ShieldPoseManager>();
            if (_shieldPoseManager != null)
            {
                StartCoroutine(ResetPose());
            }

            this.Subscribe<TeamsAreReadyForGameplay>(OnTeamsReadyForGameplay);

            if (StartBool == true)
            {
                SeePlayerName = PlayerName;
                StartBool = false;
            }
            Debug.Log($"{gameObject.name} {SeePlayerName}");
        }

        void OnTeamsReadyForGameplay(TeamsAreReadyForGameplay data)
        {
            foreach (IDriver driver in data.AllDrivers)
            {
                if (driver.ActorTransform == _transform) continue;
                _otherDrivers.Add(driver);
            }
        }

        private IEnumerator MoveCoroutine(Vector2 position, float movementSpeed)
        {
            Vector3 targetPosition = position;
            _hasTarget = true;
            while (_hasTarget)
            {
                yield return null;
                float maxDistanceDelta = movementSpeed * Time.deltaTime;
                _tempPosition = Vector3.MoveTowards(_playerCharacterTransform.position, targetPosition, maxDistanceDelta);
                _playerCharacterTransform.position = _tempPosition;
                _hasTarget = !(Mathf.Approximately(_tempPosition.x, targetPosition.x) && Mathf.Approximately(_tempPosition.y, targetPosition.y));
            }
            //_playerCharacterAnimator.SetTrigger("Shield Animation Trigger");
            _playerCharacterSpriteRenderer.sprite = _playerCharacterSpriteSheet[_isUsingShield ? 3 : 1];
        }

        private IEnumerator ResetPose()
        {
            yield return new WaitUntil(() => _shieldPoseManager.MaxPoseIndex > 0);
            _currentPoseIndex = 0;
            _shieldPoseManager.SetPose(_currentPoseIndex);
            _shieldPoseManager.SetHitboxActive(false);
            _shieldPoseManager.SetShow(false);
            _maxPoseIndex = _shieldPoseManager.MaxPoseIndex;
        }

        private IEnumerator ShieldDeformDelay(int poseIndex)
        {
            yield return new WaitForSeconds(_shieldDeformDelay);
            _shieldPoseManager.SetPose(poseIndex);
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

        public bool IsBusy => _isMoving || _hasTarget;

        public float MovementSpeed => _movementSpeed;

        public void MoveTo(Vector2 targetPosition, int teleportUpdateNumber)
        {
            _isMoving = true;
            //_playerCharacterAnimator.SetTrigger("Moving Trigger");
            _playerCharacterSpriteRenderer.sprite = _playerCharacterSpriteSheet[0];
            _playerCharacterTransform.position = transform.position;

            float targetDistance = (targetPosition - new Vector2(_transform.position.x, _transform.position.y)).magnitude;
            float movementTimeS = (float)_syncedFixedUpdateClock.ToSeconds(Mathf.Max(teleportUpdateNumber - _syncedFixedUpdateClock.UpdateCount, 1));
            float movementSpeed = targetDistance / movementTimeS;

            StartCoroutine(MoveCoroutine(targetPosition, movementSpeed));
            _syncedFixedUpdateClock.ExecuteOnUpdate(teleportUpdateNumber, 1, () =>
            {
                _transform.position = targetPosition;
                _playerCharacterTransform.position = targetPosition;
                _isMoving = false;
            });
        }

        private void FixedUpdate()
        {
            bool useShield = false;
            foreach (IDriver driver in _otherDrivers)
            {
                if ((_transform.position - driver.ActorTransform.position).sqrMagnitude < _shieldActivationDistance * _shieldActivationDistance)
                {
                    useShield = true;
                    break;
                }
            }

            if (_isUsingShield == useShield) return;
            _isUsingShield = useShield;

            if (useShield)
            {
                _playerCharacterSpriteRenderer.sprite = _playerCharacterSpriteSheet[3];
                _shieldPoseManager.SetHitboxActive(true);
                _shieldPoseManager.SetShow(true);
            }
            else
            {
                if (!_isMoving)
                {
                    _playerCharacterSpriteRenderer.sprite = _playerCharacterSpriteSheet[1];
                }
                _shieldPoseManager.SetShow(false);
                _shieldPoseManager.SetHitboxActive(false);
            }

        }

        public void SetPlayerDriver(PlayerDriverPhoton playerDriver)
        {
            _playerDriver = playerDriver;
        }

        public void SetRotation(float angle)
        {
            var multiplier = Mathf.Round (angle / _angleLimit);
            var newAngle = _angleLimit * multiplier;
            _geometryRoot.eulerAngles = new Vector3(0, 0, newAngle);
        }

        public void SetCharacterRotation(float angle)
        {
            var multiplier = Mathf.Round(angle / _angleLimit);
            var newAngle = _angleLimit * multiplier;
            _playerCharacterTransform.eulerAngles = new Vector3(0, 0, newAngle);
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

        public void SetCharacterPose(int poseIndex)
        {
            StartCoroutine(ShieldDeformDelay(poseIndex));
        }

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
    }
}
