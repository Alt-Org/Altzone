using Altzone.Scripts.Model;
using UnityEngine;

namespace Battle.Scripts.Battle
{
    /// <summary>
    /// General player management interface to manipulate player state without concerns how (or where) it is represented visually.<br />
    /// Player visual state can be managed locally and/or remotely.
    /// </summary>
    /// <remarks>
    /// WIKI page: https://github.com/Alt-Org/Altzone/wiki/Player-Prefab
    /// </remarks>
    internal interface IPlayerDriver
    {
        string NickName { get; }

        /// <summary>
        /// ActorNumber is unique identifier for any player.
        /// </summary>
        /// <remarks>
        /// Currently Photon uses positive and static player negative values.
        /// </remarks>
        int ActorNumber { get; }
        
        /// <summary>
        /// Number of "our" other instances that have reported.
        /// </summary>
        int PeerCount { get; }

        bool IsValid { get; }
        int PlayerPos { get; }
        int TeamNumber { get; }
        int MaxPoseIndex { get; }
        bool IsLocal { get; }
        CharacterModel CharacterModel { get; }
        Vector2 Position { get; }
        Transform PlayerTransform { get; }

        IPlayerActorCollision PlayerActorCollision { get; }

        void Rotate(bool isUpsideDown);

        void FixCameraRotation(Camera gameCamera);

        void MoveTo(Vector2 targetPosition);

        void SetCharacterPose(int poseIndex);

        void SetPlayMode(BattlePlayMode playMode);

        void SetShieldVisibility(bool state);

        void SetShieldResistance(int resistance);

        void SetStunned(float duration);

        void StopAndRestartBall();

        void PlayerActorDestroyed();
    }

    /// <summary>
    /// Local player callback interface for collision handling.
    /// </summary>
    internal interface IPlayerActorCollision
    {
        void OnShieldCollision(Collision2D collision, MonoBehaviour component);

        void OnHeadCollision(Collision2D collision);
    }

    /// <summary>
    /// Helper interface to manage more complex player state separately.
    /// </summary>
    internal interface IPlayerDriverState
    {
        void ResetState(IPlayerDriver playerDriver, CharacterModel characterModel);
        void CheckRotation(Vector2 position);
        void OnShieldCollision(out string debugString);
        void OnHeadCollision();
    }
}