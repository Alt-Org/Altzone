using Battle.Scripts.Battle;
using Battle.Scripts.Battle.Players;
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
    internal class PlayerDriverStatic : PlayerDriver, IPlayerDriver
    {
        [SerializeField] private int _playerPos = PhotonBattle.PlayerPosition1;
        [SerializeField] private int _teamNumber = PhotonBattle.TeamBlueValue;
        [SerializeField] private PlayerActorBase _playerPrefab;
        [SerializeField] private double _movementDelay;
        [SerializeField] private bool _isLocal;

        private IPlayerActor _playerActor;
        private IGridManager _gridManager;
        private IPlayerDriverState _state;

        private void Start()
        {
            _gridManager = Context.GetGridManager;
            _playerActor = PlayerActorBase.InstantiatePrefabFor(this, _playerPrefab);
            _state = GetPlayerDriverState(this);
            _state.ResetState(_playerActor);
            if (_teamNumber == PhotonBattle.TeamBlueValue)
            {
                ((IPlayerDriver)this).Rotate(180f);
            }
            if (!_isLocal)
            {
                return;
            }
            var playerInputHandler = Context.GetPlayerInputHandler;
            playerInputHandler.SetPlayerDriver(this);
        }

        #region IPlayerDriver

        int IPlayerDriver.PlayerPos => _playerPos;

        void IPlayerDriver.Rotate(float angle)
        {
            _playerActor.Rotate(angle);
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
            _state.DelayedMove(gridPos, (float)_movementDelay);
        }

        #endregion
    }
}
