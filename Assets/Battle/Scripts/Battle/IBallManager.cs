using UnityEngine;

namespace Battle.Scripts.Battle
{
    internal enum BallState : byte
    {
        Stopped = 0,
        NoTeam = 1,
        RedTeam = 2,
        BlueTeam = 3,
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