using Battle.Scripts.Battle.Game;
using Battle.Scripts.Battle.Players;
using UnityEngine;

namespace Battle.Scripts.Battle
{
    public static class Context
    {
        public static IPlayerInputHandler GetPlayerInputHandler() => Object.FindObjectOfType<PlayerInputHandler>();
        public static IBattleCamera GetBattleCamera => Object.FindObjectOfType<GameCamera>();
        public static IGridManager GetGridManager() => Object.FindObjectOfType<GridManager>();
        public static IBattlePlayArea GetBattlePlayArea() => Object.FindObjectOfType<PlayerPlayArea>();
    }
}
