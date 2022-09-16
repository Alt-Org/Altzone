using Altzone.Scripts.Config;

namespace Battle.Scripts.Battle
{
    /// <summary>
    /// Battle gameplay play modes.<br />
    /// Movable states are: Normal, Ghosted and SuperGhosted. In Frozen state player can not move.<br />
    /// Collisions can happen in Normal and Frozen state.<br />
    /// Disconnected state is reserved for special when if player quits or gets disconnected for eny other reason.
    /// </summary>
    /// <remarks>
    /// WIKI page: https://github.com/Alt-Org/Altzone/wiki/Player-Prefab
    /// </remarks>
    internal enum BattlePlayMode
    {
        Normal = 0,
        Frozen = 1,
        Ghosted = 2,
        SuperGhosted = 3,
        RaidGhosted = 4,
        RaidReturn = 5,
        Disconnected = 6,
    }

    internal static class BattlePlayModeExtensions
    {
        public static bool CanTransition(this BattlePlayMode currentPlayMode, BattlePlayMode newPlayMode)
        {
            switch (currentPlayMode)
            {
                case BattlePlayMode.SuperGhosted:
                    // Can not enter Frozen from SuperGhosted - this is for putting the ball in the game without collisions on starting team's side.
                    return newPlayMode != BattlePlayMode.Frozen;
                case BattlePlayMode.RaidGhosted:
                    return newPlayMode == BattlePlayMode.RaidReturn;
                default:
                    return true;
            }
        }

        public static bool CanMove(this BattlePlayMode playMode)
        {
            if (RuntimeGameConfig.Get().Features._isDisableBattleGridMovement)
            {
                return playMode == BattlePlayMode.Normal || playMode == BattlePlayMode.Ghosted || playMode == BattlePlayMode.SuperGhosted;
            }
            return playMode == BattlePlayMode.Normal || playMode == BattlePlayMode.Ghosted || playMode == BattlePlayMode.SuperGhosted || playMode == BattlePlayMode.Frozen;
        }

        public static bool CanCollide(this BattlePlayMode playMode)
        {
            return playMode == BattlePlayMode.Normal || playMode == BattlePlayMode.Frozen;
        }
    }
}
