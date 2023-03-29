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
        internal static Camera GetBattleCamera => Object.FindObjectOfType<GameCamera>().Camera;
        internal static GridManager GetGridManager => Object.FindObjectOfType<GridManager>();
        internal static IBattlePlayArea GetBattlePlayArea => Object.FindObjectOfType<PlayerPlayArea>();
    }
}
