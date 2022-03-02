using UnityEngine;

namespace Battle.Scripts.Battle.interfaces
{
    /// <summary>
    /// Local shield management interface.
    /// </summary>
    public interface IPlayerShield2
    {
        bool IsVisible { get; }
        int RotationIndex { get; }
        void Setup(string shieldName, bool isShieldRotated, bool isVisible, int playMode, int rotationIndex);
        void SetVisibility(bool isVisible);
        void SetPlayMode(int playMode);
        void SetRotation(int rotationIndex);
        void PlayHitEffects();
    }

    public interface IPlayerShield
    {
        void SetupShield(int playerPos, bool isLower);
        void SetShieldState(int playMode);
        void SetShieldRotation(int rotationIndex, Vector2 contactPoint);
    }
}
