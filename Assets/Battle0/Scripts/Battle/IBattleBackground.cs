using UnityEngine;

namespace Battle0.Scripts.Battle
{
    /// <summary>
    /// Game background.
    /// </summary>
    internal interface IBattleBackground
    {
        GameObject Background { get; }
        bool IsRotated { get; }
        void SetBackgroundImageByIndex(int index);
    }
}