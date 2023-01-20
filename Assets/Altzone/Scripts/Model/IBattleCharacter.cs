using Altzone.Scripts.Model.Dto;

namespace Altzone.Scripts.Model
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
        int CustomCharacterModelId { get; }
        int PlayerPrefabId { get; }
        Defence MainDefence { get; }
        int Speed { get; }
        int Resistance { get; }
        int Attack { get; }
        int Defence { get; }
    }
}