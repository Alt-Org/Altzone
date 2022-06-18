using UnityEngine;

namespace Battle.Scripts.Battle
{
    /// <summary>
    /// BallState enum.
    /// </summary>
    internal enum BallState : byte
    {
        Stopped = 0,
        Moving = 1,
        Ghosted = 2,
        Hidden = 3,
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
        
        void SetBallTeamColor(int teamNumber);
    }

}