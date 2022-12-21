using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.Scripts.Battle.Players
{
    public class PlayerDriverState : MonoBehaviour, IPlayerDriverState
    {
        private IPlayerDriver _playerDriver;
        private IPlayerActor _playerActor;
        private IGridManager _gridManager;
        private bool _isWaitingToMove;
        public bool CanRequestMove => !_isWaitingToMove && !_playerActor.IsBusy;

        void IPlayerDriverState.ResetState(IPlayerDriver playerDriver, IPlayerActor playerActor)
        {
            _playerDriver = playerDriver;
            _playerActor = playerActor;
            _gridManager = Context.GetGridManager;
        }
        void IPlayerDriverState.DelayedMove(GridPos gridPos, float moveExecuteDelay)
        {
            StartCoroutine(DelayTime(gridPos, moveExecuteDelay));
        }

        public void IsWaitingToMove(bool isWaitingToMove)
        {
            _isWaitingToMove = isWaitingToMove;
        }

        private IEnumerator DelayTime(GridPos gridPos, float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            var targetPosition = _gridManager.GridPositionToWorldPoint(gridPos);
            _playerActor.MoveTo(targetPosition);
            IsWaitingToMove(false);
        }
    }
}
