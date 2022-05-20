using Altzone.Scripts.Model;
using Battle.Scripts.Battle.interfaces;
using UnityEngine;

namespace Battle.Test.Scripts.Battle.Players
{
    internal interface IPlayerDriver
    {
        string NickName { get; }
        int ActorNumber { get; }
        int PlayerPos { get; }
        int MaxPoseIndex { get; }
        bool IsLocal { get; }
        CharacterModel CharacterModel { get; }

        void SetStunned(float duration);
        
        void MoveTo(Vector2 targetPosition);
 
        void SetCharacterPose(int poseIndex);
        
        void SetPlayMode(BattlePlayMode playMode);
    }
}