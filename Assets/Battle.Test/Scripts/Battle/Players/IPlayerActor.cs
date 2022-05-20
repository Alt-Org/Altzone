using Battle.Scripts.Battle.interfaces;
using UnityEngine;

namespace Battle.Test.Scripts.Battle.Players
{
    internal enum PlayerBuff
    {
        Stunned = 1,
    }
        
    internal interface IPlayerActor
    {
        float Speed { get; set; }

        void MoveTo(Vector2 targetPosition);

        void SetCharacterPose(int poseIndex);
        
        void SetPlayMode(BattlePlayMode playMode);

        void SetBuff(PlayerBuff buff, float duration);
        
        void ResetPlayerDriver();
    }
}