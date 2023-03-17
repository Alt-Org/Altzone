using System.Collections.Generic;
using System.Linq;
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
        internal static List<GameObject> GetAllPlayerDriverObjects => GameObject.FindGameObjectsWithTag("PlayerDriverPhoton").Cast<GameObject>().ToList();
        
        internal static List<IPlayerDriver> GetAllPlayerDrivers => Object.FindObjectsOfType<PlayerDriver>().Cast<IPlayerDriver>().ToList();
        internal static IPlayerInputHandler GetPlayerInputHandler => Object.FindObjectOfType<PlayerInputHandler>();
        internal static IBattleCamera GetBattleCamera => Object.FindObjectOfType<GameCamera>();
        internal static IGridManager GetGridManager => Object.FindObjectOfType<GridManager>();
        internal static IBattlePlayArea GetBattlePlayArea => Object.FindObjectOfType<PlayerPlayArea>();
    }
}
