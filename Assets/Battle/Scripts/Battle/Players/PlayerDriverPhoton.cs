using System;
using Photon.Pun;
using UnityEngine;

namespace Battle.Scripts.Battle.Players
{
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
            _playerActor = PlayerActorBase.InstantiatePrefabFor(this, _playerPrefab);
            _teamNumber = PhotonBattle.GetTeamNumber(_playerPos);
        }

        private void OnEnable()
        {
            var player = _photonView.Owner;
            _isLocal = player.IsLocal;
            _state = GetPlayerDriverState(this);
            _state.ResetState(this, _playerActor);
            if (!_isLocal)
            {
                return;
            }
            var playerInputHandler = Context.GetPlayerInputHandler;
            playerInputHandler.SetPlayerDriver(this);

            if (_teamNumber == 1)
            {
                ((IPlayerDriver)this).Rotate(180f);
            }
        }

        int IPlayerDriver.PlayerPos => _playerPos;

        void IPlayerDriver.Rotate(float angle)
        {
            _photonView.RPC(nameof(RotatePlayerRpc), RpcTarget.All, angle);
        }

        void IPlayerDriver.MoveTo(Vector2 targetPosition)
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
    }
}
