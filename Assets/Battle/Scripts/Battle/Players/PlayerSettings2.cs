using UnityEngine;

namespace Battle.Scripts.Battle.Players
{
    internal class PlayerSettings2 : MonoBehaviour
    {
        [Header("Settings"), SerializeField] private Transform _geometryRoot;
        [SerializeField] private Transform _avatarsRoot;
        [SerializeField] private Transform _shieldsRoot;
        [SerializeField] private PlayerCollider2 _avatarsCollider;
        [SerializeField] private PlayerCollider2 _shieldsCollider;

        public IPoseManager GetAvatarPoseManager => new DummyPoseManager();

        public IPoseManager GetShieldPoseManager => new DummyPoseManager();

        public void Rotate(bool isUpsideDown)
        {
            _geometryRoot.Rotate(isUpsideDown);
        }
    }

    internal class DummyPoseManager : IPoseManager
    {
        public bool IsVisible => false;

        public void Reset(int poseIndex, BattlePlayMode playMode, bool isVisible, string tag)
        {
        }

        public void SetPlayMode(BattlePlayMode playMode)
        {
        }

        public void SetVisible(bool isVisible)
        {
        }

        public void SetPose(int poseIndex)
        {
        }
    }
}