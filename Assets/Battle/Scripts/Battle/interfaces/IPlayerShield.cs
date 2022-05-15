namespace Battle.Scripts.Battle.interfaces
{
    /// <summary>
    /// Local shield management interface.
    /// </summary>
    public interface IPlayerShield
    {
        /// <summary>
        /// Is shield visible.
        /// </summary>
        bool IsVisible { get; }

        /// <summary>
        /// Can shield be rotated more?
        /// </summary>
        bool CanRotate { get; }

        /// <summary>
        /// Current shield rotation index, 0 is least rotated position.
        /// </summary>
        int RotationIndex { get; }

        /// <summary>
        /// Set shield initial state.
        /// </summary>
        void Setup(string shieldName, bool isShieldRotated, bool isVisible, int playMode, int rotationIndex);

        /// <summary>
        /// Set shield visibility.
        /// </summary>
        void SetVisibility(bool isVisible);

        /// <summary>
        /// Set shield visual look based on <c>PlayerActor</c> playmode.
        /// </summary>
        /// <param name="playMode"></param>
        void SetPlayMode(int playMode);

        /// <summary>
        /// Set shield visual state (rotation) using shield rotation index.
        /// </summary>
        /// <param name="rotationIndex"></param>
        void SetRotation(int rotationIndex);

        /// <summary>
        /// Play visual hit effects for the shield.
        /// </summary>
        void PlayHitEffects();
    }
}