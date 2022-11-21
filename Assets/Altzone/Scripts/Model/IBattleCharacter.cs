namespace Altzone.Scripts.Model
{
    /// <summary>
    /// Read-only interface for <c>IBattleCharacter</c>.
    /// </summary>
    public interface IBattleCharacter
    {
        string Name { get; }
        int CustomCharacterId { get; }
        Defence MainDefence { get; }
        int Speed { get; }
        int Resistance { get; }
        int Attack { get; }
        int Defence { get; }
    }
}