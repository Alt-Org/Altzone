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
        [SerializeField] private PlayerActor _playerPrefab;

        public PlayerActor _playerActor { get; private set; }
        private PlayerManager _playerManager;
        private GridManager _gridManager;
        private PlayerPlayArea _battlePlayArea;
        private PlayerDriverState _state;
        private SyncedFixedUpdateClockTest _syncedFixedUpdateClock;
        public PhotonView _photonView { get; private set; }
        private int _playerPos;
        private int _teamNumber;
        private float _playerMoveSpeedMultiplier;
        private double _movementMinTimeS;
        private float _arenaScaleFactor;
        private int _peerCount;
        private static bool IsNetworkSynchronize => PhotonNetwork.IsMasterClient;

        public string PlayerName;

        [Header ("Testing")]
        [SerializeField] private bool _isTesting = false;
        [SerializeField] private int _playerPrefabID;

        private void Awake()
        {
            _battlePlayArea = Context.GetBattlePlayArea;
            _photonView = PhotonView.Get(this);
            _playerManager = Context.GetPlayerManager;
            _gridManager = Context.GetGridManager;
            _playerPos = PhotonBattle.GetPlayerPos(_photonView.Owner);
            _arenaScaleFactor = _battlePlayArea.ArenaScaleFactor;
            _playerActor = InstantiatePlayerPrefab(_photonView.Owner);
            _teamNumber = PhotonBattle.GetTeamNumber(_playerPos);
            _movementMinTimeS = GameConfig.Get().Variables._playerMovementNetworkDelay;
            _playerMoveSpeedMultiplier = GameConfig.Get().Variables._playerMoveSpeedMultiplier;
            _syncedFixedUpdateClock = Context.GetSyncedFixedUpdateClock;
            //_photonView.ObservedComponents.Add((PlayerActor)_playerActor);

            this.Subscribe<TeamsAreReadyForGameplay>(OnTeamsReadyForGameplay);
        }

        void OnTeamsReadyForGameplay(TeamsAreReadyForGameplay data)
        {
            bool rotate = _teamNumber == PhotonBattle.TeamBetaValue;
            if (rotate)
            {
                Rotate(180f);
            }

            if (data.LocalPlayer.TeamNumber != _teamNumber)
            {
                _playerActor.SetCharacterRotation(rotate ? 0f : 180f);
            }
        }

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

        private void OnEnable()
        {
            var player = _photonView.Owner;
            _state ??= gameObject.AddComponent<PlayerDriverState>();
            _state.ResetState(_playerActor, _teamNumber);

            _playerManager.RegisterPlayer(this);
            _peerCount = 0;
            this.ExecuteOnNextFrame(() => _photonView.RPC(nameof(SendPlayerPeerCountRpc), RpcTarget.All));
        }

        public string NickName => _photonView.Owner.NickName;

        public int TeamNumber => _teamNumber;

        public int ActorNumber => _photonView.Owner.ActorNumber;
        
        public Transform ActorTransform => _playerActor.transform;

        public bool IsLocal => _photonView.Owner.IsLocal;

        public int PlayerPos => _playerPos;

        public int PeerCount => _peerCount;

        public void Rotate(float angle)
        {
            _playerActor.SetRotation(angle);
        }

        public void MoveTo(Vector2 targetPosition)
        {
            if (!_state.CanRequestMove) return;
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
            int teleportUpdateNumber = _syncedFixedUpdateClock.UpdateCount + _syncedFixedUpdateClock.ToUpdates(movementTimeS + PlayerActor.PLAYER_SHIELD_ANIMATION_LENGTH_SECONDS);
            _photonView.RPC(nameof(MoveRpc), RpcTarget.All, targetGridPos.Row, targetGridPos.Col, teleportUpdateNumber);
        }

        public void SetCharacterPose(int poseIndex)
        {
            if (!IsNetworkSynchronize)
            {
                return;
            }
            _photonView.RPC(nameof(SetPlayerCharacterPoseRpc), RpcTarget.All, poseIndex);
        }

        #region Photon RPC

        [PunRPC]
        /* old
        private void MoveDelayedRpc(int row, int col, double movementStartTime)
        {
            var moveExecuteDelay = Math.Max(0, movementStartTime - PhotonNetwork.Time);
            var gridPos = new GridPos(row, col);
            _state.DelayedMove(gridPos, (float)moveExecuteDelay);
        }
        */
        private void MoveRpc(int row, int col, int teleportUpdateNumber)
        {
            _state.IsWaitingToMove(true);
            var gridPos = new GridPos(row, col);
            _state.Move(gridPos, teleportUpdateNumber);
        }

        [PunRPC]
        private void SetPlayerCharacterPoseRpc(int poseIndex)
        {
            _playerActor.SetCharacterPose(poseIndex);
        }

        [PunRPC]
        private void SendPlayerPeerCountRpc()
        {
            _peerCount += 1;
            _playerManager.UpdatePeerCount();
        }

        #endregion
    }
}
