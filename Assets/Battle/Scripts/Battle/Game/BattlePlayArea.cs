using UnityEngine;

namespace Battle.Scripts.Battle.Game
{
    internal class BattlePlayArea : MonoBehaviour, IBattlePlayArea
    {
        public Rect GetPlayerPlayArea(int playerPos)
        {
            throw new System.NotImplementedException();
        }

        public Vector2 GetPlayerStartPosition(int playerPos)
        {
            throw new System.NotImplementedException();
        }

        public Collider2D BlueTeamCollider => throw new System.NotImplementedException();

        public Collider2D RedTeamCollider => throw new System.NotImplementedException();

        public Transform BlueTeamMiddlePosition => throw new System.NotImplementedException();

        public Transform RedTeamMiddlePosition => throw new System.NotImplementedException();
    }
}