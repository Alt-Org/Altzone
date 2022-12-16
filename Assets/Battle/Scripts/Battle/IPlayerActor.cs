using UnityEngine;

namespace Battle.Scripts.Battle
{
    internal interface IPlayerActor
    {
        void MoveTo(Vector2 targetPosition);

        void Rotate(float angle);
    }
}
