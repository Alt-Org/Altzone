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
        Disconnected = 4,
    }

    internal static class BattlePlayModeExtensions
    {
        public static bool CanTransition(this BattlePlayMode currentPlayMode, BattlePlayMode newPlayMode)
        {
            // Can not enter Frozen from SuperGhosted state.
            // Simplify expression refactoring - original is below and it is human readable and understandable!
            // return currentPlayMode == BattlePlayMode.SuperGhosted && newPlayMode == BattlePlayMode.Frozen ? false : true;
            return currentPlayMode != BattlePlayMode.SuperGhosted || newPlayMode != BattlePlayMode.Frozen;
        }

        public static bool CanMove(this BattlePlayMode playMode)
        {
            return playMode == BattlePlayMode.Normal || playMode >= BattlePlayMode.Ghosted;
        }

        public static bool CanCollide(this BattlePlayMode playMode)
        {
            return playMode == BattlePlayMode.Normal || playMode == BattlePlayMode.Frozen;
        }
    }
}