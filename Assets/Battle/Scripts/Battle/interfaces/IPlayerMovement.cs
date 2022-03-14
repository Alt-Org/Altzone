namespace Battle.Scripts.Battle.interfaces
{
    public interface IPlayerMovement
    {
        void Update();
        void OnDestroy();
        void SetMovementAllowed();
        void SetStopped();
        string StateString { get; }
    }
}