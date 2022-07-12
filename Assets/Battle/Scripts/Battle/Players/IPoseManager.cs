using UnityEngine;

namespace Battle.Scripts.Battle.Players
{
    internal interface IPoseManager
    {
        bool IsVisible { get; }
        int MaxPoseIndex { get; }

        void Reset(int poseIndex, BattlePlayMode playMode, bool isVisible, string tag);
        void SetPlayMode(BattlePlayMode playMode);
        void SetVisible(bool isVisible);
        void SetPose(int poseIndex);
    }
}