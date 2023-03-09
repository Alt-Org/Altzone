using UnityEngine;

namespace Battle.Scripts.Battle
{
    internal interface IPlayerActor
    {
        public bool IsBusy { get; }

        void MoveTo(Vector2 targetPosition);

        void SetRotation(float angle);

        void ShieldHit(int damage);
    }
}
