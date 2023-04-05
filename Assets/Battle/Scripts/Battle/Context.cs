using Battle.Scripts.Battle.Game;
using Battle.Scripts.Battle.Players;
using UnityEngine;

namespace Battle.Scripts.Battle
{
    /// <summary>
    /// Service locator pattern for important objects in <c>Battle</c> assembly.
    /// </summary>
    /// <remarks>
    /// After UNITY 2021.3.18 you can use faster FindObjectsByType API calls:
    /// See:  FindFirstObjectByType vs FindAnyObjectByType and FindObjectsByType vs FindObjectsOfType<br />
    /// https://gamedev.stackexchange.com/questions/204610/findobjectoftype-vs-findfirstobjectbytype<br />
    /// https://gamedev.stackexchange.com/questions/204741/whats-the-difference-between-findobjectsbytype-and-findobjectsoftype
    /// </remarks>
    internal static class Context
    {
        internal static PlayerInputHandler GetPlayerInputHandler => Object.FindObjectOfType<PlayerInputHandler>();
        internal static Camera GetBattleCamera => Object.FindObjectOfType<GameCamera>().Camera;
        internal static GridManager GetGridManager => Object.FindObjectOfType<GridManager>();
        internal static PlayerPlayArea GetBattlePlayArea => Object.FindObjectOfType<PlayerPlayArea>();
    }
}
