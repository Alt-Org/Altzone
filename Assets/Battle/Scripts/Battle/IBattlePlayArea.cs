using UnityEngine;

namespace Battle.Scripts.Battle
{
    /// <summary>
    /// Player and team gameplay areas etc.
    /// </summary>
    internal interface IBattlePlayArea
    {
        float ArenaWidth { get; }
        float ArenaHeight { get; }
        int ShieldGridWidth { get; }
        int ShieldGridHeight { get; }
        int MovementGridWidth { get; }
        int MovementGridHeight { get; }

        Rect GetPlayerPlayArea(int playerPos);

        GridPos GetPlayerStartPosition(int playerPos);
    }
}
