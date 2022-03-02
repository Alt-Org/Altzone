using UnityEngine;

namespace Battle.Scripts.Battle.interfaces
{
    internal class PlayerLineResult
    {
        public readonly IPlayerActor PlayerActor;
        public readonly float DistanceY;
        public readonly Vector2 Force;

        public PlayerLineResult(IPlayerActor playerActor, float distanceY, Vector2 force)
        {
            PlayerActor = playerActor;
            DistanceY = distanceY;
            Force = force;
        }
    }


    internal interface IPlayerLineConnector
    {
        void Connect(IPlayerActor playerActor);
        PlayerLineResult GetNearest();
        void Hide();
    }
}