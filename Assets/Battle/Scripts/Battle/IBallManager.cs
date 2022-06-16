using UnityEngine;

namespace Battle.Scripts.Battle
{
    /// <summary>
    /// BallState combines ball behaviour and visual state into one for convenience (or confusion).
    /// </summary>
    /// <remarks>
    /// NoTeam, BlueTeam and RedTeam are alias for "Moving" state but can different visual state.
    /// </remarks>
    internal enum BallState : byte
    {
        Stopped = 0,
        NoTeam = 1,
        BlueTeam = 2,
        RedTeam = 3,
        Ghosted = 4,
        Hidden = 5,
    }

    /// <summary>
    /// External ball management interface.
    /// </summary>
    /// <remarks>
    /// WIKI page: https://github.com/Alt-Org/Altzone/wiki/Pallo-ja-sen-liikkuminen
    /// </remarks>
    internal interface IBallManager
    {
        void FixCameraRotation(Camera gameCamera);

        void SetBallPosition(Vector2 position);

        void SetBallSpeed(float speed);

        void SetBallSpeed(float speed, Vector2 direction);

        void SetBallState(BallState ballState);
    }

}