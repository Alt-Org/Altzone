using System;
using Altzone.Scripts.Battle;
using Altzone.Scripts.Config;
using Battle.Scripts.Battle.Game;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace Battle.Scripts.Battle.Players
{
    /// <summary>
    /// Photon <c>PlayerDriver</c> implementation.
    /// </summary>
    internal class PlayerDriverPhoton : MonoBehaviour, IPlayerDriver
    {
        [SerializeField] private PlayerActor _playerPrefab;

        private PlayerActor _playerActor;
        private GridManager _gridManager;
        private PlayerPlayArea _battlePlayArea;
        private PlayerDriverState _state;
        private PhotonView _photonView;
        private int _playerPos;
        private int _teamNumber;
        private double _movementDelay;
        private float _arenaScaleFactor;
        private static bool IsNetworkSynchronize => PhotonNetwork.IsMasterClient;

        public string PlayerName;

        private bool _isLocal;

        [Header ("Testing")]
        [SerializeField] private bool _isTesting = false;
        [SerializeField] private int _playerPrefabID;

        private void Awake()
        {
            _battlePlayArea = Context.GetBattlePlayArea;
            _photonView = PhotonView.Get(this);
            _gridManager = Context.GetGridManager;
            _playerPos = PhotonBattle.GetPlayerPos(_photonView.Owner);
            _arenaScaleFactor = _battlePlayArea.ArenaScaleFactor;
            _playerActor = InstantiatePlayerPrefab(_photonView.Owner);
            _teamNumber = PhotonBattle.GetTeamNumber(_playerPos);
            _movementDelay = GameConfig.Get().Variables._playerMovementNetworkDelay;
            //_photonView.ObservedComponents.Add((PlayerActor)_playerActor);
        }

        private PlayerActor InstantiatePlayerPrefab(Player player)
        {
            var playerTag = $"{_teamNumber}:{_playerPos}:{player.NickName}";
            PlayerName = playerTag;
            Debug.Log($"heoooo22{playerTag}");
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
            _isLocal = player.IsLocal;
            _state ??= gameObject.AddComponent<PlayerDriverState>();
            _state.ResetState(_playerActor, _teamNumber);
            if (_teamNumber == PhotonBattle.TeamBetaValue)
            {
                ((IPlayerDriver)this).Rotate(180f);
            }
            if (!_isLocal)
            {
                return;
            }
            var playerInputHandler = Context.GetPlayerInputHandler;
            playerInputHandler.OnMoveTo = MoveTo;
        }

        #region IPlayerDriver

        string IPlayerDriver.NickName => _photonView.Owner.NickName;

        int IPlayerDriver.TeamNumber => _teamNumber;

        int IPlayerDriver.ActorNumber => _photonView.Owner.ActorNumber;

        bool IPlayerDriver.IsLocal => _photonView.Owner.IsLocal;

        int IPlayerDriver.PlayerPos => _playerPos;

        void IPlayerDriver.Rotate(float angle)
        {
            _playerActor.SetRotation(angle);
        }

        private void MoveTo(Vector2 targetPosition)
        {
            if (!_state.CanRequestMove)
            {
                return;
            }
            var gridPos = _gridManager.WorldPointToGridPosition(targetPosition);
            var isSpaceFree = _gridManager.IsMovementGridSpaceFree(gridPos, _teamNumber);
            if (!isSpaceFree)
            {
                return;
            }
            _state.IsWaitingToMove(true);
            var movementStartTime = PhotonNetwork.Time + _movementDelay;
            _photonView.RPC(nameof(MoveDelayedRpc), RpcTarget.All, gridPos.Row, gridPos.Col, movementStartTime);
        }

        void IPlayerDriver.SetCharacterPose(int poseIndex)
        {
            if (!IsNetworkSynchronize)
            {
                return;
            }
            _photonView.RPC(nameof(SetPlayerCharacterPoseRpc), RpcTarget.All, poseIndex);
        }

        #endregion

        #region Photon RPC

        [PunRPC]
        private void MoveDelayedRpc(int row, int col, double movementStartTime)
        {
            var moveExecuteDelay = Math.Max(0, movementStartTime - PhotonNetwork.Time);
            var gridPos = new GridPos(row, col);
            _state.DelayedMove(gridPos, (float)moveExecuteDelay);
        }

        [PunRPC]
        private void SetPlayerCharacterPoseRpc(int poseIndex)
        {
            _playerActor.SetCharacterPose(poseIndex);
        }

        #endregion
    }
}
