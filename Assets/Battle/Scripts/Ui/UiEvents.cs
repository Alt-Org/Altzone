using Altzone.Scripts.Battle;
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

        internal class StartRaid
        {
            public readonly int TeamNumber;
            public readonly IPlayerDriver PlayerToStart;

            public StartRaid(int teamNumber, IPlayerDriver playerToStart)
            {
                TeamNumber = teamNumber;
                PlayerToStart = playerToStart;
            }

            public override string ToString()
            {
                return $"{nameof(TeamNumber)}: {TeamNumber}, {nameof(PlayerToStart)}: {PlayerToStart}";
            }
        }

        internal class ExitRaidNotification
        {
            /// <summary>
            /// Currently exiting player, can be <c>null</c>.
            /// </summary>
            public readonly IPlayerDriver PlayerToExit;

            public ExitRaidNotification(IPlayerDriver playerToExit)
            {
                PlayerToExit = playerToExit;
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
            public readonly int RaidTeam;
            
            public WallCollision(Collision2D collision, int raidTeam) : base(collision)
            {
                RaidTeam = raidTeam;
            }

            public override string ToString()
            {
                return $"{nameof(RaidTeam)}: {RaidTeam}";
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