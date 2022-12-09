using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Battle.Scripts.Battle
{
    public interface IPlayerDriver
    {
        void MoveTo(Vector2 targetPosition);
    }
}
