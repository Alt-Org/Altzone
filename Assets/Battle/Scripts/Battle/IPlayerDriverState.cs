using Battle.Scripts.Battle.Game;

namespace Battle.Scripts.Battle
{
    internal interface IPlayerDriverState
    {
        bool CanRequestMove { get; }

        void ResetState(IPlayerActor playerActor, int teamNumber);

        void DelayedMove(GridPos gridPos, float moveExecuteDelay);

        void IsWaitingToMove(bool isWaitingToMove);
    }
}