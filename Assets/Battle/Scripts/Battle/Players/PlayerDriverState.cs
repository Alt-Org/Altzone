using UnityEngine;

using Battle.Scripts.Battle.Game;

namespace Battle.Scripts.Battle.Players
{
    public class PlayerDriverState : MonoBehaviour
    {
        internal bool MovementEnabled { get => _movementEnabled; set => _movementEnabled = value; }
        internal bool CanRequestMove => !_isWaitingToMove && !_playerActor.IsBusy;

        private int _playerPos;
        private BattleTeamNumber _teamNumber;
        private bool _movementEnabled;
        private bool _isWaitingToMove;

        private PlayerActor _playerActor;
        private GridManager _gridManager;

        // Debug
        private const string DEBUG_LOG_NAME = "[BATTLE] [PLAYER DRIVER STATE] ";
        private const string DEBUG_LOG_NAME_AND_TIME = "[{0:000000}] " + DEBUG_LOG_NAME;
        private const string DEBUG_LOG_NAME_AND_TIME_AND_PLAYER_INFO = DEBUG_LOG_NAME_AND_TIME + "(team: {1}, pos: {2}) ";

        internal void ResetState(PlayerActor playerActor, int playerPos, BattleTeamNumber teamNumber)
        {
            _playerActor = playerActor;
            _playerPos = playerPos;
            _teamNumber = teamNumber;
            _gridManager = Context.GetGridManager;
        }

        internal void Move(GridPos gridPos, int teleportUpdateNumber)
        {
            Vector2 targetPosition = _gridManager.GridPositionToWorldPoint(gridPos);
            _playerActor.MoveTo(targetPosition, teleportUpdateNumber);
            IsWaitingToMove(false);
        }

        internal void IsWaitingToMove(bool isWaitingToMove)
        {
            _isWaitingToMove = isWaitingToMove;
        }

        internal void DebugLogState(int updateCount)
        {
            Debug.Log(string.Format(
                DEBUG_LOG_NAME_AND_TIME_AND_PLAYER_INFO + "State (movement enabled: {3}, is waiting to move: {4}, player actor is busy: {5})",
                updateCount,
                _teamNumber,
                _playerPos,
                _movementEnabled,
                _isWaitingToMove,
                _playerActor.IsBusy
            ));
        }
    }
}
