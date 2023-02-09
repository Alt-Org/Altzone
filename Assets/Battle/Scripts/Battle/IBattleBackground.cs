using UnityEngine;

namespace Battle.Scripts.Battle
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
