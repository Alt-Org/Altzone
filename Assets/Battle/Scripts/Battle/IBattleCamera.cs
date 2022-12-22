using UnityEngine;

namespace Battle.Scripts.Battle
{
    internal interface IBattleCamera
    {
        Camera Camera { get; }
        bool IsRotated { get; }

        void DisableAudio();
    }
}
