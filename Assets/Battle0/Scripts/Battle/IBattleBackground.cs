using UnityEngine;

namespace Battle0.Scripts.Battle
{
    /// <summary>
    /// Game background.
    /// </summary>
    internal interface IBattleBackground
    {
        void Rotate(bool isUpsideDown);
        bool IsRotated { get; }
        void SetBackgroundImageByIndex(int index);
    }
}