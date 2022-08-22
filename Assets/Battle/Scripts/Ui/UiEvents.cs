using System.Collections.Generic;
using Altzone.Scripts.Battle;
using Battle.Scripts.Battle;
using UnityEngine;
using UnityEngine.Assertions;

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
            public override string ToString()
            {
                return $"{nameof(StartBattle)}";
            }
        }

        internal class RestartBattle
        {
            public readonly IPlayerDriver PlayerToStart;

            public RestartBattle(IPlayerDriver playerToStart)
            {
                PlayerToStart = playerToStart;
            }

            public override string ToString()
            {
                return $"{nameof(PlayerToStart)}: {PlayerToStart}";
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
                return $"{base.ToString()} {nameof(HitType)}: {HitType}";
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

        internal class SlingshotStart
        {
            public List<ITeamSlingshotTracker> TeamTrackers { get; }

            public SlingshotStart(ITeamSlingshotTracker teamTracker1, ITeamSlingshotTracker teamTracker2)
            {
                Assert.IsNotNull(teamTracker1, "teamTracker1 != null");
                var trackers = new List<ITeamSlingshotTracker> { teamTracker1 };
                if (teamTracker2 != null)
                {
                    trackers.Add(teamTracker2);
                    trackers.Sort((a, b) => a.TeamNumber.CompareTo(b.TeamNumber));
                }
                TeamTrackers = trackers;
            }

            public override string ToString()
            {
                return $"{nameof(TeamTrackers)}: {string.Join(',', TeamTrackers)}";
            }
        }

        internal class SlingshotEnd
        {
            public ITeamSlingshotTracker StartingTracker { get; }
            public ITeamSlingshotTracker OtherTracker { get; }
            public List<ITeamSlingshotTracker> TeamTrackers { get; }

            public SlingshotEnd(ITeamSlingshotTracker startingTracker, ITeamSlingshotTracker otherTracker)
            {
                StartingTracker = startingTracker;
                OtherTracker = otherTracker;
                var trackers = new List<ITeamSlingshotTracker> { startingTracker };
                if (otherTracker != null)
                {
                    trackers.Add(otherTracker);
                }
                TeamTrackers = trackers;
            }

            public override string ToString()
            {
                return $"{nameof(StartingTracker)}: {StartingTracker}, {nameof(OtherTracker)}: {OtherTracker}";
            }
        }

        #endregion

        #region Base classes

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