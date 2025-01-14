using System;
using System.Collections;
using System.Collections.Generic;
using Altzone.Scripts.Battle;
using Altzone.Scripts.Config;
using Altzone.Scripts.Config.ScriptableObjects;
using Altzone.Scripts.GA;
using Battle1.Scripts.Battle.Game;
using Battle1.Scripts.Battle.Players.PlayerClasses;
using Prg.Scripts.Common.PubSub;
using UnityConstants;
using UnityEngine;
using UnityEngine.Assertions;
using Random = UnityEngine.Random;

/*using PhotonNetwork = Battle1.PhotonUnityNetworking.Code.PhotonNetwork;*/

namespace Battle1.Scripts.Battle.Players
{
    /// <summary>
    /// <c>PlayerActor</c> for local and remote instances.
    /// </summary>
    /// <remarks>
    /// DOES NOT: Needs to derive from <c>PlayerActorBase</c> for type safe UNITY prefab instantiation.
    /// </remarks>
    [Obsolete]
    internal class PlayerActor : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private Transform _geometryRoot;
        [SerializeField] private float _movementSpeed;
        [SerializeField] private float _shieldActivationDistance;
        [SerializeField] private float _impactForce;
        [SerializeField] public string SeePlayerName;
        [SerializeField] private float _sparkleUpdateInterval;
        [SerializeField] private float _timeSinceLastUpdate;

        #endregion Serialized Fields

        #region Public

        #region Public - Constants

        public const int SpriteVariantCount = (int)SpriteVariant.B + 1;

        #endregion Public - Constants

        #region Public - Enums

        public enum SpriteVariant
        {
            A = 0,
            B = 1
        }

        #endregion Public - Enums

        #region Public -  Static Fields

        public static string PlayerName;

        #endregion Public -  Static Fields

        #region Public - Properties

        public IReadOnlyBattlePlayer BattlePlayer => _battlePlayer;
        public bool IsBusy => _isMoving || _hasTarget;
        public float MovementSpeed => _movementSpeed;
        public float ImpactForce => _impactForce;

        #endregion Public - Properties

        #region Public - Methods

        public static void InstantiatePrefabFor(BattlePlayer battlePlayer, string gameObjectName, float scale)
        {
            Debug.Log($"heoooo{gameObjectName}"); // this log could be better

            PlayerName = gameObjectName;

            Characters playerPrefabs = GameConfig.Get().Characters;

            // get instantiation args
            PlayerActor
                playerPrefab =
                    null; //playerPrefabs.GetPlayerPrefab(battlePlayer.BattleCharacter.CharacterID) as PlayerActor;
            Vector2 instantiationPosition;
            {
                Game.GridPos instantiationGridPosition =
                    Context.GetBattlePlayArea.GetPlayerStartPosition(battlePlayer.PlayerPosition);
                instantiationPosition = Context.GetGridManager.GridPositionToWorldPoint(instantiationGridPosition);
            }

            // instantiate instance
            PlayerActor instance = Instantiate(playerPrefab, instantiationPosition, Quaternion.identity);
            Assert.IsNotNull(instance, $"bad prefab: {playerPrefab.name}");

            // init instance
            instance.name = instance.name.Replace("Clone", gameObjectName);
            instance.InitInstance(battlePlayer, scale);
        }

        public void ResetSprite()
        {
            _battlePlayer.PlayerCharacter.SpritIndex = PlayerCharacter.SpriteIndexEnum.IdleWithShield;
        }

        #region Public - Methods - Setters

        public void SetSpriteVariant(SpriteVariant variant)
        {
            _battlePlayer.PlayerCharacter.SpriteVariant = variant;
            _battlePlayer.PlayerShieldManager.SetSpriteVariant(variant);
        }

        public void SetRotation(float angle)
        {
            float multiplier = Mathf.Round(angle / _angleLimit);
            float newAngle = _angleLimit * multiplier;
            //_geometryRoot.eulerAngles = new Vector3(0, 0, newAngle);
            _battlePlayer.PlayerShieldManager.transform.eulerAngles = new Vector3(0, 0, newAngle);
            _battlePlayer.PlayerCharacter.transform.eulerAngles = new Vector3(0, 0, newAngle);
            _battlePlayer.PlayerSoul.transform.eulerAngles = new Vector3(0, 0, newAngle);

            //_shieldHitboxIndicators.transform.eulerAngles = new Vector3(0, 0, newAngle);
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

        #endregion Public - Methods - Setters

        public void MoveTo(Vector2 targetPosition, int teleportUpdateNumber)
        {
            BattleTeamNumber teamNumber = _battlePlayer.BattleTeam.TeamNumber;
            int playerPosition = _battlePlayer.PlayerPosition;

            ShieldManager playerShieldManager = _battlePlayer.PlayerShieldManager;
            PlayerCharacter playerCharacter = _battlePlayer.PlayerCharacter;
            PlayerSoul playerSoul = _battlePlayer.PlayerSoul;

            Debug.Log(string.Format(
                DEBUG_LOG_NAME_AND_TIME_AND_PLAYER_INFO + "Moving (current position: {3}, target position: {4})",
                _syncedFixedUpdateClock.UpdateCount,
                teamNumber,
                playerPosition,
                playerShieldManager.transform.position,
                targetPosition
            ));

            _isMoving = true;
            playerCharacter.transform.position = playerShieldManager.transform.position;
            playerSoul.Show = true;
            Debug.Log(string.Format(DEBUG_LOG_IS_MOVING, _syncedFixedUpdateClock.UpdateCount, teamNumber,
                playerPosition, _isMoving));

            float targetDistance = (targetPosition - new Vector2(playerShieldManager.transform.position.x,
                playerShieldManager.transform.position.y)).magnitude;
            float movementTimeS =
                (float)_syncedFixedUpdateClock.ToSeconds(
                    Mathf.Max(teleportUpdateNumber - _syncedFixedUpdateClock.UpdateCount, 1));
            float movementSpeed = targetDistance / movementTimeS;

            _playerMovementIndicator.transform.position = targetPosition;
            _sparkleSprite.transform.position = targetPosition;
            Vector2 shieldPosition = new(targetPosition.x, targetPosition.y + _shieldHitboxIndicatorsYPosition);
            //_shieldHitboxIndicators.transform.position = shieldPosition;
            //_shieldHitboxIndicators.SetActive(false);

            Coroutine move = StartCoroutine(MoveCoroutine(targetPosition, movementSpeed));
            _syncedFixedUpdateClock.ExecuteOnUpdate(teleportUpdateNumber, 1, () =>
            {
                StopCoroutine(move);
                Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME_AND_PLAYER_INFO + "Move coroutine stopped",
                    _syncedFixedUpdateClock.UpdateCount, teamNumber, playerPosition));

                _hasTarget = false;

                playerShieldManager.transform.position = targetPosition;

                playerCharacter.transform.position = targetPosition;
                playerCharacter.SpritIndex = _isUsingShield
                    ? PlayerCharacter.SpriteIndexEnum.IdleWithShield
                    : PlayerCharacter.SpriteIndexEnum.IdleWithoutShield;

                playerSoul.Show = false;
                /*_shieldHitboxIndicators.SetActive(true);*/

                _isMoving = false;

                Debug.Log(string.Format(DEBUG_LOG_HAS_TARGET, _syncedFixedUpdateClock.UpdateCount, teamNumber,
                    playerPosition, _hasTarget));
                Debug.Log(string.Format(DEBUG_LOG_IS_MOVING, _syncedFixedUpdateClock.UpdateCount, teamNumber,
                    playerPosition, _isMoving));
                Debug.Log(string.Format(
                    DEBUG_LOG_NAME_AND_TIME_AND_PLAYER_INFO + "Shield teleported (current position: {3})",
                    _syncedFixedUpdateClock.UpdateCount,
                    teamNumber,
                    playerPosition,
                    playerShieldManager.transform.position
                ));

                //{ GA info

                if (!_battlePlayer.PlayerDriver.IsLocal) return;

                IReadOnlyBattlePlayer teammate = _battlePlayer.Teammate;
                if (teammate != null)
                {
                    float teammateDistance = (playerShieldManager.transform.position -
                                              teammate.PlayerShieldManager.transform.position).magnitude;
                    GameAnalyticsManager.Instance.DistanceToPlayer(teammateDistance);
                }

                GameAnalyticsManager.Instance.DistanceToWall(Context.GetBattlePlayArea.ArenaHeight / 2 -
                                                             Mathf.Abs(playerShieldManager.transform.position.y));

                //} GA info
            });

            // GA info
            /*   if (PhotonNetwork.IsMasterClient) GameAnalyticsManager.Instance.MoveCommand(targetPosition);*/
        }

        #endregion Public - Methods

        #endregion Public

        #region Private

        #region Private - Fields

        // Config
        private int _shieldResistance;
        private float _angleLimit;

        // State
        private bool _startBool = true;
        private bool _hasTarget;
        private bool _isMoving;
        private bool _isUsingShield;
        private bool _allowShieldHit;
        private int _shieldHitPoints;

        //private CharacterID _characterID;

        // Player Parts

        BattlePlayer _battlePlayer;

        private GameObject _playerMovementIndicator;

        private GameObject _sparkleSprite;

        //private GameObject _shieldHitboxIndicators;
        private float _shieldHitboxIndicatorsYPosition;

        private readonly List<IReadOnlyBattlePlayer> _otherPlayers = new();

        // Components
        private AudioSource _audioSource;

        private SyncedFixedUpdateClock _syncedFixedUpdateClock;

        #endregion Private - Fields

        #region DEBUG

        private const string DEBUG_LOG_NAME = "[BATTLE] [PLAYER ACTOR] ";
        private const string DEBUG_LOG_NAME_AND_TIME = "[{0:000000}] " + DEBUG_LOG_NAME;

        private const string DEBUG_LOG_NAME_AND_TIME_AND_PLAYER_INFO =
            DEBUG_LOG_NAME_AND_TIME + "(team: {1}, pos: {2}) ";

        private const string DEBUG_LOG_IS_MOVING = DEBUG_LOG_NAME_AND_TIME_AND_PLAYER_INFO + "Is moving: {3}";
        private const string DEBUG_LOG_HAS_TARGET = DEBUG_LOG_NAME_AND_TIME_AND_PLAYER_INFO + "Has target: {3}";

        #endregion DEBUG

        #region Private - Methods

        private void InitInstance(BattlePlayer battlePlayer, float scale)
        {
            _battlePlayer = battlePlayer;

            GameVariables variables = GameConfig.Get().Variables;

            // get config
            _shieldResistance = variables._shieldResistance;
            _angleLimit = variables._angleLimit;

            // set state
            _hasTarget = false;
            _isMoving = false;
            _isUsingShield = true;
            _allowShieldHit = true;
            _shieldHitPoints = _shieldResistance;

            gameObject.layer = _battlePlayer.PlayerPosition switch
            {
                PhotonBattle.PlayerPosition1 => Layers.Player1,
                PhotonBattle.PlayerPosition2 => Layers.Player2,
                PhotonBattle.PlayerPosition3 => Layers.Player3,
                PhotonBattle.PlayerPosition4 => Layers.Player4,
                _ => throw new UnityException($"Invalid player position {battlePlayer.PlayerPosition}"),
            };

            transform.localScale = Vector3.one * scale;

            {
                // get player parts
                IPlayerClass playerClass = _geometryRoot.GetComponentInChildren<IPlayerClass>();
                ShieldManager playerShieldManager = _geometryRoot.GetComponentInChildren<ShieldManager>();
                PlayerCharacter playerCharacter = _geometryRoot.GetComponentInChildren<PlayerCharacter>();
                PlayerSoul playerSoul = _geometryRoot.GetComponentInChildren<PlayerSoul>();

                _battlePlayer.SetPlayerActorParts(this, playerClass, playerShieldManager, playerCharacter, playerSoul);

                // init player parts
                playerShieldManager.InitInstance(_battlePlayer);
                playerCharacter.InitInstance(_battlePlayer);
                playerSoul.InitInstance(_battlePlayer);
                playerClass.InitInstance(_battlePlayer);
            }

            _playerMovementIndicator = _geometryRoot.transform.Find("PlayerPositionIndicator").gameObject;
            _sparkleSprite = _geometryRoot.transform.Find("SparkleSprite").gameObject;
            //_shieldHitboxIndicators = _geometryRoot.transform.Find("ShieldHitBoxIndicators").gameObject;

            //_shieldHitboxIndicators.SetActive(false);

            // get components
            _audioSource = GetComponent<AudioSource>();

            _syncedFixedUpdateClock = Context.GetSyncedFixedUpdateClock;

            // subscribe to messages
            this.Subscribe<TeamsAreReadyForGameplay>(OnTeamsReadyForGameplay);

            StartCoroutine(ResetShieldCoroutine());

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
            foreach (IReadOnlyBattlePlayer player in data.AllPlayers)
            {
                BattleTeamNumber teamNumber = player.BattleTeam.TeamNumber;
                if (teamNumber == data.LocalPlayer.BattleTeam.TeamNumber)
                {
                    _shieldHitboxIndicatorsYPosition = (teamNumber == BattleTeamNumber.TeamBeta ? -0.95f : 0.95f);
                }

                if (player == _battlePlayer) continue;
                _otherPlayers.Add(player);
            }
        }

        #endregion Private - Methods - Message Listeners

        #region Private - Methods - Coroutines

        private IEnumerator ResetShieldCoroutine()
        {
            yield return new WaitUntil(() => _battlePlayer.PlayerShieldManager.Initialized);
            _battlePlayer.PlayerShieldManager.SetHitboxActive(true);
            _battlePlayer.PlayerShieldManager.SetShow(true);
        }

        private IEnumerator MoveCoroutine(Vector3 targetPosition, float movementSpeed)
        {
            BattleTeamNumber teamNumber = _battlePlayer.BattleTeam.TeamNumber;
            int playerPosition = _battlePlayer.PlayerPosition;

            Transform playerSoulTransform = _battlePlayer.PlayerSoul.transform;

            Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME_AND_PLAYER_INFO + "Move coroutine started",
                _syncedFixedUpdateClock.UpdateCount, teamNumber, playerPosition));
            _hasTarget = true;
            Debug.Log(string.Format(DEBUG_LOG_HAS_TARGET, _syncedFixedUpdateClock.UpdateCount, teamNumber,
                playerPosition, _hasTarget));
            while (_hasTarget)
            {
                yield return null;
                float maxDistanceDelta = movementSpeed * Time.deltaTime;
                playerSoulTransform.position =
                    Vector3.MoveTowards(playerSoulTransform.position, targetPosition, maxDistanceDelta);
                _hasTarget = !(
                    Mathf.Approximately(playerSoulTransform.position.x, targetPosition.x) &&
                    Mathf.Approximately(playerSoulTransform.position.y, targetPosition.y
                    ));
            }
            Debug.Log(string.Format(DEBUG_LOG_HAS_TARGET, _syncedFixedUpdateClock.UpdateCount, teamNumber,
                playerPosition, _hasTarget));
            Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME_AND_PLAYER_INFO + "Move coroutine finished",
                _syncedFixedUpdateClock.UpdateCount, teamNumber, playerPosition));
        }

        #endregion Private - Methods - Coroutines

        private void FixedUpdate()
        {
            _timeSinceLastUpdate += Time.fixedDeltaTime;

            ShieldManager playerShieldManager = _battlePlayer.PlayerShieldManager;
            PlayerCharacter playerCharacter = _battlePlayer.PlayerCharacter;

            if (playerCharacter.SpriteVariant == SpriteVariant.B)
            {
                _playerMovementIndicator.SetActive(false);
                _sparkleSprite.SetActive(false);
                //_shieldHitboxIndicators.SetActive(false);
            }

            // Check if enough time has passed since the last sparkle update
            if (_timeSinceLastUpdate >= _sparkleUpdateInterval)
            {
                if (_sparkleSprite != null)
                {
                    SpriteRenderer spriteRenderer = _sparkleSprite.GetComponent<SpriteRenderer>();
                    float randomScale = Random.Range(2, 4);

                    // Set the scale of the sprite renderer with the random scale value
                    spriteRenderer.transform.localScale = new Vector3(randomScale, randomScale, 1);
                }

                _timeSinceLastUpdate = 0f;
            }

            bool useShield = true;
            foreach (IReadOnlyBattlePlayer otherPlayer in _otherPlayers)
            {
                ShieldManager otherPlayerShieldManager = otherPlayer.PlayerShieldManager;
                if ((playerShieldManager.transform.position - otherPlayerShieldManager.transform.position)
                    .sqrMagnitude < _shieldActivationDistance * _shieldActivationDistance)
                {
                    useShield = false;
                    break;
                }
            }

            if (_isUsingShield == useShield) return;
            _isUsingShield = useShield;

            if (useShield)
            {
                playerCharacter.SpritIndex = PlayerCharacter.SpriteIndexEnum.IdleWithShield;
                playerShieldManager.SetHitboxActive(true);
                playerShieldManager.SetShow(true);
            }
            else
            {
                if (!_isMoving)
                {
                    playerCharacter.SpritIndex = PlayerCharacter.SpriteIndexEnum.IdleWithoutShield;
                }
                playerShieldManager.SetShow(false);
                playerShieldManager.SetHitboxActive(false);
            }
        }

        #endregion Private - Methods

        #endregion Private
    }
}
