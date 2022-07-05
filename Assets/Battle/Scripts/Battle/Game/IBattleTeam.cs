using UnityEngine;

namespace Battle.Scripts.Battle.Game
{
    /// <summary>
    /// Interface for <c>BattleTeam</c> for managing team of one or two players internally for gameplay purposes.
    /// </summary>
    internal interface IBattleTeam
    {
        int TeamNumber { get; }
        IPlayerDriver FirstPlayer { get; }
        IPlayerDriver SecondPlayer { get; }
        int PlayerCount { get; }
        IPlayerDriver GetMyTeamMember(int actorNumber);
        int Attack { get; }
        
        void SetPlayMode(BattlePlayMode playMode);
    }
}