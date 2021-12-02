namespace Battle.Scripts.interfaces
{
    public interface IPlayerActor
    {
        int PlayerPos { get; }
        bool IsLocal { get; }
        int TeamMatePos { get; }
        int TeamNumber { get; }
        bool IsLocalTeam { get; }
        bool IsHomeTeam { get; }
        int OppositeTeam { get; }
        IPlayerActor TeamMate { get; }
        void setNormalMode();
        void setFrozenMode();
        void setGhostedMode();
        void setSpecialMode();
        void headCollision(IBallControl ballControl);
        float CurrentSpeed { get; }
    }
}