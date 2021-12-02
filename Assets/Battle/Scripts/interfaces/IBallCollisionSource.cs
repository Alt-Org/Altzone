using System;
using UnityEngine;

namespace Battle.Scripts.interfaces
{
    /// <summary>
    /// Interface to provide data about ball collisions.
    /// </summary>
    public interface IBallCollisionSource
    {
        /// <summary>
        /// Sets active team index or -1 if either team is decisively active.
        /// </summary>
        Action<int> onCurrentTeamChanged { get; set; }

        /// <summary>
        /// Externalized collision handling.
        /// </summary>
        Action<Collision2D> onCollision2D { get; set; }
    }
}