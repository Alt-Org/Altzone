using UnityEngine;

namespace Battle.Test.Scripts.Battle
{
    internal class BattleCamera : MonoBehaviour, IBattleCamera
    {
        public Camera Camera => throw new System.NotImplementedException();

        public bool IsRotated => throw new System.NotImplementedException();

        public void DisableAudio()
        {
            throw new System.NotImplementedException();
        }
    }

    internal class BattleBackground : MonoBehaviour, IBattleBackground
    {
        public GameObject Background => throw new System.NotImplementedException();

        public bool IsRotated => throw new System.NotImplementedException();

        public void SetBackgroundImageByIndex(int index)
        {
            throw new System.NotImplementedException();
        }
    }

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

    internal static class Context
    {
        internal static IBattleCamera GetBattleCamera => Object.FindObjectOfType<BattleCamera>();

        internal static IBattleBackground GetBattleBackground => Object.FindObjectOfType<BattleBackground>();

        internal static IBattlePlayArea GetBattlePlayArea => Object.FindObjectOfType<BattlePlayArea>();
    }
}