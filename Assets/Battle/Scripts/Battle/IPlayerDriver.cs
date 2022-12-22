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
        bool CanRequestMove { get; }

        void ResetState(IPlayerActor playerActor);

        void DelayedMove(GridPos gridPos, float moveExecuteDelay);

        void IsWaitingToMove(bool isWaitingToMove);
    }
}
