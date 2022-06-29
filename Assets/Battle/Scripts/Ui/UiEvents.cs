using Battle.Scripts.Battle;
using UnityEngine;

namespace Battle.Scripts.Ui
{
    /// <summary>
    /// UI Events from core battle game to UI layer that is responsible for UI management
    /// and partially responsible game state management (Active Gameplay events).
    /// </summary>
    internal static class UiEvents
    {
        #region Active Gameplay events

        internal class StartBattle
        {
        }

        internal class RestartBattle
        {
            public readonly IPlayerDriver PlayerToStart;

            public RestartBattle(IPlayerDriver playerToStart)
            {
                PlayerToStart = playerToStart;
            }
        }

        #endregion

        #region Informational events

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

            public override string ToString()
            {
                return $"{nameof(HitType)}: {HitType}";
            }
        }

        internal class WallCollision : CollisionEvent
        {
            public readonly int TeamAffected;
            
            public WallCollision(Collision2D collision, int teamAffected) : base(collision)
            {
                TeamAffected = teamAffected;
            }

            public override string ToString()
            {
                return $"{nameof(TeamAffected)}: {TeamAffected}";
            }
        }

        internal class TeamActivation
        {
            public readonly bool IsBallOnBlueTeamArea;
            public readonly bool IsBallOnRedTeamArea;

            public TeamActivation(bool isBallOnBlueTeamArea, bool isBallOnRedTeamArea)
            {
                IsBallOnBlueTeamArea = isBallOnBlueTeamArea;
                IsBallOnRedTeamArea = isBallOnRedTeamArea;
            }

            public override string ToString()
            {
                return $"{nameof(IsBallOnBlueTeamArea)}: {IsBallOnBlueTeamArea}, {nameof(IsBallOnRedTeamArea)}: {IsBallOnRedTeamArea}";
            }
        }

        #endregion

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

            protected PlayerCollisionEvent(Collision2D collision, IPlayerInfo player) : base(collision)
            {
                Player = player;
            }

            public override string ToString()
            {
                return $"ActorNumber: {Player.ActorNumber}";
            }
        }

        #endregion
    }
}