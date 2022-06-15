using UnityEngine;

namespace Battle.Test.Scripts.Battle
{
    public interface IBattleBackground
    {
        GameObject Background { get; }
        bool IsRotated { get; }
        void SetBackgroundImageByIndex(int index);
    }
}