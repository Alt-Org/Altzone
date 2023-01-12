using UnityEngine;

namespace Battle.Scripts.Battle.Players
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

        private IPlayerActor _playerActor;
        private IGridManager _gridManager;
        private IPlayerDriverState _state;

        private void Start()
        {
            _gridManager = Context.GetGridManager;
            _playerActor = PlayerActorBase.InstantiatePrefabFor(this, _playerPrefab);
            _state = GetPlayerDriverState(this);
            _state.ResetState(_playerActor);
            var playerInputHandler = Context.GetPlayerInputHandler;
            playerInputHandler.SetPlayerDriver(this);
            if (_teamNumber == 1)
            {
                ((IPlayerDriver)this).Rotate(180f);
            }
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
