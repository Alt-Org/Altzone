using System;
using System.Collections;
using Altzone.Scripts.Config;
using Battle.Scripts.Battle.Game;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using Prg.Scripts.Common.PubSub;

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

        public bool MovementEnabled { get => _state.MovementEnabled; set => _state.MovementEnabled = value; }

        //public PlayerActor PlayerActor => _playerActor;
        public int ActorNumber => _photonView.Owner.ActorNumber;
        public Transform ActorTransform => _playerActor.transform;

        public bool IsLocal => _photonView.Owner.IsLocal;
        public int PeerCount => _peerCount;

        // } Public Properties and Fields

        #region Public Methods

        public void Rotate(float angle)
        {
            _playerActor.SetRotation(angle);
        }

        public void SetCharacterPose(int poseIndex)
        {
            if (!IsNetworkSynchronize)
            {
                return;
            }
            _photonView.RPC(nameof(SetPlayerCharacterPoseRpc), RpcTarget.All, poseIndex);
        }

        public void MoveTo(Vector2 targetPosition)
        {
            if (!_state.MovementEnabled || !_state.CanRequestMove) return;
            _state.IsWaitingToMove(true);

            Vector2 position = new Vector2(ActorTransform.position.x, ActorTransform.position.y);
            GridPos gridPos = _gridManager.WorldPointToGridPosition(position);
            GridPos targetGridPos = _gridManager.WorldPointToGridPosition(targetPosition);

            if (targetGridPos.Equals(gridPos) || !_gridManager.IsMovementGridSpaceFree(targetGridPos, _teamNumber))
            {
                _state.IsWaitingToMove(false);
                return;
            }

            float movementSpeed = _playerActor.MovementSpeed * _playerMoveSpeedMultiplier;
            float distance = (targetPosition - position).magnitude;
            double movementTimeS = Math.Max(distance / movementSpeed, _movementMinTimeS);
            int teleportUpdateNumber = _syncedFixedUpdateClock.UpdateCount + _syncedFixedUpdateClock.ToUpdates(movementTimeS);
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
        private SyncedFixedUpdateClockTest _syncedFixedUpdateClock;

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
            _state.ResetState(_playerActor, _teamNumber);
            _state.MovementEnabled = false;

            _playerManager.RegisterPlayer(this);
            _peerCount = 0;
            this.ExecuteOnNextFrame(() => _photonView.RPC(nameof(SendPlayerPeerCountRpc), RpcTarget.All));
        }

        #region Message Listeners
        private void OnTeamsReadyForGameplay(TeamsAreReadyForGameplay data)
        {
            bool rotate = _teamNumber == PhotonBattle.TeamBetaValue;
            if (rotate)
            {
                Rotate(180f);
            }

            if (data.LocalPlayer.TeamNumber != _teamNumber)
            {
                _playerActor.SetCharacterRotation(rotate ? 0f : 180f);
                if (_playerActor.IsUsingNewRotionSysten)
                {
                    _playerActor.SetShieldRotation(rotate ? 0f : 180f);
                }
            }
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
        private void SetPlayerCharacterPoseRpc(int poseIndex)
        {
            _playerActor.SetCharacterPose(poseIndex);
        }

        [PunRPC]
        private void MoveRpc(int row, int col, int teleportUpdateNumber)
        {
            _playerManager.ReportMovement(teleportUpdateNumber);
            _state.IsWaitingToMove(true);
            var gridPos = new GridPos(row, col);
            _state.Move(gridPos, teleportUpdateNumber);
        }

        #endregion Photon RPC
    }
}
