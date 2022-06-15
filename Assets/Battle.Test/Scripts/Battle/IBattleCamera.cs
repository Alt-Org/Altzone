using UnityEngine;

namespace Battle.Test.Scripts.Battle
{
    public interface IBattleCamera
    {
        Camera Camera { get; }
        bool IsRotated { get; }
        void DisableAudio();
    }
}