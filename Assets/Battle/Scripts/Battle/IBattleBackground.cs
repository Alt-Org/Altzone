using UnityEngine;

namespace Battle.Scripts.Battle
{
    /// <summary>
    /// Game background.
    /// </summary>
    public interface IBattleBackground
    {
        GameObject Background { get; }
        bool IsRotated { get; }
        void SetBackgroundImageByIndex(int index);
    }
}