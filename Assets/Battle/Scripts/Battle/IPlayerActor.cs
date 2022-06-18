using UnityEngine;

namespace Battle.Scripts.Battle
{
    internal enum PlayerBuff
    {
        Stunned = 1,
    }

    /// <summary>
    /// Local player management interface to keep visual player representation synchronized with "player state" managed elsewhere.<br />
    /// Player state can be managed locally and/or remotely.
    /// </summary>
    /// <remarks>
    /// WIKI page: https://github.com/Alt-Org/Altzone/wiki/Player-Prefab
    /// </remarks>
    internal interface IPlayerActor
    {
        /// <summary>
        /// <c>IPlayerActor</c> game object instance root.
        /// </summary>
        GameObject GameObject { get; }
        
        /// <summary>
        /// <c>IPlayerActor</c> game object <c>Transform</c> for all measurements.
        /// </summary>
        Transform Transform { get; }

        int MaxPoseIndex { get; }
        
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