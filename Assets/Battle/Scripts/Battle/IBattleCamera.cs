using UnityEngine;

namespace Battle.Scripts.Battle
{
    public interface IBattleCamera
    {
        Camera Camera { get; }
        bool IsRotated { get; }
        void DisableAudio();
    }
}
