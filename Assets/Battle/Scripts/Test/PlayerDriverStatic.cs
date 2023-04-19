using System;
using Altzone.Scripts.Config;
using Battle.Scripts.Battle;
using Battle.Scripts.Battle.Game;
using Battle.Scripts.Battle.Players;
using Unity.Collections;
using UnityEngine;

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
    internal class PlayerDriverStatic : MonoBehaviour, IPlayerDriverCallback
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

        [Header("Live Data"), SerializeField, ReadOnly] private int _actorNumber;

        private void Awake()
        {
            _battlePlayArea = Context.GetBattlePlayArea;
            _arenaScaleFactor = _battlePlayArea.ArenaScaleFactor;
            _movementDelay = GameConfig.Get().Variables._playerMovementNetworkDelay;
        }

        private void Start()
        {
            if (string.IsNullOrWhiteSpace(_settings._nickName))
            {
                _settings._nickName = name;
            }
            _gridManager = Context.GetGridManager;
            var playerTag = $"{_settings._teamNumber}:{_settings._playerPos}:{_settings._nickName}";
            _playerActor = PlayerActor.InstantiatePrefabFor(this, _settings._playerPos, _playerPrefab, playerTag, _arenaScaleFactor);
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
            var playerInputHandler = Context.GetPlayerInputHandler;
            playerInputHandler._hostForInput = gameObject;
            playerInputHandler.OnMoveTo = OnMoveTo;
        }

        public string NickName => _settings._nickName;

        public int TeamNumber => _settings._teamNumber;

        public int ActorNumber => _actorNumber;

        public bool IsLocal => _settings._isLocal;

        public int PlayerPos => _settings._playerPos;

        public void Rotate(float angle)
        {
            _playerActor.SetRotation(angle);
        }

        private void OnMoveTo(Vector2 targetPosition)
        {
            // Make this public if you want to test it.
            if (!_state.CanRequestMove)
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
            _state.DelayedMove(gridPos, (float)_movementDelay);
        }

        void IPlayerDriverCallback.SetCharacterPose(int poseIndex)
        {
            _playerActor.SetCharacterPose(poseIndex);
        }
    }
}
