
using System;

using Unity.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

using Altzone.Scripts.Config;
using Altzone.Scripts.Model.Poco.Game;
using Prg.Scripts.Common.PubSub;

using Battle.Scripts.Battle;
using Battle.Scripts.Battle.Game;
using Battle.Scripts.Battle.Players;

namespace Battle.Scripts.Test
{
    /// <summary>
    /// Static <c>PlayerDriver</c> implementation.
    /// </summary>
    /// <remarks>
    /// <para>Set our ExecutionOrder a bit lower to let other components initialize properly before us.</para>
    /// <para>this (class) was strictly for testing purposes but it was converted to work as bot in tutorial sling scene. (wasn't my idea)</para>
    /// </remarks>
    [DefaultExecutionOrder(100)]
    internal class PlayerDriverStatic : MonoBehaviour, IPlayerDriver
    {
        [Serializable]
        internal class Settings
        {
            public string _nickName;
            public int _playerPos = PhotonBattle.PlayerPosition1;
            public int _teamNumber = (int)BattleTeamNumber.TeamAlpha;
            public bool _isLocal;
        }

        // Serialized Fields
        [Header("Settings"), SerializeField] private Settings _settings;
        [Obsolete("_playerPrefab is deprecated, please use _playerCharacterID instead")]
        [Tooltip("Player Prefab is deprecated, please use Player Character ID instead")]
        [SerializeField] private PlayerActor _playerPrefab;
        [SerializeField] private CharacterID _playerCharacterID;
        [Header("Live Data"), SerializeField, ReadOnly] private int _actorNumber;

        // { Public Properties and Fields

        public string NickName => _settings._nickName;
        public bool IsLocal => _settings._isLocal;
        public int ActorNumber => _actorNumber;

        public bool MovementEnabled { get => _state.MovementEnabled; set => _state.MovementEnabled = value; }

        public IReadOnlyBattlePlayer BattlePlayer => _battlePlayer;

        // } Public Properties and Fields

        #region Public Methods

        public void Rotate(float angle)
        {
            _battlePlayer.PlayerActor.SetRotation(angle);
        }

        #endregion Public Methods

        // Config
        private float _playerMovementSpeed;
        private double _movementMinTimeS;
        private float _arenaScaleFactor;

        private PlayerDriverState _state;

        private BattlePlayer _battlePlayer;

        // Important Objects
        private PlayerManager _playerManager;
        private GridManager _gridManager;
        private PlayerPlayArea _battlePlayArea;
        private SyncedFixedUpdateClock _syncedFixedUpdateClock;

        private void Awake()
        {
            // get important objects
            _playerManager = Context.GetPlayerManager;
            _gridManager = Context.GetGridManager;
            _battlePlayArea = Context.GetBattlePlayArea;
            _syncedFixedUpdateClock = Context.GetSyncedFixedUpdateClock;

            // create battle player
            _battlePlayer = new BattlePlayer(_settings._playerPos, null/*<- this needs to be changed later*/, true, this);

            // get config
            _movementMinTimeS = GameConfig.Get().Variables._networkDelay;
            _playerMovementSpeed = _battlePlayer.PlayerActor.MovementSpeed * GameConfig.Get().Variables._playerMoveSpeedMultiplier;
            _arenaScaleFactor = _battlePlayArea.ArenaScaleFactor;

            // subscribe to messages
            this.Subscribe<BallSlinged>(OnBallSlinged);
            this.Subscribe<TeamsAreReadyForGameplay>(OnTeamsAreReadyForGameplay);
        }

        private void Start()
        {
            if (string.IsNullOrWhiteSpace(_settings._nickName))
            {
                _settings._nickName = name;
            }

            string playerTag = $"{_settings._teamNumber}:{_battlePlayer.PlayerPosition}:{_settings._nickName}";

            PlayerActor.InstantiatePrefabFor(_battlePlayer,playerTag, _arenaScaleFactor);
            _state ??= gameObject.AddComponent<PlayerDriverState>();
            _state.ResetState(_battlePlayer.PlayerActor, _battlePlayer.PlayerPosition, _battlePlayer.BattleTeam.TeamNumber);
            _state.MovementEnabled = false;

            _playerManager.RegisterPlayer(_battlePlayer, (BattleTeamNumber)_settings._teamNumber);

            // this needs to be changed later (see PlayerDriverPhoton.OnTeamsReadyForGameplay)
            if (_battlePlayer.BattleTeam.TeamNumber == BattleTeamNumber.TeamBeta)
            {
                Rotate(180f);
            }

            // this doesn't do anything
            // at some point there probably was code after this that should only execute when _settings._isLocal is true
            // it's probably good idea to know why this is here before removing it
            if (!_settings._isLocal)
            {
                return;
            }
        }

        #region Message Listeners

        private void OnTeamsAreReadyForGameplay(TeamsAreReadyForGameplay data)
        {
            PickRandomPosition();
        }

        private void OnBallSlinged(BallSlinged data)
        {
            PickRandomPosition();
        }

        #endregion Message Listeners

        private void PickRandomPosition()
        {
            OnMoveTo(_gridManager.GridPositionToWorldPoint(new GridPos(5, Random.Range(1, 25))));
        }

        private void OnMoveTo(Vector2 targetPosition)
        {
            if (!_state.MovementEnabled || !_state.CanRequestMove) return;
            _state.IsWaitingToMove(true);

            Vector2 position = new(_battlePlayer.PlayerShieldManager.transform.position.x, _battlePlayer.PlayerShieldManager.transform.position.y);
            GridPos gridPos = _gridManager.WorldPointToGridPosition(position);
            GridPos targetGridPos = _gridManager.ClampGridPosition(_gridManager.WorldPointToGridPosition(targetPosition));

            if (targetGridPos.Equals(gridPos) || !_gridManager.IsMovementGridSpaceFree(targetGridPos, _battlePlayer.BattleTeam.TeamNumber))
            {
                _state.IsWaitingToMove(false);
                return;
            }

            float distance = (targetPosition - position).magnitude;
            double movementTimeS = Math.Max(distance / _playerMovementSpeed, _movementMinTimeS);
            int teleportUpdateNumber = _syncedFixedUpdateClock.UpdateCount + _syncedFixedUpdateClock.ToUpdates(movementTimeS);

            _playerManager.ReportMovement(teleportUpdateNumber);
            _state.Move(targetGridPos, teleportUpdateNumber);
        }
    }
}
