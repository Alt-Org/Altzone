using UnityEngine;

namespace Battle.Scripts.Battle
{
    internal interface IPlayerDriver
    {
        void Rotate(float angle);

        void MoveTo(Vector2 targetPosition);

        void MoveTo(GridPos gridPos);
    }
}
