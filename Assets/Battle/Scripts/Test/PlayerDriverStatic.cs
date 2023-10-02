
using System;
using Random = UnityEngine.Random;
using Altzone.Scripts.Config;
using Battle.Scripts.Battle;
using Battle.Scripts.Battle.Game;
using Battle.Scripts.Battle.Players;
using Unity.Collections;
using UnityEngine;
using Prg.Scripts.Common.PubSub;

namespace Battle.Scripts.Test
{
    /// <summary>
    /// Static <c>PlayerDriver</c> implementation.
    /// </summary>
    /// <remarks>
    /// Set our ExecutionOrder a bit lower to let other components initialize properly before us.<br />
    /// Note that this (class) is strictly for testing purposes!
    /// </remarks>
    [DefaultExecutionOrder(100)]
    internal class PlayerDriverStatic : MonoBehaviour, IDriver
    {
        [Serializable]
        internal class Settings
        {
            public string _nickName;
            public int _playerPos = PhotonBattle.PlayerPosition1;
            public int _teamNumber = PhotonBattle.TeamAlphaValue;
            public bool _isLocal;
        }

        [Header("Settings"), SerializeField] private Settings _settings;

        [SerializeField] private PlayerActor _playerPrefab;

        private double _movementDelay;
        private PlayerActor _playerActor;
        private GridManager _gridManager;
        private PlayerDriverState _state;
        private PlayerPlayArea _battlePlayArea;
        private float _arenaScaleFactor;
        private float _playerMoveSpeedMultiplier;
        private double _movementMinTimeS;
        private SyncedFixedUpdateClockTest _syncedFixedUpdateClock;


        private void OnTeamsAreReadyForGameplay(TeamsAreReadyForGameplay data)
        {
            PickRandomPosition();
        }
        private void OnBallslinged(BallSlinged data)
        {
            PickRandomPosition();

        }
        private void PickRandomPosition()
        {
            OnMoveTo(_gridManager.GridPositionToWorldPoint(new GridPos(5, Random.Range(1, 25))));

        }

        [Header("Live Data"), SerializeField, ReadOnly] private int _actorNumber;

        private void Awake()
        {
            _battlePlayArea = Context.GetBattlePlayArea;
            _arenaScaleFactor = _battlePlayArea.ArenaScaleFactor;
            _movementDelay = GameConfig.Get().Variables._playerMovementNetworkDelay;
           PlayerManager playerManager = FindObjectOfType<PlayerManager>();
            playerManager.RegisterBot(this);
            this.Subscribe<BallSlinged>(OnBallslinged);
            this.Subscribe<TeamsAreReadyForGameplay>(OnTeamsAreReadyForGameplay);
            _movementMinTimeS = GameConfig.Get().Variables._playerMovementNetworkDelay;
            _playerMoveSpeedMultiplier = GameConfig.Get().Variables._playerMoveSpeedMultiplier;
            _syncedFixedUpdateClock = Context.GetSyncedFixedUpdateClock;
        }

        private void Start()
        {
            if (string.IsNullOrWhiteSpace(_settings._nickName))
            {
                _settings._nickName = name;
            }
            _gridManager = Context.GetGridManager;
            var playerTag = $"{_settings._teamNumber}:{_settings._playerPos}:{_settings._nickName}";
            _playerActor = PlayerActor.InstantiatePrefabFor(null, _settings._playerPos, _playerPrefab, playerTag, _arenaScaleFactor);
            _state ??= gameObject.AddComponent<PlayerDriverState>();
            _state.ResetState(_playerActor, _settings._teamNumber);
            if (_settings._teamNumber == PhotonBattle.TeamBetaValue)
            {
                Rotate(180f);
            }
            if (!_settings._isLocal)
            {
                return;
            }
            
        }

        public string NickName => _settings._nickName;

        public int TeamNumber => _settings._teamNumber;

        public int ActorNumber => _actorNumber;

        public bool IsLocal => _settings._isLocal;

        public int PlayerPos => _settings._playerPos;

        public Transform ActorTransform => _playerActor.transform;

        public void Rotate(float angle)
        {
            _playerActor.SetRotation(angle);
        }
        private void OnMoveTo(Vector2 targetPosition)
        {
            // Make this public if you want to test it.
            //
            //             OLD
            /* if (!_state.CanRequestMove)
           {
               return;
           }
           var gridPos = _gridManager.WorldPointToGridPosition(targetPosition);
           var isSpaceFree = _gridManager.IsMovementGridSpaceFree(gridPos, _settings._teamNumber);
           if (!isSpaceFree)
           {
               return;
           }
           _state.IsWaitingToMove(true);
           */

            if (!_state.CanRequestMove) return;
            _state.IsWaitingToMove(true);

            Vector2 position = new Vector2(ActorTransform.position.x, ActorTransform.position.y);
            GridPos gridPos = _gridManager.WorldPointToGridPosition(position);
            GridPos targetGridPos = _gridManager.WorldPointToGridPosition(targetPosition);

            if (targetGridPos.Equals(gridPos) || !_gridManager.IsMovementGridSpaceFree(targetGridPos, _settings._teamNumber))
            {
                _state.IsWaitingToMove(false);
                return;
            }

            float movementSpeed = _playerActor.MovementSpeed * _playerMoveSpeedMultiplier;
            float distance = (targetPosition - position).magnitude;
            double movementTimeS = Math.Max(distance / movementSpeed, _movementMinTimeS);
            int teleportUpdateNumber = _syncedFixedUpdateClock.UpdateCount + _syncedFixedUpdateClock.ToUpdates(movementTimeS);

            _state.Move(targetGridPos, teleportUpdateNumber);
        }

        public void SetCharacterPose(int poseIndex)
        {
            throw new NotImplementedException("only PlayerDriverPhoton can do this");
        }
    }
}
