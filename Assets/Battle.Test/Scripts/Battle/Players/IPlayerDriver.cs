using Altzone.Scripts.Model;

namespace Battle.Test.Scripts.Battle.Players
{
    public interface IPlayerDriver
    {
        string NickName { get; }
        int ActorNumber { get; }
        int PlayerPos { get; }
        CharacterModel CharacterModel { get; }
    }
}