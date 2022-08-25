using UnityEngine;

namespace Battle.Scripts.Battle
{
    /// <summary>
    /// Player and team gameplay areas etc.
    /// </summary>
    internal interface IBattlePlayArea
    {
        Vector2 GetPlayAreaCenterPosition { get; }
        
        Rect GetPlayerPlayArea(int playerPos);
        Vector2 GetPlayerStartPosition(int playerPos);
        
        public Collider2D BlueTeamCollider { get; }
        public Collider2D RedTeamCollider { get; }

        public Transform BlueTeamTransform { get; }
        public Transform RedTeamTransform { get; }
    }
}