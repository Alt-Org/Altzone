using System;

using UnityEngine;

using Photon.Pun;
using Photon.Realtime;

using Altzone.Scripts.Config;
using Prg.Scripts.Common.PubSub;

using Battle.Scripts.Battle.Game;

namespace Battle.Scripts.Battle.Players
{
    /// <summary>
    /// Photon <c>PlayerDriver</c> implementation.
    /// </summary>
    internal class PlayerDriverPhoton : MonoBehaviour, IDriver
    {
        // Serialized Fields
        [SerializeField] private PlayerActor _playerPrefab;
        [Header("Testing")]
        [SerializeField] private bool _isTesting = false;
        [SerializeField] private int _playerPrefabID;

        // { Public Properties and Fields

        public string PlayerName;
        public string NickName => _photonView.Owner.NickName;
        public int TeamNumber => _teamNumber;
        public int PlayerPos => _playerPos;

        public bool MovementEnabled
        {
            get => _state.MovementEnabled;
            set
            {
                _state.MovementEnabled = value;
                Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME_AND_PLAYER_INFO + "Movement set to {3}", _syncedFixedUpdateClock.UpdateCount, _teamNumber, _playerPos, _state.MovementEnabled));
            }
        }

        public PlayerActor PlayerActor => _playerActor;
        public int ActorNumber => _photonView.Owner.ActorNumber;
        public Transform ActorShieldTransform => _playerActor.ShieldTransform;
        public Transform ActorCharacterTransform => _playerActor.CharacterTransform;
        public Transform ActorSoulTransform => _playerActor.SoulTransform;

        public bool IsLocal => _photonView.Owner.IsLocal;
        public int PeerCount => _peerCount;

        // } Public Properties and Fields

        #region Public Methods

        public void MoveTo(Vector2 targetPosition)
        {
            // debug
            const string DEBUG_LOG_MOVEMENT_DENIED = DEBUG_LOG_NAME_AND_TIME_AND_PLAYER_INFO + "Movement denied";

            Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME_AND_PLAYER_INFO + "Movement requested", _syncedFixedUpdateClock.UpdateCount, _teamNumber, _playerPos));
            _state.DebugLogState(_syncedFixedUpdateClock.UpdateCount);
            if (!_state.MovementEnabled || !_state.CanRequestMove)
            {
                Debug.Log(string.Format(DEBUG_LOG_MOVEMENT_DENIED, _syncedFixedUpdateClock.UpdateCount, _teamNumber, _playerPos));
                return;
            }
            _state.IsWaitingToMove(true);
            _state.DebugLogState(_syncedFixedUpdateClock.UpdateCount);

            Vector2 position = new Vector2(_playerActor.ShieldTransform.position.x, _playerActor.ShieldTransform.position.y);
            GridPos gridPos = _gridManager.WorldPointToGridPosition(position);
            GridPos targetGridPos = _gridManager.ClampGridPosition(_gridManager.WorldPointToGridPosition(targetPosition));

            Debug.Log(string.Format(
                DEBUG_LOG_NAME_AND_TIME_AND_PLAYER_INFO + "Movement info (current position: {3}, current grid position: ({4}), target position: {5}, target grid position: ({6}))",
                _syncedFixedUpdateClock.UpdateCount,
                _teamNumber,
                _playerPos,
                position,
                gridPos,
                targetPosition,
                targetGridPos
            ));

            if (targetGridPos.Equals(gridPos) || !_gridManager.IsMovementGridSpaceFree(targetGridPos, _teamNumber))
            {
                Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME_AND_PLAYER_INFO + "Invalid movement destination", _syncedFixedUpdateClock.UpdateCount, _teamNumber, _playerPos));
                Debug.Log(string.Format(DEBUG_LOG_MOVEMENT_DENIED, _syncedFixedUpdateClock.UpdateCount, _teamNumber, _playerPos));
                _state.IsWaitingToMove(false);
                _state.DebugLogState(_syncedFixedUpdateClock.UpdateCount);
                return;
            }

            float movementSpeed = _playerActor.MovementSpeed * _playerMoveSpeedMultiplier;
            float distance = (targetPosition - position).magnitude;
            double movementTimeS = Math.Max(distance / movementSpeed, _movementMinTimeS);
            int teleportUpdateNumber = _syncedFixedUpdateClock.UpdateCount + _syncedFixedUpdateClock.ToUpdates(movementTimeS);
            Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME_AND_PLAYER_INFO + "Sending player movement network message", _syncedFixedUpdateClock.UpdateCount, _teamNumber, _playerPos));
            _photonView.RPC(nameof(MoveRpc), RpcTarget.All, targetGridPos.Row, targetGridPos.Col, teleportUpdateNumber);
        }

        #endregion Public Methods

        // Player Data
        private int _teamNumber;
        private int _playerPos;

        // Config
        private float _playerMoveSpeedMultiplier;
        private double _movementMinTimeS;
        private float _arenaScaleFactor;

        private PlayerDriverState _state;

        private int _peerCount;
        private static bool IsNetworkSynchronize => PhotonNetwork.IsMasterClient;

        private PlayerActor _playerActor;

        // Components
        private PhotonView _photonView;

        // Important Objects
        private PlayerManager _playerManager;
        private GridManager _gridManager;
        private PlayerPlayArea _battlePlayArea;
        private SyncedFixedUpdateClock _syncedFixedUpdateClock;

        // Debug
        private const string DEBUG_LOG_NAME = "[BATTLE] [PLAYER DRIVER PHOTON] ";
        private const string DEBUG_LOG_NAME_AND_TIME = "[{0:000000}] " + DEBUG_LOG_NAME;
        private const string DEBUG_LOG_NAME_AND_TIME_AND_PLAYER_INFO = DEBUG_LOG_NAME_AND_TIME + "(team: {1}, pos: {2}) ";

        private void Awake()
        {
            _photonView = PhotonView.Get(this);

            // get important objects
            _playerManager = Context.GetPlayerManager;
            _gridManager = Context.GetGridManager;
            _battlePlayArea = Context.GetBattlePlayArea;
            _syncedFixedUpdateClock = Context.GetSyncedFixedUpdateClock;

            // get player data
            _playerPos = PhotonBattle.GetPlayerPos(_photonView.Owner);
            _teamNumber = PhotonBattle.GetTeamNumber(_playerPos);

            // get config
            _playerMoveSpeedMultiplier = GameConfig.Get().Variables._playerMoveSpeedMultiplier;
            _movementMinTimeS = GameConfig.Get().Variables._networkDelay;
            _arenaScaleFactor = _battlePlayArea.ArenaScaleFactor;

            _playerActor = InstantiatePlayerPrefab(_photonView.Owner);

            // subscribe to messages
            this.Subscribe<TeamsAreReadyForGameplay>(OnTeamsReadyForGameplay);
        }

        private void OnEnable()
        {
            var player = _photonView.Owner;
            _state ??= gameObject.AddComponent<PlayerDriverState>();
            _state.ResetState(_playerActor, _playerPos, _teamNumber);
            _state.MovementEnabled = false;

            _playerManager.RegisterPlayer(this);
            _peerCount = 0;
            this.ExecuteOnNextFrame(() => _photonView.RPC(nameof(SendPlayerPeerCountRpc), RpcTarget.All));
        }

        #region Message Listeners
        private void OnTeamsReadyForGameplay(TeamsAreReadyForGameplay data)
        {
            _playerActor.SetRotation(_teamNumber == PhotonBattle.TeamAlphaValue ?  0 : 180f);
            _playerActor.SetSpriteVariant(data.LocalPlayer.TeamNumber == _teamNumber ? PlayerActor.SpriteVariant.A : PlayerActor.SpriteVariant.B);
            _playerActor.ResetSprite();
        }
        #endregion Message Listeners

        private PlayerActor InstantiatePlayerPrefab(Player player)
        {
            var playerTag = $"{_teamNumber}:{_playerPos}:{player.NickName}";
            PlayerName = playerTag;
            name = name.Replace("Clone", playerTag);
            if (_playerPrefab != null)
            {
                return PlayerActor.InstantiatePrefabFor(this, _playerPos, _playerPrefab, playerTag, _arenaScaleFactor);
            }

            var playerPrefabs = GameConfig.Get().PlayerPrefabs;
            var playerPrefabId = PhotonBattle.GetPlayerPrefabId(player);
            if (_isTesting)
            {
                playerPrefabId = _playerPrefabID.ToString();
            }
            var playerPrefab = playerPrefabs.GetPlayerPrefab(playerPrefabId) as PlayerActor;
            var playerActor = PlayerActor.InstantiatePrefabFor(this, _playerPos, playerPrefab, playerTag, _arenaScaleFactor);
            return playerActor;
        }

        #region Photon RPC

        [PunRPC]
        private void SendPlayerPeerCountRpc()
        {
            _peerCount += 1;
            _playerManager.UpdatePeerCount();
        }

        [PunRPC]
        private void MoveRpc(int row, int col, int teleportUpdateNumber)
        {
            var gridPos = new GridPos(row, col);

            // debug
            GridPos debugGridPos = _gridManager.WorldPointToGridPosition(new Vector2(_playerActor.ShieldTransform.position.x, _playerActor.ShieldTransform.position.y));

            Debug.Log(string.Format(DEBUG_LOG_NAME_AND_TIME_AND_PLAYER_INFO + "Received player movement network message (current grid position: ({3}), target grid position: ({4}))",
                _syncedFixedUpdateClock.UpdateCount,
                _teamNumber,
                _playerPos,
                debugGridPos,
                gridPos
            ));

            _playerManager.ReportMovement(teleportUpdateNumber);
            _state.IsWaitingToMove(true);
            _state.DebugLogState(_syncedFixedUpdateClock.UpdateCount);
            _state.Move(gridPos, teleportUpdateNumber);
        }

        #endregion Photon RPC
    }
}
