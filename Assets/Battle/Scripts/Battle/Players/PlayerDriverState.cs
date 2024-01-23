using System.Collections;
using Battle.Scripts.Battle.Game;
using UnityEngine;

namespace Battle.Scripts.Battle.Players
{
    public class PlayerDriverState : MonoBehaviour
    {
        [SerializeField] private bool _autoRotate = true; //old

        internal bool MovementEnabled { get => _movementEnabled; set => _movementEnabled = value; }
        internal bool CanRequestMove => !_isWaitingToMove && !_playerActor.IsBusy;

        private int _playerPos;
        private int _teamNumber;
        private bool _movementEnabled;
        private bool _isWaitingToMove;

        private PlayerActor _playerActor;
        private GridManager _gridManager;

        internal void ResetState(PlayerActor playerActor, int teamNumber)
        {
            _playerActor = playerActor;
            _teamNumber = teamNumber;
            _gridManager = Context.GetGridManager;
        }

        internal void Move(GridPos gridPos, int teleportUpdateNumber)
        {
            var targetPosition = _gridManager.GridPositionToWorldPoint(gridPos);
            _playerActor.MoveTo(targetPosition, teleportUpdateNumber);
            IsWaitingToMove(false);
        }

        internal void IsWaitingToMove(bool isWaitingToMove)
        {
            _isWaitingToMove = isWaitingToMove;
        }
    }
}
