using System.Collections;
using UnityEngine;

namespace Battle.Scripts.Battle.Players
{
    public class PlayerDriverState : MonoBehaviour, IPlayerDriverState
    {
        private IPlayerActor _playerActor;
        private IGridManager _gridManager;
        private bool _isWaitingToMove;

        private IEnumerator DelayTime(GridPos gridPos, float waitTime)
        {
            yield return new WaitForSeconds(waitTime);
            var targetPosition = _gridManager.GridPositionToWorldPoint(gridPos);
            _playerActor.MoveTo(targetPosition);
            ((IPlayerDriverState)this).IsWaitingToMove(false);
        }

        #region IPlayerDriverState

        bool IPlayerDriverState.CanRequestMove => !_isWaitingToMove && !_playerActor.IsBusy;

        void IPlayerDriverState.ResetState(IPlayerActor playerActor)
        {
            _playerActor = playerActor;
            _gridManager = Context.GetGridManager;
        }

        void IPlayerDriverState.DelayedMove(GridPos gridPos, float moveExecuteDelay)
        {
            StartCoroutine(DelayTime(gridPos, moveExecuteDelay));
        }

        void IPlayerDriverState.IsWaitingToMove(bool isWaitingToMove)
        {
            _isWaitingToMove = isWaitingToMove;
        }

        #endregion
    }
}
