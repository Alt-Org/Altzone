using Battle.Scripts.Battle.Game;
using UnityEngine;

namespace Battle.Scripts.Battle
{
    public static class Context
    {
        public static IBattleCamera GetBattleCamera => Object.FindObjectOfType<GameCamera>();
        public static IGridManager GetGridManager() => Object.FindObjectOfType<GridManager>();
        public static IBattlePlayArea GetBattlePlayArea() => Object.FindObjectOfType<PlayerPlayArea>();
    }
}
