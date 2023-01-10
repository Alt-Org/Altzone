using Battle.Scripts.Battle.Game;
using Battle.Scripts.Battle.Players;
using UnityEngine;

namespace Battle.Scripts.Battle
{
    /// <summary>
    /// Service locator pattern for important objects in <c>Battle</c> assembly.
    /// </summary>
    internal static class Context
    {
        internal static IPlayerInputHandler GetPlayerInputHandler => Object.FindObjectOfType<PlayerInputHandler>();
        internal static IBattleCamera GetBattleCamera => Object.FindObjectOfType<GameCamera>();
        internal static IGridManager GetGridManager => Object.FindObjectOfType<GridManager>();
        internal static IBattlePlayArea GetBattlePlayArea => Object.FindObjectOfType<PlayerPlayArea>();
    }
}
