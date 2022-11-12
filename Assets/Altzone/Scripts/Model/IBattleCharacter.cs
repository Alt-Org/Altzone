namespace Altzone.Scripts.Model
{
    /// <summary>
    /// Read-only interface for <c>IBattleCharacter</c>.
    /// </summary>
    public interface IBattleCharacter
    {
        Defence MainDefence { get; }
        int Speed { get; }
        int Resistance { get; }
        int Attack { get; }
        int Defence { get; }        
    }
}