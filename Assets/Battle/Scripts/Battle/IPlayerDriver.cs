using UnityEngine;

namespace Battle.Scripts.Battle
{
    internal interface IPlayerDriver
    {
        int PlayerPos { get; }

        void Rotate(float angle);

        void MoveTo(Vector2 targetPosition);
    }

    internal interface IPlayerDriverState
    {
        void ResetState(IPlayerDriver playerDriver, IPlayerActor playerActor);
        void DelayedMove(GridPos gridPos, float moveExecuteDelay);
        void IsWaitingToMove(bool isWaitingToMove);
        bool CanRequestMove { get; }
    }
}
