using Battle.Scripts.Battle.interfaces;
using UnityEngine;

namespace Battle.Test.Scripts.Battle.Players
{
    internal interface IPlayerActor
    {
        float Speed { get; set; }

        void MoveTo(Vector2 targetPosition);

        void SetCharacterPose(int poseIndex);
        
        void SetPlayMode(BattlePlayMode playMode);

        void ResetPlayerDriver();
    }
}