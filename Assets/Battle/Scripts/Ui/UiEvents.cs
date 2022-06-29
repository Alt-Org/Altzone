using Battle.Scripts.Battle;
using UnityEngine;

namespace Battle.Scripts.Ui
{
    internal static class UiEvents
    {
        internal class HeadCollision : PlayerCollisionEvent
        {
            public HeadCollision(Collision2D collision, IPlayerInfo player) : base(collision, player)
            {
            }
        }

        internal class ShieldCollision : PlayerCollisionEvent
        {
            public readonly string HitType;

            public ShieldCollision(Collision2D collision, IPlayerInfo player, string hitType) : base(collision, player)
            {
                HitType = hitType;
            }
        }

        internal class WallCollision : CollisionEvent
        {
            public WallCollision(Collision2D collision) : base(collision)
            {
            }
        }

        #region base classes

        internal class CollisionEvent
        {
            public readonly Collision2D Collision;

            protected CollisionEvent(Collision2D collision)
            {
                Collision = collision;
            }
        }

        internal class PlayerCollisionEvent : CollisionEvent
        {
            public readonly IPlayerInfo Player;

            public PlayerCollisionEvent(Collision2D collision, IPlayerInfo player) : base(collision)
            {
                Player = player;
            }
        }

        #endregion
    }
}