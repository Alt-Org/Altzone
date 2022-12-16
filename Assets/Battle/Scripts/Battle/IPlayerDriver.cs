using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.Scripts.Battle
{
    public interface IPlayerDriver
    {
        void Rotate(float angle);

        void MoveTo(Vector2 targetPosition);

        void MoveTo(GridPos gridPos);
    }
}
