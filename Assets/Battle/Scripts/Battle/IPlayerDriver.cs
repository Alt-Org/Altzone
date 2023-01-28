using Battle.Scripts.Battle.Players;

namespace Battle.Scripts.Battle
{
    internal interface IPlayerDriver : IPlayerInputTarget
    {
        string NickName { get; }

        int TeamNumber { get; }

        int ActorNumber { get; }

        bool IsLocal { get; }

        int PlayerPos { get; }

        void Rotate(float angle);
    }

    internal interface IPlayerDriverState
    {
        bool CanRequestMove { get; }

        void ResetState(IPlayerActor playerActor, int teamNumber);

        void DelayedMove(GridPos gridPos, float moveExecuteDelay);

        void IsWaitingToMove(bool isWaitingToMove);
    }
}
