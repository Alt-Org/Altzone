using UnityEngine;

namespace Battle.Test.Scripts.Battle
{
    internal interface IBattlePlayArea
    {
        Rect GetPlayerPlayArea(int playerPos);
        Vector2 GetPlayerStartPosition(int playerPos);
        
        public Collider2D BlueTeamCollider { get; }
        public Collider2D RedTeamCollider { get; }

        public Transform BlueTeamMiddlePosition { get; }
        public Transform RedTeamMiddlePosition { get; }
    }
}