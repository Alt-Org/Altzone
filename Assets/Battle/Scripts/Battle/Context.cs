using Battle.Scripts.Battle.Game;
using UnityEngine;

namespace Battle.Scripts.Battle
{
    /// <summary>
    /// Service locator pattern for important objects in this game.
    /// </summary>
    internal static class Context
    {
        internal static IBattleCamera GetBattleCamera => Object.FindObjectOfType<BattleCamera>();

        internal static IBattleBackground GetBattleBackground => Object.FindObjectOfType<BattleBackground>();

        internal static IBattlePlayArea GetBattlePlayArea => Object.FindObjectOfType<BattlePlayArea>();
    }
}