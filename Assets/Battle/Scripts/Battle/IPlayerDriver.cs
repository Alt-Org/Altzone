namespace Battle.Scripts.Battle
{
    internal interface IPlayerDriver
    {
        int PlayerPos { get; }

        void Rotate(float angle);
    }

    internal interface IPlayerDriverState
    {
        bool CanRequestMove { get; }

        void ResetState(IPlayerActor playerActor);

        void DelayedMove(GridPos gridPos, float moveExecuteDelay);

        void IsWaitingToMove(bool isWaitingToMove);
    }
}
