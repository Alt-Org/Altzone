using Battle.Scripts.Battle.interfaces;
using UnityEngine;

namespace Battle.Test.Scripts.Battle.Players
{
    internal enum PlayerBuff
    {
        Stunned = 1,
    }

    /// <summary>
    /// Local player management interface to keep visual player representation synchronized with "player state" managed elsewhere.<br />
    /// Player state can be managed locally and/or remotely.
    /// </summary>
    internal interface IPlayerActor
    {
        int MaxPoseIndex { get; }
        
        Vector2 Position { get; }
        
        float Speed { get; set; }
        
        int CurrentResistance { get; set; }

        void Rotate(bool isUpsideDown);
        
        void FixCameraRotation(Camera gameCamera);
        
        void MoveTo(Vector2 targetPosition);

        void SetCharacterPose(int poseIndex);
        
        void SetPlayMode(BattlePlayMode playMode);
        
        void SetShieldVisibility(bool state);

        void SetBuff(PlayerBuff buff, float duration);
        
        void ResetPlayerDriver();
    }
}