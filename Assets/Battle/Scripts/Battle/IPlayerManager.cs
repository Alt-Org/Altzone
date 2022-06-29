using System;
using Battle.Scripts.Battle.Game;
using UnityEngine;

namespace Battle.Scripts.Battle
{
    /// <summary>
    /// Manager for gameplay activities where one or more players are involved.<br />
    /// Requires player's registration in order to participate.
    /// </summary>
    internal interface IPlayerManager
    {
        int PlayerCount { get; }

        IPlayerDriver LocalPlayer { get; }

        void ForEach(Action<IPlayerDriver> action);

        BattleTeam GetBattleTeam(int teamNumber);

        BattleTeam GetOppositeTeam(int teamNumber);

        ITeamSnapshotTracker GetTeamSnapshotTracker(int teamNumber);

        IPlayerDriver GetPlayerByActorNumber(int actorNumber);

        IPlayerDriver GetPlayerByLastBallHitTime(int teamNumber);
        
        void RegisterPlayer(IPlayerDriver playerDriver);

        void UpdatePeerCount(IPlayerDriver playerDriver);

        void UnregisterPlayer(IPlayerDriver playerDriver, GameObject playerInstanceRoot);
    }
}