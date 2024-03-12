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
        [SerializeField] private float _impactForce;
        [SerializeField] private bool _useNewRotationSysten; // unused (old and "new" rotation systems are replaced by newer system)
        [SerializeField] private Sprite[] _playerCharacterSpriteSheet;
        [SerializeField] public string SeePlayerName;

        // Public Constants
        public const int SPRITE_VARIANT_A = 0;
        public const int SPRITE_VARIANT_B = 1;
        public const int SPRITE_VARIANT_COUNT = SPRITE_VARIANT_B + 1;

        // Public Static Fields
        public static string PlayerName;

        // Public Properties
        public bool IsBusy => _isMoving || _hasTarget;
        public float MovementSpeed => _movementSpeed;
        //public bool IsUsingNewRotionSysten => _useNewRotationSysten;
        public Transform ShieldTransform => _playerShield.Transform;
        public Transform CharacterTransform => _playerCharacter.Transform;
        public Transform SoulTransform => _playerSoul.Transform;
        public bool SpecialAbilityOverridesBallBounce => _playerClass.SpecialAbilityOverridesBallBounce;
        public float ImpactForce => _impactForce;

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

        public void ResetSprite()
        {
            _playerCharacter.SetSprite(PlayerCharacter.INDE_WITH_SHIELD_SPRITE_INDEX, _playerCharacterSpriteSheet);
        }

        #region Public Methods - Setters

        public void SetPlayerDriver(PlayerDriverPhoton playerDriver)
        {
            _playerDriver = playerDriver;
        }

        public void SetSpriteVariant(int variant)
        {
            _playerCharacter.SetSpriteVariant(variant);
            _playerShield.PoseManager.SetSpriteVariant(variant);
        }


        public void SetRotation(float angle)
        {
            float multiplier = Mathf.Round(angle / _angleLimit);
            float newAngle = _angleLimit * multiplier; 
            //_geometryRoot.eulerAngles = new Vector3(0, 0, newAngle);
            _playerShield.Transform.eulerAngles = new Vector3(0, 0, newAngle + 180f);
            _playerCharacter.Transform.eulerAngles = new Vector3(0, 0, newAngle);
            _playerSoul.Transform.eulerAngles = new Vector3(0, 0, newAngle);
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

        #endregion Public Methods - Setters

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
            //_playerCharacter.SetSprite(PlayerCharacter.MOVING_SPRITE_INDEX, _playerCharacterSpriteSheet);
            _playerCharacter.Transform.position = _playerShield.Transform.position;
            _playerSoul.setShow(true);
            Debug.Log(string.Format(DEBUG_LOG_IS_MOVING, _syncedFixedUpdateClock.UpdateCount, _playerDriver.TeamNumber, _playerDriver.PlayerPos, _isMoving));

            float targetDistance = (targetPosition - new Vector2(_playerShield.Transform.position.x, _playerShield.Transform.position.y)).magnitude;
            float movementTimeS = (float)_syncedFixedUpdateClock.ToSeconds(Mathf.Max(teleportUpdateNumber - _syncedFixedUpdateClock.UpdateCount, 1));
            float movementSpeed = targetDistance / movementTimeS;

            //Coroutine animation = StartCoroutine(TeleportAnimationCoroutine(movementTimeS));
            Coroutine move = StartCoroutine(MoveCoroutine(targetPosition, movementSpeed));
            _syncedFixedUpdateClock.ExecuteOnUpdate(teleportUpdateNumber, 1, () =>
            {
                //StopCoroutine(animation);
                StopCoroutine(move);
                //Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME_AND_PLAYER_INFO + "Teleport animatio coroutine stopped", _syncedFixedUpdateClock.UpdateCount, _playerDriver.TeamNumber, _playerDriver.PlayerPos));
                Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME_AND_PLAYER_INFO + "Move coroutine stopped", _syncedFixedUpdateClock.UpdateCount, _playerDriver.TeamNumber, _playerDriver.PlayerPos));

                _hasTarget = false;

                _playerShield.Transform.position = targetPosition;
                _playerShield.PoseManager.SetShieldSpriteOpacity(1f);

                _playerCharacter.Transform.position = targetPosition;
                _playerCharacter.SetSprite(_isUsingShield ? PlayerCharacter.INDE_WITH_SHIELD_SPRITE_INDEX : PlayerCharacter.INDE_WITHOUT_SHIELD_SPRITE_INDEX, _playerCharacterSpriteSheet);
                _playerCharacter.Opacity = 1f;

                _playerSoul.setShow(false);

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

        public void ActivateSpecialAbility()
        {
            _playerClass.ActivateSpecialAbility();
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

        private class PlayerCharacter
        {
            public const int INDE_WITHOUT_SHIELD_SPRITE_INDEX = 0;
            public const int INDE_WITH_SHIELD_SPRITE_INDEX = 1;
            public const int MOVING_SPRITE_INDEX = 2;
            public const int SPRITE_COUNT = MOVING_SPRITE_INDEX + 1;

            public Transform Transform;
            public float Opacity
            {
                get => _spriteRenderers[_spriteVariant].color.a;
                set
                {
                    Color color = _spriteRenderers[_spriteVariant].color;
                    color.a = value;
                    _spriteRenderers[_spriteVariant].color = color;
                }
            }

            public PlayerCharacter(Transform transform)
            {
                Transform = transform;
                _spriteGameObjects = new GameObject[SPRITE_VARIANT_COUNT];
                _spriteGameObjects[SPRITE_VARIANT_A] = transform.Find("SpriteA").gameObject;
                _spriteGameObjects[SPRITE_VARIANT_B] = transform.Find("SpriteB").gameObject;
                _spriteRenderers = new SpriteRenderer[SPRITE_VARIANT_COUNT];
                _spriteRenderers[SPRITE_VARIANT_A] = _spriteGameObjects[SPRITE_VARIANT_A].GetComponent<SpriteRenderer>();
                _spriteRenderers[SPRITE_VARIANT_B] = _spriteGameObjects[SPRITE_VARIANT_B].GetComponent<SpriteRenderer>();
            }

            public void SetSpriteVariant(int variant)
            {
                _spriteGameObjects[_spriteVariant].SetActive(false);
                _spriteVariant = variant;
                _spriteGameObjects[_spriteVariant].SetActive(true);
            }

            public void SetSprite(int index, Sprite[] spriteSheet)
            {
                _spriteRenderers[_spriteVariant].sprite = spriteSheet[SPRITE_COUNT * _spriteVariant + index];
            }

            private GameObject[] _spriteGameObjects;
            private SpriteRenderer[] _spriteRenderers;
            private int _spriteVariant;
        }
        private PlayerCharacter _playerCharacter;

        private class PlayerSoul
        {
            public Transform Transform;

            public PlayerSoul(Transform transform)
            {
                Transform = transform;
                _spriteGameObjects = new GameObject[SPRITE_VARIANT_COUNT];
                _spriteGameObjects[SPRITE_VARIANT_A] = transform.Find("SpriteA").gameObject;
                _spriteGameObjects[SPRITE_VARIANT_B] = transform.Find("SpriteB").gameObject;
                _spriteRenderers = new SpriteRenderer[SPRITE_VARIANT_COUNT];
                _spriteRenderers[SPRITE_VARIANT_A] = _spriteGameObjects[SPRITE_VARIANT_A].GetComponent<SpriteRenderer>();
                _spriteRenderers[SPRITE_VARIANT_B] = _spriteGameObjects[SPRITE_VARIANT_B].GetComponent<SpriteRenderer>();
            }

            public void setShow(bool show)
            {
                _spriteRenderers[_spriteVariant].enabled = show;
            }

            public void SetSpriteVariant(int variant)
            {
                _spriteGameObjects[_spriteVariant].SetActive(false);
                _spriteVariant = variant;
                _spriteGameObjects[_spriteVariant].SetActive(true);
            }

            private GameObject[] _spriteGameObjects;
            private SpriteRenderer[] _spriteRenderers;
            private int _spriteVariant;
        }
        private PlayerSoul _playerSoul;

        private Vector3 _tempPosition;

        // Drivers
        private PlayerDriverPhoton _playerDriver;
        private List<IDriver> _otherDrivers = new();

        // Components
        //private Transform _transform;
        //private ShieldPoseManager _shieldPoseManager;
        private AudioSource _audioSource;

        private SyncedFixedUpdateClockTest _syncedFixedUpdateClock;

        // Debug
        private const string DEBUG_LOG_NAME = "[BATTLE] [PLAYER ACTOR] ";
        private const string DEBUG_LOG_NAME_AND_TIME = "[{0:000000}] " + DEBUG_LOG_NAME;
        private const string DEBUG_LOG_NAME_AND_TIME_AND_PLAYER_INFO = DEBUG_LOG_NAME_AND_TIME + "(team: {1}, pos: {2}) ";
        private const string DEBUG_LOG_IS_MOVING = DEBUG_LOG_NAME_AND_TIME_AND_PLAYER_INFO + "Is moving: {3}";
        private const string DEBUG_LOG_HAS_TARGET = DEBUG_LOG_NAME_AND_TIME_AND_PLAYER_INFO + "Has target: {3}";

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

            _playerClass = _geometryRoot.GetComponentInChildren<IPlayerClass>();
            _playerShield = new(_geometryRoot.Find("BoxShield"));
            _playerCharacter = new(_geometryRoot.Find("PLayerCharacter"));
            _playerSoul = new(_geometryRoot.Find("PLayerSoul"));

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

        #region Message Listeners
        void OnTeamsReadyForGameplay(TeamsAreReadyForGameplay data)
        {
            foreach (IDriver driver in data.AllDrivers)
            {
                if (driver.ActorShieldTransform == _playerShield.Transform) continue;
                _otherDrivers.Add(driver);
            }
        }
        #endregion Message Listeners

        #region Coroutines

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
                //_tempPosition = Vector3.MoveTowards(_playerCharacter.Transform.position, targetPosition, maxDistanceDelta);
                _tempPosition = Vector3.MoveTowards(_playerSoul.Transform.position, targetPosition, maxDistanceDelta);
                //_playerCharacter.Transform.position = _tempPosition;
                _playerSoul.Transform.position = _tempPosition;
                _hasTarget = !(Mathf.Approximately(_tempPosition.x, targetPosition.x) && Mathf.Approximately(_tempPosition.y, targetPosition.y));
            }
            Debug.Log(string.Format(DEBUG_LOG_HAS_TARGET, _syncedFixedUpdateClock.UpdateCount, _playerDriver.TeamNumber, _playerDriver.PlayerPos, _hasTarget));
            Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME_AND_PLAYER_INFO + "Move coroutine finished", _syncedFixedUpdateClock.UpdateCount, _playerDriver.TeamNumber, _playerDriver.PlayerPos));
        }

        private IEnumerator TeleportAnimationCoroutine(float duration)
        {
            Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME_AND_PLAYER_INFO + "Teleport animatio coroutine started", _syncedFixedUpdateClock.UpdateCount, _playerDriver.TeamNumber, _playerDriver.PlayerPos));
            float time = 0;
            while (time < duration)
            {
                yield return null;
                time += Time.deltaTime;
                float opacity = Mathf.Sin((time / duration * 4 + 0.5f) * Mathf.PI) * 0.5f + 0.5f;
                _playerCharacter.Opacity = opacity;
                _playerShield.PoseManager.SetShieldSpriteOpacity(opacity);
            }

            Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME_AND_PLAYER_INFO + "Teleport animatio coroutine finished", _syncedFixedUpdateClock.UpdateCount, _playerDriver.TeamNumber, _playerDriver.PlayerPos));
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

        #endregion Coroutines

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
                _playerCharacter.SetSprite(PlayerCharacter.INDE_WITH_SHIELD_SPRITE_INDEX, _playerCharacterSpriteSheet);
                _playerShield.PoseManager.SetHitboxActive(true);
                _playerShield.PoseManager.SetShow(true);
            }
            else
            {
                if (!_isMoving)
                {
                    _playerCharacter.SetSprite(PlayerCharacter.INDE_WITHOUT_SHIELD_SPRITE_INDEX, _playerCharacterSpriteSheet);
                }
                _playerShield.PoseManager.SetShow(false);
                _playerShield.PoseManager.SetHitboxActive(false);
            }

        }
    }
}
