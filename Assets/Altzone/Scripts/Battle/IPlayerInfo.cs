namespace Altzone.Scripts.Battle
{
    /// <summary>
    /// Readonly global <c>Player</c> info.
    /// </summary>
    public interface IPlayerInfo
    {
        /// <summary>
        /// Non-zero <c>ActorNumber</c> is unique identifier for any local or remote player.
        /// </summary>
        /// <remarks>
        /// <c>Photon</c> uses positive and static player (can use) negative values.
        /// </remarks>
        int ActorNumber { get; }

        /// <summary>
        /// Is this <c>Player</c> a local or remote instance.
        /// </summary>
        bool IsLocal { get; }

        double LastBallHitTime { get; }
    }
}