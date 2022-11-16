using UnityEngine;

namespace Battle.Scripts.Battle
{
    /// <summary>
    /// Player and team gameplay areas etc.
    /// </summary>
    public interface IBattlePlayArea
    {
        Rect GetPlayerPlayArea(int playerPos);

        Vector2 ArenaSize { get; }

        int ShieldGridWidth { get; }
        int ShieldGridHeight { get; }
        int MovementGridWidth { get; }
        int MovementGridHeight { get; }
    }
}
