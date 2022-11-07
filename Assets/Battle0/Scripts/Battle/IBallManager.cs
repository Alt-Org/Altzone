using UnityEngine;

namespace Battle0.Scripts.Battle
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
        IBallCollision BallCollision { get; }
        
        void FixCameraRotation(Camera gameCamera);

        void SetBallPosition(Vector2 position);

        void SetBallSpeed(float speed);

        void SetBallSpeed(float speed, Vector2 direction);

        void SetBallState(BallState ballState);
        
        void SetBallLocalTeamColor(int teamNumber);
    }

    /// <summary>
    /// Local player callback interface for collision handling.
    /// </summary>
    internal interface IBallCollision
    {
        void OnBrickCollision(Collision2D collision);

        void OnHeadCollision(Collision2D collision);
    }
}