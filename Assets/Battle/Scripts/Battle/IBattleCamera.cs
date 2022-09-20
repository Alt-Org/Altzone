using UnityEngine;

namespace Battle.Scripts.Battle
{
    /// <summary>
    /// Game camera.
    /// </summary>
    public interface IBattleCamera
    {
        Camera Camera { get; }
        bool IsRotated { get; }
        void DisableAudio();
    }
}