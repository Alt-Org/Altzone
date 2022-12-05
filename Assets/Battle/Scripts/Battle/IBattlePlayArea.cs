using UnityEngine;

namespace Battle.Scripts.Battle
{
    /// <summary>
    /// Player and team gameplay areas etc.
    /// </summary>
    public interface IBattlePlayArea
    {
        Rect GetPlayerPlayArea(int playerPos);

        float ArenaWidth { get; }
        float ArenaHeight { get; }
        int ShieldGridWidth { get; }
        int ShieldGridHeight { get; }
        int MovementGridWidth { get; }
        int MovementGridHeight { get; }
    }
}
