namespace Altzone.Scripts.Battle
{
    /// <summary>
    /// Raid gameplay mode interface for Battle.
    /// </summary>
    public interface IRaidEvent
    {
        void RaidStart(int teamNumber, IPlayerInfo playerInfo);
        
        void RaidBonus(int teamNumber, IPlayerInfo playerInfo);
        
        void RaidStop(int teamNumber, IPlayerInfo playerInfo);
    }

    /// <summary>
    /// Battle gameplay mode interface for Raid.
    /// </summary>
    public interface IBattleEvent
    {
        void PlayerClosedRaid();
    }
}