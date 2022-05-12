namespace Battle.Scripts.Battle.interfaces
{
    /// <summary>
    /// Local shield management interface.
    /// </summary>
    public interface IPlayerShield
    {
        bool IsVisible { get; }
        int RotationIndex { get; }
        void Setup(string shieldName, bool isShieldRotated, bool isVisible, int playMode, int rotationIndex);
        void SetVisibility(bool isVisible);
        void SetPlayMode(int playMode);
        void SetRotation(int rotationIndex);
        void PlayHitEffects();
    }
}
