using UnityEngine;

namespace Battle.Test.Scripts.Battle.Players
{
    public interface IPlayerActor
    {
        float Speed { get; set; }

        void MoveTo(Vector2 targetPosition);
    }
}