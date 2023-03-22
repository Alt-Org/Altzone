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
        float ArenaScaleFactor { get; }
        int GridWidth { get; }
        int GridHeight { get; }
        int MiddleAreaHeight { get; }

        Rect GetPlayerPlayArea(int teamNumber);

        GridPos GetPlayerStartPosition(int playerPos);
    }
}
