using UnityEngine;

namespace Battle.Scripts.Battle
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