using Altzone.Scripts.Model;
using UnityEngine;

namespace Battle.Test.Scripts.Battle.Players
{
    public interface IPlayerDriver
    {
        string NickName { get; }
        int ActorNumber { get; }
        int PlayerPos { get; }
        CharacterModel CharacterModel { get; }

        void MoveTo(Vector2 targetPosition);
    }
}