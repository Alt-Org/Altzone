namespace Altzone.Scripts.Model
{
    /// <summary>
    /// Read-only interface for Battle Character that is composite of <c>CharacterModel</c> and <c>CustomCharacterModel</c>.
    /// </summary>
    public interface IBattleCharacter
    {
        string Name { get; }
        int CustomCharacterModelId { get; }
        Defence MainDefence { get; }
        int Speed { get; }
        int Resistance { get; }
        int Attack { get; }
        int Defence { get; }
    }
}