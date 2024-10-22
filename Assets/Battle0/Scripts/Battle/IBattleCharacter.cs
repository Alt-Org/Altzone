using Altzone.Scripts.Model.Poco.Game;

namespace Battle0.Scripts.Battle
{
    /// <summary>
    /// Read-only interface for Battle Character that is composite of <c>CharacterClassModel</c> and <c>CustomCharacterModel</c>.
    /// </summary>
    /// <remarks>
    /// See https://github.com/Alt-Org/Altzone/wiki/Battle-Pelihahmo
    /// </remarks>
    public interface IBattleCharacter
    {
        string Name { get; }
        string CharacterClassName { get; }
        int CustomCharacterModelId { get; }
        int CharacterClassModelId { get; }
        int PlayerPrefabId { get; }
        CharacterClassID GestaltCycle { get; }
        int Speed { get; }
        int Resistance { get; }
        int Attack { get; }
        int Defence { get; }
    }
}