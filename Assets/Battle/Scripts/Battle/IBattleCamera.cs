using UnityEngine;

namespace Battle.Scripts.Battle
{
    internal interface IBattleCamera
    {
        Camera Camera { get; }
        void Rotate(bool isUpsideDown);
        bool IsRotated { get; }

        void DisableAudio();
    }
}
