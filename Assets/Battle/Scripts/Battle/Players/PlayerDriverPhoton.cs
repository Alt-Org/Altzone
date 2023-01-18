using System;
using Altzone.Scripts.Config;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace Battle.Scripts.Battle.Players
{
    /// <summary>
    /// Photon <c>PlayerDriver</c> implementation.
    /// </summary>
    internal class PlayerDriverPhoton : PlayerDriver, IPlayerDriver
    {
        [SerializeField] private PlayerActorBase _playerPrefab;
        [SerializeField] private double _movementDelay;

        private IPlayerActor _playerActor;
        private IGridManager _gridManager;
        private IBattlePlayArea _battlePlayArea;
        private IPlayerDriverState _state;
        private PhotonView _photonView;
        private int _playerPos;
        private int _teamNumber;
        private bool _isLocal;

        private void Awake()
        {
            _battlePlayArea = Context.GetBattlePlayArea;
            _photonView = PhotonView.Get(this);
            _gridManager = Context.GetGridManager;
            _playerPos = PhotonBattle.GetPlayerPos(_photonView.Owner);
            _playerActor = InstantiatePlayerPrefab(_photonView.Owner);
            _teamNumber = PhotonBattle.GetTeamNumber(_playerPos);
        }

        private IPlayerActor InstantiatePlayerPrefab(Player player)
        {
            if (_playerPrefab != null)
            {
                return PlayerActorBase.InstantiatePrefabFor(this, _playerPrefab);
            }
            var playerPrefabs = GameConfig.Get().PlayerPrefabs;
            var playerPrefabId = PhotonBattle.GetPlayerPrefabId(player);
            var original = playerPrefabs.GetPlayerPrefab(playerPrefabId);
            var playerPrefab = original.GetComponent<PlayerActorBase>();
            var playerActor = PlayerActorBase.InstantiatePrefabFor(this, playerPrefab);
            return playerActor;
        }

        private void OnEnable()
        {
            var player = _photonView.Owner;
            _isLocal = player.IsLocal;
            _state = GetPlayerDriverState(this);
            _state.ResetState(_playerActor);
            if (!_isLocal)
            {
                return;
            }
            var playerInputHandler = Context.GetPlayerInputHandler;
            playerInputHandler.SetPlayerDriver(this);

            if (_teamNumber == PhotonBattle.TeamRedValue)
            {
                ((IPlayerDriver)this).Rotate(180f);
            }
        }

        #region IPlayerDriver

        int IPlayerDriver.PlayerPos => _playerPos;

        void IPlayerDriver.Rotate(float angle)
        {
            _photonView.RPC(nameof(RotatePlayerRpc), RpcTarget.All, angle);
        }

        void IPlayerInputTarget.MoveTo(Vector2 targetPosition)
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
        private void RotatePlayerRpc(float angle)
        {
            _playerActor.Rotate(angle);
        }

        #endregion
    }
}
